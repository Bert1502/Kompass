using Kompass.Domain.Economics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class KostenpositionConfiguration
    : IEntityTypeConfiguration<Kostenposition>
{
    public void Configure(EntityTypeBuilder<Kostenposition> builder)
    {
        builder.ToTable("Kostenpositionen");

        builder.HasKey(kostenposition => kostenposition.Id);

        builder.Property(kostenposition => kostenposition.Bezeichnung)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(kostenposition => kostenposition.Betrag)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(kostenposition => kostenposition.Kostenart)
            .HasConversion<int>()
            .IsRequired();
    }
}