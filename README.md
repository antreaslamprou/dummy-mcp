# DummyJSON MCP Server

A lightweight .NET MCP (Model Context Protocol) server that exposes [DummyJSON](https://dummyjson.com/) product API functionality as tools for AI assistants.

The server allows an MCP-compatible client to search products, retrieve product details, explore categories, and simulate product creation, updates, and deletion.

## Features

- Search products by keyword.
- Retrieve products with pagination metadata.
- Get a product by ID.
- List product categories and category details.
- Retrieve products by category.
- Simulate adding, updating, and deleting products.
- Check connectivity to the DummyJSON API.
- Expose tools through an MCP Streamable HTTP endpoint.
- Start a temporary Cloudflare Tunnel using the included Windows PowerShell script.

## Available Tools

| Tool | Description |
|---|---|
| `search_products` | Search products using a keyword. |
| `get_products` | Retrieve the product listing and pagination information. |
| `get_product` | Retrieve a product by its numeric ID. |
| `get_product_category_list` | Get a list of product category names. |
| `get_product_categories` | Retrieve product categories and their details. |
| `get_products_in_category` | Retrieve products belonging to a category. |
| `add_product` | Simulate adding a product. |
| `update_product` | Simulate updating an existing product. |
| `delete_product` | Simulate deleting a product by ID. |
| `dummy_health_check` | Check connectivity to the DummyJSON API. |

**Note:** DummyJSON simulates product creation, updates, and deletion. These operations do not permanently modify the underlying product database.

## Requirements

- .NET SDK compatible with the project.
- Internet access to reach DummyJSON.
- Windows and PowerShell for the Cloudflare Tunnel helper script.
- An MCP-compatible client.

## Configuration

The server uses the `DummyJson` configuration section.

Example `appsettings.json`:

```json
{
  "DummyJson": {
    "BaseUrl": "https://dummyjson.com/",
    "TimeoutSeconds": 30
  }
}
```

| Setting | Description | Default |
|---|---|---|
| `BaseUrl` | Base URL of the DummyJSON API. | `https://dummyjson.com/` |
| `TimeoutSeconds` | HTTP request timeout in seconds. | `30` |

## Running the Server

From the project directory, run:

```powershell
dotnet restore
dotnet build
dotnet run
```

The MCP endpoint is exposed at:

```text
http://localhost:5000/mcp
```

Ensure the application is configured to listen on port `5000`, or use the port configured for your environment.

## Cloudflare Tunnel (Windows)

The repository includes `start-clouflare-tunnel.ps1`, a PowerShell helper script for exposing the local MCP server through a temporary Cloudflare Tunnel.

The script is intended to:

- Install the `cloudflared` Windows service if required.
- Check whether the local service is running on the configured port.
- Use port `5000` by default, or accept a custom port through `-Port`.
- Create a temporary public tunnel to the local server.
- Return the temporary Cloudflare URL for connecting an external MCP client.

The generated URL is temporary and is expected to change when the script is restarted.

### Usage

Start the tunnel using the default port:

```powershell
.\start-clouflare-tunnel.ps1
```

Use a custom port:

```powershell
.\start-clouflare-tunnel.ps1 -Port 5001
```

Replace `5001` with the port on which your MCP server is listening.

Once the tunnel starts, use the generated public URL with the MCP endpoint path:

```text
https://<temporary-cloudflare-url>/mcp
```

Keep the script running while the tunnel is needed. The tunnel URL is not intended to be a permanent endpoint.

> **Note:** Run PowerShell with the permissions required by the script to install or manage the Windows service. Review the script before running it, especially if it modifies system services.

## Connecting an MCP Client

Configure your MCP-compatible client to use the generated tunnel URL, including `/mcp`.

Example endpoint:

```text
https://example.trycloudflare.com/mcp
```

Replace the example hostname with the URL printed by your script.

The endpoint must be reachable while the .NET server and Cloudflare Tunnel are running.

## Troubleshooting

- **Connection errors:** Confirm the .NET server is running and listening on the expected port.
- **Invalid request URI:** Verify that `DummyJson:BaseUrl` is an absolute URL and that the configured `HttpClient` has the correct `BaseAddress`.
- **Tool invocation errors:** Check the application logs for exceptions raised by individual tool methods.
- **Tunnel errors:** Verify that `cloudflared` is installed and that the selected local port is accessible.
- **Expired tunnel URL:** Restart the script and update the MCP client with the newly generated URL.
- **DummyJSON API errors:** Confirm that `https://dummyjson.com/` is reachable from the host machine.