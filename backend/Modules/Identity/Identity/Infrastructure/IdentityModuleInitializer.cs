using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Infrastructure;

internal sealed class IdentityModuleInitializer : IHostedService
{
    internal static readonly string[] Roles = [RoleNames.Administrator, RoleNames.Explorer];

    private const string DemoPassword = "Password1!";

    // WellKnownUsers in Shared.Tests carries the IDs of admin1, explorer1 and explorer2, so
    // that every module's test database knows the users its seeds refer to. Change both together.
    private static readonly (Guid Id, string Email, string Role)[] DemoUsers =
    [
        (Guid.Parse("10000000-0000-0000-0000-000000000001"), "admin1@explorer.dev", RoleNames.Administrator),
        (Guid.Parse("10000000-0000-0000-0000-000000000002"), "admin2@explorer.dev", RoleNames.Administrator),
        (Guid.Parse("20000000-0000-0000-0000-000000000001"), "explorer1@explorer.dev", RoleNames.Explorer),
        (Guid.Parse("20000000-0000-0000-0000-000000000002"), "explorer2@explorer.dev", RoleNames.Explorer),
        (Guid.Parse("20000000-0000-0000-0000-000000000003"), "explorer3@explorer.dev", RoleNames.Explorer)
    ];

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public IdentityModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        if (_environment.IsDevelopment() && !await dbContext.Users.AnyAsync())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var (id, email, role) in DemoUsers)
            {
                var user = new ApplicationUser { Id = id, UserName = email, Email = email };
                await userManager.CreateAsync(user, DemoPassword);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
