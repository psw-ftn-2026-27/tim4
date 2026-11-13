using Payment.Application.PersonalWallet;
using Payment.Domain.Wallets;
using Shared.Domain;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<PageResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId, int page, int pageSize, TransactionStatus? status);
}
