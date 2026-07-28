using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class WirtschaftlichkeitsannahmenConfiguration
    : IEntityTypeConfiguration<Wirtschaftlichkeitsannahmen>
{
    public void Configure(EntityTypeBuilder<Wirtschaftlichkeitsannahmen> builder)
    {
        builder.ToTable("Wirtschaftlichkeitsannahmen");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.BetrachtungszeitraumJahre)
            .IsRequired();

        builder.Property(a => a.DiskontsatzProzent)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(a => a.InflationsrateProzent)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(a => a.Co2PreisProTonne)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(a => a.JaehrlicherCo2PreisanstiegProzent)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(a => a.WartungUndInstandhaltungProJahr)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.NutzungsdauerJahre)
            .IsRequired();

        builder.Property(a => a.RestwertProzent)
            .HasPrecision(10, 4)
            .IsRequired();

        // Verknüpfung zur Modernisierungsalternative über Shadow-Property
        builder.HasOne<Modernisierungsalternative>()
            .WithOne()
            .HasForeignKey<Wirtschaftlichkeitsannahmen>(
                "ModernisierungsalternativeId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property<Guid>("ModernisierungsalternativeId")
            .IsRequired();

        builder.HasMany(a => a.Energietraeger)
            .WithOne()
            .HasForeignKey("WirtschaftlichkeitsannahmenId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Energietraeger)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
