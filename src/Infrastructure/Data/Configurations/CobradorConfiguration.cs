using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class CobradorConfiguration : IEntityTypeConfiguration<Cobrador>
{
    public void Configure(EntityTypeBuilder<Cobrador> builder)
    {
        builder.Property(c => c.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Apellido)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Telefono)
            .HasMaxLength(50);

        builder.Property(c => c.Celular)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Comision)
            .HasPrecision(5, 2);

        builder.Property(c => c.UsuarioId)
            .HasMaxLength(450);

        builder.HasIndex(c => c.UsuarioId)
            .IsUnique()
            .HasFilter("[UsuarioId] IS NOT NULL");

        builder.Ignore(c => c.NombreCompleto);
    }
}
