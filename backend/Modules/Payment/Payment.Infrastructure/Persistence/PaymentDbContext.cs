using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.Wallets;
using Payment.Domain.Products;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Wallets;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Infrastructure.Persistence.Purchases;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Npgsql;
using Shared.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureProducts();
        builder.ConfigureShoppingCarts();
        builder.ConfigurePurchases();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException("Podaci su u medjuvremenu izmenjeni. Osvezi prikaz i ponovi radnju.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName:
                "IX_ShoppingCarts_UserId" or "IX_Wallets_UserId" or "IX_Purchases_CheckoutId"
                or "IX_PurchaseItems_BuyerId_EntityId" or "IX_OrderItems_ShoppingCartId_ProductId" })
        {
            throw new DomainException("Podaci su u medjuvremenu izmenjeni ili je kupovina vec zavrsena. Osvezi prikaz.");
        }
    }
}
