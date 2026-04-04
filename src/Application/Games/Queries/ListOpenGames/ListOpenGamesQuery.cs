using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Queries.ListOpenGames;

public record ListOpenGamesQuery : IRequest<IList<OpenGameVm>>;

public class ListOpenGamesQueryHandler : IRequestHandler<ListOpenGamesQuery, IList<OpenGameVm>>
{
    private readonly IApplicationDbContext _context;

    public ListOpenGamesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IList<OpenGameVm>> Handle(ListOpenGamesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Games
            .Include(g => g.Players)
            .Where(g => g.Status != GameStatus.Finished && g.Players.Count < 12)
            .OrderByDescending(g => g.Id)
            .Select(g => new OpenGameVm
            {
                Code = g.Code,
                Name = g.Name,
                HostName = g.Players.FirstOrDefault(p => p.Id == g.HostPlayerId) != null
                    ? g.Players.First(p => p.Id == g.HostPlayerId).Name
                    : "",
                PlayerCount = g.Players.Count,
                ScoreLimit = g.ScoreLimit,
                InProgress = g.Status == GameStatus.InProgress
            })
            .ToListAsync(cancellationToken);
    }
}

public class OpenGameVm
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int ScoreLimit { get; set; }
    public bool InProgress { get; set; }
}
