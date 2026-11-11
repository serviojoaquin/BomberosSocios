using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class SocioConfiguration : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> builder)
    {
        builder.Property(s => s.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Domicilio)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(s => s.DomicilioDeCobro)
            .HasMaxLength(300);

        builder.Property(s => s.Telefono)
            .HasMaxLength(50);

        builder.Property(s => s.Celular)
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        builder.Property(s => s.Web)
            .HasMaxLength(200);

        builder.Property(s => s.Estado)
            .HasConversion<int>();

        builder.HasIndex(s => s.NumeroDocumento);

        builder.HasOne(s => s.Categoria)
            .WithMany(c => c.Socios)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Cobrador)
            .WithMany(c => c.Socios)
            .HasForeignKey(s => s.CobradorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
