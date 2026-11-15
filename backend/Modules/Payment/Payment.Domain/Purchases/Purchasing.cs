using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

public sealed class Purchasing
{
    public void AddToCart(ShoppingCart cart, Product product, IReadOnlyCollection<Purchase> existingPurchases)
    {
        EnsureNotOwned(cart.UserId, [product.EntityId], existingPurchases);
        cart.Add(product);
    }

    public void EnsureNotCompleted(Purchase? completed)
    {
        if (completed is not null) throw new DomainException("Ova kupovina je vec zavrsena.");
    }

    public Purchase Complete(ShoppingCart cart, Wallet wallet, Guid checkoutId,
        IReadOnlyCollection<Purchase> existingPurchases, DateTime now)
    {
        cart.EnsureCheckout(checkoutId);
        EnsureNotOwned(cart.UserId, cart.Items.Select(item => item.EntityId), existingPurchases);
        if (cart.UserId != wallet.UserId) throw new DomainException("The wallet must belong to the cart owner.");
        var purchase = new Purchase(cart.UserId, checkoutId, cart.Items, now);
        wallet.Pay(purchase.Id, purchase.Items.Select(item => new PurchasePayment(item.EntityId, item.Price)).ToList(), now);
        cart.CompleteCheckout(checkoutId);
        return purchase;
    }

    private static void EnsureNotOwned(Guid userId, IEnumerable<Guid> entityIds, IReadOnlyCollection<Purchase> purchases)
    {
        if (purchases.Where(purchase => purchase.UserId == userId).SelectMany(purchase => purchase.Items)
            .Any(item => entityIds.Contains(item.EntityId)))
            throw new DomainException("Vec si kupio ovaj proizvod.");
    }
}
