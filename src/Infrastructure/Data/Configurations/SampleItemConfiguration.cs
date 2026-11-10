using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class SampleItemConfiguration : IEntityTypeConfiguration<SampleItem>
{
    public void Configure(EntityTypeBuilder<SampleItem> builder)
    {
        builder.Property(c => c.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Amount)
            .HasPrecision(18, 2);
    }
}
