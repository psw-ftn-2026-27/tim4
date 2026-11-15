namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, long?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds);
}
