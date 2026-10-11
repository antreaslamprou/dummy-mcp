param(
    [int]$Port = 5000
)

$ErrorActionPreference = "Stop"

Write-Host "`n=== Cloudflare Tunnel ===" -ForegroundColor Cyan

# ---------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------
function Update-SessionPath {
    $env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
                [Environment]::GetEnvironmentVariable("Path", "User")
}

function Find-Cloudflared {
    $cmd = Get-Command cloudflared -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # Fallback to common install locations (winget / manual / MSI)
    $candidates = @(
        "$env:ProgramFiles\cloudflared\cloudflared.exe",
        "${env:ProgramFiles(x86)}\cloudflared\cloudflared.exe",
        "$env:LOCALAPPDATA\Microsoft\WinGet\Links\cloudflared.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { return $c }
    }
    return $null
}

# Read a file that another process is still writing to
function Read-SharedFile([string]$Path) {
    if (-not (Test-Path $Path)) { return "" }
    try {
        $fs = [System.IO.File]::Open($Path, 'Open', 'Read', 'ReadWrite')
        try {
            $sr = New-Object System.IO.StreamReader($fs)
            return $sr.ReadToEnd()
        }
        finally { $fs.Dispose() }
    }
    catch { return "" }
}

# ---------------------------------------------------------------
# 1. Install cloudflared if needed
# ---------------------------------------------------------------
$cloudflared = Find-Cloudflared

if (-not $cloudflared) {
    Write-Host "cloudflared not found. Installing via winget..." -ForegroundColor Yellow

    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        throw "winget is not available. Install cloudflared manually: https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/"
    }

    winget install --id Cloudflare.cloudflared --exact `
        --accept-source-agreements `
        --accept-package-agreements

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install cloudflared (winget exit code $LASTEXITCODE)."
    }

    Update-SessionPath
    $cloudflared = Find-Cloudflared

    if (-not $cloudflared) {
        throw "cloudflared was installed but could not be located. Open a new terminal and re-run the script."
    }
}

Write-Host "Using cloudflared: $cloudflared" -ForegroundColor DarkGray

# ---------------------------------------------------------------
# 2. Check that something is listening on the port
# ---------------------------------------------------------------
$listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
    Select-Object -First 1

if (-not $listener) {
    Write-Host "No application is listening on port $Port." -ForegroundColor Red
    exit 1
}

$owner = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
$ownerName = if ($owner) { $owner.ProcessName } else { "unknown" }
Write-Host "Application detected on port $Port (process: $ownerName, PID $($listener.OwningProcess))." -ForegroundColor Green

# ---------------------------------------------------------------
# 3. Start the tunnel
# ---------------------------------------------------------------
# 127.0.0.1 avoids 502 errors when "localhost" resolves to ::1 (IPv6)
# but the app only listens on IPv4.
$localUrl = "http://127.0.0.1:$Port"

# cloudflared writes its logs to STDERR. Stdout and stderr MUST go to
# different files, otherwise Start-Process throws an error.
$outLog = Join-Path $env:TEMP "cloudflared.out.log"
$errLog = Join-Path $env:TEMP "cloudflared.err.log"
Remove-Item $outLog, $errLog -Force -ErrorAction SilentlyContinue

Write-Host "`nStarting tunnel for $localUrl ..." -ForegroundColor Cyan
Write-Host "Waiting for Cloudflare URL..." -ForegroundColor Yellow

$process = Start-Process -FilePath $cloudflared `
    -ArgumentList "tunnel", "--url", $localUrl `
    -RedirectStandardOutput $outLog `
    -RedirectStandardError $errLog `
    -NoNewWindow `
    -PassThru

$foundUrl  = $false
$startTime = Get-Date
$timeoutSec = 60

try {
    while (-not $process.HasExited) {
        if (-not $foundUrl) {
            $content = (Read-SharedFile $errLog) + "`n" + (Read-SharedFile $outLog)

            # Exclude api.trycloudflare.com, which also shows up in the logs
            $match = [regex]::Matches($content, 'https://(?!api\.)[a-zA-Z0-9-]+\.trycloudflare\.com') |
                Select-Object -First 1

            if ($match) {
                $foundUrl  = $true
                $publicUrl = $match.Value

                Write-Host "`nTunnel is ready!" -ForegroundColor Green
                Write-Host "Public URL: $publicUrl"
                Write-Host "MCP URL:    $publicUrl/mcp" -ForegroundColor Green
                Write-Host "`nPress Ctrl+C to stop the tunnel."
            }
            elseif (((Get-Date) - $startTime).TotalSeconds -gt $timeoutSec) {
                Write-Host "`nTimed out waiting for the tunnel URL. Last log output:" -ForegroundColor Red
                Write-Host (Read-SharedFile $errLog)
                break
            }
        }

        Start-Sleep -Milliseconds 500
        $process.Refresh()
    }

    if ($process.HasExited) {
        Write-Host "`ncloudflared exited with code $($process.ExitCode)." -ForegroundColor Yellow
        if (-not $foundUrl) {
            Write-Host "Log output:" -ForegroundColor Yellow
            Write-Host (Read-SharedFile $errLog)
        }
    }
}
finally {
    if ($process -and -not $process.HasExited) {
        Write-Host "`nStopping tunnel..." -ForegroundColor Yellow
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}