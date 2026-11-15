using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductReadRepository : IProductReadRepository
{
    private readonly PaymentDbContext _context;

    public ProductReadRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<Guid, long?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds)
    {
        var requested = entityIds.Distinct().ToArray();
        var prices = await _context.Products
            .Where(product => requested.Contains(product.EntityId))
            .Select(product => new { product.EntityId, product.Price })
            .ToDictionaryAsync(product => product.EntityId, product => (long?)product.Price);

        foreach (var entityId in requested)
        {
            prices.TryAdd(entityId, null);
        }

        return prices;
    }
}
