using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Currency.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Currency
{
    public class CurrencyControllerTests
    {
        private readonly Mock<ICurrencyBL> _currencyBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _currencyBLMock = new Mock<ICurrencyBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new CurrencyController(
                _serviceProviderMock.Object,
                _currencyBLMock.Object
            );
        }

        [Fact]
        public async Task SelectCurrencyByUID_ValidUID_ReturnsCurrency()
        {
            // Arrange
            var currency = new Currency { UID = "test-uid", Name = "Test Currency" };
            _currencyBLMock.Setup(x => x.SelectCurrencyByUID("test-uid"))
                .ReturnsAsync(currency);

            // Act
            var result = await _controller.SelectCurrencyByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCurrency = Assert.IsType<Currency>(okResult.Value);
            Assert.Equal("test-uid", returnedCurrency.UID);
        }

        [Fact]
        public async Task SelectCurrencyByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _currencyBLMock.Setup(x => x.SelectCurrencyByUID("invalid-uid"))
                .ReturnsAsync((ICurrency)null);

            // Act
            var result = await _controller.SelectCurrencyByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateCurrency_ValidCurrency_ReturnsSuccess()
        {
            // Arrange
            var currency = new Currency { UID = "new-uid", Name = "New Currency" };
            _currencyBLMock.Setup(x => x.CreateCurrency(It.IsAny<ICurrency>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateCurrency(currency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateCurrency_InvalidCurrency_ReturnsError()
        {
            // Arrange
            var currency = new Currency { UID = "new-uid", Name = "New Currency" };
            _currencyBLMock.Setup(x => x.CreateCurrency(It.IsAny<ICurrency>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateCurrency(currency);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCurrency_ValidCurrency_ReturnsSuccess()
        {
            // Arrange
            var currency = new Currency { UID = "existing-uid", Name = "Updated Currency" };
            _currencyBLMock.Setup(x => x.SelectCurrencyByUID("existing-uid"))
                .ReturnsAsync(currency);
            _currencyBLMock.Setup(x => x.UpdateCurrency(It.IsAny<Currency>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateCurrency(currency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateCurrency_NonExistentCurrency_ReturnsNotFound()
        {
            // Arrange
            var currency = new Currency { UID = "non-existent-uid", Name = "Updated Currency" };
            _currencyBLMock.Setup(x => x.SelectCurrencyByUID("non-existent-uid"))
                .ReturnsAsync((ICurrency)null);

            // Act
            var result = await _controller.UpdateCurrency(currency);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCurrency_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _currencyBLMock.Setup(x => x.DeleteCurrency("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteCurrency("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteCurrency_InvalidUID_ReturnsError()
        {
            // Arrange
            _currencyBLMock.Setup(x => x.DeleteCurrency("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteCurrency("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllCurrencyDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var currencyList = new List<ICurrency>
            {
                new Currency { UID = "currency1", Name = "Currency 1" },
                new Currency { UID = "currency2", Name = "Currency 2" }
            };

            _currencyBLMock.Setup(x => x.SelectAllCurrencyDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<ICurrency>
                {
                    PagedData = currencyList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllCurrencyDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<ICurrency>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllCurrencyDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllCurrencyDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 