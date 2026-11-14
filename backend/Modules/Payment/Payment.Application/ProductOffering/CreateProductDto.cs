namespace Payment.Application.ProductOffering;

public sealed record CreateProductDto(Guid EntityId, string Name, Guid OwnerId, decimal Price);
