using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal static class GiftCardConfiguration
{
    public static void ConfigureGiftCards(this ModelBuilder builder)
    {
        ConfigureGiftCard(builder.Entity<GiftCard>());
    }

    private static void ConfigureGiftCard(EntityTypeBuilder<GiftCard> builder)
    {
        builder.Property(giftCard => giftCard.Id).ValueGeneratedNever();
        builder.Property(giftCard => giftCard.Version).IsConcurrencyToken();
        builder.Property(giftCard => giftCard.Message).HasMaxLength(GiftCard.MaxMessageLength);
        builder.HasIndex(giftCard => giftCard.GiverId);
        builder.HasIndex(giftCard => giftCard.RecipientId);
    }
}
