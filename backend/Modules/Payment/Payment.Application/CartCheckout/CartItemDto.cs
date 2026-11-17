namespace Payment.Application.CartCheckout;

public sealed record CartItemDto(Guid Id, Guid ProductId, Guid EntityId, string Name, long Price);
