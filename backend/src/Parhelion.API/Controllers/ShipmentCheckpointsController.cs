using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para checkpoints de envío (trazabilidad).
/// Los checkpoints son inmutables: solo se pueden crear, no modificar ni eliminar.
/// </summary>
[ApiController]
[Route("api/shipment-checkpoints")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class ShipmentCheckpointsController : ControllerBase
{
    private readonly IShipmentCheckpointService _checkpointService;

    public ShipmentCheckpointsController(IShipmentCheckpointService checkpointService)
    {
        _checkpointService = checkpointService;
    }

    /// <summary>
    /// Obtiene todos los checkpoints con paginación.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipmentCheckpointResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un checkpoint por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentCheckpointResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(new { error = "Checkpoint no encontrado" });
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los checkpoints de un envío (timeline completo).
    /// </summary>
    [HttpGet("by-shipment/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ShipmentCheckpointResponse>>> ByShipment(
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetByShipmentAsync(shipmentId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el timeline visual de un envío (formato Metro).
    /// </summary>
    [HttpGet("timeline/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<CheckpointTimelineItem>>> GetTimeline(
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetTimelineAsync(shipmentId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene checkpoints filtrados por código de estatus.
    /// </summary>
    [HttpGet("by-status/{shipmentId:guid}/{statusCode}")]
    public async Task<ActionResult<IEnumerable<ShipmentCheckpointResponse>>> ByStatus(
        Guid shipmentId,
        string statusCode,
        CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetByStatusCodeAsync(shipmentId, statusCode, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el último checkpoint de un envío.
    /// </summary>
    [HttpGet("last/{shipmentId:guid}")]
    public async Task<ActionResult<ShipmentCheckpointResponse>> GetLast(
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _checkpointService.GetLastCheckpointAsync(shipmentId, cancellationToken);
        if (result == null) return NotFound(new { error = "No hay checkpoints para este envío" });
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo checkpoint de trazabilidad.
    /// Los checkpoints son inmutables: una vez creados, no se pueden modificar.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShipmentCheckpointResponse>> Create(
        [FromBody] CreateShipmentCheckpointRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { error = "No se pudo determinar el usuario" });

        var result = await _checkpointService.CreateAsync(request, userId.Value, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    // No PUT/DELETE - checkpoints are immutable

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
