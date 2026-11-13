using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
internal static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Administrator = new()
    {
        Id = WellKnownUsers.Administrator,
        UserName = "administrator@test.com",
        NormalizedUserName = "ADMINISTRATOR@TEST.COM",
        Email = "administrator@test.com",
        NormalizedEmail = "ADMINISTRATOR@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "1c7e9b2a-3d4f-4e5a-8b6c-0d1e2f3a4b5c"
    };

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer@test.com",
        NormalizedUserName = "EXPLORER@TEST.COM",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly ApplicationUser SecondExplorer = new()
    {
        Id = WellKnownUsers.SecondExplorer,
        UserName = "anna.explorer@test.com",
        NormalizedUserName = "ANNA.EXPLORER@TEST.COM",
        Email = "anna.explorer@test.com",
        NormalizedEmail = "ANNA.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "9e8d7c6b-5a4f-4e3d-2c1b-0a9f8e7d6c5b"
    };

    public static readonly IdentityUserRole<Guid> AdministratorRole = new() { UserId = WellKnownUsers.Administrator, RoleId = RoleSeed.Administrator.Id };
    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };
    public static readonly IdentityUserRole<Guid> SecondExplorerRole = new() { UserId = WellKnownUsers.SecondExplorer, RoleId = RoleSeed.Explorer.Id };

    // The second explorer's email sorts before the first one's, so the insertion order is
    // not alphabetical and a query that forgets to sort by email fails its test.
    public static object[] All => [Administrator, AdministratorRole, Explorer, ExplorerRole, SecondExplorer, SecondExplorerRole];
}
