using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.StartGame;

public record StartGameCommand : IRequest
{
    public int GameId { get; init; }

    public int HostPlayerId { get; init; }
}

public class StartGameCommandHandler : IRequestHandler<StartGameCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public StartGameCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task Handle(StartGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.Id == request.GameId, cancellationToken);

        Guard.Against.NotFound(request.GameId, game);
        Guard.Against.Expression(
            x => x != game.HostPlayerId,
            request.HostPlayerId,
            "Apenas o host pode iniciar o jogo.");
        Guard.Against.Expression(
            x => x < 2,
            game.Players.Count,
            "São necessários ao menos 2 jogadores para iniciar.");

        // Set first judge
        game.Players[0].IsJudge = true;
        game.Status = GameStatus.InProgress;

        await _context.SaveChangesAsync(cancellationToken);
        await _notifier.GameStarted(game.Code, cancellationToken);

        // Start first round
        await StartRound(game, cancellationToken);
    }

    private async Task StartRound(Game game, CancellationToken cancellationToken)
    {
        var usedCardIds = await _context.Rounds
            .Where(r => r.GameId == game.Id)
            .Select(r => r.BlackCardId)
            .ToListAsync(cancellationToken);

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
        await _context.SaveChangesAsync(cancellationToken);

        // Deal hand to each non-judge (fill up to 5)
        await DealHands(game, round.Id, cancellationToken);

        await _notifier.RoundStarted(game.Code, round.Id, blackCard.Text, cancellationToken);
    }

    private async Task DealHands(Game game, int roundId, CancellationToken cancellationToken)
    {
        var nonJudges = game.Players.Where(p => !p.IsJudge).ToList();

        // Cards already in anyone's hand
        var dealtCardIds = await _context.PlayerCards
            .Where(pc => pc.GameId == game.Id)
            .Select(pc => pc.CardId)
            .ToListAsync(cancellationToken);

        foreach (var player in nonJudges)
        {
            var currentHandCount = await _context.PlayerCards
                .CountAsync(pc => pc.PlayerId == player.Id && pc.GameId == game.Id, cancellationToken);

            var needed = 5 - currentHandCount;
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
