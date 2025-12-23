namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipos de transacciones de inventario para el Kardex.
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>Entrada de mercancía externa (de proveedor)</summary>
    Receipt = 0,
    
    /// <summary>Almacenamiento (de recepción a ubicación)</summary>
    PutAway = 1,
    
    /// <summary>Movimiento interno entre zonas</summary>
    InternalMove = 2,
    
    /// <summary>Surtido para envío (picking)</summary>
    Picking = 3,
    
    /// <summary>Empaque para despacho</summary>
    Packing = 4,
    
    /// <summary>Salida del almacén</summary>
    Dispatch = 5,
    
    /// <summary>Ajuste de inventario (+/-)</summary>
    Adjustment = 6,
    
    /// <summary>Baja por daño/caducidad</summary>
    Scrap = 7,
    
    /// <summary>Devolución de cliente</summary>
    Return = 8
}
