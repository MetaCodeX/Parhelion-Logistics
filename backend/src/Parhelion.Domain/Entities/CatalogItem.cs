using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Catálogo maestro de productos/SKUs.
/// Normaliza datos fijos que se repiten en ShipmentItems.
/// </summary>
public class CatalogItem : TenantEntity
{
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    /// <summary>Unidad de medida base: Pza, Kg, Lt, Caja</summary>
    public string BaseUom { get; set; } = "Pza";
    
    // ========== DIMENSIONES POR DEFECTO ==========
    
    public decimal DefaultWeightKg { get; set; }
    public decimal DefaultWidthCm { get; set; }
    public decimal DefaultHeightCm { get; set; }
    public decimal DefaultLengthCm { get; set; }
    
    /// <summary>Volumen calculado en metros cúbicos</summary>
    public decimal DefaultVolumeM3 => (DefaultWidthCm * DefaultHeightCm * DefaultLengthCm) / 1_000_000m;
    
    // ========== FLAGS DE MANEJO ESPECIAL ==========
    
    public bool RequiresRefrigeration { get; set; }
    public bool IsHazardous { get; set; }
    public bool IsFragile { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
}
