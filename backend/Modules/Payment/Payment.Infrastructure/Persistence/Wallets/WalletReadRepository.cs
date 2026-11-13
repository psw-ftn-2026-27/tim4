using Microsoft.EntityFrameworkCore;
using Payment.Application.PersonalWallet;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Dictionary<Guid, long>> GetBalancesAsync(IReadOnlyCollection<Guid> userIds) => _dbContext.Wallets
        .Where(wallet => userIds.Contains(wallet.UserId))
        .Select(wallet => new { wallet.UserId, wallet.Balance })
        .ToDictionaryAsync(wallet => wallet.UserId, wallet => wallet.Balance);

    public Task<WalletBalanceDto?> GetBalanceAsync(Guid userId) => _dbContext.Wallets
        .Where(wallet => wallet.UserId == userId)
        .Select(wallet => new WalletBalanceDto(wallet.Balance))
        .SingleOrDefaultAsync();

    public async Task<PageResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId, int page, int pageSize, TransactionStatus? status)
    {
        var transactions = _dbContext.Wallets.Where(wallet => wallet.UserId == userId)
            .SelectMany(wallet => wallet.Transactions);
        transactions = status switch
        {
            TransactionStatus.Available => transactions.Where(transaction => transaction.CancelledAt == null),
            TransactionStatus.Cancelled => transactions.Where(transaction => transaction.CancelledAt != null),
            _ => transactions
        };
        var totalCount = await transactions.CountAsync();
        var items = await transactions.OrderByDescending(transaction => transaction.SubmittedAt)
            .ThenBy(transaction => transaction.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(transaction => new WalletTransactionDto(
                transaction.Id, transaction.Amount, transaction.SubmittedAt, transaction.AdministratorId,
                WalletTransaction.GetStatus(transaction.CancelledAt),
                transaction.CancelledByAdministratorId, transaction.CancelledAt))
            .ToListAsync();

        return new PageResult<WalletTransactionDto>(items, totalCount);
    }
}
