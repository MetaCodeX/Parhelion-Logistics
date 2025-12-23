using Microsoft.Extensions.Logging;
using Parhelion.Application.Interfaces;
using Parhelion.Domain.Enums;
using System.Text;

namespace Parhelion.Infrastructure.Services.Documents;

/// <summary>
/// Implementación del servicio de generación de PDFs.
/// Genera PDFs dinámicamente en memoria usando plantillas HTML → PDF.
/// Para producción, se puede integrar con QuestPDF o similar.
/// </summary>
public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PdfGeneratorService> _logger;

    public PdfGeneratorService(IUnitOfWork unitOfWork, ILogger<PdfGeneratorService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(DocumentType documentType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return documentType switch
        {
            DocumentType.ServiceOrder => await GenerateServiceOrderAsync(entityId, cancellationToken),
            DocumentType.Waybill => await GenerateWaybillAsync(entityId, cancellationToken),
            DocumentType.Manifest => await GenerateManifestAsync(entityId, cancellationToken),
            DocumentType.TripSheet => await GenerateTripSheetAsync(entityId, DateTime.UtcNow.Date, cancellationToken),
            DocumentType.POD => await GeneratePodAsync(entityId, cancellationToken),
            _ => throw new NotSupportedException($"Tipo de documento no soportado: {documentType}")
        };
    }

    public async Task<byte[]> GenerateServiceOrderAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Envío no encontrado: {shipmentId}");

        var origin = await _unitOfWork.Locations.GetByIdAsync(shipment.OriginLocationId, cancellationToken);
        var destination = await _unitOfWork.Locations.GetByIdAsync(shipment.DestinationLocationId, cancellationToken);
        var items = await _unitOfWork.ShipmentItems.FindAsync(i => i.ShipmentId == shipmentId, cancellationToken);

        var html = GenerateServiceOrderHtml(shipment, origin, destination, items.ToList());
        _logger.LogInformation("Generated ServiceOrder PDF for shipment {ShipmentId}", shipmentId);
        
        return ConvertHtmlToPdf(html);
    }

    public async Task<byte[]> GenerateWaybillAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Envío no encontrado: {shipmentId}");

        var origin = await _unitOfWork.Locations.GetByIdAsync(shipment.OriginLocationId, cancellationToken);
        var destination = await _unitOfWork.Locations.GetByIdAsync(shipment.DestinationLocationId, cancellationToken);
        var items = await _unitOfWork.ShipmentItems.FindAsync(i => i.ShipmentId == shipmentId, cancellationToken);

        var html = GenerateWaybillHtml(shipment, origin, destination, items.ToList());
        _logger.LogInformation("Generated Waybill PDF for shipment {ShipmentId}", shipmentId);
        
        return ConvertHtmlToPdf(html);
    }

    public async Task<byte[]> GenerateManifestAsync(Guid routeId, CancellationToken cancellationToken = default)
    {
        var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(routeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruta no encontrada: {routeId}");

        // Obtener todos los envíos asignados a esta ruta
        var shipments = await _unitOfWork.Shipments.FindAsync(s => s.AssignedRouteId == routeId, cancellationToken);

        var html = GenerateManifestHtml(route, shipments.ToList());
        _logger.LogInformation("Generated Manifest PDF for route {RouteId}", routeId);
        
        return ConvertHtmlToPdf(html);
    }

    public async Task<byte[]> GenerateTripSheetAsync(Guid driverId, DateTime date, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken)
            ?? throw new KeyNotFoundException($"Chofer no encontrado: {driverId}");

        var employee = await _unitOfWork.Employees.GetByIdAsync(driver.EmployeeId, cancellationToken);
        var user = employee != null ? await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken) : null;

        // Obtener envíos del chofer para esa fecha
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);
        var shipments = await _unitOfWork.Shipments.FindAsync(
            s => s.DriverId == driverId && 
                 s.ScheduledDeparture >= startOfDay && 
                 s.ScheduledDeparture < endOfDay, 
            cancellationToken);

        var html = GenerateTripSheetHtml(driver, user?.FullName ?? "N/A", date, shipments.ToList());
        _logger.LogInformation("Generated TripSheet PDF for driver {DriverId} on {Date}", driverId, date);
        
        return ConvertHtmlToPdf(html);
    }

    public async Task<byte[]> GeneratePodAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Envío no encontrado: {shipmentId}");

        var origin = await _unitOfWork.Locations.GetByIdAsync(shipment.OriginLocationId, cancellationToken);
        var destination = await _unitOfWork.Locations.GetByIdAsync(shipment.DestinationLocationId, cancellationToken);
        
        // Buscar documento POD existente con firma
        var podDocs = await _unitOfWork.ShipmentDocuments.FindAsync(
            d => d.ShipmentId == shipmentId && d.DocumentType == DocumentType.POD, 
            cancellationToken);
        var podDoc = podDocs.OrderByDescending(d => d.SignedAt).FirstOrDefault();

        var html = GeneratePodHtml(shipment, origin, destination, podDoc);
        _logger.LogInformation("Generated POD PDF for shipment {ShipmentId}", shipmentId);
        
        return ConvertHtmlToPdf(html);
    }

    #region HTML Templates

    private string GenerateServiceOrderHtml(
        Domain.Entities.Shipment shipment,
        Domain.Entities.Location? origin,
        Domain.Entities.Location? destination,
        List<Domain.Entities.ShipmentItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;}h1{color:#1a365d;}table{width:100%;border-collapse:collapse;margin:20px 0;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#f4f4f4;}.header{display:flex;justify-content:space-between;}.logo{font-size:24px;font-weight:bold;color:#2563eb;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<div class='header'><div class='logo'>PARHELION LOGISTICS</div><div>Orden de Servicio</div></div>");
        sb.AppendLine($"<h2>Tracking: {shipment.TrackingNumber}</h2>");
        sb.AppendLine($"<p><strong>Fecha:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>");
        sb.AppendLine($"<p><strong>Origen:</strong> {origin?.Name ?? "N/A"} - {origin?.FullAddress ?? ""}</p>");
        sb.AppendLine($"<p><strong>Destino:</strong> {destination?.Name ?? "N/A"} - {destination?.FullAddress ?? ""}</p>");
        sb.AppendLine($"<p><strong>Destinatario:</strong> {shipment.RecipientName} - {shipment.RecipientPhone}</p>");
        sb.AppendLine($"<p><strong>Estado:</strong> {shipment.Status}</p>");
        sb.AppendLine("<h3>Items</h3><table><tr><th>Descripción</th><th>Cantidad</th><th>Peso (kg)</th><th>Volumen (m³)</th></tr>");
        foreach (var item in items)
        {
            sb.AppendLine($"<tr><td>{item.Description}</td><td>{item.Quantity}</td><td>{item.WeightKg:F2}</td><td>{item.VolumeM3:F3}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<p><strong>Total:</strong> {shipment.TotalWeightKg:F2} kg / {shipment.TotalVolumeM3:F3} m³</p>");
        sb.AppendLine($"<p><strong>Valor declarado:</strong> ${shipment.DeclaredValue:N2} MXN</p>");
        sb.AppendLine("<hr><p style='font-size:10px;color:#666;'>Documento generado automáticamente por Parhelion Logistics</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private string GenerateWaybillHtml(
        Domain.Entities.Shipment shipment,
        Domain.Entities.Location? origin,
        Domain.Entities.Location? destination,
        List<Domain.Entities.ShipmentItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;}h1{color:#1a365d;text-align:center;}table{width:100%;border-collapse:collapse;margin:20px 0;}th,td{border:1px solid #000;padding:8px;text-align:left;}th{background:#e5e7eb;}.box{border:2px solid #000;padding:15px;margin:10px 0;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>CARTA PORTE</h1>");
        sb.AppendLine("<h2 style='text-align:center;'>PARHELION LOGISTICS</h2>");
        sb.AppendLine($"<div class='box'><strong>Folio:</strong> {shipment.TrackingNumber}<br><strong>Fecha:</strong> {DateTime.UtcNow:yyyy-MM-dd}</div>");
        sb.AppendLine("<table><tr><th width='50%'>REMITENTE (Origen)</th><th width='50%'>DESTINATARIO</th></tr>");
        sb.AppendLine($"<tr><td>{origin?.Name ?? "N/A"}<br>{origin?.FullAddress ?? ""}</td><td>{shipment.RecipientName}<br>{destination?.Name ?? "N/A"}<br>{destination?.FullAddress ?? ""}<br>Tel: {shipment.RecipientPhone}</td></tr></table>");
        sb.AppendLine("<h3>Mercancías</h3><table><tr><th>Descripción</th><th>Cant.</th><th>Peso</th><th>Dimensiones</th><th>Valor</th></tr>");
        foreach (var item in items)
        {
            sb.AppendLine($"<tr><td>{item.Description}</td><td>{item.Quantity}</td><td>{item.WeightKg:F2} kg</td><td>{item.WidthCm}x{item.HeightCm}x{item.LengthCm} cm</td><td>${item.DeclaredValue:N2}</td></tr>");
        }
        sb.AppendLine($"<tr><td colspan='2'><strong>TOTALES</strong></td><td><strong>{shipment.TotalWeightKg:F2} kg</strong></td><td><strong>{shipment.TotalVolumeM3:F3} m³</strong></td><td><strong>${shipment.DeclaredValue:N2}</strong></td></tr></table>");
        sb.AppendLine("<div style='margin-top:50px;'><table><tr><td width='33%' style='height:80px;border-bottom:1px solid #000;'>Firma Remitente</td><td width='33%' style='border-bottom:1px solid #000;'>Firma Transportista</td><td width='33%' style='border-bottom:1px solid #000;'>Firma Destinatario</td></tr></table></div>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private string GenerateManifestHtml(
        Domain.Entities.RouteBlueprint route,
        List<Domain.Entities.Shipment> shipments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;}h1{color:#1a365d;}table{width:100%;border-collapse:collapse;margin:20px 0;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#1e40af;color:white;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>MANIFIESTO DE CARGA</h1>");
        sb.AppendLine("<h2>PARHELION LOGISTICS</h2>");
        sb.AppendLine($"<p><strong>Ruta:</strong> {route.Name}</p>");
        sb.AppendLine($"<p><strong>Fecha:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>");
        sb.AppendLine($"<p><strong>Total Envíos:</strong> {shipments.Count}</p>");
        sb.AppendLine("<table><tr><th>#</th><th>Tracking</th><th>Destinatario</th><th>Peso (kg)</th><th>Estado</th></tr>");
        var i = 1;
        foreach (var ship in shipments)
        {
            sb.AppendLine($"<tr><td>{i++}</td><td>{ship.TrackingNumber}</td><td>{ship.RecipientName}</td><td>{ship.TotalWeightKg:F2}</td><td>{ship.Status}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<p><strong>Peso Total:</strong> {shipments.Sum(s => s.TotalWeightKg):F2} kg</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private string GenerateTripSheetHtml(
        Domain.Entities.Driver driver,
        string driverName,
        DateTime date,
        List<Domain.Entities.Shipment> shipments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;}h1{color:#1a365d;}table{width:100%;border-collapse:collapse;margin:20px 0;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#059669;color:white;}.checkbox{width:20px;height:20px;border:1px solid #000;display:inline-block;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>HOJA DE RUTA</h1>");
        sb.AppendLine("<h2>PARHELION LOGISTICS</h2>");
        sb.AppendLine($"<p><strong>Chofer:</strong> {driverName}</p>");
        sb.AppendLine($"<p><strong>Licencia:</strong> {driver.LicenseNumber}</p>");
        sb.AppendLine($"<p><strong>Fecha:</strong> {date:yyyy-MM-dd}</p>");
        sb.AppendLine($"<p><strong>Entregas programadas:</strong> {shipments.Count}</p>");
        sb.AppendLine("<table><tr><th>#</th><th>Tracking</th><th>Destinatario</th><th>Dirección</th><th>✓</th></tr>");
        var i = 1;
        foreach (var ship in shipments)
        {
            sb.AppendLine($"<tr><td>{i++}</td><td>{ship.TrackingNumber}</td><td>{ship.RecipientName}</td><td>{ship.DeliveryInstructions ?? "Ver destino"}</td><td><div class='checkbox'></div></td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("<div style='margin-top:40px;'><p><strong>Firma del chofer:</strong> _________________________</p></div>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private string GeneratePodHtml(
        Domain.Entities.Shipment shipment,
        Domain.Entities.Location? origin,
        Domain.Entities.Location? destination,
        Domain.Entities.ShipmentDocument? podDoc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;}h1{color:#1a365d;text-align:center;}.box{border:2px solid #000;padding:15px;margin:10px 0;}.signature{border:1px solid #000;height:100px;margin:10px 0;text-align:center;padding-top:40px;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>PRUEBA DE ENTREGA (POD)</h1>");
        sb.AppendLine("<h2 style='text-align:center;'>PARHELION LOGISTICS</h2>");
        sb.AppendLine($"<div class='box'><strong>Tracking:</strong> {shipment.TrackingNumber}<br><strong>Entregado:</strong> {shipment.DeliveredAt?.ToString("yyyy-MM-dd HH:mm") ?? "Pendiente"}</div>");
        sb.AppendLine($"<p><strong>Origen:</strong> {origin?.Name ?? "N/A"}</p>");
        sb.AppendLine($"<p><strong>Destino:</strong> {destination?.Name ?? "N/A"} - {destination?.FullAddress ?? ""}</p>");
        sb.AppendLine($"<p><strong>Destinatario:</strong> {shipment.RecipientName}</p>");
        
        if (podDoc?.SignatureBase64 != null)
        {
            sb.AppendLine($"<p><strong>Firmado por:</strong> {podDoc.SignedByName ?? "N/A"}</p>");
            sb.AppendLine($"<p><strong>Fecha firma:</strong> {podDoc.SignedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}</p>");
            sb.AppendLine($"<div class='signature'><img src='data:image/png;base64,{podDoc.SignatureBase64}' style='max-height:90px;'/></div>");
        }
        else
        {
            sb.AppendLine("<div class='signature'>FIRMA DEL RECEPTOR</div>");
            sb.AppendLine("<p style='text-align:center;'>Nombre: _________________________</p>");
        }
        
        sb.AppendLine("<hr><p style='font-size:10px;color:#666;'>Documento generado automáticamente. Este comprobante certifica la entrega del envío.</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    #endregion

    /// <summary>
    /// Convierte HTML a PDF.
    /// NOTA: Para producción real, usar QuestPDF, iTextSharp, o Puppeteer.
    /// Esta implementación temporal retorna el HTML como bytes para pruebas.
    /// </summary>
    private byte[] ConvertHtmlToPdf(string html)
    {
        // TODO: Integrar con QuestPDF o Puppeteer para PDF real
        // Por ahora retornamos HTML como bytes (el navegador lo puede abrir)
        // En producción: return QuestPDF.GeneratePdf(html);
        
        // Crear un PDF básico con el HTML embebido
        // Usamos un formato simple que los navegadores pueden interpretar
        return Encoding.UTF8.GetBytes(html);
    }
}
