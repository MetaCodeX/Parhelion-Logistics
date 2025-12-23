using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces;

/// <summary>
/// Servicio para generación dinámica de PDFs.
/// Los PDFs se generan en memoria cuando se solicitan, no se almacenan.
/// </summary>
public interface IPdfGeneratorService
{
    /// <summary>
    /// Genera un PDF de Orden de Servicio para un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerateServiceOrderAsync(Guid shipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un PDF de Carta Porte (Waybill) para un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerateWaybillAsync(Guid shipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un PDF de Manifiesto de Carga para una ruta.
    /// </summary>
    /// <param name="routeId">ID de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerateManifestAsync(Guid routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un PDF de Hoja de Ruta para un chofer en una fecha.
    /// </summary>
    /// <param name="driverId">ID del chofer.</param>
    /// <param name="date">Fecha de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerateTripSheetAsync(Guid driverId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un PDF de Proof of Delivery para un envío.
    /// Incluye firma digital si está disponible.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GeneratePodAsync(Guid shipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un PDF según el tipo de documento.
    /// </summary>
    /// <param name="documentType">Tipo de documento.</param>
    /// <param name="entityId">ID de la entidad (shipment, route, etc.).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerateAsync(DocumentType documentType, Guid entityId, CancellationToken cancellationToken = default);
}
