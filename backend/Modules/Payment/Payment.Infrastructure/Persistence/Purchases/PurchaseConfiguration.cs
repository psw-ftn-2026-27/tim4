using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal static class PurchaseConfiguration
{
    public static void ConfigurePurchases(this ModelBuilder builder)
    {
        ConfigurePurchase(builder.Entity<Purchase>());
        ConfigureItem(builder.Entity<PurchaseItem>());
    }

    private static void ConfigurePurchase(EntityTypeBuilder<Purchase> builder)
    {
        builder.Property(purchase => purchase.Id).ValueGeneratedNever();
        builder.HasIndex(purchase => purchase.CheckoutId).IsUnique();
        builder.Ignore(purchase => purchase.Total);
        builder.HasMany(purchase => purchase.Items).WithOne().HasForeignKey("PurchaseId")
            .IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(purchase => purchase.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsOne(purchase => purchase.AppliedCoupon, coupon =>
            coupon.Property(value => value.Code).HasMaxLength(8));
    }

    private static void ConfigureItem(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("PurchaseItems");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.HasIndex(item => new { item.BuyerId, item.EntityId }).IsUnique();
    }
}
