using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.JoinGame;

public record JoinGameCommand : IRequest<JoinGameResult>
{
    public string GameCode { get; init; } = string.Empty;

    public string PlayerName { get; init; } = string.Empty;
}

public record JoinGameResult(int GameId, int PlayerId, bool GameInProgress);

public class JoinGameCommandHandler : IRequestHandler<JoinGameCommand, JoinGameResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public JoinGameCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<JoinGameResult> Handle(JoinGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.Code == request.GameCode, cancellationToken);

        Guard.Against.NotFound(request.GameCode, game);
        Guard.Against.Expression(
            x => x == GameStatus.Finished,
            game.Status,
            "Este jogo já foi encerrado.");
        Guard.Against.Expression(
            x => x >= 12,
            game.Players.Count,
            "A sala está cheia (máximo 12 jogadores).");

        var player = new Player { GameId = game.Id, Name = request.PlayerName };
        _context.Players.Add(player);
        await _context.SaveChangesAsync(cancellationToken);

        // If joining mid-game, deal cards to the new player
        if (game.Status == GameStatus.InProgress)
        {
            await DealCards(game, player, cancellationToken);
        }

        await _notifier.PlayerJoined(game.Code, player.Id, player.Name, cancellationToken);

        return new JoinGameResult(game.Id, player.Id, game.Status == GameStatus.InProgress);
    }

    private async Task DealCards(Game game, Player player, CancellationToken cancellationToken)
    {
        var dealtCardIds = await _context.PlayerCards
            .Where(pc => pc.GameId == game.Id)
            .Select(pc => pc.CardId)
            .ToListAsync(cancellationToken);

        var newCards = await _context.Cards
            .Where(c => c.Type == CardType.White && !dealtCardIds.Contains(c.Id))
            .OrderBy(_ => EF.Functions.Random())
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var card in newCards)
        {
            _context.PlayerCards.Add(new PlayerCard
            {
                GameId = game.Id,
                PlayerId = player.Id,
                CardId = card.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
