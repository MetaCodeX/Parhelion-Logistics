using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Documento B2B asociado a un envío.
/// Tipos: ServiceOrder, Waybill (Carta Porte), Manifest, TripSheet, POD.
/// </summary>
public class ShipmentDocument : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = null!;
    
    /// <summary>
    /// "System" para documentos automáticos, "User" para uploads manuales.
    /// </summary>
    public string GeneratedBy { get; set; } = null!;
    
    public DateTime GeneratedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // ========== FILE METADATA ==========
    
    /// <summary>
    /// Nombre original del archivo subido.
    /// </summary>
    public string? OriginalFileName { get; set; }
    
    /// <summary>
    /// Tipo MIME del archivo (application/pdf, image/png, etc.)
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// Tamaño del archivo en bytes.
    /// </summary>
    public long? FileSizeBytes { get; set; }
    
    // ========== POD (PROOF OF DELIVERY) FIELDS ==========
    
    /// <summary>
    /// Firma digital en formato Base64 (solo para DocumentType.POD).
    /// </summary>
    public string? SignatureBase64 { get; set; }
    
    /// <summary>
    /// Nombre de la persona que firmó la recepción.
    /// </summary>
    public string? SignedByName { get; set; }
    
    /// <summary>
    /// Fecha y hora de la firma.
    /// </summary>
    public DateTime? SignedAt { get; set; }
    
    /// <summary>
    /// Latitud GPS donde se capturó la firma.
    /// </summary>
    public decimal? SignatureLatitude { get; set; }
    
    /// <summary>
    /// Longitud GPS donde se capturó la firma.
    /// </summary>
    public decimal? SignatureLongitude { get; set; }

    // Navigation Properties
    public Shipment Shipment { get; set; } = null!;
}

