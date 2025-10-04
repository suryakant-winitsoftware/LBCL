using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Holiday.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Modules.Holiday.Model.Classes;
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.Holiday
{
    public class HolidayControllerTests
    {
        private readonly Mock<IHolidayBL> _holidayBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly HolidayController _controller;

        public HolidayControllerTests()
        {
            _holidayBLMock = new Mock<IHolidayBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new HolidayController(
                _serviceProviderMock.Object,
                _holidayBLMock.Object
            );
        }

        [Fact]
        public async Task SelectHolidayByUID_ValidUID_ReturnsHoliday()
        {
            // Arrange
            var holiday = new Holiday { UID = "test-uid", Name = "Test Holiday", Date = DateTime.Now };
            _holidayBLMock.Setup(x => x.SelectHolidayByUID("test-uid"))
                .ReturnsAsync(holiday);

            // Act
            var result = await _controller.SelectHolidayByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHoliday = Assert.IsType<Holiday>(okResult.Value);
            Assert.Equal("test-uid", returnedHoliday.UID);
        }

        [Fact]
        public async Task SelectHolidayByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _holidayBLMock.Setup(x => x.SelectHolidayByUID("invalid-uid"))
                .ReturnsAsync((IHoliday)null);

            // Act
            var result = await _controller.SelectHolidayByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateHoliday_ValidHoliday_ReturnsSuccess()
        {
            // Arrange
            var holiday = new Holiday { UID = "new-uid", Name = "New Holiday", Date = DateTime.Now };
            _holidayBLMock.Setup(x => x.CreateHoliday(It.IsAny<IHoliday>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateHoliday(holiday);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateHoliday_InvalidHoliday_ReturnsError()
        {
            // Arrange
            var holiday = new Holiday { UID = "new-uid", Name = "New Holiday", Date = DateTime.Now };
            _holidayBLMock.Setup(x => x.CreateHoliday(It.IsAny<IHoliday>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateHoliday(holiday);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateHoliday_ValidHoliday_ReturnsSuccess()
        {
            // Arrange
            var holiday = new Holiday { UID = "existing-uid", Name = "Updated Holiday", Date = DateTime.Now };
            _holidayBLMock.Setup(x => x.SelectHolidayByUID("existing-uid"))
                .ReturnsAsync(holiday);
            _holidayBLMock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateHoliday(holiday);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateHoliday_NonExistentHoliday_ReturnsNotFound()
        {
            // Arrange
            var holiday = new Holiday { UID = "non-existent-uid", Name = "Updated Holiday", Date = DateTime.Now };
            _holidayBLMock.Setup(x => x.SelectHolidayByUID("non-existent-uid"))
                .ReturnsAsync((IHoliday)null);

            // Act
            var result = await _controller.UpdateHoliday(holiday);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteHoliday_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _holidayBLMock.Setup(x => x.DeleteHoliday("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteHoliday("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteHoliday_InvalidUID_ReturnsError()
        {
            // Arrange
            _holidayBLMock.Setup(x => x.DeleteHoliday("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteHoliday("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllHolidayDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var holidayList = new List<IHoliday>
            {
                new Holiday { UID = "holiday1", Name = "Holiday 1", Date = DateTime.Now },
                new Holiday { UID = "holiday2", Name = "Holiday 2", Date = DateTime.Now.AddDays(1) }
            };

            _holidayBLMock.Setup(x => x.SelectAllHolidayDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<IHoliday>
                {
                    PagedData = holidayList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllHolidayDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<IHoliday>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllHolidayDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllHolidayDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetHolidaysByDateRange_ValidRange_ReturnsHolidays()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(7);
            var holidayList = new List<IHoliday>
            {
                new Holiday { UID = "holiday1", Name = "Holiday 1", Date = startDate },
                new Holiday { UID = "holiday2", Name = "Holiday 2", Date = endDate }
            };

            _holidayBLMock.Setup(x => x.GetHolidaysByDateRange(startDate, endDate))
                .ReturnsAsync(holidayList);

            // Act
            var result = await _controller.GetHolidaysByDateRange(startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHolidays = Assert.IsType<List<IHoliday>>(okResult.Value);
            Assert.Equal(2, returnedHolidays.Count);
        }

        [Fact]
        public async Task GetHolidaysByDateRange_InvalidRange_ReturnsBadRequest()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(-1); // Invalid range

            // Act
            var result = await _controller.GetHolidaysByDateRange(startDate, endDate);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task IsHoliday_ValidDate_ReturnsTrue()
        {
            // Arrange
            var date = DateTime.Now;
            _holidayBLMock.Setup(x => x.IsHoliday(date))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IsHoliday(date);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task IsHoliday_NonHolidayDate_ReturnsFalse()
        {
            // Arrange
            var date = DateTime.Now;
            _holidayBLMock.Setup(x => x.IsHoliday(date))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.IsHoliday(date);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value);
        }
    }
} 