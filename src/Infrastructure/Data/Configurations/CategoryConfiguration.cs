using BaseApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseApp.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Amount)
            .HasPrecision(18, 2);
    }
}
