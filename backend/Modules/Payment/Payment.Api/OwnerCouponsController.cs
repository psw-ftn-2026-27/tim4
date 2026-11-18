using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.OwnerCoupons;
using Shared.Api;
using Shared.Domain;

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
    public async Task<ActionResult<PageResult<OwnerCouponDto>>> GetMyCoupons(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.GetMyCouponsAsync(User.GetUserId(), page, pageSize);

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
