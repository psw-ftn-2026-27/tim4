using Payment.Application.ShoppingCart;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartReadRepository
{
    Task<CartDto?> GetCartAsync(Guid userId);
    Task<CartCheckoutDto?> GetCheckoutAsync(Guid userId, Guid checkoutId);
}
