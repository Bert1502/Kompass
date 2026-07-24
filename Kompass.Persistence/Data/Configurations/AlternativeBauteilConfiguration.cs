using Kompass.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kompass.Persistence.Data.Configurations;

public sealed class AlternativeBauteilConfiguration
    : IEntityTypeConfiguration<AlternativeBauteil>
{
    public void Configure(EntityTypeBuilder<AlternativeBauteil> builder)
    {
        builder.ToTable("AlternativeBauteile");

        builder.HasKey(alternativeBauteil => alternativeBauteil.Id);

        builder.Property(alternativeBauteil => alternativeBauteil.Bemerkung)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(alternativeBauteil => alternativeBauteil.Bauteilcode)
            .WithMany()
            .HasForeignKey("BauteilcodeId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(alternativeBauteil => alternativeBauteil.Bauteilcode)
            .IsRequired();
    }
}