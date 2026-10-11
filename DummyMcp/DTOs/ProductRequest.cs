namespace DummyMcp.DTOs;

public record CreateProduct(
    string Title,
    string Category,
    decimal Price,
    decimal DiscountPercentage,
    decimal Rating,
    int Stock,
    IReadOnlyList<string> Tags,
    string Brand,
    string Sku,
    int Weight,
    ProductDimensions Dimensions,
    string WarrantyInformation,
    string ShippingInformation,
    string AvailabilityStatus,
    IReadOnlyList<ProductReviews> Reviews,
    string ReturnPolicy,
    int MinimumOrderQuantity,
    ProductMeta Meta,
    string Thumbnail,
    IReadOnlyList<string> Images
);

public record UpdateProduct(
    int Id,
    string? Title = null,
    string? Category = null,
    decimal? Price = null,
    decimal? DiscountPercentage = null,
    decimal? Rating = null,
    int? Stock = null,
    IReadOnlyList<string>? Tags = null,
    string? Brand = null,
    string? Sku = null,
    int? Weight = null,
    ProductDimensions? Dimensions = null,
    string? WarrantyInformation = null,
    string? ShippingInformation = null,
    string? AvailabilityStatus = null,
    IReadOnlyList<ProductReviews>? Reviews = null,
    string? ReturnPolicy = null,
    int? MinimumOrderQuantity = null,
    ProductMeta? Meta = null,
    string? Thumbnail = null,
    IReadOnlyList<string>? Images = null
);

public record DeletedProduct(
    int Id,
    string Title,
    string Category,
    decimal Price,
    decimal DiscountPercentage,
    decimal Rating,
    int Stock,
    IReadOnlyList<string> Tags,
    string Brand,
    string Sku,
    int Weight,
    ProductDimensions Dimensions,
    string WarrantyInformation,
    string ShippingInformation,
    string AvailabilityStatus,
    IReadOnlyList<ProductReviews> Reviews,
    string ReturnPolicy,
    int MinimumOrderQuantity,
    ProductMeta Meta,
    string Thumbnail,
    IReadOnlyList<string> Images,
    bool IsDeleted,
    DateTimeOffset DeletedOn
);

public record TotalProducts(
    IReadOnlyList<Product> Products,
    int Total,
    int Skip,
    int Limit
);