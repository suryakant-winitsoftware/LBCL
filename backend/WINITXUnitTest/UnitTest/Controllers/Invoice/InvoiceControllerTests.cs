using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers.Invoice;
using WINITAPI.Controllers.Invoice.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Invoice
{
    public class InvoiceControllerTests
    {
        private readonly Mock<IInvoiceBL> _invoiceBLMock;
        private readonly Mock<IDBService> _dbServiceMock;
        private readonly Mock<ILogger<InvoiceController>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly InvoiceController _controller;

        public InvoiceControllerTests()
        {
            _invoiceBLMock = new Mock<IInvoiceBL>();
            _dbServiceMock = new Mock<IDBService>();
            _loggerMock = new Mock<ILogger<InvoiceController>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _controller = new InvoiceController(
                _serviceProviderMock.Object,
                _invoiceBLMock.Object,
                _dbServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var invoiceList = new List<IInvoiceViewModel>
            {
                new InvoiceViewModelDCO { InvoiceUID = "inv1", InvoiceNumber = "INV001" },
                new InvoiceViewModelDCO { InvoiceUID = "inv2", InvoiceNumber = "INV002" }
            };

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(invoiceList);

            // Act
            var result = await _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IInvoiceViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_WithSorting_ReturnsSortedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria { FieldName = "InvoiceNumber", SortDirection = SortDirection.Asc }
            };
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            var invoiceList = new List<IInvoiceViewModel>
            {
                new InvoiceViewModelDCO { InvoiceUID = "inv1", InvoiceNumber = "INV001" },
                new InvoiceViewModelDCO { InvoiceUID = "inv2", InvoiceNumber = "INV002" }
            };

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(invoiceList);

            // Act
            var result = await _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IInvoiceViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_WithFiltering_ReturnsFilteredResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria { FieldName = "Status", Operator = FilterOperator.Equals, Value = "PAID" }
            };
            var pageNumber = 1;
            var pageSize = 10;

            var invoiceList = new List<IInvoiceViewModel>
            {
                new InvoiceViewModelDCO { InvoiceUID = "inv1", InvoiceNumber = "INV001", Status = "PAID" }
            };

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(invoiceList);

            // Act
            var result = await _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IInvoiceViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("PAID", returnValue.Data.First().Status);
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_WithPaging_ReturnsPagedResponse()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 2;
            var pageSize = 1;

            var invoiceList = new List<IInvoiceViewModel>
            {
                new InvoiceViewModelDCO { InvoiceUID = "inv2", InvoiceNumber = "INV002" }
            };

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync(invoiceList);

            // Act
            var result = await _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IInvoiceViewModel>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Single(returnValue.Data);
            Assert.Equal("INV002", returnValue.Data.First().InvoiceNumber);
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_NoData_ReturnsNotFound()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ReturnsAsync((IEnumerable<IInvoiceViewModel>)null);

            // Act
            var result = await _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectInvoiceByUID_ValidUID_ReturnsInvoice()
        {
            // Arrange
            var invoice = new Invoice { UID = "test-uid", InvoiceNumber = "INV001" };
            _invoiceBLMock.Setup(x => x.GetInvoiceByUID("test-uid"))
                .ReturnsAsync(invoice);

            // Act
            var result = await _controller.SelectInvoiceByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<IInvoice>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.UID);
        }

        [Fact]
        public async Task SelectInvoiceByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoiceByUID("invalid-uid"))
                .ReturnsAsync((IInvoice)null);

            // Act
            var result = await _controller.SelectInvoiceByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateInvoice_ValidInvoice_ReturnsCreated()
        {
            // Arrange
            var invoiceViewModel = new InvoiceViewModelDCO
            {
                InvoiceUID = "new-uid",
                InvoiceNumber = "INV001",
                InvoiceDate = DateTime.Now,
                Status = "PAID",
                InvoiceType = "SALES",
                StoreUID = "store1",
                CustomerUID = "cust1",
                CurrencyUID = "USD",
                TotalAmount = 1000.00m
            };

            _invoiceBLMock.Setup(x => x.SaveInvoice(It.IsAny<InvoiceViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateInvoice(invoiceViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateInvoice_WithMinimalData_ReturnsCreated()
        {
            // Arrange
            var invoiceViewModel = new InvoiceViewModelDCO
            {
                InvoiceUID = "new-uid",
                InvoiceDate = DateTime.Now
            };

            _invoiceBLMock.Setup(x => x.SaveInvoice(It.IsAny<InvoiceViewModelDCO>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateInvoice(invoiceViewModel);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(1, createdResult.Value);
        }

        [Fact]
        public async Task CreateInvoice_InvalidInvoice_ReturnsError()
        {
            // Arrange
            var invoiceViewModel = new InvoiceViewModelDCO
            {
                InvoiceUID = "new-uid",
                InvoiceNumber = "INV001",
                InvoiceDate = DateTime.Now
            };

            _invoiceBLMock.Setup(x => x.SaveInvoice(It.IsAny<InvoiceViewModelDCO>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateInvoice(invoiceViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Insert Failed", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetInvoicePrintView_ValidUID_ReturnsPrintView()
        {
            // Arrange
            var printView = new InvoicePrintView 
            { 
                InvoiceUID = "test-uid", 
                InvoiceNumber = "INV001",
                InvoiceDate = DateTime.Now,
                Status = "PAID",
                StoreName = "Test Store",
                CustomerName = "Test Customer",
                TotalAmount = 1000.00m
            };
            _invoiceBLMock.Setup(x => x.GetInvoicePrintView("test-uid"))
                .ReturnsAsync(printView);

            // Act
            var result = await _controller.GetInvoicePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IInvoicePrintView>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal("test-uid", returnValue.Data.InvoiceUID);
            Assert.Equal("Test Store", returnValue.Data.StoreName);
        }

        [Fact]
        public async Task GetInvoicePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoicePrintView("invalid-uid"))
                .ReturnsAsync((IInvoicePrintView)null);

            // Act
            var result = await _controller.GetInvoicePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetInvoiceLinePrintView_ValidUID_ReturnsLinePrintViews()
        {
            // Arrange
            var linePrintViews = new List<IInvoiceLinePrintView>
            {
                new InvoiceLinePrintView 
                { 
                    InvoiceUID = "test-uid", 
                    LineNumber = 1,
                    ItemCode = "ITEM001",
                    ItemName = "Test Item 1",
                    Quantity = 10,
                    UnitPrice = 100,
                    LineTotal = 1000.00m
                },
                new InvoiceLinePrintView 
                { 
                    InvoiceUID = "test-uid", 
                    LineNumber = 2,
                    ItemCode = "ITEM002",
                    ItemName = "Test Item 2",
                    Quantity = 5,
                    UnitPrice = 200,
                    LineTotal = 1000.00m
                }
            };

            _invoiceBLMock.Setup(x => x.GetInvoiceLinePrintView("test-uid"))
                .ReturnsAsync(linePrintViews);

            // Act
            var result = await _controller.GetInvoiceLinePrintView("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<IInvoiceLinePrintView>>>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal("ITEM001", returnValue.Data.First().ItemCode);
            Assert.Equal("ITEM002", returnValue.Data.Last().ItemCode);
        }

        [Fact]
        public async Task GetInvoiceLinePrintView_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoiceLinePrintView("invalid-uid"))
                .ReturnsAsync((IEnumerable<IInvoiceLinePrintView>)null);

            // Act
            var result = await _controller.GetInvoiceLinePrintView("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SelectInvoiceDetailsAll_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>();
            var filterCriterias = new List<FilterCriteria>();
            var pageNumber = 1;
            var pageSize = 10;

            _invoiceBLMock.Setup(x => x.SelectInvoiceDetailsAll(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.SelectInvoiceDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias));
        }

        [Fact]
        public async Task CreateInvoice_WhenExceptionOccurs_ReturnsError()
        {
            // Arrange
            var invoiceViewModel = new InvoiceViewModelDCO
            {
                InvoiceUID = "new-uid",
                InvoiceNumber = "INV001",
                InvoiceDate = DateTime.Now
            };

            _invoiceBLMock.Setup(x => x.SaveInvoice(It.IsAny<InvoiceViewModelDCO>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateInvoice(invoiceViewModel);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, errorResult.StatusCode);
            var returnValue = Assert.IsType<ApiResponse<object>>(errorResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Contains("Error creating Invoice Details", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task GetInvoicePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoicePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetInvoicePrintView("test-uid"));
        }

        [Fact]
        public async Task GetInvoiceLinePrintView_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoiceLinePrintView("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetInvoiceLinePrintView("test-uid"));
        }

        [Fact]
        public async Task SelectInvoiceByUID_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _invoiceBLMock.Setup(x => x.GetInvoiceByUID("test-uid"))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SelectInvoiceByUID("test-uid"));
        }
    }
} 