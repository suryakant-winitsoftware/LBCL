using Microsoft.Extensions.DependencyInjection;
using Moq;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.BL.Classes;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.Models.Common;
using WINITXUnitTest.UnitTest.Common.Fixtures;
using WINITXUnitTest.UnitTest.Common.TestBase;
using Xunit;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;

namespace WINITXUnitTest.UnitTest.Modules.Route.BL.Tests;

public class RouteBLTests : BaseTest
{
    private readonly Mock<IRouteDL> _mockRouteDL;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly IRouteBL _routeBL;

    public RouteBLTests()
    {
        // Setup mock Route data layer
        _mockRouteDL = new Mock<IRouteDL>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        // Create service collection and register services
        var services = new ServiceCollection();
        services.AddScoped<IRouteBL, RouteBL>();
        services.AddScoped<IRouteDL>(sp => _mockRouteDL.Object);
        services.AddSingleton<IServiceProvider>(sp => _mockServiceProvider.Object);

        // Build service provider and get RouteBL instance
        var serviceProvider = services.BuildServiceProvider();
        _routeBL = serviceProvider.GetRequiredService<IRouteBL>();
    }

    [Fact]
    public async Task SelectRouteAllDetails_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedRoutes = RouteTestFixture.CreateSampleRouteList();
        var pagedResponse = new PagedResponse<IRoute>
        {
            PagedData = expectedRoutes,
            TotalCount = expectedRoutes.Count
        };

