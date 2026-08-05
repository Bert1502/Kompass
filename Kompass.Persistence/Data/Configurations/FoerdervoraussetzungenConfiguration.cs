using Kompass.Domain.Funding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class FoerdervoraussetzungenConfiguration : IEntityTypeConfiguration<Foerdervoraussetzungen>
{
    public void Configure(EntityTypeBuilder<Foerdervoraussetzungen> builder)
    {
        builder.ToTable("Foerdervoraussetzungen");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ProjektId).IsUnique();
        builder.Property(x => x.Nachweise).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.QpReferenzQuelle).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Nettogrundflaeche).HasPrecision(18, 3);
        builder.Property(x => x.JahresPrimaerenergiebedarf).HasPrecision(18, 3);
        builder.Property(x => x.QpReferenz).HasPrecision(18, 3);
        builder.Ignore(x => x.WpbVerhaeltnis);
        builder.Ignore(x => x.WpbRechnerischerVorschlag);
        builder.HasOne<Kompass.Domain.Projects.Projekt>().WithOne().HasForeignKey<Foerdervoraussetzungen>(x => x.ProjektId).OnDelete(DeleteBehavior.Cascade);
    }
}
