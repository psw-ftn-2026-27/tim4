using System.Globalization;
using Games.Application;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeBrowsing;
using Games.Application.LocationChallengePlaying;
using Games.Application.LocationChallenges;
using Games.Application.QuizAttempts;
using Games.Application.QuizAuthoring;
using Games.Application.QuizLifecycle;
using Games.Application.QuizSolving;
using Games.Application.Quizzes;
using Games.Domain.QuizAttempts;
using Games.Application.SocialChallengePreparation;
using Games.Application.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizAttempts;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();

        services.AddScoped<QuizAuthoringService>();
        services.AddScoped<QuizAuthoringQueries>();
        services.AddScoped<QuizLifecycleService>();

        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<QuizAnswering>();
        services.AddScoped<QuizSolvingService>();
        services.AddScoped<QuizSolvingQueries>();

        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeAttemptRepository, LocationChallengeAttemptRepository>();
        services.AddScoped<ILocationChallengePlayReadRepository, LocationChallengePlayReadRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<LocationChallengeAuthoringService>();
        services.AddScoped<LocationChallengeAuthoringQueries>();
        services.AddScoped<LocationChallengeBrowsingQueries>();
        services.AddScoped<LocationChallengePlayingService>();
        services.AddScoped<LocationChallengePlayingQueries>();
        services.AddSingleton(provider =>
        {
            var activationDistance = provider.GetRequiredService<IConfiguration>()
                ["Games:LocationChallenges:ActivationDistanceInMeters"];
            return new LocationChallengeActivation(string.IsNullOrWhiteSpace(activationDistance)
                ? null
                : double.Parse(activationDistance, NumberStyles.Float, CultureInfo.InvariantCulture));
        });

        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();
        services.AddScoped<SocialChallengePreparationService>();
        services.AddScoped<SocialChallengePreparationQueries>();

        return services;
    }
}
