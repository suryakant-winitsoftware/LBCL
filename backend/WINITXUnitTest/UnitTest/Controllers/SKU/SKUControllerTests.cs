using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;
using WINITServices.Interfaces.CacheHandler;
using WINITXUnitTest.UnitTest.Common.Fixtures;
using WINITXUnitTest.UnitTest.Common.TestBase;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.SKU;

public class SKUControllerTests : BaseTest
{
    private readonly Mock<ISKUBL> _mockSKUBL;
    private readonly Mock<ISortHelper> _mockSortHelper;
    private readonly Mock<ISKUPriceLadderingBL> _mockSKUPriceLadderingBL;
    private readonly Mock<ISKUClassGroupItemsBL> _mockSKUClassGroupItemsBL;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<Winit.Modules.Store.BL.Interfaces.IStoreBL> _mockStoreBL;
    private readonly Mock<Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL> _mockSKUPriceListBL;
    private readonly Mock<Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL> _mockSKUGroupTypeBL;
    private readonly Mock<Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL> _mockDropDownsBL;
    private readonly Mock<Winit.Modules.Promotion.BL.Interfaces.IPromotionBL> _mockPromotionBL;
    private readonly Mock<Winit.Modules.Location.BL.Interfaces.ILocationBL> _mockLocationBL;
    private readonly Mock<Winit.Modules.Setting.BL.Interfaces.ISettingBL> _mockSettingBL;
    private readonly Mock<IOptions<ApiSettings>> _mockApiSettings;
    private readonly DataPreparationController _dataPreparationController;
    private readonly SKUController _controller;

    public SKUControllerTests()
    {
        _mockSKUBL = new Mock<ISKUBL>();
        _mockSortHelper = new Mock<ISortHelper>();
        _mockSKUPriceLadderingBL = new Mock<ISKUPriceLadderingBL>();
        _mockSKUClassGroupItemsBL = new Mock<ISKUClassGroupItemsBL>();
        _mockCacheService = new Mock<ICacheService>();
        _mockStoreBL = new Mock<Winit.Modules.Store.BL.Interfaces.IStoreBL>();
        _mockSKUPriceListBL = new Mock<Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL>();
        _mockSKUGroupTypeBL = new Mock<Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL>();
        _mockDropDownsBL = new Mock<Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL>();
        _mockPromotionBL = new Mock<Winit.Modules.Promotion.BL.Interfaces.IPromotionBL>();
        _mockLocationBL = new Mock<Winit.Modules.Location.BL.Interfaces.ILocationBL>();
        _mockSettingBL = new Mock<Winit.Modules.Setting.BL.Interfaces.ISettingBL>();
        _mockApiSettings = new Mock<IOptions<ApiSettings>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockSKUBL.Object);
        services.AddSingleton(_mockSortHelper.Object);
        services.AddSingleton(_mockSKUPriceLadderingBL.Object);
        services.AddSingleton(_mockSKUClassGroupItemsBL.Object);
        services.AddSingleton(_mockStoreBL.Object);
        services.AddSingleton(_mockSKUPriceListBL.Object);
        services.AddSingleton(_mockSKUGroupTypeBL.Object);
        services.AddSingleton(_mockDropDownsBL.Object);
        services.AddSingleton(_mockPromotionBL.Object);
        services.AddSingleton(_mockLocationBL.Object);
        services.AddSingleton(_mockSettingBL.Object);
        services.AddSingleton(_mockApiSettings.Object);
        services.AddSingleton(_mockCacheService.Object);

        var serviceProvider = services.BuildServiceProvider();
        
        _dataPreparationController = new DataPreparationController(
            serviceProvider,
            _mockSKUBL.Object,
            _mockStoreBL.Object,
            _mockSKUPriceListBL.Object,
            _mockSKUGroupTypeBL.Object,
            _mockDropDownsBL.Object,
            _mockPromotionBL.Object,
            _mockLocationBL.Object,
            _mockApiSettings.Object,
            _mockSettingBL.Object,
            _mockSKUClassGroupItemsBL.Object
        );

