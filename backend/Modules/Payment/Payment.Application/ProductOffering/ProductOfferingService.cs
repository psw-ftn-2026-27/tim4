using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Application.ProductOffering;

public sealed class ProductOfferingService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductOfferingService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product(dto.EntityId, dto.Name, dto.OwnerId, dto.Price);
        var existing = await _productRepository.GetByEntityIdAsync(dto.EntityId);
        if (existing is not null)
        {
            existing.EnsureSameOffer(product);
            return;
        }

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
