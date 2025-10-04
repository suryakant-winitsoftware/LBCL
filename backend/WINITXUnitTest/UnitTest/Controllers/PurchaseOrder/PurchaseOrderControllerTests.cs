using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers.PurchaseOrder;
using WINITAPI.Controllers.PurchaseOrder.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.PurchaseOrder
{
    public class PurchaseOrderControllerTests
    {
        private readonly Mock<IPurchaseOrderBL> _purchaseOrderBLMock;
        private readonly Mock<IDBService> _dbServiceMock;
        private readonly Mock<ILogger<PurchaseOrderController>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly PurchaseOrderController _controller;

        public PurchaseOrderControllerTests()
        {
            _purchaseOrderBLMock = new Mock<IPurchaseOrderBL>();
            _dbServiceMock = new Mock<IDBService>();
            _loggerMock = new Mock<ILogger<PurchaseOrderController>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _controller = new PurchaseOrderController(
                _serviceProviderMock.Object,
                _purchaseOrderBLMock.Object,
                _dbServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var purchaseOrderList = new List<IPurchaseOrderViewModel>
            {
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po1", PurchaseOrderNumber = "PO001" },
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po2", PurchaseOrderNumber = "PO002" }
            };

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(purchaseOrderList);

            // Act
            var result = await _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IPurchaseOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_WithSorting_ReturnsSortedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria { FieldName = "PurchaseOrderNumber", SortDirection = SortDirection.Asc }
            };
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var purchaseOrderList = new List<IPurchaseOrderViewModel>
            {
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po1", PurchaseOrderNumber = "PO001" },
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po2", PurchaseOrderNumber = "PO002" }
            };

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(purchaseOrderList);

            // Act
            var result = await _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IPurchaseOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_WithFiltering_ReturnsFilteredResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria { FieldName = "Status", Operator = FilterOperator.Equals, Value = "FINALIZED" }
            };
            var pageNumber = 1;
            var pageSize = 10;

            var purchaseOrderList = new List<IPurchaseOrderViewModel>
            {
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po1", PurchaseOrderNumber = "PO001", Status = "FINALIZED" }
            };

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(purchaseOrderList);

            // Act
            var result = await _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IPurchaseOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("FINALIZED", returnValue.Data.First().Status);
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_WithPaging_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 2;
            var pageSize = 1;

            var purchaseOrderList = new List<IPurchaseOrderViewModel>
            {
                new PurchaseOrderViewModelDCO { PurchaseOrderUID = "po2", PurchaseOrderNumber = "PO002" }
            };

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(purchaseOrderList);

            // Act
            var result = await _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IPurchaseOrderViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("PO002", returnValue.Data.First().PurchaseOrderNumber);
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_NoData_ReturnsNotFound()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync((IEnumerable<IPurchaseOrderViewModel>)null);

            // Act
            var result = await _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectPurchaseOrderByUID_ValidUID_ReturnsPurchaseOrder()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder { UID = "test-uid", PurchaseOrderNumber = "PO001" };
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderByUID("test-uid"))
                .ReturnsAsync(purchaseOrder);

            // Act
            var result = await _controller.SelectPurchaseOrderByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<IPurchaseOrder>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.UID);
        }

        [Fact]
        public async Task SelectPurchaseOrderByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderByUID("invalid-uid"))
                .ReturnsAsync((IPurchaseOrder)null);

            // Act
            var result = await _controller.SelectPurchaseOrderByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreatePurchaseOrder_ValidPurchaseOrder_ReturnsCreated()
        {
            // Arrange
            var purchaseOrderViewModel = new PurchaseOrderViewModelDCO
            {
                PurchaseOrderUID = "new-uid",
                PurchaseOrderNumber = "PO001",
                OrderDate = DateTime.Now,
                Status = "FINALIZED",
                OrderType = "PURCHASE",
                StoreUID = "store1",
                SupplierPO = "SPO123",
                CurrencyUID = "USD"
            };

            _purchaseOrderBLMock.Setup(x => x.SavePurchaseOrder(It.IsAny<PurchaseOrderViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreatePurchaseOrder_WithMinimalData_ReturnsCreated()
        {
            // Arrange
            var purchaseOrderViewModel = new PurchaseOrderViewModelDCO
            {
                PurchaseOrderUID = "new-uid",
                OrderDate = DateTime.Now
            };

            _purchaseOrderBLMock.Setup(x => x.SavePurchaseOrder(It.IsAny<PurchaseOrderViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreatePurchaseOrder_InvalidPurchaseOrder_ReturnsError()
        {
            // Arrange
            var purchaseOrderViewModel = new PurchaseOrderViewModelDCO
            {
                PurchaseOrderUID = "new-uid",
                PurchaseOrderNumber = "PO001",
                OrderDate = DateTime.Now
            };

            _purchaseOrderBLMock.Setup(x => x.SavePurchaseOrder(It.IsAny<PurchaseOrderViewModelDCO>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Insert Failed", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetPurchaseOrderPrintView_ValidUID_ReturnsPrintView()
        {
            // Arrange
            var printView = new PurchaseOrderPrintView 
            { 
                PurchaseOrderUID = "test-uid", 
                PurchaseOrderNumber = "PO001",
                OrderDate = DateTime.Now,
                Status = "FINALIZED",
                StoreName = "Test Store",
                SupplierName = "Test Supplier"
            };
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderPrintView("test-uid"))
                .ReturnsAsync(printView);

            // Act
            var result = await _controller.GetPurchaseOrderPrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IPurchaseOrderPrintView>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.PurchaseOrderUID);
            Assert.Equal("Test Store", returnValue.Data.StoreName);
        }

        [Fact]
        public async Task GetPurchaseOrderPrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderPrintView("invalid-uid"))
                .ReturnsAsync((IPurchaseOrderPrintView)null);

            // Act
            var result = await _controller.GetPurchaseOrderPrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPurchaseOrderLinePrintView_ValidUID_ReturnsLinePrintViews()
        {
            // Arrange
            var linePrintViews = new List<IPurchaseOrderLinePrintView>
            {
                new PurchaseOrderLinePrintView 
                { 
                    PurchaseOrderUID = "test-uid", 
                    LineNumber = 1,
                    ItemCode = "ITEM001",
                    ItemName = "Test Item 1",
                    Quantity = 10,
                    UnitPrice = 100
                },
                new PurchaseOrderLinePrintView 
                { 
                    PurchaseOrderUID = "test-uid", 
                    LineNumber = 2,
                    ItemCode = "ITEM002",
                    ItemName = "Test Item 2",
                    Quantity = 5,
                    UnitPrice = 200
                }
            };

            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderLinePrintView("test-uid"))
                .ReturnsAsync(linePrintViews);

            // Act
            var result = await _controller.GetPurchaseOrderLinePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IPurchaseOrderLinePrintView>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal("ITEM001", returnValue.Data.First().ItemCode);
            Assert.Equal("ITEM002", returnValue.Data.Last().ItemCode);
        }

        [Fact]
        public async Task GetPurchaseOrderLinePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderLinePrintView("invalid-uid"))
                .ReturnsAsync((IEnumerable<IPurchaseOrderLinePrintView>)null);

            // Act
            var result = await _controller.GetPurchaseOrderLinePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectPurchaseOrderDetailsAll_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _purchaseOrderBLMock.Setup(x => x.SelectPurchaseOrderDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.SelectPurchaseOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias));
        }

        [Fact]
        public async Task CreatePurchaseOrder_WhenExceptionOccurs_ReturnsError()
        {
            // Arrange
            var purchaseOrderViewModel = new PurchaseOrderViewModelDCO
            {
                PurchaseOrderUID = "new-uid",
                PurchaseOrderNumber = "PO001",
                OrderDate = DateTime.Now
            };

            _purchaseOrderBLMock.Setup(x => x.SavePurchaseOrder(It.IsAny<PurchaseOrderViewModelDCO>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Error creating Purchase Order Details", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetPurchaseOrderPrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderPrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPurchaseOrderPrintView("test-uid"));
        }

        [Fact]
        public async Task GetPurchaseOrderLinePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderLinePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPurchaseOrderLinePrintView("test-uid"));
        }

        [Fact]
        public async Task SelectPurchaseOrderByUID_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _purchaseOrderBLMock.Setup(x => x.GetPurchaseOrderByUID("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SelectPurchaseOrderByUID("test-uid"));
        }
    }
} 