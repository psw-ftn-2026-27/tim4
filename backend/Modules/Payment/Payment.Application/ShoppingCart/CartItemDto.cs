namespace Payment.Application.ShoppingCart;

public sealed record CartItemDto(Guid Id, Guid ProductId, Guid EntityId, string Name, long Price);
