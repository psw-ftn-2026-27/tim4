namespace Identity.Contracts;

// One page of explorers and the number of all explorers. Contracts references nothing,
// so this mirrors Shared.Domain's PageResult instead of using it.
public sealed record ExplorerPage(List<ExplorerSummary> Items, int TotalCount);
