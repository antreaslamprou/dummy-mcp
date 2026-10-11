namespace DummyMcp.DTOs;

public record Product(
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
    IReadOnlyList<string> Images
);

public record ProductDimensions(
    decimal Width,
    decimal Height,
    decimal Depth
);

public record ProductReviews(
    int Rating,
    string Comment,
    DateTimeOffset Date,
    string ReviewerName,
    string ReviewerEmail
);

public record ProductMeta(
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Barcode,
    string QrCode
);

public record ProductCategory(
    string Slug,
    string Name,
    string Url
);