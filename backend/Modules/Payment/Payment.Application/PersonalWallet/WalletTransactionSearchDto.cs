using Payment.Domain.Wallets;

namespace Payment.Application.PersonalWallet;

public sealed record WalletTransactionSearchDto(TransactionStatus? Status);
