using Kompass.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class ModernisierungsalternativeConfiguration
    : IEntityTypeConfiguration<Modernisierungsalternative>
{
    public void Configure(
        EntityTypeBuilder<Modernisierungsalternative> builder)
    {
        builder.ToTable("Modernisierungsalternativen");

        builder.HasKey(alternative => alternative.Id);

        builder.Property(alternative => alternative.Bezeichnung)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(alternative => alternative.Kurztext)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(alternative => alternative.QuellSnapshotId);

        builder.Ignore(alternative => alternative.Gesamtkosten);

        builder.HasMany(alternative => alternative.Bauteile)
            .WithOne()
            .HasForeignKey("ModernisierungsalternativeId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(alternative => alternative.Bauteile)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(alternative => alternative.Kostenpositionen)
            .WithOne()
            .HasForeignKey("ModernisierungsalternativeId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(alternative => alternative.Kostenpositionen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
