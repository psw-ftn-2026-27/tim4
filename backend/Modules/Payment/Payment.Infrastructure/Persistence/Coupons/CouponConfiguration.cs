using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal static class CouponConfiguration
{
    public static void ConfigureCoupons(this ModelBuilder builder)
    {
        ConfigureCoupon(builder.Entity<Coupon>());
    }

    private static void ConfigureCoupon(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(coupon => coupon.Id).ValueGeneratedNever();
        builder.OwnsOne(coupon => coupon.Code, code =>
        {
            code.Property(value => value.Value).HasColumnName("Code").HasMaxLength(8);
            code.HasIndex(value => value.Value).IsUnique();
        });
        builder.Navigation(coupon => coupon.Code).IsRequired();
    }
}
