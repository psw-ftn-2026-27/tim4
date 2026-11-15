using Payment.Application.Products;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;

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

    public async Task UpdateProductPriceAsync(Guid entityId, Guid ownerId, long price)
    {
        var product = await _productRepository.GetByEntityIdAsync(entityId)
            ?? throw new NotFoundException("The product does not exist.");
        product.ChangePrice(ownerId, price);
        await _unitOfWork.SaveChangesAsync();
    }
}
