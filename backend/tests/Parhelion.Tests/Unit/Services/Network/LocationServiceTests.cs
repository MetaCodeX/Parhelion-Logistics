using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Infrastructure.Services.Network;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Network;

/// <summary>
/// Tests para LocationService.
/// </summary>
public class LocationServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public LocationServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new LocationService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLocation_ReturnsLocation()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new LocationService(uow);

        // Act
        var result = await service.GetByIdAsync(ids.LocationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MTY", result.Code);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new LocationService(uow);
        var request = new CreateLocationRequest(
            Code: "GDL",
            Name: "Guadalajara Hub",
            Type: "RegionalHub",
            FullAddress: "789 Logistics Ave",
            Latitude: 20.6597m,
            Longitude: -103.3496m,
            CanReceive: true,
            CanDispatch: true,
            IsInternal: true
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("GDL", result.Data!.Code);
    }

    [Fact]
    public async Task ExistsAsync_ExistingLocation_ReturnsTrue()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new LocationService(uow);

        // Act
        var exists = await service.ExistsAsync(ids.LocationId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task SearchByNameAsync_MatchingTerm_ReturnsResults()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new LocationService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.SearchByNameAsync(ids.TenantId, "Monterrey", request);

        // Assert
        Assert.NotNull(result);
    }
}
