using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletAdministration;
using Payment.Application.Wallets;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "administrator")]
public sealed class WalletAdministrationController : ControllerBase
{
    private readonly WalletAdministrationQueries _queries;
    private readonly WalletAdministrationService _service;

    public WalletAdministrationController(WalletAdministrationQueries queries, WalletAdministrationService service)
    {
        _queries = queries;
        _service = service;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PageResult<ExplorerWalletDto>>> GetWallets(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => await _queries.GetWalletsAsync(page, pageSize);

    [HttpGet("users/{userId:guid}/transactions")]
    public async Task<ActionResult<PageResult<WalletTransactionDto>>> GetTransactions(
        Guid userId, [FromQuery] WalletTransactionSearchDto search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.GetTransactionsAsync(userId, search, page, pageSize);

    [HttpPost("users/{userId:guid}/payments")]
    public async Task<ActionResult> Credit(Guid userId, CreditWalletDto dto)
    {
        await _service.CreditAsync(userId, User.GetUserId(), dto);
        return NoContent();
    }
}
