using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.JoinGame;

public record JoinGameCommand : IRequest<JoinGameResult>
{
    public string GameCode { get; init; } = string.Empty;

    public string PlayerName { get; init; } = string.Empty;
}

public record JoinGameResult(int GameId, int PlayerId);

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
            x => x != GameStatus.WaitingForPlayers,
            game.Status,
            "O jogo já começou ou foi encerrado.");
        Guard.Against.Expression(
            x => x >= 12,
            game.Players.Count,
            "A sala está cheia (máximo 12 jogadores).");

        var player = new Player { GameId = game.Id, Name = request.PlayerName };
        _context.Players.Add(player);
        await _context.SaveChangesAsync(cancellationToken);

        await _notifier.PlayerJoined(game.Code, player.Id, player.Name, cancellationToken);

        return new JoinGameResult(game.Id, player.Id);
    }
}
