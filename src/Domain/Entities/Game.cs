using Zynko.Domain.Common;
using Zynko.Domain.Enums;

namespace Zynko.Domain.Entities;

public class Game : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;

    public DateTimeOffset CreatedAt { get; set; }

    public int ScoreLimit { get; set; } = 5;

    public int CurrentRoundIndex { get; set; } = 0;

    public int? HostPlayerId { get; set; }

    public IList<Player> Players { get; set; } = new List<Player>();

    public IList<Round> Rounds { get; set; } = new List<Round>();
}
