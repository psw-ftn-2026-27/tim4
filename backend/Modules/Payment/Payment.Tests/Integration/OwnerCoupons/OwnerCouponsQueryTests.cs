using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.OwnerCoupons;
using Payment.Tests.Integration.Seeds;
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

        var coupons = (await client.GetFromJsonAsync<List<OwnerCouponDto>>("/api/payment/coupons", JsonOptions))!;

        coupons.Select(coupon => coupon.Code).Should().BeEquivalentTo(CouponSeed.All
            .Where(coupon => coupon.OwnerId == WellKnownUsers.Explorer).Select(coupon => coupon.Code.Value));
        coupons.Single(coupon => coupon.Code == CouponSeed.ExplorerExpiringToday.Code.Value).IsValid.Should().BeTrue();
        coupons.Single(coupon => coupon.Code == CouponSeed.ExplorerExpiredYesterday.Code.Value).IsValid.Should().BeFalse();
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
