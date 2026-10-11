using DummyMcp.Configurations;
using DummyMcp.Tools;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Fetch the appsettings (environment variables) 
builder.Services.Configure<DummyJsonOptions>(
    builder.Configuration.GetSection(DummyJsonOptions.SectionName)
);

// Register the Dummy Json client, to be able to call the endpoints
builder.Services.AddHttpClient("DummyJson", (services, client) =>
{
    var options = services
        .GetRequiredService<IOptions<DummyJsonOptions>>()
        .Value;

    Console.WriteLine($"Configured BaseUrl: '{options.BaseUrl}'");

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// Register the MCP server endpoints
builder.Services.AddHealthChecks();
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Expose the endpoints
app.MapHealthChecks("/health");
app.MapMcp("/mcp");

app.Run();