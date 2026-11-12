using Identity.Contracts;
using Identity.Core;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityApi : IIdentityApi
{
    private const string ExplorerRole = "explorer";

    private readonly IdentityModuleDbContext _dbContext;

    public IdentityApi(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ExplorerSummary>> GetExplorersAsync()
    {
        return Explorers()
            .OrderBy(user => user.Email)
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .ToListAsync();
    }

    public Task<bool> ExplorerExistsAsync(Guid userId)
    {
        return Explorers().AnyAsync(user => user.Id == userId);
    }

    private IQueryable<ApplicationUser> Explorers()
    {
        return from user in _dbContext.Users
               join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
               join role in _dbContext.Roles on userRole.RoleId equals role.Id
               where role.Name == ExplorerRole
               select user;
    }
}
