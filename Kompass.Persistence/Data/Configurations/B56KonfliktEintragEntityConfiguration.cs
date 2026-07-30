using Kompass.Persistence.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class B56KonfliktEintragEntityConfiguration
    : IEntityTypeConfiguration<B56KonfliktEintragEntity>
{
    public void Configure(
        EntityTypeBuilder<B56KonfliktEintragEntity> builder)
    {
        builder.ToTable("B56KonfliktEintraege");

        builder.HasKey(x => x.KonfliktId);

        builder.Property(x => x.Bereich)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Schluessel)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Feld)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Aenderung)
            .IsRequired();

        builder.Property(x => x.Entscheidung)
            .IsRequired();

        builder.Property(x => x.AlterWert)
            .HasMaxLength(500);

        builder.Property(x => x.NeuerWert)
            .HasMaxLength(500);

        builder.HasIndex(x => new
        {
            x.ProjektId,
            x.VorgaengerSnapshotId,
            x.NachfolgerSnapshotId
        });

        builder.HasIndex(x => new
        {
            x.ProjektId,
            x.VorgaengerSnapshotId,
            x.NachfolgerSnapshotId,
            x.Bereich,
            x.Schluessel,
            x.Feld
        }).IsUnique();
    }
}
