using Zynko.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zynko.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.Property(g => g.Code)
            .HasMaxLength(6)
            .IsRequired();

        builder.HasIndex(g => g.Code).IsUnique();

        builder.HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Rounds)
            .WithOne(r => r.Game)
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
