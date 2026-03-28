using Zynko.Domain.Common;

namespace Zynko.Domain.Entities;

public class Submission : BaseEntity
{
    public int RoundId { get; set; }

    public Round Round { get; set; } = null!;

    public int PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    public int WhiteCardId { get; set; }

    public Card WhiteCard { get; set; } = null!;

    public bool IsWinner { get; set; } = false;
}
