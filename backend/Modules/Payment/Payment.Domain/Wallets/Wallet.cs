using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    private readonly List<WalletTransaction> _transactions = [];

    public Guid UserId { get; private set; }
    public long Balance { get; private set; }
    public Guid Version { get; private set; }
    public IReadOnlyList<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
        Version = Guid.NewGuid();
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
        Version = Guid.NewGuid();
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
        Version = Guid.NewGuid();
    }

    public void Pay(Guid purchaseId, IReadOnlyCollection<PurchasePayment> payments, DateTime submittedAt)
    {
        if (payments.Count == 0 || payments.Select(payment => payment.EntityId).Distinct().Count() != payments.Count)
            throw new DomainException("A purchase requires distinct payment items.");
        var transactions = payments.Select(payment =>
            new WalletTransaction(purchaseId, payment.EntityId, payment.Amount, submittedAt)).ToList();
        var total = transactions.Sum(transaction => (decimal)transaction.Amount);
        if (total > Balance) throw new DomainException("Nema dovoljno EC za kupovinu.");
        if (_transactions.Any(transaction => transaction.PurchaseId == purchaseId))
            throw new DomainException("Ova kupovina je vec zavrsena.");
        Balance -= (long)total;
        _transactions.AddRange(transactions);
        Version = Guid.NewGuid();
    }

    public long GetAvailableBalance() => Balance;
}
