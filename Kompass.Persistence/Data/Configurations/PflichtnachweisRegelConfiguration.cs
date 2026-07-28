using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class PflichtnachweisRegelConfiguration
    : IEntityTypeConfiguration<PflichtnachweisRegel>
{
    public void Configure(
        EntityTypeBuilder<PflichtnachweisRegel> builder)
    {
        builder.ToTable("PflichtnachweisRegeln");

        builder.HasKey(regel => regel.Id);

        builder.Property(regel => regel.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.Beschreibung)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(regel => regel.Zeitpunkt)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(regel => regel.IstPflicht)
            .IsRequired();

        builder.Property(regel => regel.GueltigAb)
            .IsRequired();

        builder.Property(regel => regel.GueltigBis);
    }
}
