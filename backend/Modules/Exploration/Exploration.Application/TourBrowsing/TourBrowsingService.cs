using Payment.Contracts;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingService
{
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingService(IPaymentApi paymentApi)
    {
        _paymentApi = paymentApi;
    }

    // The cart and its rules belong to Payment, so this command only passes the request on
    // through Payment's contract; Exploration loads and saves nothing of its own.
    public async Task AddToCartAsync(Guid touristId, Guid tourId)
    {
        await _paymentApi.AddToCartAsync(touristId, tourId);
    }
}
