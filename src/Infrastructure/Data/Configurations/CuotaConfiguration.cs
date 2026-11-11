using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class CuotaConfiguration : IEntityTypeConfiguration<Cuota>
{
    public void Configure(EntityTypeBuilder<Cuota> builder)
    {
        builder.Property(c => c.Importe)
            .HasPrecision(8, 2);

        builder.Property(c => c.Mes)
            .HasConversion<int>();

        builder.Property(c => c.Estado)
            .HasConversion<int>();

        builder.HasIndex(c => new { c.SocioId, c.Ano, c.Mes })
            .IsUnique();

        builder.HasIndex(c => new { c.Ano, c.Estado });

        builder.HasOne(c => c.Socio)
            .WithMany(s => s.Cuotas)
            .HasForeignKey(c => c.SocioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Cobrador)
            .WithMany(x => x.CuotasCobradas)
            .HasForeignKey(c => c.CobradorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.EstaPaga);
    }
}
