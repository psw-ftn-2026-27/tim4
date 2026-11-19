using Games.Application;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<LocationChallenge> LocationChallenges => Set<LocationChallenge>();
    public DbSet<LocationChallengeAttempt> LocationChallengeAttempts => Set<LocationChallengeAttempt>();
    public DbSet<SocialChallenge> SocialChallenges => Set<SocialChallenge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureQuizzes();
        builder.ConfigureLocationChallenges();
        builder.ConfigureLocationChallengeAttempts();
        builder.ConfigureSocialChallenges();
    }
}
