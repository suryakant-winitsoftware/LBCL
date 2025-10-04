using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Product.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Product.BL.Interfaces;
using Winit.Modules.Product.Model.Classes;
using Winit.Modules.Product.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Product
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductBL> _productBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productBLMock = new Mock<IProductBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new ProductsController(
                _serviceProviderMock.Object,
                _productBLMock.Object
            );
        }

        [Fact]
        public async Task SelectProductByUID_ValidUID_ReturnsProduct()
        {
            // Arrange
            var product = new Product { UID = "test-uid", Name = "Test Product" };
            _productBLMock.Setup(x => x.SelectProductByUID("test-uid"))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.SelectProductByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal("test-uid", returnedProduct.UID);
        }

        [Fact]
        public async Task SelectProductByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _productBLMock.Setup(x => x.SelectProductByUID("invalid-uid"))
                .ReturnsAsync((IProduct)null);

            // Act
            var result = await _controller.SelectProductByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateProduct_ValidProduct_ReturnsSuccess()
        {
            // Arrange
            var product = new Product { UID = "new-uid", Name = "New Product" };
            _productBLMock.Setup(x => x.CreateProduct(It.IsAny<IProduct>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateProduct(product);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateProduct_InvalidProduct_ReturnsError()
        {
            // Arrange
            var product = new Product { UID = "new-uid", Name = "New Product" };
            _productBLMock.Setup(x => x.CreateProduct(It.IsAny<IProduct>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateProduct(product);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_ValidProduct_ReturnsSuccess()
        {
            // Arrange
            var product = new Product { UID = "existing-uid", Name = "Updated Product" };
            _productBLMock.Setup(x => x.SelectProductByUID("existing-uid"))
                .ReturnsAsync(product);
            _productBLMock.Setup(x => x.UpdateProduct(It.IsAny<Product>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateProduct(product);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_NonExistentProduct_ReturnsNotFound()
        {
            // Arrange
            var product = new Product { UID = "non-existent-uid", Name = "Updated Product" };
            _productBLMock.Setup(x => x.SelectProductByUID("non-existent-uid"))
                .ReturnsAsync((IProduct)null);

            // Act
            var result = await _controller.UpdateProduct(product);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _productBLMock.Setup(x => x.DeleteProduct("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteProduct("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteProduct_InvalidUID_ReturnsError()
        {
            // Arrange
            _productBLMock.Setup(x => x.DeleteProduct("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteProduct("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllProductDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var productList = new List<IProduct>
            {
                new Product { UID = "prod1", Name = "Product 1" },
                new Product { UID = "prod2", Name = "Product 2" }
            };

            _productBLMock.Setup(x => x.SelectAllProductDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<IProduct>
                {
                    PagedData = productList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllProductDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<IProduct>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllProductDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllProductDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 