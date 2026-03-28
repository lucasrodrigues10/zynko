using Zynko.Domain.Entities;

namespace Zynko.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Card> Cards { get; }

    DbSet<Game> Games { get; }

    DbSet<Player> Players { get; }

    DbSet<PlayerCard> PlayerCards { get; }

    DbSet<Round> Rounds { get; }

    DbSet<Submission> Submissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
