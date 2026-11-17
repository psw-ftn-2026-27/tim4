using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductRepository : IProductRepository
{
    public Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> productIds) =>
        _dbContext.Products.Where(product => productIds.Contains(product.Id)).ToListAsync();
    private readonly PaymentDbContext _dbContext;

    public ProductRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByEntityIdAsync(Guid entityId) => _dbContext.Products
        .SingleOrDefaultAsync(product => product.EntityId == entityId);

    public void Add(Product product) => _dbContext.Products.Add(product);
}
