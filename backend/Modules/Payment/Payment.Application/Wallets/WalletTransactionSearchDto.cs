using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public sealed record WalletTransactionSearchDto(TransactionStatus? Status);
