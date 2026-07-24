using Kompass.Domain.B56.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class B56ImportDateiConfiguration
    : IEntityTypeConfiguration<B56ImportDatei>
{
    public void Configure(EntityTypeBuilder<B56ImportDatei> builder)
    {
        builder.ToTable("B56ImportDateien");

        builder.HasKey(importDatei => importDatei.Id);

        builder.Property(importDatei => importDatei.Dateiname)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(importDatei => importDatei.ImportZeitpunkt)
            .IsRequired();

        builder.HasMany(importDatei => importDatei.Zeilen)
            .WithOne()
            .HasForeignKey("B56ImportDateiId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(importDatei => importDatei.Zeilen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}