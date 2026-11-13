using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public sealed record WalletTransactionDto(
    Guid Id,
    int Amount,
    DateTime SubmittedAt,
    Guid AdministratorId,
    TransactionStatus Status,
    Guid? CancelledByAdministratorId,
    DateTime? CancelledAt);
