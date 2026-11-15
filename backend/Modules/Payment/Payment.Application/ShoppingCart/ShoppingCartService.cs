using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Application.Wallets;
using Payment.Domain.Purchases;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Application.ShoppingCart;

public sealed class ShoppingCartService
{
    private readonly IShoppingCartRepository _carts;
    private readonly IProductRepository _products;
    private readonly IPurchaseRepository _purchases;
    private readonly IWalletRepository _wallets;
    private readonly Purchasing _purchasing;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartService(IShoppingCartRepository carts, IProductRepository products,
        IPurchaseRepository purchases, IWalletRepository wallets, Purchasing purchasing, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _products = products;
        _purchases = purchases;
        _wallets = wallets;
        _purchasing = purchasing;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(Guid userId, AddToCartDto dto)
    {
        var product = await _products.GetByEntityIdAsync(dto.EntityId)
            ?? throw new NotFoundException("The product does not exist.");
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is null)
        {
            cart = new Cart(userId);
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

    public async Task<Guid> ConfirmCartCheckoutAsync(Guid userId, Guid checkoutId)
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
        var purchase = _purchasing.Complete(cart, wallet, checkoutId, owned, DateTime.UtcNow);
        _purchases.Add(purchase);
        await _unitOfWork.SaveChangesAsync();
        return purchase.Id;
    }

    private async Task<Cart> GetCartAsync(Guid userId) => await _carts.GetByUserIdAsync(userId)
        ?? throw new NotFoundException("The cart does not exist.");
}
