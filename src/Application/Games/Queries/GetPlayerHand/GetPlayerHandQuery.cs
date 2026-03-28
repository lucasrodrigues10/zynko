using Zynko.Application.Common.Interfaces;

namespace Zynko.Application.Games.Queries.GetPlayerHand;

public record GetPlayerHandQuery(int GameId, int PlayerId) : IRequest<IList<CardDto>>;

public class GetPlayerHandQueryHandler : IRequestHandler<GetPlayerHandQuery, IList<CardDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlayerHandQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IList<CardDto>> Handle(GetPlayerHandQuery request, CancellationToken cancellationToken)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.Id == request.PlayerId && p.GameId == request.GameId, cancellationToken);

        Guard.Against.NotFound(request.PlayerId, player);

        var hand = await _context.PlayerCards
            .Where(pc => pc.PlayerId == request.PlayerId && pc.GameId == request.GameId)
            .Include(pc => pc.Card)
            .Select(c => new CardDto { Id = c.Card.Id, Text = c.Card.Text })
            .ToListAsync(cancellationToken);

        return hand;
    }
}

public class CardDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
