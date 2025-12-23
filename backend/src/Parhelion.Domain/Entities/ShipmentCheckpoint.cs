using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Evento de trazabilidad del envío.
/// INMUTABLE: Los checkpoints no se modifican, solo se agregan nuevos.
/// </summary>
public class ShipmentCheckpoint : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Guid? LocationId { get; set; }
    public CheckpointStatus StatusCode { get; set; }
    public string? Remarks { get; set; }
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Usuario que registró el checkpoint (siempre requerido).
    /// Sobrescribe el campo nullable de BaseEntity.
    /// </summary>
    public new Guid CreatedByUserId { get; set; }
    
    // ========== TRAZABILIDAD DE CARGUEROS ==========
    
    /// <summary>Chofer que manejó el paquete en este checkpoint</summary>
    public Guid? HandledByDriverId { get; set; }
    
    /// <summary>Camión donde se cargó el paquete</summary>
    public Guid? LoadedOntoTruckId { get; set; }
    
    /// <summary>Tipo de acción: Loaded, Unloaded, Transferred, Delivered, etc.</summary>
    public string? ActionType { get; set; }
    
    /// <summary>Nombre del custodio anterior (quien entregó)</summary>
    public string? PreviousCustodian { get; set; }
    
    /// <summary>Nombre del nuevo custodio (quien recibió)</summary>
    public string? NewCustodian { get; set; }
    
    /// <summary>Almacenista que manejó el paquete en este checkpoint</summary>
    public Guid? HandledByWarehouseOperatorId { get; set; }
    
    // ========== GEOLOCALIZACIÓN ==========
    
    /// <summary>Latitud donde se registró el checkpoint</summary>
    public decimal? Latitude { get; set; }
    
    /// <summary>Longitud donde se registró el checkpoint</summary>
    public decimal? Longitude { get; set; }

    // Navigation Properties
    public Shipment Shipment { get; set; } = null!;
    public Location? Location { get; set; }
    public User CreatedBy { get; set; } = null!;
    public Driver? HandledByDriver { get; set; }
    public WarehouseOperator? HandledByWarehouseOperator { get; set; }
    public Truck? LoadedOntoTruck { get; set; }
}
