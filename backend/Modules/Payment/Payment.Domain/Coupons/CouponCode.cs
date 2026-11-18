using System.Security.Cryptography;

namespace Payment.Domain.Coupons;

public sealed record CouponCode
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 8;

    public string Value { get; private init; }

    private CouponCode()
    {
        Value = null!;
    }

    private CouponCode(string value)
    {
        Value = value;
    }

    public static CouponCode Generate() => new(RandomNumberGenerator.GetString(Characters, Length));
}
