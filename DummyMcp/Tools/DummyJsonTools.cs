using System.ComponentModel;
using System.Globalization;
using DummyMcp.DTOs;
using ModelContextProtocol.Server;

namespace DummyMcp.Tools;

// Create the MCP server tools for calling the DummyJson API
[McpServerToolType,
Description("Provides tools for calling the DummyJson API for retrieviend, altering, updating or deleting products and product categories. The changes will not be reflected in the database however the API will mock the answer like it was stored in the databse.")]
public class DummyJsonTools
{
    private readonly HttpClient _httpClient;

    public DummyJsonTools(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("DummyJson");
    }

    // Helper Functions
    private static string CalculateParameterQuery(int limit, int skip, string? sortBy, string? order, DateTimeOffset? modifiedAfter, DateTimeOffset? modifiedBefore)
    {
        var parameters = new List<string>();

        if (limit > 0)
            parameters.Add($"limit={limit}");

        if (skip > 0)
            parameters.Add($"skip={skip}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            parameters.Add($"sortBy={sortBy}");

        if (!string.IsNullOrWhiteSpace(order))
            parameters.Add($"order={order}");
        
        if (modifiedAfter.HasValue)
        {
            string date = modifiedAfter.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);

            parameters.Add($"modifiedAfter={Uri.EscapeDataString(date)}");
        }

        if (modifiedBefore.HasValue)
        {
            string date = modifiedBefore.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);

            parameters.Add($"modifiedBefore={Uri.EscapeDataString(date)}");
        }
        
