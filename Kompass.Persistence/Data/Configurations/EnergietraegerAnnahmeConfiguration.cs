using Kompass.Domain.Economics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class EnergietraegerAnnahmeConfiguration
    : IEntityTypeConfiguration<EnergietraegerAnnahme>
{
    public void Configure(
        EntityTypeBuilder<EnergietraegerAnnahme> builder)
    {
        builder.ToTable("EnergietraegerAnnahmen");

        builder.HasKey(annahme => annahme.Id);

        builder.Property(annahme => annahme.Energietraeger)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(annahme => annahme.Preis)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(annahme => annahme.Preissteigerungsrate)
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(annahme => annahme.Co2Faktor)
            .HasPrecision(10, 6)
            .IsRequired();

        builder.Property(annahme => annahme.Co2Preis)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(annahme => annahme.Co2Preissteigerungsrate)
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(annahme => annahme.EndenergieIstZustand)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(annahme => annahme.EndenergieAlternative)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Ignore(annahme => annahme.Einsparung);
    }
}
