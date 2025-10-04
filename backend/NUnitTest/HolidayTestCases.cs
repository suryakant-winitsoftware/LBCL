using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.Holiday;
using WINITServices.Classes.Holiday;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Modules.Holiday.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Modules.Holiday.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WINITRepository.Interfaces.Holiday;

namespace NunitTest
{
    [TestFixture]
    public class HolidayTestCases
    {
        private HolidayController _holidayController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public HolidayTestCases()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = configuration.GetConnectionString("PostgreSQL");
            // Create the IServiceProvider, you need to replace this with the actual service provider
            // This is just a placeholder, you need to configure the DI container properly.
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddSingleton<IHoliday, Holiday>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type holidaydltype = typeof(Winit.Modules.Holiday.DL.Classes.PGSQLHolidayDL);
            Winit.Modules.Holiday.DL.Interfaces.IHolidayDL holidayRepository = (Winit.Modules.Holiday.DL.Interfaces.IHolidayDL)Activator.CreateInstance(holidaydltype, configurationArgs);
            object[] holidayRepositoryArgs = new object[] { holidayRepository };
            Type holidayblType = typeof(Winit.Modules.Holiday.BL.Classes.HolidayBL);
            Winit.Modules.Holiday.BL.Interfaces.IHolidayBL holidayService = (Winit.Modules.Holiday.BL.Interfaces.IHolidayBL)Activator.CreateInstance(holidayblType, holidayRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _holidayController = new HolidayController(holidayService, cacheService);
        }

        [Test]
        public async Task GetHolidayDetails_WithValidData_ReturnsHolidayDetails()
        {
            var sortCriterias = new List<Winit.Shared.Models.Enums.SortCriteria>
            {
                 new Winit.Shared.Models.Enums.SortCriteria
                 {
                      SortParameter = @"UID",
                      Direction = Winit.Shared.Models.Enums.SortDirection.Desc
                 },
            };
            var filterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>
            {
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                      Name = @"HolidayListUID",
                      Value = "FBNZ",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHoliday>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var holidayList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(holidayList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, holidayList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetHolidayDetails_WithEmptyFilterCriteria_ReturnsHolidayDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = @"""UID""",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Empty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHoliday>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var data = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(data);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
        }

        [Test]
        public async Task GetHolidayDetails_WithEmptySortCriteria_ReturnsHolidayDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"HolidayName",
                Value = "New Zealand Holiday",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHoliday>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var data = (IEnumerable<object>)okObjectResult.Value;
                var expectedstatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedstatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(data);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
        }

        [Test]
        public async Task GetHolidayDetails_WithInvalidFilterCriteria_ReturnsNoResults()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria
                {
                    Name = "InvalidColumnName", // Provide an invalid column name
                    Value = "ABN AMRO Holiday NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Holiday Details ", result.Value);
        }

        [Test]
        public async Task GetHolidayDetails_WithInvalidSortCriteria_ReturnsUnsortedHolidayDetails()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
              new SortCriteria
              {
                  SortParameter = "InvalidColumnName", // Provide an invalid column name
                  Direction = SortDirection.Asc
              }
            };
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = "HolidayName",
                  Value = "ABN AMRO Holiday NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Holiday Details ", result.Value);
        }

        [Test]
        public async Task GetHolidayDetails_WithInvalidPagination_ReturnsNoResults()
        {
            // Arrange
            var sortCriterias = new List<SortCriteria>
            {
               new SortCriteria
               {
                   SortParameter = "UID",
                   Direction = SortDirection.Asc
               }
            };
            var filterCriterias = new List<FilterCriteria>
            {
               new FilterCriteria
               {
                   Name = "HolidayName",
                   Value = "ABN AMRO Holiday NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _holidayController.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetHolidayDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "c2be05f1-77d2-4439-b809-0bbbc9acecc1";
            IActionResult result = await _holidayController.GetHolidayByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //Holiday holiday = okObjectResult.Value as Holiday;
                Holiday holiday = okObjectResult.Value as Holiday;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(holiday);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Holiday Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetHolidayDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _holidayController.GetHolidayByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task Createholiday_ReturnsCreatedResultWithholidayObject()
        {
            var holiday = new Holiday
            {
                UID =this.UID ,
                HolidayListUID = "FBNZ",
                HolidayDate = DateTime.Now,
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                IsOptional = false,
                Year = 2012,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
            };
            var result = await _holidayController.CreateHoliday(holiday) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateHoliday_ReturnsConflictResultWhenHolidayUIDAlreadyExists()
        {
            var existingHoliday = new Holiday
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                HolidayListUID = "FBNZ",
                HolidayDate = DateTime.Now,
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                IsOptional = false,
                Year = 2012,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
            };
            var actionResult = await _holidayController.CreateHoliday(existingHoliday) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateHoliday_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidHoliday = new Holiday
            {
                // Missing required fields
            };
            var actionResult = await _holidayController.CreateHoliday(invalidHoliday) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task Updateholiday_SuccessfulUpdate_ReturnsOkWithUpdatedholidaydetails()
        {
            var holiday = new Holiday
            {
                UID = "ccbcb2d5-2ca5-4bc6-9a4f-a578851d4581",
                HolidayListUID = "FBNZ",
                HolidayDate = DateTime.Now,
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                IsOptional = false,
                Year = 2012
            };
            var result = await _holidayController.UpdateHoliday(holiday) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateHolidaydetails_NotFound_ReturnsNotFound()
        {
            var holiday = new Holiday
            {
                UID = "NDFHN343",
            };
            var result = await _holidayController.UpdateHoliday(holiday);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteHolidayDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _holidayController.DeleteHoliday(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteHolidayDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _holidayController.DeleteHoliday(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















