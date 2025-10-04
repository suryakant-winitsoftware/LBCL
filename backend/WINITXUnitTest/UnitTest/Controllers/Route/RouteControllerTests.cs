using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers.Route;
using WINITAPI.Controllers.Route.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Route
{
    public class RouteControllerTests
    {
        private readonly Mock<IRouteBL> _routeBLMock;
        private readonly Mock<IDBService> _dbServiceMock;
        private readonly Mock<ILogger<RouteController>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly RouteController _controller;

        public RouteControllerTests()
        {
            _routeBLMock = new Mock<IRouteBL>();
            _dbServiceMock = new Mock<IDBService>();
            _loggerMock = new Mock<ILogger<RouteController>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _controller = new RouteController(
                _serviceProviderMock.Object,
                _routeBLMock.Object,
                _dbServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SelectRouteDetailsAll_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var routeList = new List<IRouteViewModel>
            {
                new RouteViewModelDCO { RouteUID = "route1", RouteNumber = "R001" },
                new RouteViewModelDCO { RouteUID = "route2", RouteNumber = "R002" }
            };

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(routeList);

            // Act
            var result = await _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IRouteViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectRouteDetailsAll_WithSorting_ReturnsSortedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria { FieldName = "RouteNumber", SortDirection = SortDirection.Asc }
            };
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var routeList = new List<IRouteViewModel>
            {
                new RouteViewModelDCO { RouteUID = "route1", RouteNumber = "R001" },
                new RouteViewModelDCO { RouteUID = "route2", RouteNumber = "R002" }
            };

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(routeList);

            // Act
            var result = await _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IRouteViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectRouteDetailsAll_WithFiltering_ReturnsFilteredResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria { FieldName = "Status", Operator = FilterOperator.Equals, Value = "ACTIVE" }
            };
            var pageNumber = 1;
            var pageSize = 10;

            var routeList = new List<IRouteViewModel>
            {
                new RouteViewModelDCO { RouteUID = "route1", RouteNumber = "R001", Status = "ACTIVE" }
            };

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(routeList);

            // Act
            var result = await _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IRouteViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("ACTIVE", returnValue.Data.First().Status);
        }

        [Fact]
        public async Task SelectRouteDetailsAll_WithPaging_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 2;
            var pageSize = 1;

            var routeList = new List<IRouteViewModel>
            {
                new RouteViewModelDCO { RouteUID = "route2", RouteNumber = "R002" }
            };

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(routeList);

            // Act
            var result = await _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IRouteViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("R002", returnValue.Data.First().RouteNumber);
        }

        [Fact]
        public async Task SelectRouteDetailsAll_NoData_ReturnsNotFound()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync((IEnumerable<IRouteViewModel>)null);

            // Act
            var result = await _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectRouteByUID_ValidUID_ReturnsRoute()
        {
            // Arrange
            var route = new Route { UID = "test-uid", RouteNumber = "R001" };
            _routeBLMock.Setup(x => x.GetRouteByUID("test-uid"))
                .ReturnsAsync(route);

            // Act
            var result = await _controller.SelectRouteByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<IRoute>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.UID);
        }

        [Fact]
        public async Task SelectRouteByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRouteByUID("invalid-uid"))
                .ReturnsAsync((IRoute)null);

            // Act
            var result = await _controller.SelectRouteByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateRoute_ValidRoute_ReturnsCreated()
        {
            // Arrange
            var routeViewModel = new RouteViewModelDCO
            {
                RouteUID = "new-uid",
                RouteNumber = "R001",
                RouteDate = DateTime.Now,
                Status = "ACTIVE",
                RouteType = "DELIVERY",
                StoreUID = "store1",
                VehicleUID = "vehicle1",
                DriverUID = "driver1"
            };

            _routeBLMock.Setup(x => x.SaveRoute(It.IsAny<RouteViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateRoute(routeViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateRoute_WithMinimalData_ReturnsCreated()
        {
            // Arrange
            var routeViewModel = new RouteViewModelDCO
            {
                RouteUID = "new-uid",
                RouteDate = DateTime.Now
            };

            _routeBLMock.Setup(x => x.SaveRoute(It.IsAny<RouteViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateRoute(routeViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateRoute_InvalidRoute_ReturnsError()
        {
            // Arrange
            var routeViewModel = new RouteViewModelDCO
            {
                RouteUID = "new-uid",
                RouteNumber = "R001",
                RouteDate = DateTime.Now
            };

            _routeBLMock.Setup(x => x.SaveRoute(It.IsAny<RouteViewModelDCO>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateRoute(routeViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Insert Failed", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetRoutePrintView_ValidUID_ReturnsPrintView()
        {
            // Arrange
            var printView = new RoutePrintView 
            { 
                RouteUID = "test-uid", 
                RouteNumber = "R001",
                RouteDate = DateTime.Now,
                Status = "ACTIVE",
                StoreName = "Test Store",
                VehicleNumber = "V001",
                DriverName = "Test Driver"
            };
            _routeBLMock.Setup(x => x.GetRoutePrintView("test-uid"))
                .ReturnsAsync(printView);

            // Act
            var result = await _controller.GetRoutePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IRoutePrintView>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.RouteUID);
            Assert.Equal("Test Store", returnValue.Data.StoreName);
        }

        [Fact]
        public async Task GetRoutePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRoutePrintView("invalid-uid"))
                .ReturnsAsync((IRoutePrintView)null);

            // Act
            var result = await _controller.GetRoutePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetRouteLinePrintView_ValidUID_ReturnsLinePrintViews()
        {
            // Arrange
            var linePrintViews = new List<IRouteLinePrintView>
            {
                new RouteLinePrintView 
                { 
                    RouteUID = "test-uid", 
                    LineNumber = 1,
                    CustomerCode = "CUST001",
                    CustomerName = "Test Customer 1",
                    Address = "Test Address 1",
                    Sequence = 1
                },
                new RouteLinePrintView 
                { 
                    RouteUID = "test-uid", 
                    LineNumber = 2,
                    CustomerCode = "CUST002",
                    CustomerName = "Test Customer 2",
                    Address = "Test Address 2",
                    Sequence = 2
                }
            };

            _routeBLMock.Setup(x => x.GetRouteLinePrintView("test-uid"))
                .ReturnsAsync(linePrintViews);

            // Act
            var result = await _controller.GetRouteLinePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IRouteLinePrintView>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal("CUST001", returnValue.Data.First().CustomerCode);
            Assert.Equal("CUST002", returnValue.Data.Last().CustomerCode);
        }

        [Fact]
        public async Task GetRouteLinePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRouteLinePrintView("invalid-uid"))
                .ReturnsAsync((IEnumerable<IRouteLinePrintView>)null);

            // Act
            var result = await _controller.GetRouteLinePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectRouteDetailsAll_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _routeBLMock.Setup(x => x.SelectRouteDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.SelectRouteDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias));
        }

        [Fact]
        public async Task CreateRoute_WhenExceptionOccurs_ReturnsError()
        {
            // Arrange
            var routeViewModel = new RouteViewModelDCO
            {
                RouteUID = "new-uid",
                RouteNumber = "R001",
                RouteDate = DateTime.Now
            };

            _routeBLMock.Setup(x => x.SaveRoute(It.IsAny<RouteViewModelDCO>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateRoute(routeViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Error creating Route Details", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetRoutePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRoutePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetRoutePrintView("test-uid"));
        }

        [Fact]
        public async Task GetRouteLinePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRouteLinePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetRouteLinePrintView("test-uid"));
        }

        [Fact]
        public async Task SelectRouteByUID_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _routeBLMock.Setup(x => x.GetRouteByUID("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SelectRouteByUID("test-uid"));
        }
    }
} 