using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para metadata de documentos de envío.
/// Los PDFs se generan dinámicamente via /api/documents.
/// Este controller maneja solo el registro y metadata de documentos.
/// </summary>
[ApiController]
[Route("api/shipment-documents")]
[Authorize]
[Produces("application/json")]
public class ShipmentDocumentsController : ControllerBase
{
    private readonly IShipmentDocumentService _documentService;

    public ShipmentDocumentsController(IShipmentDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Obtiene todos los documentos con paginación.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipmentDocumentResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un documento por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentDocumentResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _documentService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(new { error = "Documento no encontrado" });
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los documentos de un envío.
    /// </summary>
    [HttpGet("by-shipment/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ShipmentDocumentResponse>>> ByShipment(
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentService.GetByShipmentAsync(shipmentId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene documentos filtrados por tipo.
    /// </summary>
    [HttpGet("by-type/{shipmentId:guid}/{documentType}")]
    public async Task<ActionResult<IEnumerable<ShipmentDocumentResponse>>> ByType(
        Guid shipmentId,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DocumentType>(documentType, out var docType))
            return BadRequest(new { error = "Tipo de documento inválido" });

        var result = await _documentService.GetByTypeAsync(shipmentId, docType, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra un documento (metadata).
    /// Para generar el PDF real, usar /api/documents/{type}/{entityId}.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<ShipmentDocumentResponse>> Create(
        [FromBody] CreateShipmentDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Captura firma digital para POD.
    /// </summary>
    [HttpPost("pod/{shipmentId:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<PodCaptureResponse>> CapturePod(
        Guid shipmentId,
        [FromBody] CapturePodRequest request,
        CancellationToken cancellationToken = default)
    {
        // Crear registro de documento POD con firma
        var createRequest = new CreateShipmentDocumentRequest(
            ShipmentId: shipmentId,
            DocumentType: DocumentType.POD.ToString(),
            FileUrl: $"/api/documents/pod/{shipmentId}", // Link al endpoint de generación
            GeneratedBy: "Driver",
            ExpiresAt: null
        );

        var result = await _documentService.CreateAsync(createRequest, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        // TODO: Actualizar documento con firma via service
        // Por ahora retornamos respuesta básica
        
        return Ok(new PodCaptureResponse(
            DocumentId: result.Data!.Id,
            ShipmentId: shipmentId,
            TrackingNumber: "", // Se obtiene del servicio
            SignedAt: DateTime.UtcNow,
            SignedByName: request.SignedByName,
            FileUrl: $"/api/documents/pod/{shipmentId}"
        ));
    }

    /// <summary>
    /// Elimina un documento (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _documentService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
