using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.LeaveGame;

public record LeaveGameCommand : IRequest
{
    public string GameCode { get; init; } = string.Empty;
    public int PlayerId { get; init; }
}

public class LeaveGameCommandHandler : IRequestHandler<LeaveGameCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IGameNotifier _notifier;

    public LeaveGameCommandHandler(IApplicationDbContext context, IGameNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task Handle(LeaveGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.Code == request.GameCode, cancellationToken);

        if (game == null) return;

        var player = game.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null) return;

        _context.Players.Remove(player);

        if (game.Players.Count == 1)
        {
            // Last player leaving — delete the game and all related data
            var roundIds = game.Rounds.Select(r => r.Id).ToList();
            if (roundIds.Count > 0)
            {
                var submissions = await _context.Submissions
                    .Where(s => roundIds.Contains(s.RoundId))
                    .ToListAsync(cancellationToken);
                _context.Submissions.RemoveRange(submissions);
            }

            var playerCards = await _context.PlayerCards
                .Where(pc => pc.GameId == game.Id)
                .ToListAsync(cancellationToken);
            _context.PlayerCards.RemoveRange(playerCards);

            _context.Games.Remove(game);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        // Transfer host if needed
        if (game.HostPlayerId == request.PlayerId)
        {
            var newHost = game.Players.First(p => p.Id != request.PlayerId);
            game.HostPlayerId = newHost.Id;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _notifier.PlayerLeft(game.Code, request.PlayerId, cancellationToken);
    }
}
