using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITAPI;

namespace IntegrationTest.WINITAPIController.SKU;

/// <summary>
/// Integration tests for SKUController covering all CRUD operations and business logic scenarios.
/// Tests verify API endpoints, request/response handling, and error scenarios.
/// </summary>
public class SKUControllerIntegrationTests : BaseIntegrationTest
{
    private const string BASE_URL = "/api/SKU";
    private readonly string _testUID = Guid.NewGuid().ToString();

    public SKUControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region SelectSKUByUID Tests

    /// <summary>
    /// Tests successful retrieval of SKU by UID.
    /// </summary>
    [Fact]
    public async Task SelectSKUByUID_WithValidUID_ReturnsOkWithSKU()
    {
        // Arrange
        var expectedSKU = SKUTestDataHelper.CreateMockSKU(_testUID);
        
        _mockSKUBL.Setup(x => x.SelectSKUByUID(_testUID))
                  .ReturnsAsync(expectedSKU);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectSKUByUID?uid={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Verify business layer was called
        _mockSKUBL.Verify(x => x.SelectSKUByUID(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of SKU by UID when SKU does not exist.
    /// </summary>
    [Fact]
    public async Task SelectSKUByUID_WithNonExistentUID_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUID = Guid.NewGuid().ToString();
        _mockSKUBL.Setup(x => x.SelectSKUByUID(nonExistentUID))
                  .ReturnsAsync((ISKU)null);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectSKUByUID?UID={nonExistentUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _mockSKUBL.Verify(x => x.SelectSKUByUID(nonExistentUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of SKU by UID when business layer throws exception.
    /// </summary>
    [Fact]
    public async Task SelectSKUByUID_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.SelectSKUByUID(It.IsAny<string>()))
                  .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectSKUByUID?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region CreateSKU Tests

    /// <summary>
    /// Tests successful creation of SKU with valid data.
    /// </summary>
    [Fact]
    public async Task CreateSKU_WithValidData_ReturnsOkWithCreatedId()
    {
        // Arrange
        var newSKU = SKUTestDataHelper.CreateConcreteSKUV1();
        var expectedId = 1;
        
        _mockSKUBL.Setup(x => x.CreateSKU(It.IsAny<ISKUV1>()))
                  .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateSKU", CreateJsonContent(newSKU));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        _mockSKUBL.Verify(x => x.CreateSKU(It.IsAny<ISKUV1>()), Times.Once);
    }

    /// <summary>
    /// Tests creation failure when business layer throws exception.
    /// </summary>
    [Fact]
    public async Task CreateSKU_WhenCreationFails_ReturnsInternalServerError()
    {
        // Arrange
        var newSKU = SKUTestDataHelper.CreateConcreteSKUV1();
        
        _mockSKUBL.Setup(x => x.CreateSKU(It.IsAny<ISKUV1>()))
                  .ThrowsAsync(new Exception("Database error"));

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateSKU", CreateJsonContent(newSKU));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        _mockSKUBL.Verify(x => x.CreateSKU(It.IsAny<ISKUV1>()), Times.Once);
    }

    /// <summary>
    /// Tests creation with null data returns bad request.
    /// </summary>
    [Fact]
    public async Task CreateSKU_WithNullData_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateSKU", CreateJsonContent((object)null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        _mockSKUBL.Verify(x => x.CreateSKU(It.IsAny<ISKUV1>()), Times.Never);
    }

    #endregion

    #region UpdateSKU Tests

    /// <summary>
    /// Tests successful update of existing SKU.
    /// </summary>
    [Fact]
    public async Task UpdateSKU_WithValidData_ReturnsOkWithUpdatedId()
    {
        // Arrange
        var existingSKU = SKUTestDataHelper.CreateMockSKU(_testUID);
        var updateSKU = SKUTestDataHelper.CreateConcreteSKU(_testUID);
        var expectedId = 1;

        _mockSKUBL.Setup(x => x.SelectSKUByUID(_testUID))
                  .ReturnsAsync(existingSKU);
        
        _mockSKUBL.Setup(x => x.UpdateSKU(It.IsAny<ISKU>()))
                  .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateSKU", CreateJsonContent(updateSKU));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        _mockSKUBL.Verify(x => x.SelectSKUByUID(_testUID), Times.Once);
        _mockSKUBL.Verify(x => x.UpdateSKU(It.IsAny<ISKU>()), Times.Once);
    }

    /// <summary>
    /// Tests update failure when SKU doesn't exist.
    /// </summary>
    [Fact]
    public async Task UpdateSKU_WhenSKUNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateSKU = SKUTestDataHelper.CreateConcreteSKU(_testUID);
        
        _mockSKUBL.Setup(x => x.SelectSKUByUID(_testUID))
                  .ReturnsAsync((ISKU)null);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateSKU", CreateJsonContent(updateSKU));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // Verify business layer was called for select but not update
        _mockSKUBL.Verify(x => x.SelectSKUByUID(_testUID), Times.Once);
        _mockSKUBL.Verify(x => x.UpdateSKU(It.IsAny<ISKU>()), Times.Never);
    }

    /// <summary>
    /// Tests update failure when business layer throws exception.
    /// </summary>
    [Fact]
    public async Task UpdateSKU_WhenUpdateFails_ReturnsInternalServerError()
    {
        // Arrange
        var existingSKU = SKUTestDataHelper.CreateMockSKU(_testUID);
        var updateSKU = SKUTestDataHelper.CreateConcreteSKU(_testUID);

        _mockSKUBL.Setup(x => x.SelectSKUByUID(_testUID))
                  .ReturnsAsync(existingSKU);
        
        _mockSKUBL.Setup(x => x.UpdateSKU(It.IsAny<ISKU>()))
                  .ThrowsAsync(new Exception("Update failed"));

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateSKU", CreateJsonContent(updateSKU));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        // Verify business layer was called
        _mockSKUBL.Verify(x => x.SelectSKUByUID(_testUID), Times.Once);
        _mockSKUBL.Verify(x => x.UpdateSKU(It.IsAny<ISKU>()), Times.Once);
    }

    #endregion

    #region DeleteSKU Tests

    /// <summary>
    /// Tests successful deletion of SKU by UID.
    /// </summary>
    [Fact]
    public async Task DeleteSKU_WithValidUID_ReturnsOkWithDeletedId()
    {
        // Arrange
        var expectedId = 1;

        _mockSKUBL.Setup(x => x.DeleteSKU(_testUID))
                  .ReturnsAsync(expectedId);

        // Act
        var response = await _client.DeleteAsync($"{BASE_URL}/DeleteSKU?uid={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Verify business layer was called
        _mockSKUBL.Verify(x => x.DeleteSKU(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests deletion of SKU when business layer delete fails.
    /// </summary>
    [Fact]
    public async Task DeleteSKU_WhenDeleteFails_ReturnsInternalServerError()
    {
        // Arrange
        _mockSKUBL.Setup(x => x.DeleteSKU(_testUID))
                  .ReturnsAsync(0); // Indicates failure

        // Act
        var response = await _client.DeleteAsync($"{BASE_URL}/DeleteSKU?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region SelectAllSKUDetails Tests

    /// <summary>
    /// Tests successful retrieval of all SKU details with paging.
    /// </summary>
    [Fact]
    public async Task SelectAllSKUDetails_WithValidRequest_ReturnsOkWithPagedData()
    {
        // Arrange
        var pagingRequest = SKUTestDataHelper.CreateValidPagingRequest();
        var mockSKUList = new List<ISKU> { SKUTestDataHelper.CreateMockSKU(_testUID) };
        
        // Setup cache service mocks to simulate the controller's caching behavior
        _mockCacheService.Setup(x => x.GetKeyByPattern(It.IsAny<string>()))
                         .Returns(new List<string> { "test-key" });
        
        _mockCacheService.Setup(x => x.GetMultiple<string>(It.IsAny<List<string>>()))
                         .Returns(new Dictionary<string, string> { { "test-key", _testUID } });
        
        _mockCacheService.Setup(x => x.HGet<ISKU>(It.IsAny<string>(), It.IsAny<List<string>>()))
                         .Returns(mockSKUList);

        // Setup sort helper mock
        _mockSortHelper.Setup(x => x.Sort(It.IsAny<List<ISKU>>(), It.IsAny<List<SortCriteria>>()))
                      .ReturnsAsync(mockSKUList);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllSKUDetails", CreateJsonContent(pagingRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Verify cache service was called (this is what the controller actually uses)
        _mockCacheService.Verify(x => x.GetKeyByPattern(It.IsAny<string>()), Times.AtLeastOnce);
        _mockSortHelper.Verify(x => x.Sort(It.IsAny<List<ISKU>>(), It.IsAny<List<SortCriteria>>()), Times.Once);
        
        // Note: The controller does NOT call _mockSKUBL.SelectAllSKUDetails, so we don't verify that
    }

    /// <summary>
    /// Tests retrieval of all SKU details with invalid paging request.
    /// </summary>
    [Fact]
    public async Task SelectAllSKUDetails_WithInvalidPagingRequest_ReturnsBadRequest()
    {
        // Arrange
        var invalidPagingRequest = new PagingRequest
        {
            PageNumber = -1,
            PageSize = -1
        };

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllSKUDetails", CreateJsonContent(invalidPagingRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests retrieval of all SKU details with null paging request.
    /// </summary>
    [Fact]
    public async Task SelectAllSKUDetails_WithNullPagingRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllSKUDetails", CreateJsonContent(null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region SelectSKUMasterByUID Tests

    /// <summary>
    /// Tests successful retrieval of SKU master data by UID.
    /// </summary>
    [Fact]
    public async Task SelectSKUMasterByUID_WithValidUID_ReturnsOkWithMasterData()
    {
        // Arrange
        var expectedSKUMaster = CreateMockSKUMaster(_testUID);
        _mockSKUBL.Setup(x => x.SelectSKUMasterByUID(_testUID))
                  .ReturnsAsync(expectedSKUMaster);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectSKUMasterByUID?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockSKUBL.Verify(x => x.SelectSKUMasterByUID(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of SKU master data when SKU master does not exist.
    /// </summary>
    [Fact]
    public async Task SelectSKUMasterByUID_WithNonExistentUID_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUID = Guid.NewGuid().ToString();
        _mockSKUBL.Setup(x => x.SelectSKUMasterByUID(nonExistentUID))
                  .ReturnsAsync((ISKUMaster)null);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectSKUMasterByUID?UID={nonExistentUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock ISKUMaster object for testing.
    /// </summary>
    /// <param name="uid">The UID for the SKU master</param>
    /// <returns>Mock ISKUMaster object</returns>
    private static ISKUMaster CreateMockSKUMaster(string uid)
    {
        var mockSKUMaster = new Mock<ISKUMaster>();
        mockSKUMaster.Setup(x => x.SKU).Returns(SKUTestDataHelper.CreateMockSKU(uid));
        mockSKUMaster.Setup(x => x.SKUAttributes).Returns(new List<ISKUAttributes>());
        mockSKUMaster.Setup(x => x.SKUUOMs).Returns(new List<ISKUUOM>());
        mockSKUMaster.Setup(x => x.ApplicableTaxUIDs).Returns(new List<string>());
        mockSKUMaster.Setup(x => x.SKUConfigs).Returns(new List<ISKUConfig>());
        mockSKUMaster.Setup(x => x.CustomSKUFields).Returns(new List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>());
        mockSKUMaster.Setup(x => x.FileSysList).Returns(new List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>());
        return mockSKUMaster.Object;
    }

    #endregion
} 