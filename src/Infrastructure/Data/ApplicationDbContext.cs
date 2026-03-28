using System.Reflection;
using Zynko.Application.Common.Interfaces;
using Zynko.Domain.Entities;
using Zynko.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Zynko.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<PlayerCard> PlayerCards => Set<PlayerCard>();

    public DbSet<Round> Rounds => Set<Round>();

    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
