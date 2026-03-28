using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Queries.GetGame;

public record GetGameQuery(string Code) : IRequest<GameVm>;

public class GetGameQueryHandler : IRequestHandler<GetGameQuery, GameVm>
{
    private readonly IApplicationDbContext _context;

    public GetGameQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GameVm> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
                .ThenInclude(r => r.BlackCard)
            .Include(g => g.Rounds)
                .ThenInclude(r => r.Submissions)
                    .ThenInclude(s => s.WhiteCard)
            .FirstOrDefaultAsync(g => g.Code == request.Code, cancellationToken);

        Guard.Against.NotFound(request.Code, game);

        var currentRound = game.Rounds.LastOrDefault();

        return new GameVm
        {
            Id = game.Id,
            Code = game.Code,
            Status = game.Status,
            ScoreLimit = game.ScoreLimit,
            CurrentRoundIndex = game.CurrentRoundIndex,
            HostPlayerId = game.HostPlayerId,
            Players = game.Players.Select(p => new PlayerVm
            {
                Id = p.Id,
                Name = p.Name,
                Score = p.Score,
                IsJudge = p.IsJudge
            }).ToList(),
            CurrentRound = currentRound == null ? null : new RoundVm
            {
                Id = currentRound.Id,
                BlackCardText = currentRound.BlackCard.Text,
                Status = currentRound.Status,
                WinnerSubmissionId = currentRound.WinnerSubmissionId,
                Submissions = currentRound.Submissions.Select(s => new SubmissionVm
                {
                    Id = s.Id,
                    PlayerId = s.PlayerId,
                    WhiteCardText = currentRound.Status == RoundStatus.Judging || currentRound.Status == RoundStatus.Finished
                        ? s.WhiteCard.Text
                        : null,
                    IsWinner = s.IsWinner
                }).ToList()
            }
        };
    }
}

public class GameVm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public int ScoreLimit { get; set; }
    public int CurrentRoundIndex { get; set; }
    public int? HostPlayerId { get; set; }
    public IList<PlayerVm> Players { get; set; } = [];
    public RoundVm? CurrentRound { get; set; }
}

public class PlayerVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsJudge { get; set; }
}

public class RoundVm
{
    public int Id { get; set; }
    public string BlackCardText { get; set; } = string.Empty;
    public RoundStatus Status { get; set; }
    public int? WinnerSubmissionId { get; set; }
    public IList<SubmissionVm> Submissions { get; set; } = [];
}

public class SubmissionVm
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string? WhiteCardText { get; set; }
    public bool IsWinner { get; set; }
}
