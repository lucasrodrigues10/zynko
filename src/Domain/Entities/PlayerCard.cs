using Zynko.Domain.Common;

namespace Zynko.Domain.Entities;

public class PlayerCard : BaseEntity
{
    public int GameId { get; set; }

    public int PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public int CardId { get; set; }

    public Card Card { get; set; } = null!;
}
