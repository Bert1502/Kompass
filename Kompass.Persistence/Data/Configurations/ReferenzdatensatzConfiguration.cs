using Kompass.Domain.Referenzdaten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class ReferenzdatensatzConfiguration : IEntityTypeConfiguration<Referenzdatensatz>
{
    public void Configure(EntityTypeBuilder<Referenzdatensatz> builder)
    {
        builder.ToTable("Referenzdatensaetze");

        builder.HasKey(datensatz => datensatz.Id);

        builder.Property(datensatz => datensatz.FachlicheBezeichnung)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(datensatz => datensatz.Parameterart)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(datensatz => datensatz.Wert)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(datensatz => datensatz.Einheit)
            .HasMaxLength(64);

        builder.Property(datensatz => datensatz.Bezugsgroesse)
            .HasMaxLength(128);

        builder.Property(datensatz => datensatz.EnergietraegerOderKategorie)
            .HasMaxLength(128);

        builder.Property(datensatz => datensatz.Ebene)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(datensatz => datensatz.Quelle)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(datensatz => datensatz.Herausgeber)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(datensatz => datensatz.QuellenVerweis)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(datensatz => datensatz.Versionsstand)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(datensatz => datensatz.Datenstatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(datensatz => datensatz.Qualitaetsstatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(datensatz => datensatz.Importart)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(datensatz => datensatz.LetzteAktualisierungUtc)
            .IsRequired();

        builder.HasIndex(datensatz => new
        {
            datensatz.Parameterart,
            datensatz.Ebene,
            datensatz.ProjektId,
            datensatz.UnternehmenId,
            datensatz.Bezugsgroesse,
            datensatz.EnergietraegerOderKategorie,
            datensatz.GueltigAb,
            datensatz.Versionsstand
        }).IsUnique();
    }
}
