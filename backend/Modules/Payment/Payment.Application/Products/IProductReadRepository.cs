namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, decimal?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds);
}
