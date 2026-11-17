using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public sealed record WalletTransactionDto(
    Guid Id,
    long Amount,
    DateTime SubmittedAt,
    Guid? AdministratorId,
    TransactionStatus Status,
    Guid? CancelledByAdministratorId,
    DateTime? CancelledAt,
    TransactionKind Kind,
    Guid? PurchaseId,
    Guid? EntityId);
