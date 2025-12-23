using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Infrastructure.Services.Core;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Core;

/// <summary>
/// Tests para EmployeeService.
/// </summary>
public class EmployeeServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public EmployeeServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsEmployee()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);

        // Act
        var result = await service.GetByIdAsync(ids.EmployeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("XAXX010101000", result.Rfc);
    }

    [Fact]
    public async Task GetByRfcAsync_ExistingRfc_ReturnsEmployee()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);

        // Act
        var result = await service.GetByRfcAsync("XAXX010101000");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ids.EmployeeId, result.Id);
    }

    [Fact]
    public async Task GetByTenantAsync_ReturnsTenantEmployees()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetByTenantAsync(ids.TenantId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByDepartmentAsync_ValidDepartment_ReturnsEmployees()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetByDepartmentAsync(ids.TenantId, "OPS", request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingEmployee_ReturnsTrue()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new EmployeeService(uow);

        // Act
        var exists = await service.ExistsAsync(ids.EmployeeId);

        // Assert
        Assert.True(exists);
    }
}
