using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Customer.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Customer.BL.Interfaces;
using Winit.Modules.Customer.Model.Classes;
using Winit.Modules.Customer.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Customer
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerBL> _customerBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly CustomerController _controller;

        public CustomerControllerTests()
        {
            _customerBLMock = new Mock<ICustomerBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new CustomerController(
                _serviceProviderMock.Object,
                _customerBLMock.Object
            );
        }

        [Fact]
        public async Task SelectCustomerByUID_ValidUID_ReturnsCustomer()
        {
            // Arrange
            var customer = new Customer { UID = "test-uid", Name = "Test Customer" };
            _customerBLMock.Setup(x => x.SelectCustomerByUID("test-uid"))
                .ReturnsAsync(customer);

            // Act
            var result = await _controller.SelectCustomerByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCustomer = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal("test-uid", returnedCustomer.UID);
        }

        [Fact]
        public async Task SelectCustomerByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _customerBLMock.Setup(x => x.SelectCustomerByUID("invalid-uid"))
                .ReturnsAsync((ICustomer)null);

            // Act
            var result = await _controller.SelectCustomerByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateCustomer_ValidCustomer_ReturnsSuccess()
        {
            // Arrange
            var customer = new Customer { UID = "new-uid", Name = "New Customer" };
            _customerBLMock.Setup(x => x.CreateCustomer(It.IsAny<ICustomer>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateCustomer(customer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateCustomer_InvalidCustomer_ReturnsError()
        {
            // Arrange
            var customer = new Customer { UID = "new-uid", Name = "New Customer" };
            _customerBLMock.Setup(x => x.CreateCustomer(It.IsAny<ICustomer>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateCustomer(customer);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_ValidCustomer_ReturnsSuccess()
        {
            // Arrange
            var customer = new Customer { UID = "existing-uid", Name = "Updated Customer" };
            _customerBLMock.Setup(x => x.SelectCustomerByUID("existing-uid"))
                .ReturnsAsync(customer);
            _customerBLMock.Setup(x => x.UpdateCustomer(It.IsAny<Customer>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateCustomer(customer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateCustomer_NonExistentCustomer_ReturnsNotFound()
        {
            // Arrange
            var customer = new Customer { UID = "non-existent-uid", Name = "Updated Customer" };
            _customerBLMock.Setup(x => x.SelectCustomerByUID("non-existent-uid"))
                .ReturnsAsync((ICustomer)null);

            // Act
            var result = await _controller.UpdateCustomer(customer);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _customerBLMock.Setup(x => x.DeleteCustomer("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteCustomer("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteCustomer_InvalidUID_ReturnsError()
        {
            // Arrange
            _customerBLMock.Setup(x => x.DeleteCustomer("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteCustomer("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllCustomerDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var customerList = new List<ICustomer>
            {
                new Customer { UID = "customer1", Name = "Customer 1" },
                new Customer { UID = "customer2", Name = "Customer 2" }
            };

            _customerBLMock.Setup(x => x.SelectAllCustomerDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<ICustomer>
                {
                    PagedData = customerList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllCustomerDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<ICustomer>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllCustomerDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllCustomerDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerCreditLimit_ValidUID_ReturnsCreditLimit()
        {
            // Arrange
            var creditLimit = new CustomerCreditLimit { CustomerUID = "test-uid", Limit = 1000 };
            _customerBLMock.Setup(x => x.GetCustomerCreditLimit("test-uid"))
                .ReturnsAsync(creditLimit);

            // Act
            var result = await _controller.GetCustomerCreditLimit("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCreditLimit = Assert.IsType<CustomerCreditLimit>(okResult.Value);
            Assert.Equal("test-uid", returnedCreditLimit.CustomerUID);
        }

        [Fact]
        public async Task GetCustomerCreditLimit_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _customerBLMock.Setup(x => x.GetCustomerCreditLimit("invalid-uid"))
                .ReturnsAsync((ICustomerCreditLimit)null);

            // Act
            var result = await _controller.GetCustomerCreditLimit("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateCustomerCreditLimit_ValidCreditLimit_ReturnsSuccess()
        {
            // Arrange
            var creditLimit = new CustomerCreditLimit { CustomerUID = "test-uid", Limit = 2000 };
            _customerBLMock.Setup(x => x.UpdateCustomerCreditLimit(It.IsAny<ICustomerCreditLimit>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateCustomerCreditLimit(creditLimit);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateCustomerCreditLimit_InvalidCreditLimit_ReturnsError()
        {
            // Arrange
            var creditLimit = new CustomerCreditLimit { CustomerUID = "test-uid", Limit = -1000 };
            _customerBLMock.Setup(x => x.UpdateCustomerCreditLimit(It.IsAny<ICustomerCreditLimit>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.UpdateCustomerCreditLimit(creditLimit);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }
    }
} 