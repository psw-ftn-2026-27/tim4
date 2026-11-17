using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _context;
    public ShoppingCartRepository(PaymentDbContext context) { _context = context; }

    public Task<ShoppingCart?> GetByUserIdAsync(Guid userId) => _context.ShoppingCarts
        .Include(cart => cart.Items).SingleOrDefaultAsync(cart => cart.UserId == userId);

    public void Add(ShoppingCart cart) => _context.ShoppingCarts.Add(cart);
}
