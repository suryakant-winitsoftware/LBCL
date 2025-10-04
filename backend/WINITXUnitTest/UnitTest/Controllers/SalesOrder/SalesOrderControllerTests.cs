using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers.SalesOrder;
using WINITAPI.Controllers.SalesOrder.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.SalesOrder
{
    public class SalesOrderControllerTests
    {
        private readonly Mock<ISalesOrderBL> _salesOrderBLMock;
        private readonly Mock<IDBService> _dbServiceMock;
        private readonly Mock<ILogger<SalesOrderController>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly SalesOrderController _controller;

        public SalesOrderControllerTests()
        {
            _salesOrderBLMock = new Mock<ISalesOrderBL>();
            _dbServiceMock = new Mock<IDBService>();
            _loggerMock = new Mock<ILogger<SalesOrderController>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _controller = new SalesOrderController(
                _serviceProviderMock.Object,
                _salesOrderBLMock.Object,
                _dbServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var salesOrderList = new List<ISalesOrderViewModel>
            {
                new SalesOrderViewModelDCO { SalesOrderUID = "so1", SalesOrderNumber = "SO001" },
                new SalesOrderViewModelDCO { SalesOrderUID = "so2", SalesOrderNumber = "SO002" }
            };

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(salesOrderList);

            // Act
            var result = await _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<ISalesOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_WithSorting_ReturnsSortedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria { FieldName = "SalesOrderNumber", SortDirection = SortDirection.Asc }
            };
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var salesOrderList = new List<ISalesOrderViewModel>
            {
                new SalesOrderViewModelDCO { SalesOrderUID = "so1", SalesOrderNumber = "SO001" },
                new SalesOrderViewModelDCO { SalesOrderUID = "so2", SalesOrderNumber = "SO002" }
            };

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(salesOrderList);

            // Act
            var result = await _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<ISalesOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_WithFiltering_ReturnsFilteredResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria { FieldName = "Status", Operator = FilterOperator.Equals, Value = "FINALIZED" }
            };
            var pageNumber = 1;
            var pageSize = 10;

            var salesOrderList = new List<ISalesOrderViewModel>
            {
                new SalesOrderViewModelDCO { SalesOrderUID = "so1", SalesOrderNumber = "SO001", Status = "FINALIZED" }
            };

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(salesOrderList);

            // Act
            var result = await _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<ISalesOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("FINALIZED", returnValue.Data.First().Status);
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_WithPaging_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 2;
            var pageSize = 1;

            var salesOrderList = new List<ISalesOrderViewModel>
            {
                new SalesOrderViewModelDCO { SalesOrderUID = "so2", SalesOrderNumber = "SO002" }
            };

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(salesOrderList);

            // Act
            var result = await _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<ISalesOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("SO002", returnValue.Data.First().SalesOrderNumber);
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_NoData_ReturnsNotFound()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync((IEnumerable<ISalesOrderViewModel>)null);

            // Act
            var result = await _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectSalesOrderByUID_ValidUID_ReturnsSalesOrder()
        {
            // Arrange
            var salesOrder = new SalesOrder { UID = "test-uid", SalesOrderNumber = "SO001" };
            _salesOrderBLMock.Setup(x => x.GetSalesOrderByUID("test-uid"))
                .ReturnsAsync(salesOrder);

            // Act
            var result = await _controller.SelectSalesOrderByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<ISalesOrder>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.UID);
        }

        [Fact]
        public async Task SelectSalesOrderByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderByUID("invalid-uid"))
                .ReturnsAsync((ISalesOrder)null);

            // Act
            var result = await _controller.SelectSalesOrderByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateSalesOrder_ValidSalesOrder_ReturnsCreated()
        {
            // Arrange
            var salesOrderViewModel = new SalesOrderViewModelDCO
            {
                SalesOrderUID = "new-uid",
                SalesOrderNumber = "SO001",
                OrderDate = DateTime.Now,
                Status = "FINALIZED",
                OrderType = "PRESALES",
                StoreUID = "store1",
                CustomerPO = "PO123",
                CurrencyUID = "USD"
            };

            _salesOrderBLMock.Setup(x => x.SaveSalesOrder(It.IsAny<SalesOrderViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateSalesOrder_WithMinimalData_ReturnsCreated()
        {
            // Arrange
            var salesOrderViewModel = new SalesOrderViewModelDCO
            {
                SalesOrderUID = "new-uid",
                OrderDate = DateTime.Now
            };

            _salesOrderBLMock.Setup(x => x.SaveSalesOrder(It.IsAny<SalesOrderViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateSalesOrder_InvalidSalesOrder_ReturnsError()
        {
            // Arrange
            var salesOrderViewModel = new SalesOrderViewModelDCO
            {
                SalesOrderUID = "new-uid",
                SalesOrderNumber = "SO001",
                OrderDate = DateTime.Now
            };

            _salesOrderBLMock.Setup(x => x.SaveSalesOrder(It.IsAny<SalesOrderViewModelDCO>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Insert Failed", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetSalesOrderPrintView_ValidUID_ReturnsPrintView()
        {
            // Arrange
            var printView = new SalesOrderPrintView 
            { 
                SalesOrderUID = "test-uid", 
                SalesOrderNumber = "SO001",
                OrderDate = DateTime.Now,
                Status = "FINALIZED",
                StoreName = "Test Store",
                CustomerName = "Test Customer"
            };
            _salesOrderBLMock.Setup(x => x.GetSalesOrderPrintView("test-uid"))
                .ReturnsAsync(printView);

            // Act
            var result = await _controller.GetSalesOrderPrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<ISalesOrderPrintView>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.SalesOrderUID);
            Assert.Equal("Test Store", returnValue.Data.StoreName);
        }

        [Fact]
        public async Task GetSalesOrderPrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderPrintView("invalid-uid"))
                .ReturnsAsync((ISalesOrderPrintView)null);

            // Act
            var result = await _controller.GetSalesOrderPrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSalesOrderLinePrintView_ValidUID_ReturnsLinePrintViews()
        {
            // Arrange
            var linePrintViews = new List<ISalesOrderLinePrintView>
            {
                new SalesOrderLinePrintView 
                { 
                    SalesOrderUID = "test-uid", 
                    LineNumber = 1,
                    ItemCode = "ITEM001",
                    ItemName = "Test Item 1",
                    Quantity = 10,
                    UnitPrice = 100
                },
                new SalesOrderLinePrintView 
                { 
                    SalesOrderUID = "test-uid", 
                    LineNumber = 2,
                    ItemCode = "ITEM002",
                    ItemName = "Test Item 2",
                    Quantity = 5,
                    UnitPrice = 200
                }
            };

            _salesOrderBLMock.Setup(x => x.GetSalesOrderLinePrintView("test-uid"))
                .ReturnsAsync(linePrintViews);

            // Act
            var result = await _controller.GetSalesOrderLinePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<ISalesOrderLinePrintView>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal("ITEM001", returnValue.Data.First().ItemCode);
            Assert.Equal("ITEM002", returnValue.Data.Last().ItemCode);
        }

        [Fact]
        public async Task GetSalesOrderLinePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderLinePrintView("invalid-uid"))
                .ReturnsAsync((IEnumerable<ISalesOrderLinePrintView>)null);

            // Act
            var result = await _controller.GetSalesOrderLinePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectSalesOrderDetailsAll_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _salesOrderBLMock.Setup(x => x.SelectSalesOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias));
        }

        [Fact]
        public async Task CreateSalesOrder_WhenExceptionOccurs_ReturnsError()
        {
            // Arrange
            var salesOrderViewModel = new SalesOrderViewModelDCO
            {
                SalesOrderUID = "new-uid",
                SalesOrderNumber = "SO001",
                OrderDate = DateTime.Now
            };

            _salesOrderBLMock.Setup(x => x.SaveSalesOrder(It.IsAny<SalesOrderViewModelDCO>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Error creating Sales Order Details", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetSalesOrderPrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderPrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSalesOrderPrintView("test-uid"));
        }

        [Fact]
        public async Task GetSalesOrderLinePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderLinePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSalesOrderLinePrintView("test-uid"));
        }

        [Fact]
        public async Task SelectSalesOrderByUID_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _salesOrderBLMock.Setup(x => x.GetSalesOrderByUID("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SelectSalesOrderByUID("test-uid"));
        }
    }
} 