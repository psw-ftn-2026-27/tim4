using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal static class ProductConfiguration
{
    public static void ConfigureProducts(this ModelBuilder builder)
    {
        ConfigureProduct(builder.Entity<Product>());
    }

    private static void ConfigureProduct(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.HasIndex(product => product.EntityId).IsUnique();
    }
}