        return parameters.Count == 0
        ? ""
        : "?" + string.Join("&", parameters);
    }

    private static DateTimeOffset? ParseDate(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var result))
        {
            throw new ArgumentException(
                $"{parameterName} must be a valid ISO 8601 date-time, " +
                "for example 2026-01-01T00:00:00Z.");
        }

        return result;
    }

    // --> Create all DummyJson MCP Server Tools <-- //

    [McpServerTool, 
    Description("Checks wether the current client can communicate with the DummyJson API. If this call is not succeeded, it means that either party fails to verify the connection.")]
    public async Task<bool> HealthCheck()
    {
        using var response = await _httpClient.GetAsync("test");
        var responseString = await response.Content.ReadAsStringAsync();
        return responseString.Contains("ok");
    }

    #region ProductsMcpTools

    [McpServerTool, 
    Description("Returns the total products in the database, with the total counter, skip counter and limit counter.")]
    public async Task<TotalProducts?> GetProducts(
        //TODO
        [Description("The number of results to show. Default to 0, which fetches 30 results.")] int limit = 0,
        [Description("The number of results to skip. Default to 0. Can be used for pagination.")] int skip = 0,
        [Description("The filed the sorting is done on. Default by id.")] string? sortBy = null,
        [Description("The order of sorting. Can be either \"asc\" or \"desc\".")] string? order = null,
        [Description("Will return items that the meta data have been modified after the date provided.")] string? modifiedAfter = null,
        [Description("Will return items that the meta data have been modified before the date provided.")] string? modifiedBefore = null
    )
    {
        DateTimeOffset? after = ParseDate(modifiedAfter, nameof(modifiedAfter));
        DateTimeOffset? before = ParseDate(modifiedBefore, nameof(modifiedBefore));
        var parameterQuery = CalculateParameterQuery(limit, skip, sortBy, order, after, before);
    
        using var response = await _httpClient.GetAsync($"products{parameterQuery}");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<TotalProducts>();
        return products;
    }

    [McpServerTool, 
    Description("Returns the product by id provided.")]
    public async Task<Product?> GetProduct(
        [Description("The unique numeric id of the product to retrieve.")] int id
    )
    {
        using var response = await _httpClient.GetAsync($"products/{id}");
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        return product;
    }

    [McpServerTool, 
    Description("Returns all products under a category.")]
    public async Task<TotalProducts?> GetProductsByCategory(
        [Description("The category name for the products to retrieved.")] string categoryName
    )
    {
        using var response = await _httpClient.GetAsync($"products/category/{categoryName}");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<TotalProducts>();
        return products;
    }
    
    [McpServerTool, 
    Description("Search through all products.")]
    public async Task<TotalProducts?> SearchProducts(
        [Description("The string literal that will be searched through all products.")] string query
    )
    {
        using var response = await _httpClient.GetAsync($"products/search?q={query}");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<TotalProducts>();
        return products;
    }

    [McpServerTool, 
    Description("Get all categories and their details.")]
    public async Task<List<ProductCategory>?> GetProductCategories()
    {
        using var response = await _httpClient.GetAsync("products/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();
        return categories;
    }
    
    [McpServerTool, 
    Description("Get all category names as a list.")]
    public async Task<List<string>?> GetProductCategoryList()
    {
        using var response = await _httpClient.GetAsync("products/category-list");
        response.EnsureSuccessStatusCode();
        var categoryList = await response.Content.ReadFromJsonAsync<List<string>>();
        return categoryList;
    }

    [McpServerTool, 
    Description("Add a new product. The added product will not be added to the databse however the API will mock the response as it was added.")]
    public async Task<Product?> AddProduct(
        [Description("The new product details.")] CreateProduct newProduct
    )
    {
        using var response = await _httpClient.PostAsJsonAsync("products/add", newProduct);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        return product;
    }

    [McpServerTool, 
    Description("Update an existing product details. The product will not be eddited on the databse however the API will mock the response as it was added.")]
    public async Task<Product?> UpdateProduct(
        [Description("The whole product details, including the id to specify which product will be updated.")] UpdateProduct updatedProduct)
    {
        using var response = await _httpClient.PutAsJsonAsync($"products/{updatedProduct.Id}", updatedProduct);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        return product;
    }

    [McpServerTool, 
    Description("Delete a product, that will basically be marked as deleted. The product will not be deleted or editted on the databse however the API will mock the response as it was added.")]
    public async Task<DeletedProduct?> DeleteProduct(
        [Description("The id for the product that will be deleted.")] int id
    )
    {
        using var response = await _httpClient.DeleteAsync($"products/{id}");
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<DeletedProduct>();
        return product;
    }

    #endregion

    #region CartsMcpTools

    [McpServerTool, 
    Description("Returns all products within a cart in the database, with the total counter, skip counter and limit counter.")]
    public async Task<TotalCarts?> GetCarts()
    {
        using var response = await _httpClient.GetAsync("carts");
        response.EnsureSuccessStatusCode();
        var carts = await response.Content.ReadFromJsonAsync<TotalCarts>();
        return carts;
    }

    [McpServerTool, 
    Description("Returns all products within a cart by cart id provided.")]
    public async Task<Cart?> GetCart(
        [Description("The unique numeric id of the cart to retrieve.")] int id
    )
    {
        using var response = await _httpClient.GetAsync($"carts/{id}");
        response.EnsureSuccessStatusCode();
        var cart = await response.Content.ReadFromJsonAsync<Cart>();
        return cart;
    }

    [McpServerTool, 
    Description("Returns all carts with their products under a user.")]
    public async Task<TotalCarts?> GetCartsByUser(
        [Description("The user/owner id of the carts.")] int userId
    )
    {
        using var response = await _httpClient.GetAsync($"carts/user/{userId}");
        response.EnsureSuccessStatusCode();
        var carts = await response.Content.ReadFromJsonAsync<TotalCarts>();
        return carts;
    }

    [McpServerTool, 
    Description("Add a new cart with products. The added cart will not be added to the databse however the API will mock the response as it was added.")]
    public async Task<Cart?> AddCart(
        [Description("The new product details.")] CreateCart newCart
    )
    {
        using var response = await _httpClient.PostAsJsonAsync("carts/add", newCart);
        response.EnsureSuccessStatusCode();
        var cart = await response.Content.ReadFromJsonAsync<Cart>();
        return cart;
    }

    [McpServerTool, 
    Description("Update an existing cart with new products. The cart will not be eddited on the databse however the API will mock the response as it was added.")]
    public async Task<Cart?> UpdateCart(
        [Description("The whole product details, including the id to specify which product will be updated.")] UpdateCart updatedCart,
        [Description("The cart id to specify which cart will be updated.")] int cartId)
    {
        using var response = await _httpClient.PutAsJsonAsync($"carts/{cartId}", updatedCart);
        response.EnsureSuccessStatusCode();
        var cart = await response.Content.ReadFromJsonAsync<Cart?>();
        return cart;
    }

    [McpServerTool, 
    Description("Delete a cart, that will basically be marked as deleted. The cart will not be deleted or editted on the databse however the API will mock the response as it was added.")]
    public async Task<DeletedCart?> DeleteCart(
        [Description("The id for the cart that will be deleted.")] int id
    )
    {
        using var response = await _httpClient.DeleteAsync($"carts/{id}");
        response.EnsureSuccessStatusCode();
        var cart = await response.Content.ReadFromJsonAsync<DeletedCart>();
        return cart;
    }

    #endregion
}