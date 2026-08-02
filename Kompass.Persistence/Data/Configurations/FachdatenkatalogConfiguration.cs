using Kompass.Domain.Fachdaten;
using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Massnahmen;
using Kompass.Domain.Materialien;
using Kompass.Domain.Projects;
using Kompass.Domain.Regelwerke;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class FachdatenquelleConfiguration : IEntityTypeConfiguration<Fachdatenquelle>
{
    public void Configure(EntityTypeBuilder<Fachdatenquelle> builder)
    {
        builder.ToTable("Fachdatenquellen");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FachlicheId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Quellenart).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Referenz).HasMaxLength(2048);
        builder.Property(x => x.PruefsummeSha256).HasMaxLength(64);
        builder.Property(x => x.Notizen).HasMaxLength(4000);
        builder.HasIndex(x => x.FachlicheId).IsUnique();
    }
}

public sealed class RegelwerkConfiguration : IEntityTypeConfiguration<Regelwerk>
{
    public void Configure(EntityTypeBuilder<Regelwerk> builder)
    {
        builder.ToTable("Regelwerke");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Titel).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Herausgeber).HasMaxLength(256);
        builder.Property(x => x.Fassung).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.Code, x.Version }).IsUnique();
        builder.HasOne<Fachdatenquelle>().WithMany().HasForeignKey(x => x.QuelleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Anforderungen).WithOne().HasForeignKey(x => x.RegelwerkId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Anforderungen).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RegelwerksanforderungConfiguration : IEntityTypeConfiguration<Regelwerksanforderung>
{
    public void Configure(EntityTypeBuilder<Regelwerksanforderung> builder)
    {
        builder.ToTable("Regelwerksanforderungen");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FachlicheId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Anforderungsart).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Bezeichnung).HasMaxLength(512).IsRequired();
        builder.Property(x => x.GebaeudekategorieCode).HasMaxLength(64);
        builder.Property(x => x.BauteiltypCode).HasMaxLength(64);
        builder.Property(x => x.RandbedingungCode).HasMaxLength(64);
        builder.Property(x => x.TemperaturkategorieCode).HasMaxLength(64);
        builder.Property(x => x.Vergleichsoperator).HasMaxLength(16);
        builder.Property(x => x.Einheit).HasMaxLength(64);
        builder.Property(x => x.Textwert).HasMaxLength(2048);
        builder.HasIndex(x => new { x.RegelwerkId, x.FachlicheId }).IsUnique();
    }
}

