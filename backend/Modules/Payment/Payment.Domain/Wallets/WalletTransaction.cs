using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class WalletTransaction : Entity
{
    public int Amount { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public Guid AdministratorId { get; private set; }
    public Guid? CancelledByAdministratorId { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private WalletTransaction() { }

    internal WalletTransaction(int amount, Guid administratorId, DateTime submittedAt)
        : base(Guid.NewGuid())
    {
        if (amount <= 0)
        {
            throw new DomainException("A payment must contain a positive number of EC.");
        }

        Amount = amount;
        AdministratorId = administratorId;
        SubmittedAt = submittedAt;
    }

    public TransactionStatus GetStatus() => GetStatus(CancelledAt);

    public static TransactionStatus GetStatus(DateTime? cancelledAt) => cancelledAt is null
        ? TransactionStatus.Available : TransactionStatus.Cancelled;

    internal void Reverse(Guid administratorId, DateTime now)
    {
        if (now < SubmittedAt || now >= SubmittedAt.AddHours(24) || CancelledAt is not null)
        {
            throw new DomainException("Only an unreversed payment less than 24 hours old can be reversed.");
        }

        CancelledByAdministratorId = administratorId;
        CancelledAt = now;
    }
}
