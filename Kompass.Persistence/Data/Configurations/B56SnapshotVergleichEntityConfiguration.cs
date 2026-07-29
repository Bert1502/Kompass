using Kompass.Persistence.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class B56SnapshotVergleichEntityConfiguration
    : IEntityTypeConfiguration<B56SnapshotVergleichEntity>
{
    public void Configure(
        EntityTypeBuilder<B56SnapshotVergleichEntity> builder)
    {
        builder.ToTable("B56SnapshotVergleiche");

        builder.HasKey(x => x.VergleichId);

        builder.Property(x => x.VergleichJson)
            .IsRequired();

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.HatAenderungen)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.ProjektId,
                x.VorgaengerSnapshotId,
                x.NachfolgerSnapshotId
            })
            .IsUnique();
    }
}
