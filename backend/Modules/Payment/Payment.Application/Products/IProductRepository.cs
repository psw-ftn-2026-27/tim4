using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> productIds);
    Task<Product?> GetByEntityIdAsync(Guid entityId);
    Task<List<Product>> GetByOwnerAsync(Guid ownerId);
    void Add(Product product);
}
