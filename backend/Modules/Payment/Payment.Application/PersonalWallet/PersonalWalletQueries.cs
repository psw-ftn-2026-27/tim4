using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.PersonalWallet;

public sealed class PersonalWalletQueries
{
    private readonly IWalletReadRepository _walletReadRepository;

    public PersonalWalletQueries(IWalletReadRepository walletReadRepository)
    {
        _walletReadRepository = walletReadRepository;
    }

    public async Task<WalletBalanceDto> GetAvailableBalanceAsync(Guid userId)
    {
        return await _walletReadRepository.GetBalanceAsync(userId) ?? new WalletBalanceDto(0);
    }

    public Task<PageResult<WalletTransactionDto>> GetUserTransactionsAsync(
        Guid userId, WalletTransactionSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _walletReadRepository.GetTransactionsAsync(userId, page, pageSize, search.Status);
    }
}
