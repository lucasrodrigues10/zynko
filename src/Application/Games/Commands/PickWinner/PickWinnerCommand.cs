using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.PickWinner;

public record PickWinnerCommand : IRequest
{
    public int GameId { get; init; }

    public int RoundId { get; init; }

    public int SubmissionId { get; init; }
}

public class PickWinnerCommandHandler : IRequestHandler<PickWinnerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public PickWinnerCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task Handle(PickWinnerCommand request, CancellationToken cancellationToken)
    {
        var round = await _context.Rounds
            .Include(r => r.Submissions)
                .ThenInclude(s => s.WhiteCard)
            .Include(r => r.Game)
                .ThenInclude(g => g.Players)
            .FirstOrDefaultAsync(r => r.Id == request.RoundId && r.GameId == request.GameId, cancellationToken);

        Guard.Against.NotFound(request.RoundId, round);

        var submission = round.Submissions.FirstOrDefault(s => s.Id == request.SubmissionId);
        Guard.Against.NotFound(request.SubmissionId, submission);

        submission.IsWinner = true;
        round.WinnerSubmissionId = submission.Id;
        round.Status = RoundStatus.Finished;

        var winner = round.Game.Players.First(p => p.Id == submission.PlayerId);
        winner.Score++;

        // Rotate judge
        var players = round.Game.Players.ToList();
        var currentJudge = players.First(p => p.IsJudge);
        currentJudge.IsJudge = false;
        players[(players.IndexOf(currentJudge) + 1) % players.Count].IsJudge = true;

        round.Game.CurrentRoundIndex++;

        var isGameFinished = round.Game.Players.Any(p => p.Score >= round.Game.ScoreLimit);
        if (isGameFinished)
            round.Game.Status = GameStatus.Finished;

        await _context.SaveChangesAsync(cancellationToken);

        var scores = round.Game.Players.Select(p => new PlayerScoreNotification(p.Id, p.Name, p.Score, p.IsJudge));

        if (isGameFinished)
        {
            await _notifier.GameFinished(round.Game.Code, scores, cancellationToken);
        }
        else
        {
            await _notifier.RoundFinished(round.Game.Code, winner.Id, winner.Name, submission.WhiteCard.Text, scores, cancellationToken);
        }
    }
}
