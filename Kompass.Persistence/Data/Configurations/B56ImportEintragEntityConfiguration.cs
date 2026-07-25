using Kompass.Persistence.Data.Entities;
using Kompass.Application.B56Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class B56ImportEintragEntityConfiguration
    : IEntityTypeConfiguration<B56ImportEintragEntity>
{
    public void Configure(
        EntityTypeBuilder<B56ImportEintragEntity> builder)
    {
        builder.ToTable("B56ImportEintraege");

        builder.HasKey(x => x.ImportId);

        builder.Property(x => x.Projektname)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Originaldateiname)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Archivdateipfad)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Dateiendung)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FachdatenJson);

        builder.Property(x => x.SnapshotSchemaVersion)
            .HasDefaultValue(
                B56SnapshotVersionen.AktuelleSchemaVersion)
            .IsRequired();

        builder.Property(x => x.ParserVersion)
            .HasMaxLength(50)
            .HasDefaultValue(
                B56SnapshotVersionen.LegacyParserVersion)
            .IsRequired();

        builder.Property(x => x.SnapshotStatus)
            .HasConversion<int>()
            .HasDefaultValue(
                B56SnapshotStatus.TechnischGeprueft)
            .IsRequired();

        builder.Property(x => x.BestaetigtAm);

        builder.Property(x => x.VerworfenAm);

        builder.HasIndex(x => x.ProjektId);

        builder.HasIndex(x => new
        {
            x.ProjektId,
            x.Sha256
        });

        builder.HasIndex(x => x.ImportiertAm);
    }
}
