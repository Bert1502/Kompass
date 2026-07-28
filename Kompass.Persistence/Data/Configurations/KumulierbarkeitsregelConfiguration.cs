using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class KumulierbarkeitsregelConfiguration
    : IEntityTypeConfiguration<Kumulierbarkeitsregel>
{
    public void Configure(
        EntityTypeBuilder<Kumulierbarkeitsregel> builder)
    {
        builder.ToTable("Kumulierbarkeitsregeln");

        builder.HasKey(regel => regel.Id);

        builder.Property(regel => regel.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(regel => regel.Beschreibung)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(regel => regel.GueltigAb)
            .IsRequired();

        builder.Property(regel => regel.GueltigBis);
    }
}
