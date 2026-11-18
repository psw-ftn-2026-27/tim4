using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.OwnerCoupons;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.OwnerCoupons;

public sealed class OwnerCouponsCommandTests : BaseIntegrationTest
{
    public OwnerCouponsCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task An_owner_creates_a_coupon_that_is_saved_with_a_generated_code(bool forOneProduct)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        Guid? productId = forOneProduct ? ProductSeed.FreeOffer.Id : null;
        var validUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
        int before;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            before = await arrangeContext.Coupons.CountAsync();
        }

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(25, validUntil, productId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Coupons.CountAsync()).Should().Be(before + 1);
        var seededIds = CouponSeed.All.Select(coupon => coupon.Id).ToList();
        var coupon = await context.Coupons.SingleAsync(coupon => !seededIds.Contains(coupon.Id));
        coupon.OwnerId.Should().Be(WellKnownUsers.Explorer);
        coupon.ProductId.Should().Be(productId);
        coupon.DiscountPercent.Should().Be(25);
        coupon.ValidUntil.Should().Be(validUntil);
        coupon.Code.Value.Should().HaveLength(8);
    }

    [Fact]
    public async Task A_coupon_for_another_owners_product_is_rejected_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var validUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
        int before;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            before = await arrangeContext.Coupons.CountAsync();
        }

        var response = await client.PostAsJsonAsync("/api/payment/coupons",
            new CreateCouponDto(25, validUntil, ProductSeed.ExistingOffer.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Coupons.CountAsync()).Should().Be(before);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Only_signed_in_explorers_can_create_coupons(bool administrator)
    {
        var client = administrator
            ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator")
            : Factory.CreateClient();
        var validUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(25, validUntil, null));

        response.StatusCode.Should().Be(administrator ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
    }
}
