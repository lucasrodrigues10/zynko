using Zynko.Domain.Common;

namespace Zynko.Domain.Entities;

public class Player : BaseEntity
{
    public int GameId { get; set; }

    public Game Game { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Score { get; set; } = 0;

    public bool IsJudge { get; set; } = false;
}
