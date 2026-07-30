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

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Bereich)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Schluessel)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Feld)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProjektId,
            x.NachfolgerImportId
        });

        builder.HasIndex(x => new
        {
            x.ProjektId,
            x.VorgaengerImportId,
            x.NachfolgerImportId
        });
    }
}
