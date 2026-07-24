using Kompass.Domain.B56.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class B56ImportZeileConfiguration
    : IEntityTypeConfiguration<B56ImportZeile>
{
    public void Configure(EntityTypeBuilder<B56ImportZeile> builder)
    {
        builder.ToTable("B56ImportZeilen");

        builder.HasKey(importZeile => importZeile.Id);

        builder.Property(importZeile => importZeile.Bauteilcode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(importZeile => importZeile.Bezeichnung)
            .HasMaxLength(500)
            .IsRequired();
    }
}