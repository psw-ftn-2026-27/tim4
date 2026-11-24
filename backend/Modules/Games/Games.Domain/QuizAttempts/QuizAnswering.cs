using Games.Domain.Experience;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

public sealed class QuizAnswering
{
    public EarnedXp? Answer(Quiz quiz, QuizAttempt attempt, Guid questionId, IReadOnlyList<int> selectedOptions, DateTime now)
    {
        if (attempt.QuizId != quiz.Id)
        {
            throw new DomainException("The attempt does not belong to this quiz.");
        }
        if (quiz.Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be solved.");
        }
        attempt.EnsureCanContinue();

        var nextQuestion = quiz.Questions.First(question => attempt.Answers.All(answer => answer.QuestionId != question.Id));
        if (nextQuestion.Id != questionId)
        {
            throw new DomainException("Quiz questions are answered in the order they are given.");
        }

        var isLastQuestion = nextQuestion == quiz.Questions[^1];
        return attempt.RecordAnswer(questionId, selectedOptions, nextQuestion.Score(selectedOptions), isLastQuestion, now);
    }
}
