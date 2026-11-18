namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<ExplorerPage> GetExplorersAsync(int page, int pageSize);

    Task<bool> ExplorerExistsAsync(Guid userId);

    // Null when no explorer has this email, administrators included; letter case is ignored.
    Task<ExplorerSummary?> FindExplorerByEmailAsync(string email);

    // One item per explorer among the given IDs; unknown and administrator IDs are left out.
    Task<List<ExplorerSummary>> GetExplorersByIdsAsync(IReadOnlyCollection<Guid> userIds);
}
