namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<List<ExplorerSummary>> GetExplorersAsync();

    Task<bool> ExplorerExistsAsync(Guid userId);
}
