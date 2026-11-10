using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.Property(c => c.Descripcion)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(c => c.Descripcion)
            .IsUnique();

        foreach (var property in typeof(Categoria).GetProperties().Where(p => p.PropertyType == typeof(decimal)))
        {
            builder.Property(property.Name).HasPrecision(8, 2);
        }
    }
}