        _mockRouteDL.Setup(x => x.SelectRouteAllDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>(),
            It.IsAny<string>()
        )).ReturnsAsync(pagedResponse);

        // Act
        var result = await _routeBL.SelectRouteAllDetails(
            CreateSortCriteria("Name"),
            1,
            10,
            CreateFilterCriteria("IsActive", "true"),
            true,
            "ORG-001"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRoutes.Count, result.TotalCount);
        Assert.Equal(expectedRoutes.Count, result.PagedData.Count());
    }

    [Fact]
    public async Task SelectRouteChangeLogAllDetails_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedChangeLogs = RouteTestFixture.CreateSampleRouteChangeLogList();
        var pagedResponse = new PagedResponse<IRouteChangeLog>
        {
            PagedData = expectedChangeLogs,
            TotalCount = expectedChangeLogs.Count
        };

        _mockRouteDL.Setup(x => x.SelectRouteChangeLogAllDetails(
            It.IsAny<List<SortCriteria>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<FilterCriteria>>(),
            It.IsAny<bool>()
        )).ReturnsAsync(pagedResponse);

        // Act
        var result = await _routeBL.SelectRouteChangeLogAllDetails(
            CreateSortCriteria("ModifiedDate"),
            1,
            10,
            CreateFilterCriteria("RouteUID", "ROUTE-001"),
            true
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChangeLogs.Count, result.TotalCount);
        Assert.Equal(expectedChangeLogs.Count, result.PagedData.Count());
    }

    [Fact]
    public async Task SelectRouteDetailByUID_ShouldReturnRoute()
    {
        // Arrange
        var expectedRoute = RouteTestFixture.CreateSampleRoute();
        _mockRouteDL.Setup(x => x.SelectRouteDetailByUID(It.IsAny<string>()))
            .ReturnsAsync(expectedRoute);

        // Act
        var result = await _routeBL.SelectRouteDetailByUID("ROUTE-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRoute.UID, result.UID);
        Assert.Equal(expectedRoute.Name, result.Name);
    }

    [Fact]
    public async Task CreateRouteDetails_ShouldReturnSuccess()
    {
        // Arrange
        var route = RouteTestFixture.CreateSampleRoute();
        _mockRouteDL.Setup(x => x.CreateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(1);

        // Act
        var result = await _routeBL.CreateRouteDetails(route);

        // Assert
        Assert.Equal(1, result);
        _mockRouteDL.Verify(x => x.CreateRouteDetails(It.Is<IRoute>(r => r.UID == route.UID)), Times.Once);
    }

    [Fact]
    public async Task UpdateRouteDetails_ShouldReturnSuccess()
    {
        // Arrange
        var route = RouteTestFixture.CreateSampleRoute();
        _mockRouteDL.Setup(x => x.UpdateRouteDetails(It.IsAny<IRoute>()))
            .ReturnsAsync(1);

        // Act
        var result = await _routeBL.UpdateRouteDetails(route);

        // Assert
        Assert.Equal(1, result);
        _mockRouteDL.Verify(x => x.UpdateRouteDetails(It.Is<IRoute>(r => r.UID == route.UID)), Times.Once);
    }

    [Fact]
    public async Task DeleteRouteDetail_ShouldReturnSuccess()
    {
        // Arrange
        _mockRouteDL.Setup(x => x.DeleteRouteDetail(It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var result = await _routeBL.DeleteRouteDetail("ROUTE-001");

        // Assert
        Assert.Equal(1, result);
        _mockRouteDL.Verify(x => x.DeleteRouteDetail("ROUTE-001"), Times.Once);
    }

    [Fact]
    public async Task CreateRouteMaster_ShouldReturnSuccess()
    {
        // Arrange
        var routeMaster = RouteTestFixture.CreateSampleRouteMaster();
        _mockRouteDL.Setup(x => x.CreateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(1);

        // Act
        var result = await _routeBL.CreateRouteMaster(routeMaster);

        // Assert
        Assert.Equal(1, result);
        _mockRouteDL.Verify(x => x.CreateRouteMaster(It.Is<RouteMaster>(r => r.Route.UID == routeMaster.Route.UID)), Times.Once);
    }

    [Fact]
    public async Task UpdateRouteMaster_ShouldReturnSuccess()
    {
        // Arrange
        var routeMaster = RouteTestFixture.CreateSampleRouteMaster();
        _mockRouteDL.Setup(x => x.UpdateRouteMaster(It.IsAny<RouteMaster>()))
            .ReturnsAsync(1);

        // Act
        var result = await _routeBL.UpdateRouteMaster(routeMaster);

        // Assert
        Assert.Equal(1, result);
        _mockRouteDL.Verify(x => x.UpdateRouteMaster(It.Is<RouteMaster>(r => r.Route.UID == routeMaster.Route.UID)), Times.Once);
    }

    [Fact]
    public async Task SelectRouteMasterViewByUID_ShouldReturnRouteMasterView()
    {
        // Arrange
        var routeMaster = RouteTestFixture.CreateSampleRouteMaster();
        var routeMasterView = new RouteMasterView();
        
        // Setup the mock to return the expected tuple structure
        _mockRouteDL.Setup(x => x.SelectRouteMasterViewByUID(It.IsAny<string>()))
            .ReturnsAsync((
                new List<IRouteChangeLog> { routeMaster.Route },
                new List<IRouteSchedule> { routeMaster.RouteSchedule },
                new List<IRouteScheduleDaywise> { routeMaster.RouteScheduleDaywise },
                new List<IRouteScheduleFortnight> { routeMaster.RouteScheduleFortnight },
                new List<IRouteCustomer> { routeMaster.RouteCustomersList[0] },
                new List<IRouteUser> { routeMaster.RouteUserList[0] }
            ));

        // Setup the service provider to return a concrete RouteMasterView
        _mockServiceProvider.Setup(x => x.GetRequiredService<IRouteMasterView>())
            .Returns(routeMasterView);

        // Act
        var result = await _routeBL.SelectRouteMasterViewByUID("ROUTE-001");

        // Assert
        Assert.NotNull(result);
        _mockRouteDL.Verify(x => x.SelectRouteMasterViewByUID("ROUTE-001"), Times.Once);
        _mockServiceProvider.Verify(x => x.GetRequiredService<IRouteMasterView>(), Times.Once);
    }

    [Fact]
    public async Task GetVehicleDDL_ShouldReturnSelectionItems()
    {
        // Arrange
        var expectedVehicles = RouteTestFixture.CreateSampleVehicleDDL();
        _mockRouteDL.Setup(x => x.GetVehicleDDL(It.IsAny<string>()))
            .ReturnsAsync(expectedVehicles);

        // Act
        var result = await _routeBL.GetVehicleDDL("ORG-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedVehicles.Count, result.Count);
    }

    [Fact]
    public async Task GetWareHouseDDL_ShouldReturnSelectionItems()
    {
        // Arrange
        var expectedWarehouses = RouteTestFixture.CreateSampleWarehouseDDL();
        _mockRouteDL.Setup(x => x.GetWareHouseDDL(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedWarehouses);

        // Act
        var result = await _routeBL.GetWareHouseDDL("ORG-TYPE-001", "PARENT-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWarehouses.Count, result.Count);
    }

    [Fact]
    public async Task GetUserDDL_ShouldReturnSelectionItems()
    {
        // Arrange
        var expectedUsers = RouteTestFixture.CreateSampleUserDDL();
        _mockRouteDL.Setup(x => x.GetUserDDL(It.IsAny<string>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _routeBL.GetUserDDL("ORG-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUsers.Count, result.Count);
    }

    [Fact]
    public async Task GetRoutesByStoreUID_ShouldReturnRoutes()
    {
        // Arrange
        var expectedRoutes = RouteTestFixture.CreateSampleRouteList();
        _mockRouteDL.Setup(x => x.GetRoutesByStoreUID(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedRoutes);

        // Act
        var result = await _routeBL.GetRoutesByStoreUID("ORG-001", "STORE-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRoutes.Count, result.Count);
    }
} 