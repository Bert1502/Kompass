using Kompass.Domain.Economics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class EnergietraegerAnnahmeConfiguration
    : IEntityTypeConfiguration<EnergietraegerAnnahme>
{
    public void Configure(EntityTypeBuilder<EnergietraegerAnnahme> builder)
    {
        builder.ToTable("EnergietraegerAnnahmen");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Energietraeger)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.PreisProKwh)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(a => a.JaehrlicherPreisanstiegProzent)
            .HasPrecision(10, 4)
            .IsRequired();
    }
}
