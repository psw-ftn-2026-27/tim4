namespace Payment.Contracts;

public sealed record CreateProductRequest(Guid EntityId, string Name, Guid OwnerId, decimal Price);
