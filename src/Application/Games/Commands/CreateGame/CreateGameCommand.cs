using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Domain.Enums;

namespace Zynko.Application.Games.Commands.CreateGame;

public record CreateGameCommand : IRequest<CreateGameResult>
{
    public string HostName { get; init; } = string.Empty;

    public int ScoreLimit { get; init; } = 5;
}

public record CreateGameResult(int GameId, string GameCode, int PlayerId);

public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, CreateGameResult>
{
    private readonly IApplicationDbContext _context;

    public CreateGameCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateGameResult> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var host = new Player { Name = request.HostName };

        var game = new Game
        {
            Code = GenerateCode(),
            Status = GameStatus.WaitingForPlayers,
            CreatedAt = DateTimeOffset.UtcNow,
            ScoreLimit = request.ScoreLimit,
            Players = { host }
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync(cancellationToken);

        game.HostPlayerId = host.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateGameResult(game.Id, game.Code, host.Id);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
