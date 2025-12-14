using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Sku)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(c => c.Description)
            .HasMaxLength(1000);
            
        builder.Property(c => c.BaseUom)
            .HasMaxLength(20);
        
        // Dimensiones por defecto
        builder.Property(c => c.DefaultWeightKg).HasPrecision(10, 3);
        builder.Property(c => c.DefaultWidthCm).HasPrecision(10, 2);
        builder.Property(c => c.DefaultHeightCm).HasPrecision(10, 2);
        builder.Property(c => c.DefaultLengthCm).HasPrecision(10, 2);
        
        // Ignorar propiedad calculada
        builder.Ignore(c => c.DefaultVolumeM3);
        
        // Índice único: SKU por tenant
        builder.HasIndex(c => new { c.TenantId, c.Sku })
            .IsUnique();
        
        // Relaciones
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
