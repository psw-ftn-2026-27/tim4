using Payment.Domain.Wallets;
using Shared.Domain;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<Dictionary<Guid, long>> GetBalancesAsync(IReadOnlyCollection<Guid> userIds);

    Task<PageResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId, int page, int pageSize, TransactionStatus? status);
}
