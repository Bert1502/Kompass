using Kompass.Domain.B56;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class BauteilcodeConfiguration
    : IEntityTypeConfiguration<Bauteilcode>
{
    public void Configure(EntityTypeBuilder<Bauteilcode> builder)
    {
        builder.ToTable("Bauteilcodes");

        builder.HasKey(bauteilcode => bauteilcode.Id);

        builder.Property(bauteilcode => bauteilcode.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bauteilcode => bauteilcode.Bezeichnung)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(bauteilcode => bauteilcode.Code)
            .IsUnique();
    }
}