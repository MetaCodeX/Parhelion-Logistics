using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Infrastructure.Services.Fleet;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Fleet;

/// <summary>
/// Tests para TruckService.
/// </summary>
public class TruckServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public TruckServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTruck_ReturnsTruck()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);

        // Act
        var result = await service.GetByIdAsync(ids.TruckId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC-123", result.Plate);
    }

    [Fact]
    public async Task GetByPlateAsync_ExistingPlate_ReturnsTruck()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);

        // Act
        var result = await service.GetByPlateAsync("ABC-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ids.TruckId, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);
        var request = new CreateTruckRequest(
            Plate: "XYZ-789",
            Model: "Kenworth",
            Type: "DryBox",
            MaxCapacityKg: 15000,
            MaxVolumeM3: 60,
            Vin: "1HGBH41JXMN109186",
            EngineNumber: "ENG123",
            Year: 2023,
            Color: "White",
            InsurancePolicy: "POL-001",
            InsuranceExpiration: DateTime.UtcNow.AddYears(1),
            VerificationNumber: "VER-001",
            VerificationExpiration: DateTime.UtcNow.AddMonths(6)
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("XYZ-789", result.Data!.Plate);
    }

    [Fact]
    public async Task CreateAsync_DuplicatePlate_ReturnsFailure()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);
        var request = new CreateTruckRequest(
            Plate: "ABC-123", // Existing plate
            Model: "Duplicate",
            Type: "DryBox",
            MaxCapacityKg: 10000,
            MaxVolumeM3: 50,
            Vin: null, EngineNumber: null, Year: null, Color: null,
            InsurancePolicy: null, InsuranceExpiration: null,
            VerificationNumber: null, VerificationExpiration: null
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("ABC-123", result.Message);
    }

    [Fact]
    public async Task ExistsAsync_ExistingTruck_ReturnsTrue()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);

        // Act
        var exists = await service.ExistsAsync(ids.TruckId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task SetActiveStatusAsync_ChangesStatus()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TruckService(uow);

        // Act
        var result = await service.SetActiveStatusAsync(ids.TruckId, false);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Data!.IsActive);
    }
}
