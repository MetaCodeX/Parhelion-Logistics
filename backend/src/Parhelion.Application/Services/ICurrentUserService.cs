namespace Parhelion.Application.Services;

/// <summary>
/// Servicio para obtener información del usuario actual desde el contexto HTTP.
/// Se usa para auditoría automática de CreatedByUserId/LastModifiedByUserId.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>ID del usuario autenticado (null si anónimo)</summary>
    Guid? UserId { get; }
    
    /// <summary>ID del tenant del usuario actual</summary>
    Guid? TenantId { get; }
    
    /// <summary>Indica si hay un usuario autenticado</summary>
    bool IsAuthenticated { get; }
}
