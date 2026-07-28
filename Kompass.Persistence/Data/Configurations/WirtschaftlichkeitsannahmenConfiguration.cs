using Kompass.Domain.Economics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class WirtschaftlichkeitsannahmenConfiguration
    : IEntityTypeConfiguration<Wirtschaftlichkeitsannahmen>
{
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
            .WithOne()
            .HasForeignKey("WirtschaftlichkeitsannahmenId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(annahmen => annahmen.EnergietraegerAnnahmen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
