using Payment.Domain.GiftCards;

namespace Payment.Application.PersonalGiftCards;

// The read repository leaves the emails empty; the queries fill them in from Identity.
public sealed record GiftCardDto(Guid Id, Guid GiverId, string? GiverEmail, Guid RecipientId, string? RecipientEmail,
    int Amount, string Message, DateTime CreatedAt, DateOnly ValidUntil, GiftCardStatus Status);

public sealed record MyGiftCardsDto(List<GiftCardDto> Bought, List<GiftCardDto> Received);
