using Games.Application.QuizSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "explorer")]
public sealed class QuizSolvingController : ControllerBase
{
    private readonly QuizSolvingService _solvingService;
    private readonly QuizSolvingQueries _solvingQueries;

    public QuizSolvingController(QuizSolvingService solvingService, QuizSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet("map")]
    public async Task<ActionResult<List<QuizMapItemDto>>> GetQuizMap()
    {
        return await _solvingQueries.GetQuizMapAsync(User.GetUserId());
    }

    [HttpGet("{quizId:guid}/attempt")]
    public async Task<ActionResult<QuizAttemptDto>> GetAttempt(Guid quizId)
    {
        return await _solvingQueries.GetAttemptAsync(quizId, User.GetUserId());
    }

    [HttpPost("{quizId:guid}/attempt")]
    public async Task<ActionResult> Start(Guid quizId, QuizPositionDto position)
    {
        await _solvingService.StartAsync(quizId, User.GetUserId(), position);
        return NoContent();
    }

    [HttpPost("{quizId:guid}/attempt/questions/{questionId:guid}/answer")]
    public async Task<ActionResult> Answer(Guid quizId, Guid questionId, AnswerDto answer)
    {
        await _solvingService.AnswerAsync(quizId, User.GetUserId(), questionId, answer);
        return NoContent();
    }
}
