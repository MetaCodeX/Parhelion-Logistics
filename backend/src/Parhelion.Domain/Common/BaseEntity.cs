using System.ComponentModel.DataAnnotations;

namespace Parhelion.Domain.Common;

/// <summary>
/// Entidad base para todas las entidades del sistema.
/// Incluye Soft Delete, Audit Trail automático, y Concurrencia Optimista.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft Delete: true indica que la entidad fue eliminada lógicamente.
    /// </summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Concurrency token mapeado a PostgreSQL xmin.
    /// NO insertar/modificar manualmente - el DB lo maneja.
    /// </summary>
    [Timestamp]
    public uint RowVersion { get; set; }
    
    /// <summary>
    /// Usuario que creó el registro.
    /// Se llena automáticamente via AuditSaveChangesInterceptor.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
    
    /// <summary>
    /// Último usuario que modificó el registro.
    /// Se llena automáticamente via AuditSaveChangesInterceptor.
    /// </summary>
    public Guid? LastModifiedByUserId { get; set; }
}

/// <summary>
/// Entidad base para entidades que pertenecen a un tenant específico.
/// Hereda de BaseEntity para incluir Soft Delete, Audit Trail, y Concurrencia.
/// Todas las queries automáticamente filtran por TenantId via Query Filters.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}

