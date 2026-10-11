# DummyJSON MCP Server

A lightweight .NET MCP (Model Context Protocol) server that exposes the [DummyJSON API](https://dummyjson.com/) as tools for AI assistants.

The server allows MCP-compatible clients to search and retrieve products, explore product categories, manage simulated product and cart operations, and check connectivity to the DummyJSON API.

## Features

- Search products by keyword.
- Retrieve products with pagination and metadata.
- Sort products by a specified field in ascending or descending order.
- Filter products by modification date using `modifiedAfter` and `modifiedBefore`.
- Retrieve individual products by ID.
- Retrieve products by category.
- List product categories and category details.
- Simulate adding, updating, and deleting products.
- Retrieve carts and individual cart details.
- Retrieve carts belonging to a specific user.
- Simulate adding, updating, and deleting carts.
- Check connectivity to the DummyJSON API.
- Expose tools through an MCP Streamable HTTP endpoint.
- Start a temporary Cloudflare Tunnel using the included Windows PowerShell script.

## Available Tools

### Health Check

| Tool | Description |
|---|---|
| `health_check` | Checks connectivity to the DummyJSON API. |

### Products

| Tool | Description |
|---|---|
| `get_products` | Retrieves products with pagination metadata, sorting, and optional modification-date filters. |
| `get_product` | Retrieves a product by its numeric ID. |
| `get_products_by_category` | Retrieves products belonging to a specified category. |
| `search_products` | Searches products using a query string. |
| `get_product_categories` | Retrieves product categories and their details. |
| `get_product_category_list` | Retrieves a list of product category names. |
| `add_product` | Simulates adding a product. |
| `update_product` | Simulates updating an existing product. |
| `delete_product` | Simulates deleting a product by ID. |

### Carts

| Tool | Description |
|---|---|
| `get_carts` | Retrieves carts with their products and pagination metadata. |
| `get_cart` | Retrieves a cart by its numeric ID. |
| `get_carts_by_user` | Retrieves carts belonging to a specified user ID. |
| `add_cart` | Simulates adding a cart. |
| `update_cart` | Simulates updating an existing cart. |
| `delete_cart` | Simulates deleting a cart by ID. |

**Note:** DummyJSON simulates product and cart creation, updates, and deletion. These operations return mock responses and do not permanently modify the underlying database.

## Product Listing: Pagination, Sorting, and Date Filters

The `get_products` tool supports the following parameters:

| Parameter | Type | Description | Default |
|---|---|---|---|
| `limit` | Integer | Maximum number of products to return. | `0` |
| `skip` | Integer | Number of products to skip. | `0` |
| `sortBy` | String | Field used to sort the products. | API default |
| `order` | String | Sort direction: `asc` or `desc`. | API default |
| `modifiedAfter` | String | Returns products whose metadata modification date is after the specified date. | None |
| `modifiedBefore` | String | Returns products whose metadata modification date is before the specified date. | None |

### Pagination

The tool defaults to `limit = 0` and `skip = 0`. In the current implementation, a limit of zero is omitted from the request, allowing DummyJSON to apply its default page size of 30 products.

For example, to retrieve five products starting from the sixth product:

```json
{
  "limit": 5,
  "skip": 5
}
```

### Sorting

Specify a field and sort direction:

```json
{
  "limit": 10,
  "sortBy": "price",
  "order": "asc"
}
```

### Date Filtering

The date filters accept ISO 8601 date-time strings. Use a `Z` suffix for UTC timestamps or provide an explicit timezone offset.

For example, retrieve products modified after January 1, 2026:

```json
{
  "limit": 10,
  "modifiedAfter": "2026-01-01T00:00:00Z"
}
```

Retrieve products modified before the same date:

```json
{
  "limit": 10,
  "modifiedBefore": "2026-01-01T00:00:00Z"
}
```

Combine both filters to specify a date range:

```json
{
  "limit": 10,
  "modifiedAfter": "2025-01-01T00:00:00Z",
  "modifiedBefore": "2026-01-01T00:00:00Z"
}
```

The date strings are parsed by the server and passed to the API as query parameters. Filtering behavior depends on the DummyJSON API implementation.

## Requirements

- .NET SDK compatible with the project.
- Internet access to reach DummyJSON.
- An MCP-compatible client.
- Windows and PowerShell for the Cloudflare Tunnel helper script.

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

Ensure that the named `DummyJson` HTTP client is configured with the appropriate base address and timeout.

## Running the Server

From the project directory, run:

```powershell
dotnet restore
dotnet build
dotnet run
```

The MCP endpoint is expected at:

```text
http://localhost:5000/mcp
```

Ensure the application is configured to listen on port `5000`, or substitute the port configured for your environment.

## Cloudflare Tunnel (Windows)

The repository includes `start-clouflare-tunnel.ps1`, a PowerShell helper script for exposing the local MCP server through a temporary Cloudflare Tunnel.

The script is intended to:

- Install or manage the `cloudflared` Windows service if required.
- Check whether the local server is running on the configured port.
- Use port `5000` by default, or accept a custom port through `-Port`.
- Create a temporary public tunnel to the local server.
- Return the temporary Cloudflare URL for connecting an external MCP client.

The generated URL is temporary and may change when the script is restarted.

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

Keep the script running while the tunnel is needed. The URL is not intended to be a permanent endpoint.

**Security note:** Run PowerShell with the permissions required by the script to install or manage Windows services. Review the script before running it, especially if it modifies system services. Exposing a local MCP endpoint publicly can make its tools accessible to remote clients, so consider authentication and access restrictions before using it with sensitive systems.

## Connecting an MCP Client

Configure your MCP-compatible client to use the generated tunnel URL, including `/mcp`.

Example endpoint:

```text
https://example.trycloudflare.com/mcp
```

Replace the example hostname with the URL printed by your script.

The endpoint must be reachable while both the .NET server and Cloudflare Tunnel are running.

## Troubleshooting

- **Connection errors:** Confirm that the .NET server is running and listening on the expected port.
- **Invalid request URI:** Verify that `DummyJson:BaseUrl` is an absolute URL and that the named `HttpClient` has the correct `BaseAddress`.
- **Tool invocation errors:** Check the application logs for exceptions raised by individual tool methods. Ensure tool arguments match the parameter types and formats expected by the server.
- **Date-filter errors:** Supply valid ISO 8601 date-time strings, such as `2026-01-01T00:00:00Z`. Check the generated query string if filtering fails.
- **Tunnel errors:** Verify that `cloudflared` is installed and that the selected local port is accessible.
- **Expired tunnel URL:** Restart the script and update the MCP client with the newly generated URL.
- **DummyJSON API errors:** Confirm that [https://dummyjson.com/](https://dummyjson.com/) is reachable from the host machine.

## Limitations

- Product and cart mutations are simulated by DummyJSON and are not persistent.
- The default product listing returns a page of results rather than necessarily retrieving the entire catalog.
- Date-filter support depends on the behavior of the upstream API.
- Temporary Cloudflare Tunnel URLs can change and are not intended for permanent hosting.
