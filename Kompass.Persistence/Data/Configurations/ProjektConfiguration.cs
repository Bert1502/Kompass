using Kompass.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class ProjektConfiguration : IEntityTypeConfiguration<Projekt>
{
    public void Configure(EntityTypeBuilder<Projekt> builder)
    {
        builder.ToTable("Projekte");

        builder.HasKey(projekt => projekt.Id);

        builder.Property(projekt => projekt.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(projekt => projekt.InterneBezeichnung)
            .HasMaxLength(Projekt.MaxInterneBezeichnungLaenge);

        builder.Property(projekt => projekt.Bearbeitungsstatus)
            .HasDefaultValue(Bearbeitungsstatus.InBearbeitung)
            .IsRequired();

        builder.Property(projekt => projekt.QuellSnapshotId);

        builder.Property(projekt => projekt.ProjektmodellVersion)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(projekt => projekt.Auftraggeber)
            .HasMaxLength(Projekt.MaxAuftraggeberLaenge);

        builder.Property(projekt => projekt.Ansprechpartner)
            .HasMaxLength(Projekt.MaxAnsprechpartnerLaenge);

        builder.Property(projekt => projekt.Strasse)
            .HasMaxLength(Projekt.MaxStrasseLaenge);

        builder.Property(projekt => projekt.Ort)
            .HasMaxLength(Projekt.MaxOrtLaenge);

        builder.Property(projekt => projekt.Postleitzahl)
            .HasMaxLength(Projekt.MaxPostleitzahlLaenge);

        builder.Property(projekt => projekt.Gebaeudeart)
            .HasMaxLength(Projekt.MaxGebaeudeartLaenge);

        builder.HasMany(projekt => projekt.Alternativen)
            .WithOne()
            .HasForeignKey("ProjektId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(projekt => projekt.Alternativen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
