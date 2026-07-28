using Kompass.Domain.Economics;
<<<<<<< HEAD
using Kompass.Domain.Projects;
=======
>>>>>>> origin/main
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class WirtschaftlichkeitsannahmenConfiguration
    : IEntityTypeConfiguration<Wirtschaftlichkeitsannahmen>
{
<<<<<<< HEAD
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
=======
    public void Configure(
        EntityTypeBuilder<Wirtschaftlichkeitsannahmen> builder)
    {
        builder.ToTable("Wirtschaftlichkeitsannahmen");

        builder.HasKey(annahmen => annahmen.Id);

        builder.Property(annahmen => annahmen.ModernisierungsalternativeId)
            .IsRequired();

        builder.Property(annahmen => annahmen.Basis)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(
                annahmen => new
                {
                    annahmen.ModernisierungsalternativeId,
                    annahmen.Basis
                })
            .IsUnique();

        builder.Property(annahmen => annahmen.Betrachtungszeitraum)
            .IsRequired();

        builder.Property(annahmen => annahmen.Diskontsatz)
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(annahmen => annahmen.Inflationsrate)
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(annahmen => annahmen.JaehrlicheWartungsmehrkosten)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(annahmen => annahmen.Nutzungsdauer)
            .IsRequired();

        builder.Property(annahmen => annahmen.Foerderung)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasMany(annahmen => annahmen.EnergietraegerAnnahmen)
>>>>>>> origin/main
            .WithOne()
            .HasForeignKey("WirtschaftlichkeitsannahmenId")
            .OnDelete(DeleteBehavior.Cascade);

<<<<<<< HEAD
        builder.Navigation(a => a.Energietraeger)
=======
        builder.Navigation(annahmen => annahmen.EnergietraegerAnnahmen)
>>>>>>> origin/main
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
