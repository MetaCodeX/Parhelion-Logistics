using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Infrastructure.Services.Core;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Core;

/// <summary>
/// Tests para ClientService.
/// </summary>
public class ClientServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public ClientServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new ClientService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new ClientService(uow);
        var request = new CreateClientRequest(
            CompanyName: "Acme Corp",
            TradeName: "Acme",
            ContactName: "John Doe",
            Email: "acme@corp.com",
            Phone: "555-1234",
            TaxId: "ACM123456789",
            LegalName: "Acme Corporation SA de CV",
            BillingAddress: "123 Main St",
            ShippingAddress: "456 Warehouse Ave",
            PreferredProductTypes: "General",
            Priority: "High",
            Notes: null
        );

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Acme Corp", result.Data!.CompanyName);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new ClientService(uow);
        
        // Create first client
        await service.CreateAsync(new CreateClientRequest(
            "First Client", "First", "Contact", "duplicate@test.com", "555-0000",
            "FC123", "First Legal", "First St", "First Shipping",
            "General", "Normal", null
        ));

        // Try to create with same email
        var result = await service.CreateAsync(new CreateClientRequest(
            "Second Client", "Second", "Contact", "duplicate@test.com", "555-0001",
            "SC123", "Second Legal", "Second St", "Second Shipping",
            "General", "Normal", null
        ));

        // Assert
        Assert.False(result.Success);
        Assert.Contains("existe", result.Message?.ToLower() ?? string.Empty);
    }

    [Fact]
    public async Task DeleteAsync_ExistingClient_ReturnsSuccess()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new ClientService(uow);
        var createResult = await service.CreateAsync(new CreateClientRequest(
            "Delete Me", "Delete", "Contact", "delete@test.com", "555-0003",
            "DEL123", "Delete Legal", "Delete St", "Delete Ship",
            "General", "Low", null
        ));

        // Act
        var result = await service.DeleteAsync(createResult.Data!.Id);

        // Assert
        Assert.True(result.Success);
    }
}
