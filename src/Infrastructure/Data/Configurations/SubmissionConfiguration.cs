using Zynko.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zynko.Infrastructure.Data.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.WhiteCard)
            .WithMany()
            .HasForeignKey(s => s.WhiteCardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
