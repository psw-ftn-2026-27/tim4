using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.ProductOffering;
using Payment.Application.ProductPricing;
using Payment.Application.Products;
using Payment.Contracts;
using Payment.Infrastructure.Persistence.Products;
using Payment.Application.PersonalWallet;
using Payment.Application.WalletAdministration;
using Payment.Application.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;
using Payment.Application.Purchases;
using Payment.Application.PurchaseAccess;
using Payment.Application.ShoppingCart;
using Payment.Application.ShoppingCarts;
using Payment.Domain.Purchases;
using Payment.Infrastructure.Persistence.Purchases;
using Payment.Infrastructure.Persistence.ShoppingCarts;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<PersonalWalletQueries>();
        services.AddScoped<WalletAdministrationQueries>();
        services.AddScoped<WalletAdministrationService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ProductOfferingService>();
        services.AddScoped<ProductPricingQueries>();
        services.AddScoped<IPaymentApi, PaymentApi>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartReadRepository, ShoppingCartReadRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPurchaseReadRepository, PurchaseReadRepository>();
        services.AddScoped<Purchasing>();
        services.AddScoped<ShoppingCartService>();
        services.AddScoped<ShoppingCartQueries>();
        services.AddScoped<PurchaseAccessQueries>();

        return services;
    }
}
