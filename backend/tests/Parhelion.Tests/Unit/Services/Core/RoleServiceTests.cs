using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Infrastructure.Services.Core;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Core;

/// <summary>
/// Tests para RoleService.
/// </summary>
public class RoleServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public RoleServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2); // Admin and Driver roles
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsRole()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);

        // Act
        var result = await service.GetByIdAsync(ids.AdminRoleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new RoleService(uow);
        var request = new CreateRoleRequest("Manager", "Gerente de operaciones");

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Manager", result.Data!.Name);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);
        var request = new CreateRoleRequest("Admin", "Duplicate admin");

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetByNameAsync_ExistingName_ReturnsRole()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);

        // Act
        var result = await service.GetByNameAsync("Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ids.AdminRoleId, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ExistingRole_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);
        var request = new UpdateRoleRequest("Admin", "Updated Administrator description");

        // Act
        var result = await service.UpdateAsync(ids.AdminRoleId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Updated Administrator description", result.Data!.Description);
    }

    [Fact]
    public async Task DeleteAsync_ExistingRole_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new RoleService(uow);
        var createResult = await service.CreateAsync(new CreateRoleRequest("ToDelete", "To be deleted"));

        // Act
        var result = await service.DeleteAsync(createResult.Data!.Id);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ExistsAsync_ExistingRole_ReturnsTrue()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RoleService(uow);

        // Act
        var exists = await service.ExistsAsync(ids.AdminRoleId);

        // Assert
        Assert.True(exists);
    }
}
