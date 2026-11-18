using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.PersonalGiftCards;

public sealed class PersonalGiftCardsService
{
    private readonly IIdentityApi _identityApi;
    private readonly IWalletRepository _wallets;
    private readonly IGiftCardRepository _giftCards;
    private readonly GiftCardGiving _giftCardGiving;
    private readonly IUnitOfWork _unitOfWork;

    public PersonalGiftCardsService(IIdentityApi identityApi, IWalletRepository wallets,
        IGiftCardRepository giftCards, GiftCardGiving giftCardGiving, IUnitOfWork unitOfWork)
    {
        _identityApi = identityApi;
        _wallets = wallets;
        _giftCards = giftCards;
        _giftCardGiving = giftCardGiving;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> BuyGiftCardAsync(Guid giverId, BuyGiftCardDto dto)
    {
        var recipient = await _identityApi.FindExplorerByEmailAsync(dto.RecipientEmail?.Trim() ?? "")
            ?? throw new NotFoundException("Istraživač sa tim korisničkim imenom ne postoji.");
        var wallet = await _wallets.GetByUserIdAsync(giverId);
        if (wallet is null)
        {
            wallet = new Wallet(giverId);
            _wallets.Add(wallet);
        }
        var giftCard = _giftCardGiving.Give(wallet, recipient.Id, dto.Amount, dto.Message, DateTime.UtcNow);
        _giftCards.Add(giftCard);
        await _unitOfWork.SaveChangesAsync();
        return giftCard.Id;
    }
}
