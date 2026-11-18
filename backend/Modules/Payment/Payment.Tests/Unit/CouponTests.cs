using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class CouponTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_owner_coupon_discounts_the_owners_most_expensive_item_and_a_product_coupon_its_product(bool forProduct)
    {
        var ownerId = Guid.NewGuid();
        var cheap = new Product(Guid.NewGuid(), "Cheap", ownerId, 10);
        var expensive = new Product(Guid.NewGuid(), "Expensive", ownerId, 40);
        var otherOwners = new Product(Guid.NewGuid(), "Other owner's", Guid.NewGuid(), 100);
        var cart = Cart(cheap, expensive, otherOwners);
        var coupon = Issue(ownerId, [cheap, expensive], forProduct ? cheap.Id : null, 10);

        var item = coupon.AppliesTo(cart.Items);

        item.ProductId.Should().Be(forProduct ? cheap.Id : expensive.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_coupon_without_a_matching_item_in_the_cart_is_not_applied(bool forProduct)
    {
        var ownerId = Guid.NewGuid();
        var ownProduct = new Product(Guid.NewGuid(), "Own", ownerId, 10);
        var cart = Cart(new Product(Guid.NewGuid(), "Other owner's", Guid.NewGuid(), 100));
        var coupon = Issue(ownerId, [ownProduct], forProduct ? ownProduct.Id : null, 10);

        var apply = () => coupon.AppliesTo(cart.Items);

        apply.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(99, 10, 9)]
    [InlineData(19, 50, 9)]
    [InlineData(5, 1, 0)]
    [InlineData(1000, 20, 200)]
    public void The_discount_is_rounded_down_to_a_whole_ec(long price, int discountPercent, long expected)
    {
        var ownerId = Guid.NewGuid();
        var coupon = Issue(ownerId, [new Product(Guid.NewGuid(), "Own", ownerId, price)], null, discountPercent);

        var discount = coupon.DiscountFor(price);

        discount.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void A_coupon_is_valid_on_its_last_day_and_not_the_day_after(int daysAfterValidUntil, bool expected)
    {
        var ownerId = Guid.NewGuid();
        var coupon = Issue(ownerId, [new Product(Guid.NewGuid(), "Own", ownerId, 10)], null, 10);

        var valid = coupon.IsValidOn(Today.AddDays(daysAfterValidUntil));

        valid.Should().Be(expected);
    }

    private static Coupon Issue(Guid ownerId, IReadOnlyCollection<Product> products, Guid? productId, int discountPercent) =>
        new CouponIssuing().Issue(ownerId, products, productId, discountPercent, Today, Today);

    private static ShoppingCart Cart(params Product[] products)
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        foreach (var product in products) cart.Add(product);
        return cart;
    }
}
