using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CartCheckout;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/cart")]
[Authorize(Roles = "explorer")]
public sealed class CartCheckoutController : ControllerBase
{
    private readonly CartCheckoutService _service;
    private readonly CartCheckoutQueries _queries;

    public CartCheckoutController(CartCheckoutService service, CartCheckoutQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart() => await _queries.GetCartAsync(User.GetUserId());

    [HttpPost("items")]
    public async Task<ActionResult> AddToCart(AddToCartDto dto)
    {
        await _service.AddToCartAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<ActionResult> RemoveCartItem(Guid itemId)
    {
        await _service.RemoveCartItemAsync(User.GetUserId(), itemId);
        return NoContent();
    }

    [HttpPost("checkouts")]
    public async Task<ActionResult<Guid>> PrepareCheckout() =>
        await _service.PrepareCartCheckoutAsync(User.GetUserId());

    [HttpGet("checkouts/{checkoutId:guid}")]
    public async Task<ActionResult<CartCheckoutDto>> GetCheckout(Guid checkoutId) =>
        await _queries.GetCartCheckoutAsync(User.GetUserId(), checkoutId);

    [HttpPut("checkouts/{checkoutId:guid}/coupon")]
    public async Task<ActionResult> ApplyCoupon(Guid checkoutId, ApplyCouponDto dto)
    {
        await _service.ApplyCouponAsync(User.GetUserId(), checkoutId, dto);
        return NoContent();
    }

    [HttpDelete("checkouts/{checkoutId:guid}/coupon")]
    public async Task<ActionResult> RemoveCoupon(Guid checkoutId)
    {
        await _service.RemoveCouponAsync(User.GetUserId(), checkoutId);
        return NoContent();
    }

    [HttpPost("checkouts/{checkoutId:guid}/confirm")]
    public async Task<ActionResult<Guid>> ConfirmCheckout(Guid checkoutId, ConfirmCheckoutDto dto) =>
        await _service.ConfirmCartCheckoutAsync(User.GetUserId(), checkoutId, dto);

    [HttpGet("purchases/{purchaseId:guid}")]
    public async Task<ActionResult<CompletedPurchaseDto>> GetPurchase(Guid purchaseId) =>
        await _queries.GetCompletedPurchaseAsync(User.GetUserId(), purchaseId);
}
