using Kompass.Domain.Waermebruecken;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class WaermebrueckeConfiguration
    : IEntityTypeConfiguration<Waermebruecke>
{
    public void Configure(
        EntityTypeBuilder<Waermebruecke> builder)
    {
        builder.ToTable("Waermebruecken");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ProjektId)
            .IsRequired();

        builder.Property(w => w.InterneNummer)
            .IsRequired()
            .HasMaxLength(Waermebruecke.MaxInterneNummerLaenge);

        builder.Property(w => w.Bezeichnung)
            .IsRequired()
            .HasMaxLength(Waermebruecke.MaxBezeichnungLaenge);

        builder.Property(w => w.Lage)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.Planreferenz)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.Detailreferenz)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.Fremdnummer)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.Laenge)
            .HasPrecision(10, 3);

        builder.Property(w => w.Typ)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasDefaultValue(WaermebrueckeStatus.Offen);

        builder.Property(w => w.GleichwertigkeitStatus)
            .IsRequired()
            .HasDefaultValue(GleichwertigkeitStatus.NichtBewertet);

        builder.Property(w => w.Beiblatt2Referenz)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.ThermCadReferenz)
            .HasMaxLength(Waermebruecke.MaxTextLaenge);

        builder.Property(w => w.PsiWert)
            .HasPrecision(10, 6);

        builder.Property(w => w.FRsi)
            .HasPrecision(10, 6);

        builder.Property(w => w.Pruefanmerkung)
            .HasMaxLength(Waermebruecke.MaxAnmerkungLaenge);

        builder.Property(w => w.Berichtsdarstellung)
            .HasMaxLength(Waermebruecke.MaxAnmerkungLaenge);

        builder.HasIndex(
                w => new
                {
                    w.ProjektId,
                    w.InterneNummer
                })
            .IsUnique();

        builder.HasIndex(w => w.ProjektId);
    }
}
