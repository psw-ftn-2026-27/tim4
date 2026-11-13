using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletAdministration;

public sealed class WalletAdministrationService
{
    private readonly IIdentityApi _identityApi;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletAdministrationService(
        IIdentityApi identityApi, IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _identityApi = identityApi;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreditAsync(Guid userId, Guid administratorId, CreditWalletDto dto)
    {
        if (!await _identityApi.ExplorerExistsAsync(userId))
        {
            throw new NotFoundException("The explorer does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
        }

        wallet.Credit(dto.Amount, administratorId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
