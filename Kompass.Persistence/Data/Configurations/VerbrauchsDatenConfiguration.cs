using Kompass.Domain.Verbrauch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class VerbrauchsDatenConfiguration
    : IEntityTypeConfiguration<VerbrauchsDaten>
{
    public void Configure(
        EntityTypeBuilder<VerbrauchsDaten> builder)
    {
        builder.ToTable("VerbrauchsDaten");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProjektId)
            .IsRequired();

        builder.Property(v => v.PeriodeVon)
            .IsRequired();

        builder.Property(v => v.PeriodeBis)
            .IsRequired();

        builder.Property(v => v.Energietraeger)
            .IsRequired();

        builder.Property(v => v.Menge)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(v => v.Kosten)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(v => v.WitterungsbereinigungsFaktor)
            .HasPrecision(10, 6);

        builder.Property(v => v.Flaeche)
            .HasPrecision(12, 2);

        builder.Property(v => v.B56VergleichsWert)
            .HasPrecision(18, 4);

        builder.Property(v => v.AnpassungsFaktor)
            .HasPrecision(10, 6);

        builder.Property(v => v.AnpassungsBegruendung)
            .HasMaxLength(VerbrauchsDaten.MaxBegruendungLaenge);

        builder.Property(v => v.Abweichungsursache)
            .HasMaxLength(VerbrauchsDaten.MaxBegruendungLaenge);

        builder.HasIndex(v => v.ProjektId);

        builder.HasIndex(
            v => new
            {
                v.ProjektId,
                v.PeriodeVon,
                v.PeriodeBis,
                v.Energietraeger
            });
    }
}
