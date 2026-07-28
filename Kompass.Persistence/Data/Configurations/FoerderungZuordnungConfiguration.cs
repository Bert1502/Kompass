using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class FoerderungZuordnungConfiguration
    : IEntityTypeConfiguration<FoerderungZuordnung>
{
    public void Configure(
        EntityTypeBuilder<FoerderungZuordnung> builder)
    {
        builder.ToTable("FoerderungZuordnungen");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.ModernisierungsalternativeId)
            .IsRequired();

        builder.Property(z => z.FoerderprogrammId)
            .IsRequired();

        builder.HasIndex(
                z => new
                {
                    z.ModernisierungsalternativeId,
                    z.FoerderprogrammId
                })
            .IsUnique();

        builder.HasIndex(z => z.ModernisierungsalternativeId);
    }
}
