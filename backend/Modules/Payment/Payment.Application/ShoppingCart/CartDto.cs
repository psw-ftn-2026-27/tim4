namespace Payment.Application.ShoppingCart;

public sealed record CartDto(Guid? Id, List<CartItemDto> Items, long Total);
