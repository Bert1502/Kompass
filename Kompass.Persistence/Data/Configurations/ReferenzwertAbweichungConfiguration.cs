using Kompass.Domain.Referenzdaten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class ReferenzwertAbweichungConfiguration : IEntityTypeConfiguration<ReferenzwertAbweichung>
{
    public void Configure(EntityTypeBuilder<ReferenzwertAbweichung> builder)
    {
        builder.ToTable("ReferenzwertAbweichungen");

        builder.HasKey(eintrag => eintrag.Id);

        builder.Property(eintrag => eintrag.Parameterart)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(eintrag => eintrag.Bezugsgroesse)
            .HasMaxLength(128);

        builder.Property(eintrag => eintrag.EnergietraegerOderKategorie)
            .HasMaxLength(128);

        builder.Property(eintrag => eintrag.UrspruenglicherReferenzwert)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(eintrag => eintrag.VerwendeterProjektwert)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(eintrag => eintrag.Begruendung)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(eintrag => eintrag.Benutzer)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(eintrag => eintrag.AenderungszeitpunktUtc)
            .IsRequired();

        builder.HasIndex(eintrag => new
        {
            eintrag.ProjektId,
            eintrag.Parameterart,
            eintrag.AenderungszeitpunktUtc
        });
    }
}
