namespace Payment.Application.CartCheckout;

public sealed record CartDto(Guid? Id, List<CartItemDto> Items, long Total);
