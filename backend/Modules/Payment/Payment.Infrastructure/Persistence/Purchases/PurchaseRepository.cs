using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchases;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseRepository : IPurchaseRepository
{
    private readonly PaymentDbContext _context;
    public PurchaseRepository(PaymentDbContext context) { _context = context; }

    public Task<Purchase?> GetByCheckoutAsync(Guid userId, Guid checkoutId) => _context.Purchases
        .SingleOrDefaultAsync(purchase => purchase.UserId == userId && purchase.CheckoutId == checkoutId);

    public Task<List<Purchase>> GetByContentAsync(Guid userId, IReadOnlyCollection<Guid> entityIds) =>
        _context.Purchases.Include(purchase => purchase.Items)
            .Where(purchase => purchase.UserId == userId && purchase.Items.Any(item => entityIds.Contains(item.EntityId)))
            .ToListAsync();

    public Task<List<Purchase>> GetByCouponAsync(Guid userId, Guid couponId) =>
        _context.Purchases.Include(purchase => purchase.Items)
            .Where(purchase => purchase.UserId == userId && purchase.AppliedCoupon!.CouponId == couponId)
            .ToListAsync();

    public void Add(Purchase purchase) => _context.Purchases.Add(purchase);
}
