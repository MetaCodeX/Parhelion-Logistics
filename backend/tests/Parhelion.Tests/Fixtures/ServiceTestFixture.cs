using Microsoft.EntityFrameworkCore;
using Parhelion.Application.Interfaces;
using Parhelion.Infrastructure.Data;
using Parhelion.Infrastructure.Repositories;

namespace Parhelion.Tests.Fixtures;

/// <summary>
/// Fixture para tests de Services con UnitOfWork real.
/// </summary>
public class ServiceTestFixture : IDisposable
{
    /// <summary>
    /// Crea un UnitOfWork con base de datos en memoria.
    /// </summary>
    public (IUnitOfWork UnitOfWork, ParhelionDbContext Context) CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ParhelionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
            
        var context = new ParhelionDbContext(options);
        context.Database.EnsureCreated();
        var unitOfWork = new UnitOfWork(context);
        return (unitOfWork, context);
    }

    /// <summary>
    /// Crea un UnitOfWork con datos de prueba sembrados.
    /// </summary>
    public (IUnitOfWork UnitOfWork, ParhelionDbContext Context, TestIds Ids) CreateSeededUnitOfWork()
    {
        var (unitOfWork, context) = CreateUnitOfWork();
        var ids = SeedTestData(context);
        return (unitOfWork, context, ids);
    }

    private TestIds SeedTestData(ParhelionDbContext context)
    {
        var ids = new TestIds();

        // Roles
        var adminRole = new Domain.Entities.Role 
        { 
            Id = ids.AdminRoleId, 
            Name = "Admin", 
            Description = "Administrador",
            CreatedAt = DateTime.UtcNow
        };
        var driverRole = new Domain.Entities.Role 
        { 
            Id = ids.DriverRoleId, 
            Name = "Driver", 
            Description = "Chofer",
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.AddRange(adminRole, driverRole);

        // Tenant
        var tenant = new Domain.Entities.Tenant
        {
            Id = ids.TenantId,
            CompanyName = "Test Company",
            ContactEmail = "test@company.com",
            FleetSize = 5,
            DriverCount = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);

        // User
        var user = new Domain.Entities.User
        {
            Id = ids.UserId,
            TenantId = ids.TenantId,
            Email = "admin@test.com",
            PasswordHash = "hashedpassword",
            FullName = "Test Admin",
            RoleId = ids.AdminRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);

        // Location
        var location = new Domain.Entities.Location
        {
            Id = ids.LocationId,
            TenantId = ids.TenantId,
            Code = "MTY",
            Name = "Monterrey Hub",
            Type = Domain.Enums.LocationType.RegionalHub,
            FullAddress = "123 Test Ave",
            CanReceive = true,
            CanDispatch = true,
            IsInternal = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Locations.Add(location);

        // Employee
        var employee = new Domain.Entities.Employee
        {
            Id = ids.EmployeeId,
            TenantId = ids.TenantId,
            UserId = ids.UserId,
            Phone = "555-0100",
            Rfc = "XAXX010101000",
            Curp = "XEXX010101HDFABC00",
            HireDate = DateTime.UtcNow.AddYears(-1),
            Department = "OPS",
            CreatedAt = DateTime.UtcNow
        };
        context.Employees.Add(employee);

        // Truck
        var truck = new Domain.Entities.Truck
        {
            Id = ids.TruckId,
            TenantId = ids.TenantId,
            Plate = "ABC-123",
            Model = "Freightliner",
            Type = Domain.Enums.TruckType.DryBox,
            MaxCapacityKg = 10000,
            MaxVolumeM3 = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Trucks.Add(truck);

        context.SaveChanges();
        return ids;
    }

    public void Dispose() { }
}

/// <summary>
/// IDs conocidos para testing.
/// </summary>
public class TestIds
{
    public Guid TenantId { get; } = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public Guid UserId { get; } = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public Guid AdminRoleId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid DriverRoleId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public Guid LocationId { get; } = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public Guid EmployeeId { get; } = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public Guid TruckId { get; } = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
}
