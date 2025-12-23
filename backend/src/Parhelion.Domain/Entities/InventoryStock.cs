using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Inventario físico cuantificado por zona y lote.
/// Representa el saldo actual de un producto en una ubicación específica.
/// </summary>
public class InventoryStock : TenantEntity
{
    public Guid ZoneId { get; set; }
    public Guid ProductId { get; set; }
    
    /// <summary>Cantidad total en esta ubicación</summary>
    public decimal Quantity { get; set; }
    
    /// <summary>Cantidad reservada para envíos pendientes</summary>
    public decimal QuantityReserved { get; set; }
    
    /// <summary>Cantidad disponible = Quantity - QuantityReserved</summary>
    public decimal QuantityAvailable => Quantity - QuantityReserved;
    
    /// <summary>Número de lote (vital para trazabilidad)</summary>
    public string? BatchNumber { get; set; }
    
    /// <summary>Fecha de caducidad (vital para perecederos)</summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>Última fecha de conteo físico</summary>
    public DateTime? LastCountDate { get; set; }
    
    /// <summary>Costo unitario para valuación de inventario</summary>
    public decimal? UnitCost { get; set; }
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public WarehouseZone Zone { get; set; } = null!;
    public CatalogItem Product { get; set; } = null!;
}
