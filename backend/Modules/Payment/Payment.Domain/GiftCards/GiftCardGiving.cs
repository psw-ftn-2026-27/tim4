using Payment.Domain.Wallets;

namespace Payment.Domain.GiftCards;

public sealed class GiftCardGiving
{
    public GiftCard Give(Wallet giverWallet, Guid recipientId, int amount, string message, DateTime now)
    {
        var giftCard = new GiftCard(giverWallet.UserId, recipientId, amount, message, now);
        giverWallet.PayForGiftCard(giftCard.Id, giftCard.Amount, now);
        return giftCard;
    }
}
