using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Org.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Org
{
    public class OrgControllerTests
    {
        private readonly Mock<IOrgBL> _orgBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly OrgController _controller;

        public OrgControllerTests()
        {
            _orgBLMock = new Mock<IOrgBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new OrgController(
                _serviceProviderMock.Object,
                _orgBLMock.Object
            );
        }

        [Fact]
        public async Task SelectOrgByUID_ValidUID_ReturnsOrg()
        {
            // Arrange
            var org = new Org { UID = "test-uid", Name = "Test Org" };
            _orgBLMock.Setup(x => x.SelectOrgByUID("test-uid"))
                .ReturnsAsync(org);

            // Act
            var result = await _controller.SelectOrgByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOrg = Assert.IsType<Org>(okResult.Value);
            Assert.Equal("test-uid", returnedOrg.UID);
        }

        [Fact]
        public async Task SelectOrgByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _orgBLMock.Setup(x => x.SelectOrgByUID("invalid-uid"))
                .ReturnsAsync((IOrg)null);

            // Act
            var result = await _controller.SelectOrgByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrg_ValidOrg_ReturnsSuccess()
        {
            // Arrange
            var org = new Org { UID = "new-uid", Name = "New Org" };
            _orgBLMock.Setup(x => x.CreateOrg(It.IsAny<IOrg>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateOrg(org);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateOrg_InvalidOrg_ReturnsError()
        {
            // Arrange
            var org = new Org { UID = "new-uid", Name = "New Org" };
            _orgBLMock.Setup(x => x.CreateOrg(It.IsAny<IOrg>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateOrg(org);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrg_ValidOrg_ReturnsSuccess()
        {
            // Arrange
            var org = new Org { UID = "existing-uid", Name = "Updated Org" };
            _orgBLMock.Setup(x => x.SelectOrgByUID("existing-uid"))
                .ReturnsAsync(org);
            _orgBLMock.Setup(x => x.UpdateOrg(It.IsAny<Org>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateOrg(org);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateOrg_NonExistentOrg_ReturnsNotFound()
        {
            // Arrange
            var org = new Org { UID = "non-existent-uid", Name = "Updated Org" };
            _orgBLMock.Setup(x => x.SelectOrgByUID("non-existent-uid"))
                .ReturnsAsync((IOrg)null);

            // Act
            var result = await _controller.UpdateOrg(org);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrg_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _orgBLMock.Setup(x => x.DeleteOrg("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteOrg("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteOrg_InvalidUID_ReturnsError()
        {
            // Arrange
            _orgBLMock.Setup(x => x.DeleteOrg("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteOrg("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllOrgDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var orgList = new List<IOrg>
            {
                new Org { UID = "org1", Name = "Org 1" },
                new Org { UID = "org2", Name = "Org 2" }
            };

            _orgBLMock.Setup(x => x.SelectAllOrgDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<IOrg>
                {
                    PagedData = orgList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllOrgDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<IOrg>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllOrgDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllOrgDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 