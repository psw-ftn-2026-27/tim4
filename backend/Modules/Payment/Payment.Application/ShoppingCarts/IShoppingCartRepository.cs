using Payment.Domain.ShoppingCarts;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId);
    void Add(ShoppingCart cart);
}
