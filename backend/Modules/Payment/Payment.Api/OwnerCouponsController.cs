using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.OwnerCoupons;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class OwnerCouponsController : ControllerBase
{
    private readonly OwnerCouponsService _service;
    private readonly OwnerCouponsQueries _queries;

    public OwnerCouponsController(OwnerCouponsService service, OwnerCouponsQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<List<OwnerCouponDto>>> GetMyCoupons() =>
        await _queries.GetMyCouponsAsync(User.GetUserId());

    [HttpGet("products")]
    public async Task<ActionResult<List<OwnerProductDto>>> GetMyProducts() =>
        await _queries.GetMyProductsAsync(User.GetUserId());

    [HttpPost]
    public async Task<ActionResult> CreateCoupon(CreateCouponDto dto)
    {
        await _service.CreateCouponAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
