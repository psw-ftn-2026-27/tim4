namespace Payment.Application.WalletAdministration;

public sealed record ExplorerWalletDto(Guid UserId, string Email, long Balance);
