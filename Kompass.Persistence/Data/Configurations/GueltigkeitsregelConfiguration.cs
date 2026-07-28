using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class GueltigkeitsregelConfiguration
    : IEntityTypeConfiguration<Gueltigkeitsregel>
{
    public void Configure(
        EntityTypeBuilder<Gueltigkeitsregel> builder)
    {
        builder.ToTable("Gueltigkeitsregeln");

        builder.HasKey(regel => regel.Id);

        builder.Property(regel => regel.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.Bezug)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(regel => regel.GueltigAb)
            .IsRequired();

        builder.Property(regel => regel.GueltigBis);

        builder.Property(regel => regel.Beschreibung)
            .HasMaxLength(1000);
    }
}
