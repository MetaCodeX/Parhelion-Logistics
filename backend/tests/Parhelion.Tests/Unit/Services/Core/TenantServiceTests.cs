using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Infrastructure.Services.Core;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Core;

/// <summary>
/// Tests para TenantService.
/// </summary>
public class TenantServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public TenantServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTenant_ReturnsTenant()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);

        // Act
        var result = await service.GetByIdAsync(ids.TenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Company", result.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTenant_ReturnsNull()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new TenantService(uow);
        var request = new CreateTenantRequest("New Tenant Inc", "new@tenant.com", 10, 5);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("New Tenant Inc", result.Data.CompanyName);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);
        var request = new CreateTenantRequest("Another Company", "test@company.com", 5, 3);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("existe", result.Message?.ToLower() ?? string.Empty);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTenant_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);
        var request = new UpdateTenantRequest("Updated Company", "updated@company.com", 20, 10, true);

        // Act
        var result = await service.UpdateAsync(ids.TenantId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Updated Company", result.Data!.CompanyName);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTenant_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new TenantService(uow);
        var createResult = await service.CreateAsync(new CreateTenantRequest("To Delete", "delete@test.com", 1, 1));

        // Act
        var result = await service.DeleteAsync(createResult.Data!.Id);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ExistsAsync_ExistingTenant_ReturnsTrue()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);

        // Act
        var exists = await service.ExistsAsync(ids.TenantId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsTenant()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);

        // Act
        var result = await service.GetByEmailAsync("test@company.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ids.TenantId, result.Id);
    }

    [Fact]
    public async Task SetActiveStatusAsync_TogglesState()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);

        // Act
        var result = await service.SetActiveStatusAsync(ids.TenantId, false);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActive()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new TenantService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetActiveAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.True(item.IsActive));
    }
}
