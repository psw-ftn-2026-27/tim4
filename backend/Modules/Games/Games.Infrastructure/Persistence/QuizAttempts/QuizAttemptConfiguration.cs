using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal static class QuizAttemptConfiguration
{
    public static void ConfigureQuizAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureQuizAttempt(modelBuilder.Entity<QuizAttempt>());
    }

    private static void ConfigureQuizAttempt(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasIndex(attempt => new { attempt.QuizId, attempt.PlayerId }).IsUnique();
        builder.OwnsMany(attempt => attempt.Answers, ConfigureGivenAnswer);
    }

    private static void ConfigureGivenAnswer(OwnedNavigationBuilder<QuizAttempt, GivenAnswer> builder)
    {
        builder.ToJson();
    }
}
