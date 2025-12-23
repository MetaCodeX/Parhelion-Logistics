using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Bitácora de movimientos de inventario (Kardex).
/// Registra TODOS los movimientos: entradas, salidas, movimientos internos, ajustes.
/// INMUTABLE: Las transacciones no se modifican, solo se agregan.
/// </summary>
public class InventoryTransaction : TenantEntity
{
    public Guid ProductId { get; set; }
    
    /// <summary>Zona de origen (null si es entrada externa)</summary>
    public Guid? OriginZoneId { get; set; }
    
    /// <summary>Zona de destino (null si es salida)</summary>
    public Guid? DestinationZoneId { get; set; }
    
    public decimal Quantity { get; set; }
    
    public InventoryTransactionType TransactionType { get; set; }
    
    /// <summary>Usuario que ejecutó el movimiento</summary>
    public Guid PerformedByUserId { get; set; }
    
    /// <summary>Envío relacionado (si aplica)</summary>
    public Guid? ShipmentId { get; set; }
    
    /// <summary>Lote afectado</summary>
    public string? BatchNumber { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public CatalogItem Product { get; set; } = null!;
    public WarehouseZone? OriginZone { get; set; }
    public WarehouseZone? DestinationZone { get; set; }
    public User PerformedBy { get; set; } = null!;
    public Shipment? Shipment { get; set; }
}
