using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class FoerderquoteRegelConfiguration
    : IEntityTypeConfiguration<FoerderquoteRegel>
{
    public void Configure(
        EntityTypeBuilder<FoerderquoteRegel> builder)
    {
        builder.ToTable("FoerderquoteRegeln");

        builder.HasKey(regel => regel.Id);

        builder.Property(regel => regel.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.Quote)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(regel => regel.Bezugsbasis)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(regel => regel.GueltigAb)
            .IsRequired();

        builder.Property(regel => regel.GueltigBis);

        builder.Property(regel => regel.Beschreibung)
            .HasMaxLength(1000);
    }
}
