namespace DummyMcp.DTOs;

public record Cart(
    int Id,
    IReadOnlyList<CartProduct> Products,
    decimal Total,
    decimal DiscountedTotal,
    int UserId,
    int TotalProducts,
    int TotalQuantity 
);

public record CartProduct(
    int Id,
    string Title,
    decimal Price,
    int Quantity,
    decimal Total,
    decimal DiscountPercentage,
    decimal DiscountedTotal,
    string Thumbnail
);