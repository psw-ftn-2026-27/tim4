using Identity.Contracts;
using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityApi : IIdentityApi
{
    private readonly IdentityModuleDbContext _dbContext;
    private readonly ILookupNormalizer _lookupNormalizer;

    public IdentityApi(IdentityModuleDbContext dbContext, ILookupNormalizer lookupNormalizer)
    {
        _dbContext = dbContext;
        _lookupNormalizer = lookupNormalizer;
    }

    public async Task<ExplorerPage> GetExplorersAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await Explorers().CountAsync();
        var items = await Explorers()
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .ToListAsync();

        return new ExplorerPage(items, totalCount);
    }

    public Task<bool> ExplorerExistsAsync(Guid userId)
    {
        return Explorers().AnyAsync(user => user.Id == userId);
    }

    // Compares the column UserManager filled in at registration, with the same normalizer,
    // so letter case does not matter and the lookup uses the existing email index.
    public Task<ExplorerSummary?> FindExplorerByEmailAsync(string email)
    {
        var normalizedEmail = _lookupNormalizer.NormalizeEmail(email.Trim());
        return Explorers()
            .Where(user => user.NormalizedEmail == normalizedEmail)
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .SingleOrDefaultAsync();
    }

    public Task<List<ExplorerSummary>> GetExplorersByIdsAsync(IReadOnlyCollection<Guid> userIds)
    {
        return Explorers()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .ToListAsync();
    }

    private IQueryable<ApplicationUser> Explorers()
    {
        return from user in _dbContext.Users
               join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
               join role in _dbContext.Roles on userRole.RoleId equals role.Id
               where role.Name == RoleNames.Explorer
               select user;
    }
}
