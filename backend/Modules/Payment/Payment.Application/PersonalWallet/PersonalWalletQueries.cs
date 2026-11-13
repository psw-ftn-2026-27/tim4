using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.PersonalWallet;

public sealed class PersonalWalletQueries
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletReadRepository _walletReadRepository;

    public PersonalWalletQueries(IWalletRepository walletRepository, IWalletReadRepository walletReadRepository)
    {
        _walletRepository = walletRepository;
        _walletReadRepository = walletReadRepository;
    }

    public async Task<WalletBalanceDto> GetAvailableBalanceAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        return new WalletBalanceDto(wallet?.GetAvailableBalance() ?? 0);
    }

    public Task<PageResult<WalletTransactionDto>> GetUserTransactionsAsync(
        Guid userId, WalletTransactionSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _walletReadRepository.GetTransactionsAsync(userId, page, pageSize, search.Status);
    }
}
