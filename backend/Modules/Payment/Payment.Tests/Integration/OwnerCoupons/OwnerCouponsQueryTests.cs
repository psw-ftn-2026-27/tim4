using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.OwnerCoupons;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.OwnerCoupons;

public sealed class OwnerCouponsQueryTests : BaseIntegrationTest
{
    public OwnerCouponsQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task The_coupon_list_holds_only_the_owners_coupons_and_a_coupon_is_valid_through_its_last_day()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var coupons = (await client.GetFromJsonAsync<PageResult<OwnerCouponDto>>("/api/payment/coupons", JsonOptions))!;

        coupons.Items.Select(coupon => coupon.Code).Should().BeEquivalentTo(CouponSeed.All
            .Where(coupon => coupon.OwnerId == WellKnownUsers.Explorer).Select(coupon => coupon.Code.Value));
        coupons.Items.Single(coupon => coupon.Code == CouponSeed.ExplorerExpiringToday.Code.Value).IsValid.Should().BeTrue();
        coupons.Items.Single(coupon => coupon.Code == CouponSeed.ExplorerExpiredYesterday.Code.Value).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task The_coupon_list_is_paged_with_the_newest_coupon_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var ownCoupons = CouponSeed.All.Where(coupon => coupon.OwnerId == WellKnownUsers.Explorer)
            .OrderByDescending(coupon => coupon.CreatedAt).ToList();

        var secondPage = (await client.GetFromJsonAsync<PageResult<OwnerCouponDto>>(
            "/api/payment/coupons?page=2&pageSize=1", JsonOptions))!;

        secondPage.TotalCount.Should().Be(ownCoupons.Count);
        secondPage.Items.Select(coupon => coupon.Code).Should().Equal(ownCoupons[1].Code.Value);
    }

    [Fact]
    public async Task The_product_choice_holds_only_the_owners_products()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<OwnerProductDto>>("/api/payment/coupons/products", JsonOptions);

        products!.Select(product => product.Id).Should().BeEquivalentTo(ProductSeed.All
            .Where(product => product.OwnerId == WellKnownUsers.Explorer).Select(product => product.Id));
    }
}
