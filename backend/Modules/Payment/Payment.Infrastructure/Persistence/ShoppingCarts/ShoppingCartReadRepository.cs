using Microsoft.EntityFrameworkCore;
using Payment.Application.CartCheckout;
using Payment.Application.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartReadRepository : IShoppingCartReadRepository
{
    private readonly PaymentDbContext _context;
    public ShoppingCartReadRepository(PaymentDbContext context) { _context = context; }

    public Task<CartDto?> GetCartAsync(Guid userId) => _context.ShoppingCarts
        .Where(cart => cart.UserId == userId)
        .Select(cart => new CartDto(cart.Id,
            cart.Items.OrderBy(item => item.Id).Select(item => new CartItemDto(
                item.Id, item.ProductId, item.EntityId, item.Name, item.Price)).ToList(),
            cart.Items.Sum(item => item.Price))).SingleOrDefaultAsync();

    public Task<CartCheckoutDto?> GetCheckoutAsync(Guid userId, Guid checkoutId) => _context.ShoppingCarts
        .Where(cart => cart.UserId == userId && cart.CheckoutId == checkoutId)
        .Select(cart => new CartCheckoutDto(checkoutId,
            cart.Items.OrderBy(item => item.Id).Select(item => new CartItemDto(
                item.Id, item.ProductId, item.EntityId, item.Name, item.Price)).ToList(),
            cart.Items.Sum(item => item.Price),
            _context.Wallets.Where(wallet => wallet.UserId == userId)
                .Select(wallet => (long?)wallet.Balance).SingleOrDefault() ?? 0)).SingleOrDefaultAsync();
}
