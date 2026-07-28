using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class FoerderprogrammConfiguration
    : IEntityTypeConfiguration<Foerderprogramm>
{
    public void Configure(
        EntityTypeBuilder<Foerderprogramm> builder)
    {
        builder.ToTable("Foerderprogramme");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Programmkennung)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Version)
            .IsRequired();

        builder.Property(f => f.GueltigAb)
            .IsRequired();

        builder.Property(f => f.GueltigBis);

        builder.Property(f => f.Zielgruppe)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.Foerdergegenstand)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.TechnischeMindestanforderungen)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.Foerdersatz)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(f => f.Hoechstbetrag)
            .HasPrecision(18, 2);

        builder.Property(f => f.Kumulierbarkeit)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.Pflichtnachweise)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.Quellenstand)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasMany(f => f.Foerderquoten)
            .WithOne()
            .HasForeignKey("FoerderprogrammId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Foerderquoten)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Hoechstbetraege)
            .WithOne()
            .HasForeignKey("FoerderprogrammId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Hoechstbetraege)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Kumulierbarkeitsregeln)
            .WithOne()
            .HasForeignKey("FoerderprogrammId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Kumulierbarkeitsregeln)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Pflichtnachweisregeln)
            .WithOne()
            .HasForeignKey("FoerderprogrammId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Pflichtnachweisregeln)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Gueltigkeitsregeln)
            .WithOne()
            .HasForeignKey("FoerderprogrammId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Gueltigkeitsregeln)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(
                f => new
                {
                    f.Programmkennung,
                    f.Version
                })
            .IsUnique();

        builder.HasIndex(
            f => f.GueltigAb);
    }
}
