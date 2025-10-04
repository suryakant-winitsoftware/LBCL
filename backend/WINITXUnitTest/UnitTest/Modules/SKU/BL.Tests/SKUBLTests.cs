using Microsoft.Extensions.DependencyInjection;
using Moq;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.BL.Classes;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINITXUnitTest.UnitTest.Common.Fixtures;
using WINITXUnitTest.UnitTest.Common.TestBase;
using Xunit;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.CustomSKUField.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;

namespace WINITXUnitTest.UnitTest.Modules.SKU.BL.Tests;

public class SKUBLTests : BaseTest
{
    private readonly Mock<ISKUDL> _mockSkuDL;
    private readonly ISKUBL _skuBL;

    public SKUBLTests()
    {
        // Setup mock SKU data layer
        _mockSkuDL = new Mock<ISKUDL>();

        // Create service collection and register services
        var services = new ServiceCollection();
        services.AddScoped<ISKUBL, SKUBL>();
        services.AddScoped<ISKUDL>(sp => _mockSkuDL.Object);

        // Build service provider and get SKUBL instance
        var serviceProvider = services.BuildServiceProvider();
        _skuBL = serviceProvider.GetRequiredService<ISKUBL>();
    }

    [Fact]
    public async Task SelectAllSKUDetails_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedSkus = SKUTestFixture.CreateSampleSKUList();
        var pagedResponse = new PagedResponse<ISKU>
        {
            PagedData = expectedSkus,
            TotalCount = expectedSkus.Count
        };

        _mockSkuDL.Setup(x => x.SelectAllSKUDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()
        )).ReturnsAsync(pagedResponse);

        // Act
        var result = await _skuBL.SelectAllSKUDetails(
            CreateSortCriteria("Name"),
            1,
            10,
            CreateFilterCriteria("IsActive", "true"),
            true
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSkus.Count, result.TotalCount);
        Assert.Equal(expectedSkus.Count, result.PagedData.Count());
    }

    [Fact]
    public async Task SelectSKUByUID_ShouldReturnSKU()
    {
        // Arrange
        var expectedSku = SKUTestFixture.CreateSampleSKU();
        _mockSkuDL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ReturnsAsync(expectedSku);

        // Act
        var result = await _skuBL.SelectSKUByUID("TEST-SKU-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSku.UID, result.UID);
        Assert.Equal(expectedSku.Code, result.Code);
    }

    [Fact]
    public async Task CreateSKU_ShouldReturnSuccess()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKU() as ISKUV1;
        _mockSkuDL.Setup(x => x.CreateSKU(It.IsAny<ISKUV1>()))
            .ReturnsAsync(1);

        // Act
        var result = await _skuBL.CreateSKU(sku);

        // Assert
        Assert.Equal(1, result);
        _mockSkuDL.Verify(x => x.CreateSKU(It.Is<ISKUV1>(s => s.UID == sku.UID)), Times.Once);
    }

    [Fact]
    public async Task UpdateSKU_ShouldReturnSuccess()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKU();
        _mockSkuDL.Setup(x => x.UpdateSKU(It.IsAny<ISKU>()))
            .ReturnsAsync(1);

        // Act
        var result = await _skuBL.UpdateSKU(sku);

        // Assert
        Assert.Equal(1, result);
        _mockSkuDL.Verify(x => x.UpdateSKU(It.Is<ISKU>(s => s.UID == sku.UID)), Times.Once);
    }

    [Fact]
    public async Task DeleteSKU_ShouldReturnSuccess()
    {
        // Arrange
        _mockSkuDL.Setup(x => x.DeleteSKU(It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var result = await _skuBL.DeleteSKU("TEST-SKU-001");

        // Assert
        Assert.Equal(1, result);
        _mockSkuDL.Verify(x => x.DeleteSKU("TEST-SKU-001"), Times.Once);
    }

    [Fact]
    public async Task PrepareSKUMaster_ShouldReturnSKUMasterList()
    {
        // Arrange
        var skuMaster = SKUTestFixture.CreateSampleSKUMaster();
        _mockSkuDL.Setup(x => x.PrepareSKUMaster(
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>()
        )).ReturnsAsync((
            new List<ISKU> { skuMaster.SKU },
            skuMaster.SKUConfigs,
            skuMaster.SKUUOMs,
            skuMaster.SKUAttributes,
            new List<ITaxSkuMap>()
        ));

        // Act
        var result = await _skuBL.PrepareSKUMaster(
            new List<string> { "ORG-001" },
            new List<string> { "DIST-001" },
            new List<string> { "TEST-SKU-001" },
            new List<string> { "BRAND" }
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(skuMaster.SKU.UID, result[0].SKU.UID);
    }

    [Fact]
    public async Task SelectSKUMasterByUID_ShouldReturnSKUMaster()
    {
        // Arrange
        var skuMaster = SKUTestFixture.CreateSampleSKUMaster();
        _mockSkuDL.Setup(x => x.SelectSKUMasterByUID(It.IsAny<string>()))
            .ReturnsAsync((
                new List<ISKU> { skuMaster.SKU },
                skuMaster.SKUConfigs,
                skuMaster.SKUUOMs,
                skuMaster.SKUAttributes,
                new List<ICustomSKUFields>(),
                new List<IFileSys>()
            ));

        // Act
        var result = await _skuBL.SelectSKUMasterByUID("TEST-SKU-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(skuMaster.SKU.UID, result.SKU.UID);
        Assert.Single(result.SKUAttributes);
        Assert.Single(result.SKUUOMs);
        Assert.Single(result.SKUConfigs);
    }

    [Fact]
    public async Task SelectAllSKUDetailsWebView_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedSkus = new List<ISKUListView>
        {
            new SKUListView
            {
                SKUUID = "TEST-SKU-001",
                SKUCode = "SKU-001",
                SKULongName = "Test SKU Long Name",
                IsActive = true,
                BrandName = "Test Brand",
                CategoryName = "Test Category",
                BrandOwnershipName = "Test Brand Ownership",
                SubCategoryName = "Test Sub Category",
                Type = "Product",
                ProductCategoryId = "1"
            }
        };

        var pagedResponse = new PagedResponse<ISKUListView>
        {
            PagedData = expectedSkus,
            TotalCount = expectedSkus.Count
        };

        _mockSkuDL.Setup(x => x.SelectAllSKUDetailsWebView(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()
        )).ReturnsAsync(pagedResponse);

        // Act
        var result = await _skuBL.SelectAllSKUDetailsWebView(
            CreateSortCriteria("SKUCode"),
            1,
            10,
            CreateFilterCriteria("IsActive", "true"),
            true
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSkus.Count, result.TotalCount);
        Assert.Equal(expectedSkus.Count, result.PagedData.Count());
    }
} 