using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartQueries
{
    private readonly IShoppingCartReadRepository _carts;
    private readonly IPurchaseReadRepository _purchases;

    public ShoppingCartQueries(IShoppingCartReadRepository carts, IPurchaseReadRepository purchases)
    {
        _carts = carts;
        _purchases = purchases;
    }

    public async Task<CartDto> GetCartAsync(Guid userId) => await _carts.GetCartAsync(userId)
        ?? new CartDto(null, [], 0);

    public async Task<CartCheckoutDto> GetCartCheckoutAsync(Guid userId, Guid checkoutId) =>
        await _carts.GetCheckoutAsync(userId, checkoutId)
        ?? throw new NotFoundException("The prepared checkout does not exist. Refresh the cart.");

    public async Task<CompletedPurchaseDto> GetCompletedPurchaseAsync(Guid userId, Guid purchaseId) =>
        await _purchases.GetCompletedAsync(userId, purchaseId)
        ?? throw new NotFoundException("The purchase does not exist.");
}
