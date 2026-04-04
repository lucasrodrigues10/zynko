using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.StartRound;

public record StartRoundCommand(int GameId) : IRequest;

public class StartRoundCommandHandler : IRequestHandler<StartRoundCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public StartRoundCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task Handle(StartRoundCommand request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.Id == request.GameId, cancellationToken);

        Guard.Against.NotFound(request.GameId, game);
        Guard.Against.Expression(
            x => x != GameStatus.InProgress,
            game.Status,
            "Jogo não está em andamento.");

        var usedCardIds = game.Rounds.Select(r => r.BlackCardId).ToList();

        var blackCard = await _context.Cards
            .Where(c => c.Type == CardType.Black && !usedCardIds.Contains(c.Id))
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.Null(blackCard, message: "Não há mais cartas pretas disponíveis.");

        var round = new Round
        {
            GameId = game.Id,
            BlackCardId = blackCard.Id,
            Status = RoundStatus.Submitting
        };

        game.Rounds.Add(round);
        game.CurrentRoundIndex++;
        await _context.SaveChangesAsync(cancellationToken);

        // Fill hands back to 10
        await DealHands(game, cancellationToken);

        await _notifier.RoundStarted(game.Code, round.Id, blackCard.Text, cancellationToken);
    }

    private async Task DealHands(Game game, CancellationToken cancellationToken)
    {
        var nonJudges = game.Players.Where(p => !p.IsJudge).ToList();

        var dealtCardIds = await _context.PlayerCards
            .Where(pc => pc.GameId == game.Id)
            .Select(pc => pc.CardId)
            .ToListAsync(cancellationToken);

        foreach (var player in nonJudges)
        {
            var currentHandCount = await _context.PlayerCards
                .CountAsync(pc => pc.PlayerId == player.Id && pc.GameId == game.Id, cancellationToken);

            var needed = 10 - currentHandCount;
            if (needed <= 0) continue;

            var newCards = await _context.Cards
                .Where(c => c.Type == CardType.White && !dealtCardIds.Contains(c.Id))
                .OrderBy(_ => EF.Functions.Random())
                .Take(needed)
                .ToListAsync(cancellationToken);

            foreach (var card in newCards)
            {
                _context.PlayerCards.Add(new PlayerCard
                {
                    GameId = game.Id,
                    PlayerId = player.Id,
                    CardId = card.Id
                });
                dealtCardIds.Add(card.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
