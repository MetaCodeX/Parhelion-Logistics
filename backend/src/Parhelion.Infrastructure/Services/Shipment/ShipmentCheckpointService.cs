using Microsoft.Extensions.Logging;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.DTOs.Webhooks;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de ShipmentCheckpoints.
/// Incluye integración con webhooks para notificación de eventos.
/// </summary>
public class ShipmentCheckpointService : IShipmentCheckpointService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebhookPublisher _webhookPublisher;
    private readonly ILogger<ShipmentCheckpointService> _logger;

    // Labels en español para visualización
    private static readonly Dictionary<CheckpointStatus, string> StatusLabels = new()
    {
        { CheckpointStatus.Loaded, "Cargado en camión" },
        { CheckpointStatus.QrScanned, "QR escaneado" },
        { CheckpointStatus.ArrivedHub, "Llegó a Hub" },
        { CheckpointStatus.DepartedHub, "Salió de Hub" },
        { CheckpointStatus.OutForDelivery, "En camino" },
        { CheckpointStatus.DeliveryAttempt, "Intento de entrega" },
        { CheckpointStatus.Delivered, "Entregado" },
        { CheckpointStatus.Exception, "Excepción" }
    };

    public ShipmentCheckpointService(
        IUnitOfWork unitOfWork, 
        IWebhookPublisher webhookPublisher,
        ILogger<ShipmentCheckpointService> logger)
    {
        _unitOfWork = unitOfWork;
        _webhookPublisher = webhookPublisher;
        _logger = logger;
    }

    public async Task<PagedResult<ShipmentCheckpointResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.ShipmentCheckpoints.GetPagedAsync(request, filter: null, orderBy: q => q.OrderByDescending(c => c.Timestamp), cancellationToken);
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in items) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return PagedResult<ShipmentCheckpointResponse>.From(dtos, totalCount, request);
    }

    public async Task<ShipmentCheckpointResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentCheckpoints.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<IEnumerable<ShipmentCheckpointResponse>> GetByShipmentAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId, cancellationToken);
        var ordered = checkpoints.OrderBy(c => c.Timestamp).ToList();
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in ordered) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return dtos;
    }

    public async Task<OperationResult<ShipmentCheckpointResponse>> CreateAsync(CreateShipmentCheckpointRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null) return OperationResult<ShipmentCheckpointResponse>.Fail("Envío no encontrado");

        if (!Enum.TryParse<CheckpointStatus>(request.StatusCode, out var statusCode))
            return OperationResult<ShipmentCheckpointResponse>.Fail("Código de estatus inválido");

        var entity = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            LocationId = request.LocationId,
            StatusCode = statusCode,
            Remarks = request.Remarks,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            HandledByDriverId = request.HandledByDriverId,
            LoadedOntoTruckId = request.LoadedOntoTruckId,
            ActionType = request.ActionType,
            PreviousCustodian = request.PreviousCustodian,
            NewCustodian = request.NewCustodian,
            HandledByWarehouseOperatorId = request.HandledByWarehouseOperatorId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ShipmentCheckpoints.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publicar webhook checkpoint.created
        await PublishCheckpointCreatedWebhookAsync(entity, shipment, cancellationToken);

        return OperationResult<ShipmentCheckpointResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Checkpoint creado exitosamente");
    }

    public async Task<IEnumerable<ShipmentCheckpointResponse>> GetByStatusCodeAsync(Guid shipmentId, string statusCode, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CheckpointStatus>(statusCode, out var status)) return Enumerable.Empty<ShipmentCheckpointResponse>();
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId && c.StatusCode == status, cancellationToken);
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in checkpoints) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return dtos;
    }

    public async Task<ShipmentCheckpointResponse?> GetLastCheckpointAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId, cancellationToken);
        var last = checkpoints.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        return last != null ? await MapToResponseAsync(last, cancellationToken) : null;
    }

    public async Task<IEnumerable<CheckpointTimelineItem>> GetTimelineAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId, cancellationToken);
        var ordered = checkpoints.OrderBy(c => c.Timestamp).ToList();
        
        if (!ordered.Any()) return Enumerable.Empty<CheckpointTimelineItem>();

        var lastCheckpoint = ordered.Last();
        var timeline = new List<CheckpointTimelineItem>();

        foreach (var cp in ordered)
        {
            var location = cp.LocationId.HasValue 
                ? await _unitOfWork.Locations.GetByIdAsync(cp.LocationId.Value, cancellationToken) 
                : null;

            string? handlerName = null;
            if (cp.HandledByDriverId.HasValue)
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(cp.HandledByDriverId.Value, cancellationToken);
                if (driver != null)
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(driver.EmployeeId, cancellationToken);
                    if (employee != null)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken);
                        handlerName = user?.FullName;
                    }
                }
            }
            else if (cp.HandledByWarehouseOperatorId.HasValue)
            {
                var warehouseOp = await _unitOfWork.WarehouseOperators.GetByIdAsync(cp.HandledByWarehouseOperatorId.Value, cancellationToken);
                if (warehouseOp != null)
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(warehouseOp.EmployeeId, cancellationToken);
                    if (employee != null)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken);
                        handlerName = user?.FullName;
                    }
                }
            }

            timeline.Add(new CheckpointTimelineItem(
                cp.Id,
                cp.StatusCode.ToString(),
                StatusLabels.GetValueOrDefault(cp.StatusCode, cp.StatusCode.ToString()),
                location?.Name,
                location?.Code,
                cp.Timestamp,
                handlerName,
                cp.Remarks,
                cp.Id == lastCheckpoint.Id
            ));
        }

        return timeline;
    }

    private async Task PublishCheckpointCreatedWebhookAsync(ShipmentCheckpoint checkpoint, Domain.Entities.Shipment shipment, CancellationToken ct)
    {
        try
        {
            var location = checkpoint.LocationId.HasValue 
                ? await _unitOfWork.Locations.GetByIdAsync(checkpoint.LocationId.Value, ct) 
                : null;

            var webhookEvent = new CheckpointCreatedEvent(
                CheckpointId: checkpoint.Id,
                ShipmentId: shipment.Id,
                TrackingNumber: shipment.TrackingNumber,
                TenantId: shipment.TenantId,
                StatusCode: checkpoint.StatusCode.ToString(),
                LocationId: checkpoint.LocationId,
                LocationCode: location?.Code,
                Timestamp: checkpoint.Timestamp,
                HandledByDriverId: checkpoint.HandledByDriverId,
                HandledByWarehouseOperatorId: checkpoint.HandledByWarehouseOperatorId,
                Remarks: checkpoint.Remarks,
                WasQrScanned: checkpoint.StatusCode == CheckpointStatus.QrScanned
            );

            await _webhookPublisher.PublishAsync("checkpoint.created", webhookEvent, ct);
            _logger.LogInformation("Published checkpoint.created webhook for checkpoint {CheckpointId}", checkpoint.Id);
        }
        catch (Exception ex)
        {
            // Fire-and-forget: no interrumpir el flujo si falla el webhook
            _logger.LogWarning(ex, "Failed to publish checkpoint.created webhook for checkpoint {CheckpointId}", checkpoint.Id);
        }
    }

    private async Task<ShipmentCheckpointResponse> MapToResponseAsync(ShipmentCheckpoint e, CancellationToken ct)
    {
        var location = e.LocationId.HasValue ? await _unitOfWork.Locations.GetByIdAsync(e.LocationId.Value, ct) : null;
        var createdBy = await _unitOfWork.Users.GetByIdAsync(e.CreatedByUserId, ct);

        string? driverName = null;
        if (e.HandledByDriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(e.HandledByDriverId.Value, ct);
            if (driver != null)
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(driver.EmployeeId, ct);
                if (employee != null)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, ct);
                    driverName = user?.FullName;
                }
            }
        }

        var truck = e.LoadedOntoTruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.LoadedOntoTruckId.Value, ct) : null;

        return new ShipmentCheckpointResponse(e.Id, e.ShipmentId, e.LocationId, location?.Name, e.StatusCode.ToString(), e.Remarks,
            e.Timestamp, e.CreatedByUserId, createdBy?.FullName ?? "Unknown", e.HandledByDriverId, driverName, e.LoadedOntoTruckId,
            truck?.Plate, e.ActionType, e.PreviousCustodian, e.NewCustodian, e.Latitude, e.Longitude, e.CreatedAt);
    }
}

