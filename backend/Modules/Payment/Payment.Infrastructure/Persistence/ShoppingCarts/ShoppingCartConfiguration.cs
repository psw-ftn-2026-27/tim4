using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal static class ShoppingCartConfiguration
{
    public static void ConfigureShoppingCarts(this ModelBuilder builder)
    {
        ConfigureCart(builder.Entity<ShoppingCart>());
        ConfigureItem(builder.Entity<OrderItem>());
    }

    private static void ConfigureCart(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.Property(cart => cart.Id).ValueGeneratedNever();
        builder.Property(cart => cart.Version).IsConcurrencyToken();
        builder.HasIndex(cart => cart.UserId).IsUnique();
        builder.Ignore(cart => cart.Total);
        builder.HasMany(cart => cart.Items).WithOne().HasForeignKey("ShoppingCartId")
            .IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(cart => cart.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureItem(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.HasIndex("ShoppingCartId", nameof(OrderItem.ProductId)).IsUnique();
    }
}
