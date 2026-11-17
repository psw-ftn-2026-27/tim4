namespace Payment.Application.CartCheckout;

public sealed record CartCheckoutDto(Guid Id, List<CartItemDto> Items, long Total, long AvailableBalance);
