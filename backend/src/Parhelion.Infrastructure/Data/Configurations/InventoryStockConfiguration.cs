using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class InventoryStockConfiguration : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.QuantityReserved).HasPrecision(18, 4);
        builder.Property(i => i.UnitCost).HasPrecision(18, 4);
        builder.Property(i => i.BatchNumber).HasMaxLength(100);
        
        // Ignorar propiedad calculada
        builder.Ignore(i => i.QuantityAvailable);
        
        // PostgreSQL xmin concurrency token
        // Usa la columna del sistema xmin - NO crea columna física
        builder.Property(i => i.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
        
        // ========== ÍNDICES ==========
        
        // Índice único: Producto + Zona + Lote
        builder.HasIndex(i => new { i.ZoneId, i.ProductId, i.BatchNumber })
            .IsUnique();
        
        // Índice para búsqueda de stock por producto
        builder.HasIndex(i => new { i.TenantId, i.ProductId });
        
        // Índice para productos próximos a caducar
        builder.HasIndex(i => new { i.TenantId, i.ExpiryDate })
            .HasFilter("\"ExpiryDate\" IS NOT NULL");
        
        // ========== RELACIONES ==========
        
        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(i => i.Zone)
            .WithMany(z => z.InventoryStocks)
            .HasForeignKey(i => i.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(i => i.Product)
            .WithMany(p => p.InventoryStocks)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
