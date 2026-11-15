using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchases;
using Payment.Application.CartCheckout;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseReadRepository : IPurchaseReadRepository
{
    private readonly PaymentDbContext _context;
    public PurchaseReadRepository(PaymentDbContext context) { _context = context; }

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId) => _context.Purchases
        .AnyAsync(purchase => purchase.UserId == userId && purchase.Items.Any(item => item.EntityId == entityId));

    public Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId) => _context.Purchases
        .Where(purchase => purchase.UserId == userId)
        .SelectMany(purchase => purchase.Items)
        .Select(item => item.EntityId)
        .OrderBy(entityId => entityId)
        .ToListAsync();

    public Task<CompletedPurchaseDto?> GetCompletedAsync(Guid userId, Guid purchaseId) => _context.Purchases
        .Where(purchase => purchase.Id == purchaseId && purchase.UserId == userId)
        .Select(purchase => new CompletedPurchaseDto(purchase.Id, purchase.PurchasedAt,
            purchase.Items.OrderBy(item => item.Id).Select(item => new PurchasedItemDto(
                item.Id, item.ProductId, item.EntityId, item.Name, item.Price)).ToList(),
            purchase.Items.Sum(item => item.Price))).SingleOrDefaultAsync();
}
