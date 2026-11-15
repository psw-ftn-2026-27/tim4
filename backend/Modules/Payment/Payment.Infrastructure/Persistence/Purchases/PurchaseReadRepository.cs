using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCart;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseReadRepository : IPurchaseReadRepository
{
    private readonly PaymentDbContext _context;
    public PurchaseReadRepository(PaymentDbContext context) { _context = context; }

    public Task<CompletedPurchaseDto?> GetCompletedAsync(Guid userId, Guid purchaseId) => _context.Purchases
        .Where(purchase => purchase.Id == purchaseId && purchase.UserId == userId)
        .Select(purchase => new CompletedPurchaseDto(purchase.Id, purchase.PurchasedAt,
            purchase.Items.OrderBy(item => item.Id).Select(item => new PurchasedItemDto(
                item.Id, item.ProductId, item.EntityId, item.Name, item.Price)).ToList(),
            purchase.Items.Sum(item => item.Price))).SingleOrDefaultAsync();
}
