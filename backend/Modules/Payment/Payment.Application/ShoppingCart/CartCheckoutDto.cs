namespace Payment.Application.ShoppingCart;

public sealed record CartCheckoutDto(Guid Id, List<CartItemDto> Items, long Total, long AvailableBalance);
