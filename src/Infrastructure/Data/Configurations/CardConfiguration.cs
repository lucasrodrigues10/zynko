using Zynko.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zynko.Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.Property(c => c.Text)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.TextEn)
            .HasMaxLength(500);

        builder.Property(c => c.Pack)
            .HasMaxLength(100)
            .IsRequired();
    }
}
