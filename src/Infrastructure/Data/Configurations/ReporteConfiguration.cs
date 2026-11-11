using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class ReporteConfiguration : IEntityTypeConfiguration<Reporte>
{
    public void Configure(EntityTypeBuilder<Reporte> builder)
    {
        builder.Property(r => r.ReporteRaw)
            .IsRequired();

        builder.HasIndex(r => new { r.ReporteTipo, r.EntidadId, r.Ano });
    }
}
