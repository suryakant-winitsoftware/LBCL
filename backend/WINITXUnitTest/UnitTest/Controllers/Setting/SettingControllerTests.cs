using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Setting.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Setting.Model.Classes;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Setting
{
    public class SettingControllerTests
    {
        private readonly Mock<ISettingBL> _settingBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly SettingController _controller;

        public SettingControllerTests()
        {
            _settingBLMock = new Mock<ISettingBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new SettingController(
                _serviceProviderMock.Object,
                _settingBLMock.Object
            );
        }

        [Fact]
        public async Task SelectSettingByUID_ValidUID_ReturnsSetting()
        {
            // Arrange
            var setting = new Setting { UID = "test-uid", Name = "Test Setting" };
            _settingBLMock.Setup(x => x.SelectSettingByUID("test-uid"))
                .ReturnsAsync(setting);

            // Act
            var result = await _controller.SelectSettingByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSetting = Assert.IsType<Setting>(okResult.Value);
            Assert.Equal("test-uid", returnedSetting.UID);
        }

        [Fact]
        public async Task SelectSettingByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _settingBLMock.Setup(x => x.SelectSettingByUID("invalid-uid"))
                .ReturnsAsync((ISetting)null);

            // Act
            var result = await _controller.SelectSettingByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateSetting_ValidSetting_ReturnsSuccess()
        {
            // Arrange
            var setting = new Setting { UID = "new-uid", Name = "New Setting" };
            _settingBLMock.Setup(x => x.CreateSetting(It.IsAny<ISetting>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateSetting(setting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateSetting_InvalidSetting_ReturnsError()
        {
            // Arrange
            var setting = new Setting { UID = "new-uid", Name = "New Setting" };
            _settingBLMock.Setup(x => x.CreateSetting(It.IsAny<ISetting>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateSetting(setting);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateSetting_ValidSetting_ReturnsSuccess()
        {
            // Arrange
            var setting = new Setting { UID = "existing-uid", Name = "Updated Setting" };
            _settingBLMock.Setup(x => x.SelectSettingByUID("existing-uid"))
                .ReturnsAsync(setting);
            _settingBLMock.Setup(x => x.UpdateSetting(It.IsAny<Setting>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateSetting(setting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateSetting_NonExistentSetting_ReturnsNotFound()
        {
            // Arrange
            var setting = new Setting { UID = "non-existent-uid", Name = "Updated Setting" };
            _settingBLMock.Setup(x => x.SelectSettingByUID("non-existent-uid"))
                .ReturnsAsync((ISetting)null);

            // Act
            var result = await _controller.UpdateSetting(setting);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteSetting_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _settingBLMock.Setup(x => x.DeleteSetting("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteSetting("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteSetting_InvalidUID_ReturnsError()
        {
            // Arrange
            _settingBLMock.Setup(x => x.DeleteSetting("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteSetting("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllSettingDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var settingList = new List<ISetting>
            {
                new Setting { UID = "setting1", Name = "Setting 1" },
                new Setting { UID = "setting2", Name = "Setting 2" }
            };

            _settingBLMock.Setup(x => x.SelectAllSettingDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<ISetting>
                {
                    PagedData = settingList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllSettingDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<ISetting>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllSettingDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllSettingDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 