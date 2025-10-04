using Microsoft.Extensions.DependencyInjection;
using Moq;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINITXUnitTest.UnitTest.Common.Fixtures;
using WINITXUnitTest.UnitTest.Common.TestBase;
using Xunit;
using Winit.Shared.Models.Enums;
using Winit.Modules.Store.Model.Classes;

namespace WINITXUnitTest.UnitTest.Modules.Store.BL.Tests;

public class StoreBLTests : BaseTest
{
    private readonly Mock<IStoreDL> _mockStoreDL;
    private readonly IStoreBL _storeBL;

    public StoreBLTests()
    {
        // Setup mock Store data layer
        _mockStoreDL = new Mock<IStoreDL>();

        // Create service collection and register services
        var services = new ServiceCollection();
        services.AddScoped<IStoreBL, StoreBL>();
        services.AddScoped<IStoreDL>(sp => _mockStoreDL.Object);

        // Build service provider and get StoreBL instance
        var serviceProvider = services.BuildServiceProvider();
        _storeBL = serviceProvider.GetRequiredService<IStoreBL>();
    }

    [Fact]
    public async Task SelectAllStoreDetails_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedStores = new List<IStore>
        {
            new Store
            {
                UID = "STORE-001",
                Code = "ST001",
                Name = "Test Store",
                IsActive = true,
                CreatedBy = "TestUser",
                CreatedTime = DateTime.Now,
                ModifiedBy = "TestUser",
                ModifiedTime = DateTime.Now,
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                Country = "Test Country",
                PostalCode = "12345",
                Phone = "1234567890",
                Email = "test@store.com",
                StoreType = "Retail"
            }
        };

        var pagedResponse = new PagedResponse<IStore>
        {
            PagedData = expectedStores,
            TotalCount = expectedStores.Count
        };

        _mockStoreDL.Setup(x => x.SelectAllStoreDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()
        )).ReturnsAsync(pagedResponse);

        // Act
        var result = await _storeBL.SelectAllStoreDetails(
            CreateSortCriteria("Name"),
            1,
            10,
            CreateFilterCriteria("IsActive", "true"),
            true
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStores.Count, result.TotalCount);
        Assert.Equal(expectedStores.Count, result.PagedData.Count());
    }

    [Fact]
    public async Task SelectStoreByUID_ShouldReturnStore()
    {
        // Arrange
        var expectedStore = new Store
        {
            UID = "STORE-001",
            Code = "ST001",
            Name = "Test Store",
            IsActive = true,
            CreatedBy = "TestUser",
            CreatedTime = DateTime.Now,
            ModifiedBy = "TestUser",
            ModifiedTime = DateTime.Now
        };

        _mockStoreDL.Setup(x => x.SelectStoreByUID(It.IsAny<string>()))
            .ReturnsAsync(expectedStore);

        // Act
        var result = await _storeBL.SelectStoreByUID("STORE-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStore.UID, result.UID);
        Assert.Equal(expectedStore.Code, result.Code);
    }

    [Fact]
    public async Task CreateStore_ShouldReturnSuccess()
    {
        // Arrange
        var store = new Store
        {
            UID = "STORE-001",
            Code = "ST001",
            Name = "Test Store",
            IsActive = true,
            CreatedBy = "TestUser",
            CreatedTime = DateTime.Now,
            ModifiedBy = "TestUser",
            ModifiedTime = DateTime.Now
        };

        _mockStoreDL.Setup(x => x.CreateStore(It.IsAny<IStore>()))
            .ReturnsAsync(1);

        // Act
        var result = await _storeBL.CreateStore(store);

        // Assert
        Assert.Equal(1, result);
        _mockStoreDL.Verify(x => x.CreateStore(It.Is<IStore>(s => s.UID == store.UID)), Times.Once);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnSuccess()
    {
        // Arrange
        var store = new Store
        {
            UID = "STORE-001",
            Code = "ST001",
            Name = "Test Store Updated",
            IsActive = true,
            ModifiedBy = "TestUser",
            ModifiedTime = DateTime.Now
        };

        _mockStoreDL.Setup(x => x.UpdateStore(It.IsAny<IStore>()))
            .ReturnsAsync(1);

        // Act
        var result = await _storeBL.UpdateStore(store);

        // Assert
        Assert.Equal(1, result);
        _mockStoreDL.Verify(x => x.UpdateStore(It.Is<IStore>(s => s.UID == store.UID)), Times.Once);
    }

    [Fact]
    public async Task DeleteStore_ShouldReturnSuccess()
    {
        // Arrange
        _mockStoreDL.Setup(x => x.DeleteStore(It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var result = await _storeBL.DeleteStore("STORE-001");

        // Assert
        Assert.Equal(1, result);
        _mockStoreDL.Verify(x => x.DeleteStore("STORE-001"), Times.Once);
    }
} 