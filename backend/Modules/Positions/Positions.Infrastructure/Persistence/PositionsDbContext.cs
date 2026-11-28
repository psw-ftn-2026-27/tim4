using Microsoft.EntityFrameworkCore;
using Positions.Application;
using Positions.Domain.TouristPositions;
using Positions.Infrastructure.Persistence.TouristPositions;

namespace Positions.Infrastructure.Persistence;

internal sealed class PositionsDbContext : DbContext, IUnitOfWork
{
    public PositionsDbContext(DbContextOptions<PositionsDbContext> options) : base(options) { }

    public DbSet<TouristPosition> TouristPositions => Set<TouristPosition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("positions");
        builder.ConfigureTouristPositions();
    }
}
