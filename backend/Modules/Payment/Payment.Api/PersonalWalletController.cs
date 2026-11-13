using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.PersonalWallet;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize]
public sealed class PersonalWalletController : ControllerBase
{
    private readonly PersonalWalletQueries _queries;

    public PersonalWalletController(PersonalWalletQueries queries)
    {
        _queries = queries;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetAvailableBalance()
    {
        return await _queries.GetAvailableBalanceAsync(User.GetUserId());
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PageResult<WalletTransactionDto>>> GetUserTransactions(
        [FromQuery] WalletTransactionSearchDto search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _queries.GetUserTransactionsAsync(User.GetUserId(), search, page, pageSize);
    }
}
