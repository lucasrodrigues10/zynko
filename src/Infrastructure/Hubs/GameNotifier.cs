using Zynko.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Zynko.Infrastructure.Hubs;

public class GameNotifier : IGameNotifier
{
    private readonly IHubContext<GameHub> _hub;

    public GameNotifier(IHubContext<GameHub> hub)
    {
        _hub = hub;
    }

    public Task PlayerJoined(string gameCode, int playerId, string playerName, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("PlayerJoined", new { playerId, playerName }, cancellationToken);

    public Task GameStarted(string gameCode, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("GameStarted", cancellationToken);

    public Task RoundStarted(string gameCode, int roundId, string blackCardText, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("RoundStarted", new { roundId, blackCardText }, cancellationToken);

    public Task CardSubmitted(string gameCode, int submittedCount, int totalRequired, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("CardSubmitted", new { submittedCount, totalRequired }, cancellationToken);

    public Task JudgingPhase(string gameCode, IEnumerable<SubmissionNotification> submissions, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("JudgingPhase", submissions, cancellationToken);

    public Task RoundFinished(string gameCode, int winnerPlayerId, string winnerName, string whiteCardText, IEnumerable<PlayerScoreNotification> scores, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("RoundFinished", new { winnerPlayerId, winnerName, whiteCardText, scores }, cancellationToken);

    public Task GameFinished(string gameCode, IEnumerable<PlayerScoreNotification> scores, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(gameCode).SendAsync("GameFinished", new { scores }, cancellationToken);
}
