namespace Zynko.Application.Common.Interfaces;

public interface IGameNotifier
{
    Task PlayerJoined(string gameCode, int playerId, string playerName, CancellationToken cancellationToken = default);

    Task GameStarted(string gameCode, CancellationToken cancellationToken = default);

    Task RoundStarted(string gameCode, int roundId, string blackCardText, CancellationToken cancellationToken = default);

    Task CardSubmitted(string gameCode, int submittedCount, int totalRequired, CancellationToken cancellationToken = default);

    Task JudgingPhase(string gameCode, IEnumerable<SubmissionNotification> submissions, CancellationToken cancellationToken = default);

    Task RoundFinished(string gameCode, int winnerPlayerId, string winnerName, string whiteCardText, IEnumerable<PlayerScoreNotification> scores, CancellationToken cancellationToken = default);

    Task GameFinished(string gameCode, IEnumerable<PlayerScoreNotification> scores, CancellationToken cancellationToken = default);

    Task PlayerLeft(string gameCode, int playerId, CancellationToken cancellationToken = default);
}

public record SubmissionNotification(int Id, string WhiteCardText);

public record PlayerScoreNotification(int PlayerId, string Name, int Score, bool IsJudge);
