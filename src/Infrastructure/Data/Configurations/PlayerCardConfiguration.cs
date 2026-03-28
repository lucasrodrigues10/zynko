using Zynko.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zynko.Infrastructure.Data.Configurations;

public class PlayerCardConfiguration : IEntityTypeConfiguration<PlayerCard>
{
    public void Configure(EntityTypeBuilder<PlayerCard> builder)
    {
        builder.HasOne(pc => pc.Player)
            .WithMany()
            .HasForeignKey(pc => pc.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Card)
            .WithMany()
            .HasForeignKey(pc => pc.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => new { pc.GameId, pc.PlayerId });
    }
}
