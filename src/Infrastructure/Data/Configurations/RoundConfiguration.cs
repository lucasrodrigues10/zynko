using Zynko.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zynko.Infrastructure.Data.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.HasOne(r => r.BlackCard)
            .WithMany()
            .HasForeignKey(r => r.BlackCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Submissions)
            .WithOne(s => s.Round)
            .HasForeignKey(s => s.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
