using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class WalletTransaction : Entity
{
    public long Amount { get; private set; }
    public TransactionKind Kind { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public Guid? AdministratorId { get; private set; }
    public Guid? PurchaseId { get; private set; }
    public Guid? EntityId { get; private set; }
    public Guid? CancelledByAdministratorId { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private WalletTransaction() { }

    internal WalletTransaction(int amount, Guid administratorId, DateTime submittedAt)
        : base(Guid.NewGuid())
    {
        if (amount <= 0 || administratorId == Guid.Empty)
        {
            throw new DomainException("A payment must contain a positive number of EC.");
        }

        Amount = amount;
        Kind = TransactionKind.Credit;
        AdministratorId = administratorId;
        SubmittedAt = submittedAt;
    }

    internal WalletTransaction(Guid purchaseId, Guid entityId, long amount, DateTime submittedAt)
        : base(Guid.NewGuid())
    {
        if (purchaseId == Guid.Empty || entityId == Guid.Empty || amount < 0)
            throw new DomainException("A purchase payment requires a purchase, content and nonnegative amount.");
        Kind = TransactionKind.Purchase;
        PurchaseId = purchaseId;
        EntityId = entityId;
        Amount = amount;
        SubmittedAt = submittedAt;
    }

    public TransactionStatus GetStatus() => GetStatus(CancelledAt);

    public static TransactionStatus GetStatus(DateTime? cancelledAt) => cancelledAt is null
        ? TransactionStatus.Available : TransactionStatus.Cancelled;

    internal void Reverse(Guid administratorId, DateTime now)
    {
        if (Kind != TransactionKind.Credit || now < SubmittedAt || now >= SubmittedAt.AddHours(24) || CancelledAt is not null)
        {
            throw new DomainException("Only an unreversed payment less than 24 hours old can be reversed.");
        }

        CancelledByAdministratorId = administratorId;
        CancelledAt = now;
    }
}
