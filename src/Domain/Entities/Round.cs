using Zynko.Domain.Common;
using Zynko.Domain.Enums;

namespace Zynko.Domain.Entities;

public class Round : BaseEntity
{
    public int GameId { get; set; }

    public Game Game { get; set; } = null!;

    public int BlackCardId { get; set; }

    public Card BlackCard { get; set; } = null!;

    public RoundStatus Status { get; set; } = RoundStatus.Submitting;

    public int? WinnerSubmissionId { get; set; }

    public IList<Submission> Submissions { get; set; } = new List<Submission>();
}
