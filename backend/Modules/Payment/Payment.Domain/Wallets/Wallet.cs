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
            throw new DomainException("Stanje novčanika premašuje dozvoljenu vrednost.");
        }

        _transactions.Add(transaction);
        Balance += transaction.Amount;
        Version = Guid.NewGuid();
    }

    public void ReversePayment(Guid transactionId, Guid administratorId, DateTime now)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId)
            ?? throw new NotFoundException("Uplata ne postoji u ovom novčaniku.");

        if (Balance < transaction.Amount)
        {
            throw new DomainException("Uplata ne može da se stornira jer bi stanje novčanika bilo negativno.");
        }

        transaction.Reverse(administratorId, now);
        Balance -= transaction.Amount;
        Version = Guid.NewGuid();
    }

    public void Pay(Guid purchaseId, IReadOnlyCollection<PurchasePayment> payments, DateTime submittedAt)
    {
        if (payments.Count == 0 || payments.Select(payment => payment.EntityId).Distinct().Count() != payments.Count)
            throw new DomainException("Kupovina mora da sadrži različite stavke plaćanja.");
        var transactions = payments.Select(payment =>
            new WalletTransaction(purchaseId, payment.EntityId, payment.Amount, submittedAt)).ToList();
        var total = transactions.Sum(transaction => (decimal)transaction.Amount);
        if (total > Balance) throw new DomainException("Nema dovoljno EC za kupovinu.");
        if (_transactions.Any(transaction => transaction.PurchaseId == purchaseId))
            throw new DomainException("Ova kupovina je već završena.");
        Balance -= (long)total;
        _transactions.AddRange(transactions);
        Version = Guid.NewGuid();
    }

    public long GetAvailableBalance() => Balance;
}