        _controller = new SKUController(
            serviceProvider,
            _mockSKUBL.Object,
            _mockSortHelper.Object,
            _dataPreparationController,
            _mockSKUPriceLadderingBL.Object,
            _mockSKUClassGroupItemsBL.Object
        );
    }

    [Fact]
    public async Task SelectSKUByUID_WhenSKUExists_ReturnsOkResult()
    {
        // Arrange
        var expectedSku = SKUTestFixture.CreateSampleSKUV1();
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ReturnsAsync(expectedSku);

        // Act
        var result = await _controller.SelectSKUByUID("TEST-SKU-001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<ISKU>>(okResult.Value);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(expectedSku.UID, returnValue.Data.UID);
    }

    [Fact]
    public async Task SelectSKUByUID_WhenSKUDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ReturnsAsync((ISKU)null);

        // Act
        var result = await _controller.SelectSKUByUID("NON-EXISTENT-SKU");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateSKU_WhenCreationFails_ReturnsErrorResponse()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKUV1();
        _mockSKUBL.Setup(x => x.CreateSKU(It.IsAny<ISKUV1>()))
            .ReturnsAsync(0);

        // Act
        var result = await _controller.CreateSKU(sku);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("Create failed", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task CreateSKU_WhenExceptionOccurs_ReturnsErrorResponse()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKUV1();
        _mockSKUBL.Setup(x => x.CreateSKU(It.IsAny<ISKUV1>()))
            .ThrowsAsync(new Exception("Create failed"));

        // Act
        var result = await _controller.CreateSKU(sku);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("An error occurred while processing the request", returnValue.ErrorMessage);
        Assert.Contains("Create failed", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSKU_WhenSKUDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKU();
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ReturnsAsync((ISKU)null);

        // Act
        var result = await _controller.UpdateSKU((Winit.Modules.SKU.Model.Classes.SKU)sku);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSKU_WhenDeletionSucceeds_ReturnsOkResult()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.DeleteSKU(It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.DeleteSKU("TEST-SKU-001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<int>>(okResult.Value);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(1, returnValue.Data);
    }

    [Fact]
    public async Task DeleteSKU_WhenDeletionFails_ReturnsErrorResponse()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.DeleteSKU(It.IsAny<string>()))
            .ReturnsAsync(0);

        // Act
        var result = await _controller.DeleteSKU("TEST-SKU-001");

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("Delete failed", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSKU_WhenExceptionOccurs_ReturnsErrorResponse()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.DeleteSKU(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.DeleteSKU("TEST-SKU-001");

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("An error occurred while processing the request", returnValue.ErrorMessage);
        Assert.Contains("Database connection error", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task SelectAllSKUDetails_WithValidRequest_ReturnsPagedResponse()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };

        var expectedSkus = SKUTestFixture.CreateSampleSKUList();
        var pagedResponse = new PagedResponse<ISKU>
        {
            PagedData = expectedSkus,
            TotalCount = expectedSkus.Count
        };

        // Setup cache to return null (cache miss)
        _mockCacheService.Setup(x => x.HGet<ISKU>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ISKU)null);

        // Setup cache to return empty list for store linked items
        _mockCacheService.Setup(x => x.HGet<List<string>>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<string>());

        // Setup SKUBL to return the expected response
        _mockSKUBL.Setup(x => x.SelectAllSKUDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        var statusCodeResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, statusCodeResult.StatusCode);
        var returnValue = Assert.IsType<ApiResponse<PagedResponse<ISKU>>>(statusCodeResult.Value);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(expectedSkus.Count, returnValue.Data.TotalCount);
        Assert.Equal(expectedSkus.Count, returnValue.Data.PagedData.Count());
    }

    [Fact]
    public async Task SelectAllSKUDetails_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = -1,
            PageSize = -1
        };

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SelectSKUByUID_WhenExceptionOccurs_ReturnsErrorResponse()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.SelectSKUByUID("TEST-SKU-001");

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("An error occurred while processing the request", returnValue.ErrorMessage);
        Assert.Contains("Database error", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSKU_WhenExceptionOccurs_ReturnsErrorResponse()
    {
        // Arrange
        var sku = SKUTestFixture.CreateSampleSKU();
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
            .ReturnsAsync(sku);
        _mockSKUBL.Setup(x => x.UpdateSKU(It.IsAny<ISKU>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateSKU((Winit.Modules.SKU.Model.Classes.SKU)sku);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("An error occurred while processing the request", returnValue.ErrorMessage);
        Assert.Contains("Database error", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task SelectAllSKUDetails_WhenDataPreparationFails_ReturnsErrorResponse()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };

        _mockSKUBL.Setup(x => x.SelectAllSKUDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Data preparation failed"));

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
        Assert.False(returnValue.IsSuccess);
        Assert.Contains("An error occurred while processing the request", returnValue.ErrorMessage);
        Assert.Contains("Data preparation failed", returnValue.ErrorMessage);
    }

    [Fact]
    public async Task GetAllSKUMasterData_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new SKUMasterRequest
        {
            SKUUIDs = new List<string> { "TEST-SKU-001" }
        };

        var expectedMasterData = new List<ISKUMaster>
        {
            new SKUMaster
            {
                SKU = SKUTestFixture.CreateSampleSKU(),
                SKUAttributes = new List<ISKUAttributes>(),
                SKUUOMs = new List<ISKUUOM>(),
                ApplicableTaxUIDs = new List<string>(),
                SKUConfigs = new List<ISKUConfig>()
            }
        };

        _mockCacheService.Setup(x => x.HGet<ISKUMaster>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ISKUMaster)null);

        _mockSKUBL.Setup(x => x.PrepareSKUMaster(
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>()))
            .ReturnsAsync(expectedMasterData);

        // Act
        var result = await _controller.GetAllSkuMasterData(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<PagedResponse<ISKUMaster>>>(okResult.Value);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(expectedMasterData.Count, returnValue.Data.TotalCount);
    }

    [Fact]
    public async Task GetAllSKUMasterData_WithCacheHit_ReturnsCachedData()
    {
        // Arrange
        var request = new SKUMasterRequest
        {
            SKUUIDs = new List<string> { "TEST-SKU-001" }
        };

        var cachedData = new Dictionary<string, ISKUMaster>
        {
            {
                "TEST-SKU-001",
                new SKUMaster
                {
                    SKU = SKUTestFixture.CreateSampleSKU(),
                    SKUAttributes = new List<ISKUAttributes>(),
                    SKUUOMs = new List<ISKUUOM>(),
                    ApplicableTaxUIDs = new List<string>(),
                    SKUConfigs = new List<ISKUConfig>()
                }
            }
        };

        _mockCacheService.Setup(x => x.HGet<ISKUMaster>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(cachedData["TEST-SKU-001"]);

        // Act
        var result = await _controller.GetAllSkuMasterData(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<PagedResponse<ISKUMaster>>>(okResult.Value);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(1, returnValue.Data.TotalCount);
        Assert.Equal(1, returnValue.Data.PagedData.Count());
    }

    [Fact]
    public async Task GetAllSKUMasterData_WhenExceptionOccurs_Returns500()
    {
        // Arrange
        var request = new SKUMasterRequest
        {
            SKUUIDs = new List<string> { "TEST-SKU-001" }
        };

        _mockCacheService.Setup(x => x.HGet<ISKUMaster>(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Cache error"));

        // Act
        var result = await _controller.GetAllSkuMasterData(request);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task SelectAllSKUDetails_WhenCacheFails_Returns500()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };

        _mockCacheService.Setup(x => x.HGet<ISKU>(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Cache connection failed"));

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task SelectAllSKUDetails_WhenCacheReturnsNull_ReturnsDataFromDatabase()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };

        var expectedSkus = SKUTestFixture.CreateSampleSKUList();
        var pagedResponse = new PagedResponse<ISKU>
        {
            PagedData = expectedSkus,
            TotalCount = expectedSkus.Count
        };

        _mockCacheService.Setup(x => x.HGet<ISKU>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ISKU)null);

        _mockSKUBL.Setup(x => x.SelectAllSKUDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        var statusCodeResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, statusCodeResult.StatusCode);
        var returnValue = Assert.IsType<ApiResponse<PagedResponse<ISKU>>>(statusCodeResult);
        Assert.True(returnValue.IsSuccess);
        Assert.Equal(expectedSkus.Count, returnValue.Data.TotalCount);
        Assert.Equal(expectedSkus.Count, returnValue.Data.PagedData.Count());
    }

    [Fact]
    public async Task SelectAllSKUDetails_WhenDataPreparationFails_Returns500()
    {
        // Arrange
        var pagingRequest = new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };

        _mockCacheService.Setup(x => x.HGet<ISKU>(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ISKU)null);

        _mockSKUBL.Setup(x => x.SelectAllSKUDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Data preparation failed"));

        // Act
        var result = await _controller.SelectAllSKUDetails(pagingRequest);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
} 