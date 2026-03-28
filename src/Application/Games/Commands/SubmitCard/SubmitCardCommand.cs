using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.SubmitCard;

public record SubmitCardCommand : IRequest
{
    public int GameId { get; init; }

    public int PlayerId { get; init; }

    public int WhiteCardId { get; init; }
}

public class SubmitCardCommandHandler : IRequestHandler<SubmitCardCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public SubmitCardCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task Handle(SubmitCardCommand request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
                .ThenInclude(r => r.Submissions)
            .FirstOrDefaultAsync(g => g.Id == request.GameId, cancellationToken);

        Guard.Against.NotFound(request.GameId, game);

        var round = game.Rounds.LastOrDefault(r => r.Status == RoundStatus.Submitting);
        Guard.Against.Null(round, message: "Nenhuma rodada ativa para submissão.");

        Guard.Against.Expression(
            x => x,
            round.Submissions.Any(s => s.PlayerId == request.PlayerId),
            "Jogador já submeteu uma carta nesta rodada.");

        // Remove card from player's hand
        var playerCard = await _context.PlayerCards
            .FirstOrDefaultAsync(pc => pc.PlayerId == request.PlayerId && pc.CardId == request.WhiteCardId && pc.GameId == request.GameId, cancellationToken);

        if (playerCard != null)
            _context.PlayerCards.Remove(playerCard);

        round.Submissions.Add(new Submission
        {
            RoundId = round.Id,
            PlayerId = request.PlayerId,
            WhiteCardId = request.WhiteCardId
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Count non-judges who need to submit
        var nonJudgeCount = game.Players.Count(p => !p.IsJudge);
        var submittedCount = round.Submissions.Count;

        await _notifier.CardSubmitted(game.Code, submittedCount, nonJudgeCount, cancellationToken);

        // Auto-transition to judging when all have submitted
        if (submittedCount >= nonJudgeCount)
        {
            round.Status = RoundStatus.Judging;
            await _context.SaveChangesAsync(cancellationToken);

            // Load white card texts for judge
            var submissions = await _context.Submissions
                .Where(s => s.RoundId == round.Id)
                .Include(s => s.WhiteCard)
                .OrderBy(_ => EF.Functions.Random()) // shuffle so order doesn't reveal submitter
                .Select(s => new SubmissionNotification(s.Id, s.WhiteCard.Text))
                .ToListAsync(cancellationToken);

            await _notifier.JudgingPhase(game.Code, submissions, cancellationToken);
        }
    }
}
