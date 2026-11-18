namespace Payment.Application.PersonalGiftCards;

public sealed record BuyGiftCardDto(string RecipientEmail, int Amount, string Message);
