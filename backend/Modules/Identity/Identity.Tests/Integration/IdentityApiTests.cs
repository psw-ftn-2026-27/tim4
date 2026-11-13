using FluentAssertions;
using Identity.Contracts;
using Identity.Core;
using Identity.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiTests : BaseIntegrationTest
{
    public IdentityApiTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_list_holds_every_explorer_with_id_and_email_and_no_administrator()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorers = await identityApi.GetExplorersAsync(1, 100);

        explorers.Items.Should().BeEquivalentTo(SeededExplorers());
        explorers.Items.Should().NotContain(explorer => explorer.Id == UserSeed.Administrator.Id);
        explorers.TotalCount.Should().Be(SeededExplorers().Count());
    }

    [Fact]
    public async Task Explorer_list_is_sorted_by_email()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorers = await identityApi.GetExplorersAsync(1, 100);

        explorers.Items.Should().BeInAscendingOrder(explorer => explorer.Email, StringComparer.Ordinal);
    }

    [Fact]
    public async Task Explorers_are_listed_page_by_page()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();
        var secondByEmail = SeededExplorers().OrderBy(explorer => explorer.Email, StringComparer.Ordinal).ElementAt(1);

        var explorers = await identityApi.GetExplorersAsync(2, 1);

        explorers.Items.Should().ContainSingle().Which.Should().Be(secondByEmail);
        explorers.TotalCount.Should().Be(SeededExplorers().Count());
    }

    [Fact]
    public async Task Explorer_exists()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var exists = await identityApi.ExplorerExistsAsync(UserSeed.Explorer.Id);

        exists.Should().BeTrue();
    }

    public static TheoryData<Guid> NonExplorerIds => [UserSeed.Administrator.Id, Guid.Parse("00000000-0000-0000-0000-0000000000ff")];

    [Theory]
    [MemberData(nameof(NonExplorerIds))]
    public async Task Administrator_and_unknown_user_do_not_exist_as_explorers(Guid userId)
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var exists = await identityApi.ExplorerExistsAsync(userId);

        exists.Should().BeFalse();
    }

    private static IEnumerable<ExplorerSummary> SeededExplorers()
    {
        var explorerIds = UserSeed.All.OfType<IdentityUserRole<Guid>>()
            .Where(userRole => userRole.RoleId == RoleSeed.Explorer.Id)
            .Select(userRole => userRole.UserId);
        return UserSeed.All.OfType<ApplicationUser>()
            .Where(user => explorerIds.Contains(user.Id))
            .Select(user => new ExplorerSummary(user.Id, user.Email!));
    }
}
