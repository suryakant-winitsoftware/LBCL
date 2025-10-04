using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Commission.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Commission.BL.Interfaces;
using Winit.Modules.Commission.Model.Classes;
using Winit.Modules.Commission.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Commission
{
    public class CommissionControllerTests
    {
        private readonly Mock<ICommissionBL> _commissionBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly CommissionController _controller;

        public CommissionControllerTests()
        {
            _commissionBLMock = new Mock<ICommissionBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new CommissionController(
                _serviceProviderMock.Object,
                _commissionBLMock.Object
            );
        }

        [Fact]
        public async Task SelectCommissionByUID_ValidUID_ReturnsCommission()
        {
            // Arrange
            var commission = new Commission { UID = "test-uid", Name = "Test Commission" };
            _commissionBLMock.Setup(x => x.SelectCommissionByUID("test-uid"))
                .ReturnsAsync(commission);

            // Act
            var result = await _controller.SelectCommissionByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCommission = Assert.IsType<Commission>(okResult.Value);
            Assert.Equal("test-uid", returnedCommission.UID);
        }

        [Fact]
        public async Task SelectCommissionByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _commissionBLMock.Setup(x => x.SelectCommissionByUID("invalid-uid"))
                .ReturnsAsync((ICommission)null);

            // Act
            var result = await _controller.SelectCommissionByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateCommission_ValidCommission_ReturnsSuccess()
        {
            // Arrange
            var commission = new Commission { UID = "new-uid", Name = "New Commission" };
            _commissionBLMock.Setup(x => x.CreateCommission(It.IsAny<ICommission>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateCommission(commission);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateCommission_InvalidCommission_ReturnsError()
        {
            // Arrange
            var commission = new Commission { UID = "new-uid", Name = "New Commission" };
            _commissionBLMock.Setup(x => x.CreateCommission(It.IsAny<ICommission>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateCommission(commission);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCommission_ValidCommission_ReturnsSuccess()
        {
            // Arrange
            var commission = new Commission { UID = "existing-uid", Name = "Updated Commission" };
            _commissionBLMock.Setup(x => x.SelectCommissionByUID("existing-uid"))
                .ReturnsAsync(commission);
            _commissionBLMock.Setup(x => x.UpdateCommission(It.IsAny<Commission>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateCommission(commission);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateCommission_NonExistentCommission_ReturnsNotFound()
        {
            // Arrange
            var commission = new Commission { UID = "non-existent-uid", Name = "Updated Commission" };
            _commissionBLMock.Setup(x => x.SelectCommissionByUID("non-existent-uid"))
                .ReturnsAsync((ICommission)null);

            // Act
            var result = await _controller.UpdateCommission(commission);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCommission_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _commissionBLMock.Setup(x => x.DeleteCommission("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteCommission("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteCommission_InvalidUID_ReturnsError()
        {
            // Arrange
            _commissionBLMock.Setup(x => x.DeleteCommission("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteCommission("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllCommissionDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var commissionList = new List<ICommission>
            {
                new Commission { UID = "commission1", Name = "Commission 1" },
                new Commission { UID = "commission2", Name = "Commission 2" }
            };

            _commissionBLMock.Setup(x => x.SelectAllCommissionDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<ICommission>
                {
                    PagedData = commissionList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllCommissionDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<ICommission>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllCommissionDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllCommissionDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 