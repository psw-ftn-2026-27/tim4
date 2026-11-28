namespace Shared.Tests;

/// <summary>
/// Fixed user IDs that every module's seed data refers to. They are the IDs of the demo
/// users that IdentityModuleInitializer creates in every Development database, test
/// databases included, so a module whose test calls IIdentityApi sees the same users its
/// seeds refer to. Tests authenticate as them via <see cref="ExplorerApiFactory.CreateClientFor"/>.
/// </summary>
public static class WellKnownUsers
{
    public static readonly Guid Administrator = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid Explorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondExplorer = Guid.Parse("20000000-0000-0000-0000-000000000002");
}
