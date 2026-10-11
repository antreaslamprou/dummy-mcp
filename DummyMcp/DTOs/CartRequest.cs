namespace DummyMcp.DTOs;

public record CreateCart(
    int UserId,
    IReadOnlyList<QuantityProducts> Products
);

public record UpdateCart(
    bool Merge,
    IReadOnlyList<QuantityProducts> Products
);

public record DeletedCart(
    int Id,
    IReadOnlyList<CartProduct> Products,
    decimal Total,
    decimal DiscountedTotal,
    int UserId,
    int TotalProducts,
    int TotalQuantity,
    bool IsDeleted,
    DateTimeOffset DeletedOn
);

public record QuantityProducts(
    int Id,
    int Quantity
);

public record TotalCarts(
    IReadOnlyList<Cart> Carts,
    int Total,
    int Skip,
    int Limit
);