using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITAPI;

namespace IntegrationTest.WINITAPIController.Route;

/// <summary>
/// Integration tests for RouteController covering all CRUD operations, dropdown methods, and business logic scenarios.
/// Tests verify API endpoints, request/response handling, and error scenarios.
/// </summary>
public class RouteControllerIntegrationTests : BaseIntegrationTest
{
    private const string BASE_URL = "/api/Route";
    private readonly string _testUID = Guid.NewGuid().ToString();
    private readonly string _testOrgUID = Guid.NewGuid().ToString();

    public RouteControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region SelectAllRouteDetails Tests

    /// <summary>
    /// Tests successful retrieval of all route details with paging.
    /// </summary>
    [Fact]
    public async Task SelectAllRouteDetails_WithValidRequest_ReturnsOkWithPagedData()
    {
        // Arrange
        var pagingRequest = RouteTestDataHelper.CreateValidPagingRequest();
        var mockRouteList = new List<IRoute> { RouteTestDataHelper.CreateMockRoute(_testUID) };
        var pagedResponse = new PagedResponse<IRoute>
        {
            PagedData = mockRouteList,
            TotalCount = 1
        };

        _mockRouteBL.Setup(x => x.SelectRouteAllDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>(),
                _testOrgUID))
            .ReturnsAsync(pagedResponse);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllRouteDetails?OrgUID={_testOrgUID}", 
            CreateJsonContent(pagingRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.SelectRouteAllDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>(),
            _testOrgUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of all route details with invalid paging request.
    /// </summary>
    [Fact]
    public async Task SelectAllRouteDetails_WithInvalidPagingRequest_ReturnsBadRequest()
    {
        // Arrange
        var invalidPagingRequest = new PagingRequest
        {
            PageNumber = -1,
            PageSize = -1
        };

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllRouteDetails?OrgUID={_testOrgUID}", 
            CreateJsonContent(invalidPagingRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests retrieval of all route details with null paging request.
    /// </summary>
    [Fact]
    public async Task SelectAllRouteDetails_WithNullPagingRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectAllRouteDetails?OrgUID={_testOrgUID}", 
            CreateJsonContent(null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region SelectRouteChangeLogAllDetails Tests

    /// <summary>
    /// Tests successful retrieval of route change log details with paging.
    /// </summary>
    [Fact]
    public async Task SelectRouteChangeLogAllDetails_WithValidRequest_ReturnsOkWithPagedData()
    {
        // Arrange
        var pagingRequest = RouteTestDataHelper.CreateValidPagingRequest();
        var mockChangeLogList = new List<IRouteChangeLog> { RouteTestDataHelper.CreateMockRouteChangeLog(_testUID) };
        var pagedResponse = new PagedResponse<IRouteChangeLog>
        {
            PagedData = mockChangeLogList,
            TotalCount = 1
        };

        _mockRouteBL.Setup(x => x.SelectRouteChangeLogAllDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/SelectRouteChangeLogAllDetails", 
            CreateJsonContent(pagingRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.SelectRouteChangeLogAllDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()), Times.Once);
    }

    #endregion

    #region SelectRouteDetailByUID Tests

    /// <summary>
    /// Tests successful retrieval of route by UID.
    /// </summary>
    [Fact]
    public async Task SelectRouteDetailByUID_WithValidUID_ReturnsOkWithRoute()
    {
        // Arrange
        var expectedRoute = RouteTestDataHelper.CreateMockRoute(_testUID);

        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(_testUID))
            .ReturnsAsync(expectedRoute);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectRouteDetailByUID?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.SelectRouteDetailByUID(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of route by UID when route does not exist.
    /// </summary>
    [Fact]
    public async Task SelectRouteDetailByUID_WithNonExistentUID_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUID = Guid.NewGuid().ToString();
        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(nonExistentUID))
            .ReturnsAsync((IRoute)null);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectRouteDetailByUID?UID={nonExistentUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _mockRouteBL.Verify(x => x.SelectRouteDetailByUID(nonExistentUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of route by UID when business layer throws exception.
    /// </summary>
    [Fact]
    public async Task SelectRouteDetailByUID_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectRouteDetailByUID?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region CreateRouteDetails Tests

    /// <summary>
    /// Tests successful creation of route with valid data.
    /// </summary>
    [Fact]
    public async Task CreateRouteDetails_WithValidData_ReturnsOkWithCreatedId()
    {
        // Arrange
        var newRoute = RouteTestDataHelper.CreateConcreteRoute();
        var expectedId = 1;

        _mockRouteBL.Setup(x => x.CreateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateRouteDetails", CreateJsonContent(newRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.CreateRouteDetails(It.IsAny<IRoute>()), Times.Once);
    }

    /// <summary>
    /// Tests creation failure when business layer throws exception.
    /// </summary>
    [Fact]
    public async Task CreateRouteDetails_WhenCreationFails_ReturnsInternalServerError()
    {
        // Arrange
        var newRoute = RouteTestDataHelper.CreateConcreteRoute();

        _mockRouteBL.Setup(x => x.CreateRouteDetails(It.IsAny<IRoute>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateRouteDetails", CreateJsonContent(newRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        _mockRouteBL.Verify(x => x.CreateRouteDetails(It.IsAny<IRoute>()), Times.Once);
    }

    /// <summary>
    /// Tests creation when business layer returns 0 (failure).
    /// </summary>
    [Fact]
    public async Task CreateRouteDetails_WhenBusinessLayerReturnsZero_ReturnsInternalServerError()
    {
        // Arrange
        var newRoute = RouteTestDataHelper.CreateConcreteRoute();

        _mockRouteBL.Setup(x => x.CreateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(0);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateRouteDetails", CreateJsonContent(newRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region UpdateRouteDetails Tests

    /// <summary>
    /// Tests successful update of existing route.
    /// </summary>
    [Fact]
    public async Task UpdateRouteDetails_WithValidData_ReturnsOkWithUpdatedId()
    {
        // Arrange
        var existingRoute = RouteTestDataHelper.CreateMockRoute(_testUID);
        var updateRoute = RouteTestDataHelper.CreateConcreteRoute(_testUID);
        var expectedId = 1;

        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(_testUID))
            .ReturnsAsync(existingRoute);

        _mockRouteBL.Setup(x => x.UpdateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateRouteDetails", CreateJsonContent(updateRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.SelectRouteDetailByUID(_testUID), Times.Once);
        _mockRouteBL.Verify(x => x.UpdateRouteDetails(It.IsAny<IRoute>()), Times.Once);
    }

    /// <summary>
    /// Tests update failure when route doesn't exist.
    /// </summary>
    [Fact]
    public async Task UpdateRouteDetails_WhenRouteNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateRoute = RouteTestDataHelper.CreateConcreteRoute(_testUID);

        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(_testUID))
            .ReturnsAsync((IRoute)null);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateRouteDetails", CreateJsonContent(updateRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _mockRouteBL.Verify(x => x.SelectRouteDetailByUID(_testUID), Times.Once);
        _mockRouteBL.Verify(x => x.UpdateRouteDetails(It.IsAny<IRoute>()), Times.Never);
    }

    /// <summary>
    /// Tests update when business layer returns 0 (failure).
    /// </summary>
    [Fact]
    public async Task UpdateRouteDetails_WhenBusinessLayerReturnsZero_ReturnsInternalServerError()
    {
        // Arrange
        var existingRoute = RouteTestDataHelper.CreateMockRoute(_testUID);
        var updateRoute = RouteTestDataHelper.CreateConcreteRoute(_testUID);

        _mockRouteBL.Setup(x => x.SelectRouteDetailByUID(_testUID))
            .ReturnsAsync(existingRoute);

        _mockRouteBL.Setup(x => x.UpdateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(0);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateRouteDetails", CreateJsonContent(updateRoute));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region DeleteRouteDetail Tests

    /// <summary>
    /// Tests successful deletion of route by UID.
    /// </summary>
    [Fact]
    public async Task DeleteRouteDetail_WithValidUID_ReturnsOkWithDeletedId()
    {
        // Arrange
        var expectedId = 1;

        _mockRouteBL.Setup(x => x.DeleteRouteDetail(_testUID))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _client.DeleteAsync($"{BASE_URL}/DeleteRouteDetail?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.DeleteRouteDetail(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests deletion when business layer returns 0 (failure).
    /// </summary>
    [Fact]
    public async Task DeleteRouteDetail_WhenBusinessLayerReturnsZero_ReturnsInternalServerError()
    {
        // Arrange
        _mockRouteBL.Setup(x => x.DeleteRouteDetail(_testUID))
            .ReturnsAsync(0);

        // Act
        var response = await _client.DeleteAsync($"{BASE_URL}/DeleteRouteDetail?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region CreateRouteMaster Tests

    /// <summary>
    /// Tests successful creation of route master with valid data.
    /// </summary>
    [Fact]
    public async Task CreateRouteMaster_WithValidData_ReturnsOkWithCreatedId()
    {
        // Arrange
        var newRouteMaster = RouteTestDataHelper.CreateConcreteRouteMaster();
        var expectedId = 1;

        _mockRouteBL.Setup(x => x.CreateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateRouteMaster", CreateJsonContent(newRouteMaster));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.CreateRouteMaster(It.IsAny<RouteMaster>()), Times.Once);
    }

    /// <summary>
    /// Tests creation of route master when business layer returns 0 (failure).
    /// </summary>
    [Fact]
    public async Task CreateRouteMaster_WhenBusinessLayerReturnsZero_ReturnsInternalServerError()
    {
        // Arrange
        var newRouteMaster = RouteTestDataHelper.CreateConcreteRouteMaster();

        _mockRouteBL.Setup(x => x.CreateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(0);

        // Act
        var response = await _client.PostAsync($"{BASE_URL}/CreateRouteMaster", CreateJsonContent(newRouteMaster));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region UpdateRouteMaster Tests

    /// <summary>
    /// Tests successful update of route master.
    /// </summary>
    [Fact]
    public async Task UpdateRouteMaster_WithValidData_ReturnsOkWithUpdatedId()
    {
        // Arrange
        var updateRouteMaster = RouteTestDataHelper.CreateConcreteRouteMaster();
        var expectedId = 1;

        _mockRouteBL.Setup(x => x.UpdateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(expectedId);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateRouteMaster", CreateJsonContent(updateRouteMaster));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _mockRouteBL.Verify(x => x.UpdateRouteMaster(It.IsAny<RouteMaster>()), Times.Once);
    }

    /// <summary>
    /// Tests update of route master when business layer returns 0 (failure).
    /// </summary>
    [Fact]
    public async Task UpdateRouteMaster_WhenBusinessLayerReturnsZero_ReturnsInternalServerError()
    {
        // Arrange
        var updateRouteMaster = RouteTestDataHelper.CreateConcreteRouteMaster();

        _mockRouteBL.Setup(x => x.UpdateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(0);

        // Act
        var response = await _client.PutAsync($"{BASE_URL}/UpdateRouteMaster", CreateJsonContent(updateRouteMaster));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region SelectRouteMasterViewByUID Tests

    /// <summary>
    /// Tests successful retrieval of route master view by UID.
    /// </summary>
    [Fact]
    public async Task SelectRouteMasterViewByUID_WithValidUID_ReturnsOkWithMasterView()
    {
        // Arrange
        var expectedRouteMasterView = RouteTestDataHelper.CreateMockRouteMasterView(_testUID);
        _mockRouteBL.Setup(x => x.SelectRouteMasterViewByUID(_testUID))
            .ReturnsAsync(expectedRouteMasterView);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectRouteMasterViewByUID?UID={_testUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.SelectRouteMasterViewByUID(_testUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of route master view when it does not exist.
    /// </summary>
    [Fact]
    public async Task SelectRouteMasterViewByUID_WithNonExistentUID_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUID = Guid.NewGuid().ToString();
        _mockRouteBL.Setup(x => x.SelectRouteMasterViewByUID(nonExistentUID))
            .ReturnsAsync((IRouteMasterView)null);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/SelectRouteMasterViewByUID?UID={nonExistentUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Dropdown Tests

    /// <summary>
    /// Tests successful retrieval of vehicle dropdown data.
    /// </summary>
    [Fact]
    public async Task GetVehicleDDL_WithValidOrgUID_ReturnsOkWithDropdownData()
    {
        // Arrange
        var expectedDropdownData = RouteTestDataHelper.CreateMockSelectionItems();
        _mockRouteBL.Setup(x => x.GetVehicleDDL(_testOrgUID))
            .ReturnsAsync(expectedDropdownData);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetVehicleDDL?orgUID={_testOrgUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.GetVehicleDDL(_testOrgUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of vehicle dropdown when no data exists.
    /// </summary>
    [Fact]
    public async Task GetVehicleDDL_WhenNoDataExists_ReturnsNotFound()
    {
        // Arrange
        _mockRouteBL.Setup(x => x.GetVehicleDDL(_testOrgUID))
            .ReturnsAsync((List<ISelectionItem>)null);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetVehicleDDL?orgUID={_testOrgUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests successful retrieval of warehouse dropdown data.
    /// </summary>
    [Fact]
    public async Task GetWareHouseDDL_WithValidParameters_ReturnsOkWithDropdownData()
    {
        // Arrange
        var orgTypeUID = Guid.NewGuid().ToString();
        var parentUID = Guid.NewGuid().ToString();
        var expectedDropdownData = RouteTestDataHelper.CreateMockSelectionItems();
        
        _mockRouteBL.Setup(x => x.GetWareHouseDDL(orgTypeUID, parentUID))
            .ReturnsAsync(expectedDropdownData);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetWareHouseDDL?OrgTypeUID={orgTypeUID}&ParentUID={parentUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.GetWareHouseDDL(orgTypeUID, parentUID), Times.Once);
    }

    /// <summary>
    /// Tests successful retrieval of user dropdown data.
    /// </summary>
    [Fact]
    public async Task GetUserDDL_WithValidOrgUID_ReturnsOkWithDropdownData()
    {
        // Arrange
        var expectedDropdownData = RouteTestDataHelper.CreateMockSelectionItems();
        _mockRouteBL.Setup(x => x.GetUserDDL(_testOrgUID))
            .ReturnsAsync(expectedDropdownData);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetUserDDL?OrgUID={_testOrgUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.GetUserDDL(_testOrgUID), Times.Once);
    }

    /// <summary>
    /// Tests successful retrieval of routes by store UID.
    /// </summary>
    [Fact]
    public async Task GetRoutesByStoreUID_WithValidParameters_ReturnsOkWithRoutes()
    {
        // Arrange
        var storeUID = Guid.NewGuid().ToString();
        var expectedRoutes = new List<IRoute> { RouteTestDataHelper.CreateMockRoute(_testUID) };
        
        _mockRouteBL.Setup(x => x.GetRoutesByStoreUID(_testOrgUID, storeUID))
            .ReturnsAsync(expectedRoutes);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetRoutesByStoreUID?OrgUID={_testOrgUID}&StoreUID={storeUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.GetRoutesByStoreUID(_testOrgUID, storeUID), Times.Once);
    }

    /// <summary>
    /// Tests retrieval of routes by store UID with only OrgUID parameter.
    /// </summary>
    [Fact]
    public async Task GetRoutesByStoreUID_WithOnlyOrgUID_ReturnsOkWithRoutes()
    {
        // Arrange
        var expectedRoutes = new List<IRoute> { RouteTestDataHelper.CreateMockRoute(_testUID) };
        
        _mockRouteBL.Setup(x => x.GetRoutesByStoreUID(_testOrgUID, null))
            .ReturnsAsync(expectedRoutes);

        // Act
        var response = await _client.GetAsync($"{BASE_URL}/GetRoutesByStoreUID?OrgUID={_testOrgUID}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRouteBL.Verify(x => x.GetRoutesByStoreUID(_testOrgUID, null), Times.Once);
    }

    #endregion
} 