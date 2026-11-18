using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class CouponIssuingTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    [Fact]
    public void A_coupon_for_a_product_of_another_owner_is_not_issued()
    {
        var ownerId = Guid.NewGuid();
        var ownProduct = CreateProduct(ownerId);
        var otherProduct = CreateProduct(Guid.NewGuid());

        var issue = () => new CouponIssuing().Issue(ownerId, [ownProduct, otherProduct], otherProduct.Id, 10, Today, Now);

        issue.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_explorer_without_products_cannot_issue_a_coupon()
    {
        var issue = () => new CouponIssuing().Issue(Guid.NewGuid(), [], null, 10, Today, Now);

        issue.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void A_discount_outside_one_to_hundred_percent_is_rejected(int discountPercent)
    {
        var ownerId = Guid.NewGuid();

        var issue = () => new CouponIssuing().Issue(ownerId, [CreateProduct(ownerId)], null, discountPercent, Today, Now);

        issue.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void A_discount_at_the_bounds_is_accepted(int discountPercent)
    {
        var ownerId = Guid.NewGuid();

        var coupon = new CouponIssuing().Issue(ownerId, [CreateProduct(ownerId)], null, discountPercent, Today, Now);

        coupon.DiscountPercent.Should().Be(discountPercent);
    }

    [Fact]
    public void A_valid_until_date_before_today_is_rejected()
    {
        var ownerId = Guid.NewGuid();

        var issue = () => new CouponIssuing().Issue(ownerId, [CreateProduct(ownerId)], null, 10, Today.AddDays(-1), Now);

        issue.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_valid_until_today_is_accepted()
    {
        var ownerId = Guid.NewGuid();

        var coupon = new CouponIssuing().Issue(ownerId, [CreateProduct(ownerId)], null, 10, Today, Now);

        coupon.ValidUntil.Should().Be(Today);
    }

    [Fact]
    public void A_new_coupon_gets_an_eight_character_code_of_capital_letters_and_digits()
    {
        var ownerId = Guid.NewGuid();

        var coupon = new CouponIssuing().Issue(ownerId, [CreateProduct(ownerId)], null, 10, Today, Now);

        coupon.Code.Value.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    private static Product CreateProduct(Guid ownerId) => new(Guid.NewGuid(), "Resource", ownerId, 10);
}
