namespace Payment.Domain.Wallets;

public sealed record PurchasePayment(Guid EntityId, long Amount);
