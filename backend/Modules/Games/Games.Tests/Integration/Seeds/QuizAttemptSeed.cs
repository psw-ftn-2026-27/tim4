using Games.Domain.QuizAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorerMonasteriesAttemptBeforeLastQuestion;
    public static readonly QuizAttempt SecondExplorerSolvedMonasteriesAttempt;

    static QuizAttemptSeed()
    {
        var quiz = QuizSeed.PublishedMonasteriesQuiz;
        var answeredAt = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

        ExplorerMonasteriesAttemptBeforeLastQuestion = new QuizAttempt(quiz.Id, WellKnownUsers.Explorer);
        new QuizAnswering().Answer(quiz, ExplorerMonasteriesAttemptBeforeLastQuestion, quiz.Questions[0].Id, [0], answeredAt);
        new QuizAnswering().Answer(quiz, ExplorerMonasteriesAttemptBeforeLastQuestion, quiz.Questions[1].Id, [0], answeredAt);
        new QuizAnswering().Answer(quiz, ExplorerMonasteriesAttemptBeforeLastQuestion, quiz.Questions[2].Id, [0, 1], answeredAt);

        SecondExplorerSolvedMonasteriesAttempt = new QuizAttempt(quiz.Id, WellKnownUsers.SecondExplorer);
        new QuizAnswering().Answer(quiz, SecondExplorerSolvedMonasteriesAttempt, quiz.Questions[0].Id, [0], answeredAt);
        new QuizAnswering().Answer(quiz, SecondExplorerSolvedMonasteriesAttempt, quiz.Questions[1].Id, [1], answeredAt);
        new QuizAnswering().Answer(quiz, SecondExplorerSolvedMonasteriesAttempt, quiz.Questions[2].Id, [0, 2], answeredAt);
        new QuizAnswering().Answer(quiz, SecondExplorerSolvedMonasteriesAttempt, quiz.Questions[3].Id, [0], answeredAt);
    }

    public static QuizAttempt[] All => [ExplorerMonasteriesAttemptBeforeLastQuestion, SecondExplorerSolvedMonasteriesAttempt];
}
