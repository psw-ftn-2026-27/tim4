using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationQueries
{
    private readonly IIdentityApi _identityApi;
    private readonly IWalletReadRepository _walletReadRepository;

    public WalletAdministrationQueries(IIdentityApi identityApi, IWalletReadRepository walletReadRepository)
    {
        _identityApi = identityApi;
        _walletReadRepository = walletReadRepository;
    }

    public async Task<PageResult<ExplorerWalletDto>> GetWalletsAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var explorers = await _identityApi.GetExplorersAsync(page, pageSize);
        var balances = await _walletReadRepository.GetBalancesAsync(explorers.Items.Select(user => user.Id).ToList());
        var items = explorers.Items.Select(user => new ExplorerWalletDto(
            user.Id, user.Email, balances.GetValueOrDefault(user.Id))).ToList();

        return new PageResult<ExplorerWalletDto>(items, explorers.TotalCount);
    }

    public async Task<PageResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId, WalletTransactionSearchDto search, int page, int pageSize)
    {
        if (!await _identityApi.ExplorerExistsAsync(userId))
        {
            throw new NotFoundException("The explorer does not exist.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return await _walletReadRepository.GetTransactionsAsync(userId, page, pageSize, search.Status);
    }
}
