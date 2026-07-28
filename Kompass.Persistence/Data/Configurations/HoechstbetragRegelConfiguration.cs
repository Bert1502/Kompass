using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class HoechstbetragRegelConfiguration
    : IEntityTypeConfiguration<HoechstbetragRegel>
{
    public void Configure(
        EntityTypeBuilder<HoechstbetragRegel> builder)
    {
        builder.ToTable("HoechstbetragRegeln");

        builder.HasKey(regel => regel.Id);

        builder.Property(regel => regel.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.Betrag)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(regel => regel.Waehrung)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(regel => regel.Bezugsbasis)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.GueltigAb)
            .IsRequired();

        builder.Property(regel => regel.GueltigBis);

        builder.Property(regel => regel.Beschreibung)
            .HasMaxLength(1000);
    }
}