public sealed class MassnahmenkategorieConfiguration : IEntityTypeConfiguration<Massnahmenkategorie>
{
    public void Configure(EntityTypeBuilder<Massnahmenkategorie> builder)
    {
        builder.ToTable("Massnahmenkategorien"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired(); builder.Property(x => x.Bezeichnung).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class MassnahmenkatalogeintragConfiguration : IEntityTypeConfiguration<Massnahmenkatalogeintrag>
{
    public void Configure(EntityTypeBuilder<Massnahmenkatalogeintrag> builder)
    {
        builder.ToTable("Massnahmenkatalogeintraege"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired(); builder.Property(x => x.Bezeichnung).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Beschreibung).HasMaxLength(2000); builder.Property(x => x.Mengeneinheit).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired(); builder.HasIndex(x => new { x.Code, x.Version }).IsUnique();
        builder.HasOne<Massnahmenkategorie>().WithMany().HasForeignKey(x => x.KategorieId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Fachdatenquelle>().WithMany().HasForeignKey(x => x.QuelleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MaterialkategorieConfiguration : IEntityTypeConfiguration<Materialkategorie>
{
    public void Configure(EntityTypeBuilder<Materialkategorie> builder)
    {
        builder.ToTable("Materialkategorien"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired(); builder.Property(x => x.Bezeichnung).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materialien"); builder.HasKey(x => x.Id);
        builder.Property(x => x.FachlicheId).HasMaxLength(128).IsRequired(); builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Hersteller).HasMaxLength(256); builder.Property(x => x.Produktname).HasMaxLength(256); builder.Property(x => x.Produktkennung).HasMaxLength(128);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired(); builder.HasIndex(x => new { x.FachlicheId, x.Version }).IsUnique();
        builder.HasOne<Materialkategorie>().WithMany().HasForeignKey(x => x.KategorieId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Fachdatenquelle>().WithMany().HasForeignKey(x => x.QuelleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProjektmassnahmeConfiguration : IEntityTypeConfiguration<Projektmassnahme>
{
    public void Configure(EntityTypeBuilder<Projektmassnahme> builder)
    {
        builder.ToTable("Projektmassnahmen"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Bezeichnung).HasMaxLength(256).IsRequired(); builder.Property(x => x.Einheit).HasMaxLength(64); builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.HasOne<Projekt>().WithMany().HasForeignKey(x => x.ProjektId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Massnahmenkatalogeintrag>().WithMany().HasForeignKey(x => x.MassnahmenkatalogeintragId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Modernisierungsalternative>().WithMany().HasForeignKey(x => x.ModernisierungsalternativeId).OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class FoerdergeberConfiguration : IEntityTypeConfiguration<Foerdergeber>
{
    public void Configure(EntityTypeBuilder<Foerdergeber> builder)
    {
        builder.ToTable("Foerdergeber"); builder.HasKey(x => x.Id);
        builder.Property(x => x.FachlicheId).HasMaxLength(64).IsRequired(); builder.Property(x => x.Name).HasMaxLength(256).IsRequired(); builder.Property(x => x.Ebene).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.FachlicheId).IsUnique();
    }
}

public sealed class FoerdertatbestandConfiguration : IEntityTypeConfiguration<Foerdertatbestand>
{
    public void Configure(EntityTypeBuilder<Foerdertatbestand> builder)
    {
        builder.ToTable("Foerdertatbestaende"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(128).IsRequired(); builder.Property(x => x.Bezeichnung).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => new { x.FoerderprogrammId, x.Code }).IsUnique();
        builder.HasOne<Foerderprogramm>().WithMany().HasForeignKey(x => x.FoerderprogrammId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Massnahmenkatalogeintrag>().WithMany().HasForeignKey(x => x.MassnahmenkatalogeintragId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Regelwerksanforderung>().WithMany().HasForeignKey(x => x.RegelwerksanforderungId).OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class WirtschaftlicheZeitreiheConfiguration : IEntityTypeConfiguration<WirtschaftlicheZeitreihe>
{
    public void Configure(EntityTypeBuilder<WirtschaftlicheZeitreihe> builder)
    {
        builder.ToTable("WirtschaftlicheZeitreihen"); builder.HasKey(x => x.Id);
        builder.Property(x => x.FachlicheId).HasMaxLength(128).IsRequired(); builder.Property(x => x.Typ).HasMaxLength(64).IsRequired(); builder.Property(x => x.Bezeichnung).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EnergietraegerCode).HasMaxLength(64); builder.Property(x => x.Einheit).HasMaxLength(64).IsRequired(); builder.Property(x => x.Szenario).HasMaxLength(128).IsRequired(); builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.FachlicheId, x.Version }).IsUnique(); builder.HasOne<Fachdatenquelle>().WithMany().HasForeignKey(x => x.QuelleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Werte).WithOne().HasForeignKey(x => x.ZeitreiheId).OnDelete(DeleteBehavior.Cascade); builder.Navigation(x => x.Werte).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class WirtschaftlicherZeitwertConfiguration : IEntityTypeConfiguration<WirtschaftlicherZeitwert>
{
    public void Configure(EntityTypeBuilder<WirtschaftlicherZeitwert> builder)
    {
        builder.ToTable("WirtschaftlicheZeitwerte"); builder.HasKey(x => x.Id); builder.HasIndex(x => new { x.ZeitreiheId, x.Stichtag }).IsUnique();
    }
}
