using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    private readonly List<WalletTransaction> _transactions = [];

    public Guid UserId { get; private set; }
    public long Balance { get; private set; }
    public IReadOnlyList<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public void Credit(int amount, Guid administratorId, DateTime submittedAt)
    {
        var transaction = new WalletTransaction(amount, administratorId, submittedAt);
        if (Balance > long.MaxValue - transaction.Amount)
        {
            throw new DomainException("The wallet balance is too large.");
        }

        _transactions.Add(transaction);
        Balance += transaction.Amount;
    }

    public void ReversePayment(Guid transactionId, Guid administratorId, DateTime now)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId)
            ?? throw new NotFoundException("The payment does not exist in this wallet.");

        if (Balance < transaction.Amount)
        {
            throw new DomainException("The payment cannot be reversed without a negative balance.");
        }

        transaction.Reverse(administratorId, now);
        Balance -= transaction.Amount;
    }

    public long GetAvailableBalance() => Balance;
}
