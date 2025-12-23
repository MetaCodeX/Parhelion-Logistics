using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.Interfaces;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para generación dinámica de documentos PDF.
/// Los PDFs se generan on-demand y se retornan como bytes para crear blob URL en cliente.
/// No hay almacenamiento permanente de archivos.
/// </summary>
[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IPdfGeneratorService _pdfGenerator;

    public DocumentsController(IPdfGeneratorService pdfGenerator)
    {
        _pdfGenerator = pdfGenerator;
    }

    /// <summary>
    /// Genera y retorna un PDF de Orden de Servicio.
    /// El cliente debe crear un blob URL local para visualizar.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf para crear blob URL.</returns>
    [HttpGet("service-order/{shipmentId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetServiceOrder(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfBytes = await _pdfGenerator.GenerateServiceOrderAsync(shipmentId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"OrdenServicio_{shipmentId:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera y retorna un PDF de Carta Porte (Waybill).
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf.</returns>
    [HttpGet("waybill/{shipmentId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetWaybill(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfBytes = await _pdfGenerator.GenerateWaybillAsync(shipmentId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"CartaPorte_{shipmentId:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera y retorna un PDF de Manifiesto de Carga.
    /// </summary>
    /// <param name="routeId">ID de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf.</returns>
    [HttpGet("manifest/{routeId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetManifest(Guid routeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfBytes = await _pdfGenerator.GenerateManifestAsync(routeId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Manifiesto_{routeId:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera y retorna un PDF de Hoja de Ruta para un chofer.
    /// </summary>
    /// <param name="driverId">ID del chofer.</param>
    /// <param name="date">Fecha de la ruta (formato: yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf.</returns>
    [HttpGet("trip-sheet/{driverId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetTripSheet(
        Guid driverId, 
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            var pdfBytes = await _pdfGenerator.GenerateTripSheetAsync(driverId, targetDate, cancellationToken);
            return File(pdfBytes, "application/pdf", $"HojaRuta_{driverId:N}_{targetDate:yyyyMMdd}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera y retorna un PDF de Proof of Delivery.
    /// Incluye firma digital si está disponible.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf.</returns>
    [HttpGet("pod/{shipmentId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetPod(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePodAsync(shipmentId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"POD_{shipmentId:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera un PDF según el tipo de documento.
    /// Endpoint genérico para flexibilidad.
    /// </summary>
    /// <param name="documentType">Tipo de documento (ServiceOrder, Waybill, Manifest, TripSheet, POD).</param>
    /// <param name="entityId">ID de la entidad.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>PDF como application/pdf.</returns>
    [HttpGet("{documentType}/{entityId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetDocument(
        string documentType, 
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DocumentType>(documentType, ignoreCase: true, out var docType))
        {
            return BadRequest(new { error = $"Tipo de documento inválido: {documentType}" });
        }

        try
        {
            var pdfBytes = await _pdfGenerator.GenerateAsync(docType, entityId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"{docType}_{entityId:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
