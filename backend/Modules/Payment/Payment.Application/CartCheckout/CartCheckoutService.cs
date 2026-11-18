using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Application.Wallets;
using Payment.Domain.Coupons;
using Payment.Domain.Purchases;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Payment.Domain.ShoppingCarts;

namespace Payment.Application.CartCheckout;

public sealed class CartCheckoutService
{
    private readonly IShoppingCartRepository _carts;
    private readonly IProductRepository _products;
    private readonly IPurchaseRepository _purchases;
    private readonly IWalletRepository _wallets;
    private readonly ICouponRepository _coupons;
    private readonly Purchasing _purchasing;
    private readonly IUnitOfWork _unitOfWork;

    public CartCheckoutService(IShoppingCartRepository carts, IProductRepository products,
        IPurchaseRepository purchases, IWalletRepository wallets, ICouponRepository coupons, Purchasing purchasing,
        IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _products = products;
        _purchases = purchases;
        _wallets = wallets;
        _coupons = coupons;
        _purchasing = purchasing;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(Guid userId, AddToCartDto dto)
    {
        var product = await _products.GetByEntityIdAsync(dto.EntityId)
            ?? throw new NotFoundException("Proizvod ne postoji.");
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is null)
        {
            cart = new ShoppingCart(userId);
            _carts.Add(cart);
        }
        var owned = await _purchases.GetByContentAsync(userId, [product.EntityId]);
        _purchasing.AddToCart(cart, product, owned);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(Guid userId, Guid itemId)
    {
        var cart = await GetCartAsync(userId);
        cart.Remove(itemId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid> PrepareCartCheckoutAsync(Guid userId)
    {
        var cart = await GetCartAsync(userId);
        var products = await _products.GetByIdsAsync(cart.Items.Select(item => item.ProductId).ToList());
        var checkoutId = cart.PrepareCheckout(products);
        await _unitOfWork.SaveChangesAsync();
        return checkoutId;
    }

    public async Task ApplyCouponAsync(Guid userId, Guid checkoutId, ApplyCouponDto dto)
    {
        var cart = await GetCartAsync(userId);
        var coupon = await _coupons.GetByCodeAsync(CouponCode.Normalize(dto.Code))
            ?? throw new NotFoundException("Kod kupona ne postoji.");
        var used = await _purchases.GetByCouponAsync(userId, coupon.Id);
        _purchasing.ApplyCoupon(cart, checkoutId, coupon, used, DateOnly.FromDateTime(DateTime.UtcNow));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveCouponAsync(Guid userId, Guid checkoutId)
    {
        var cart = await GetCartAsync(userId);
        cart.RemoveCoupon(checkoutId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid> ConfirmCartCheckoutAsync(Guid userId, Guid checkoutId, ConfirmCheckoutDto dto)
    {
        var completed = await _purchases.GetByCheckoutAsync(userId, checkoutId);
        _purchasing.EnsureNotCompleted(completed);
        var cart = await GetCartAsync(userId);
        var wallet = await _wallets.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _wallets.Add(wallet);
        }
        var owned = await _purchases.GetByContentAsync(userId, cart.Items.Select(item => item.EntityId).ToList());
        Coupon? coupon = null;
        List<Purchase> couponPurchases = [];
        if (cart.AppliedCoupon is not null)
        {
            coupon = await _coupons.GetByIdAsync(cart.AppliedCoupon.CouponId);
            couponPurchases = await _purchases.GetByCouponAsync(userId, cart.AppliedCoupon.CouponId);
        }
        var purchase = _purchasing.Complete(cart, wallet, checkoutId, coupon, dto.TotalToPay,
            [.. owned, .. couponPurchases], DateTime.UtcNow);
        _purchases.Add(purchase);
        await _unitOfWork.SaveChangesAsync();
        return purchase.Id;
    }

    private async Task<ShoppingCart> GetCartAsync(Guid userId) => await _carts.GetByUserIdAsync(userId)
        ?? throw new NotFoundException("Korpa ne postoji.");
}
