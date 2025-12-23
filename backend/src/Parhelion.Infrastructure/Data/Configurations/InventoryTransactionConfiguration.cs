using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Quantity).HasPrecision(18, 4);
        builder.Property(t => t.BatchNumber).HasMaxLength(100);
        builder.Property(t => t.Remarks).HasMaxLength(500);
        
        // ========== ÍNDICES ==========
        
        // Kardex por producto
        builder.HasIndex(t => new { t.TenantId, t.ProductId, t.Timestamp });
        
        // Historial temporal
        builder.HasIndex(t => new { t.TenantId, t.Timestamp });
        
        // Transacciones por envío
        builder.HasIndex(t => t.ShipmentId)
            .HasFilter("\"ShipmentId\" IS NOT NULL");
        
        // ========== RELACIONES ==========
        
        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.OriginZone)
            .WithMany(z => z.OriginTransactions)
            .HasForeignKey(t => t.OriginZoneId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(t => t.DestinationZone)
            .WithMany(z => z.DestinationTransactions)
            .HasForeignKey(t => t.DestinationZoneId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(t => t.PerformedBy)
            .WithMany()
            .HasForeignKey(t => t.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.Shipment)
            .WithMany()
            .HasForeignKey(t => t.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
