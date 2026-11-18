using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class PurchasingTests
{
    [Fact]
    public void A_price_change_after_checkout_does_not_change_the_agreed_charge()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 10);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);
        product.ChangePrice(product.OwnerId, 20);
        var wallet = new Wallet(cart.UserId);
        wallet.Credit(10, Guid.NewGuid(), DateTime.UtcNow);
        var purchasing = new Purchasing();

        var purchase = purchasing.Complete(cart, wallet, checkout, null, cart.TotalToPay, [], DateTime.UtcNow);

        purchase.Total.Should().Be(10);
        wallet.Balance.Should().Be(0);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Owned_content_cannot_be_added_again()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 0);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);
        var purchasing = new Purchasing();
        var purchased = purchasing.Complete(cart, new Wallet(cart.UserId), checkout, null, 0, [], DateTime.UtcNow);

        var add = () => purchasing.AddToCart(cart, product, [purchased]);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void A_buyer_cannot_reuse_a_coupon_from_their_confirmed_purchase()
    {
        var (coupon, nextProduct, purchase) = PurchaseWithCoupon();
        var cart = PreparedCart(purchase.UserId, nextProduct, out var checkout);

        var apply = () => new Purchasing().ApplyCoupon(cart, checkout, coupon, [purchase], Today);

        apply.Should().Throw<DomainException>();
        cart.AppliedCoupon.Should().BeNull();
    }

    [Fact]
    public void Another_buyer_can_use_a_coupon_that_someone_else_already_used()
    {
        var (coupon, nextProduct, purchase) = PurchaseWithCoupon();
        var cart = PreparedCart(Guid.NewGuid(), nextProduct, out var checkout);

        new Purchasing().ApplyCoupon(cart, checkout, coupon, [purchase], Today);

        cart.AppliedCoupon!.CouponId.Should().Be(coupon.Id);
    }

    [Fact]
    public void Confirming_with_a_coupon_that_expired_after_it_was_applied_is_rejected()
    {
        var ownerId = Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "Resource", ownerId, 100);
        var coupon = new CouponIssuing().Issue(ownerId, [product], null, 10, Today, Now);
        var cart = PreparedCart(Guid.NewGuid(), product, out var checkout);
        var purchasing = new Purchasing();
        purchasing.ApplyCoupon(cart, checkout, coupon, [], Today);
        var wallet = FundedWallet(cart.UserId, 100);

        var complete = () => purchasing.Complete(cart, wallet, checkout, coupon, cart.TotalToPay, [],
            Today.AddDays(1).ToDateTime(TimeOnly.MinValue));

        complete.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirming_with_a_price_other_than_the_price_to_pay_is_rejected()
    {
        var ownerId = Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "Resource", ownerId, 100);
        var coupon = new CouponIssuing().Issue(ownerId, [product], null, 10, Today, Now);
        var cart = PreparedCart(Guid.NewGuid(), product, out var checkout);
        var purchasing = new Purchasing();
        purchasing.ApplyCoupon(cart, checkout, coupon, [], Today);
        var wallet = FundedWallet(cart.UserId, 100);

        var complete = () => purchasing.Complete(cart, wallet, checkout, coupon, cart.Total, [],
            Today.ToDateTime(TimeOnly.MinValue));

        complete.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(100);
        cart.Items.Should().ContainSingle();
    }

    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateTime Now = Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private static (Coupon Coupon, Product NextProduct, Purchase Purchase) PurchaseWithCoupon()
    {
        var ownerId = Guid.NewGuid();
        var bought = new Product(Guid.NewGuid(), "Bought", ownerId, 100);
        var next = new Product(Guid.NewGuid(), "Next", ownerId, 50);
        var coupon = new CouponIssuing().Issue(ownerId, [bought, next], null, 10, Today, Now);
        var cart = PreparedCart(Guid.NewGuid(), bought, out var checkout);
        var purchasing = new Purchasing();
        purchasing.ApplyCoupon(cart, checkout, coupon, [], Today);
        var purchase = purchasing.Complete(cart, FundedWallet(cart.UserId, 100), checkout, coupon, cart.TotalToPay, [],
            Today.ToDateTime(TimeOnly.MinValue));
        return (coupon, next, purchase);
    }

    private static ShoppingCart PreparedCart(Guid buyerId, Product product, out Guid checkout)
    {
        var cart = new ShoppingCart(buyerId);
        cart.Add(product);
        checkout = cart.PrepareCheckout([product]);
        return cart;
    }

    private static Wallet FundedWallet(Guid userId, int amount)
    {
        var wallet = new Wallet(userId);
        wallet.Credit(amount, Guid.NewGuid(), Today.ToDateTime(TimeOnly.MinValue));
        return wallet;
    }
}
