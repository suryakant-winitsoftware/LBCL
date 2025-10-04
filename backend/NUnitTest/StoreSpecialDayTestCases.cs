
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
using WINITAPI.Controllers.Store;
using Winit.Shared.Models.Enums;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using StackExchange.Redis;

namespace NunitTest
{
    [TestFixture]
    public class StoreSpecialDayTestCases
    {
        private StoreSpecialDayController _storeSpecialDayController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreSpecialDayTestCases()
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
            services.AddSingleton<IStoreSpecialDay, StoreSpecialDay>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreSpecialDaydltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreSpecialDayDL);
            Winit.Modules.Store.DL.Interfaces.IStoreSpecialDayDL StoreSpecialDayRepository = (Winit.Modules.Store.DL.Interfaces.IStoreSpecialDayDL)Activator.CreateInstance(StoreSpecialDaydltype, configurationArgs);
            object[] storeSpecialDayRepositoryArgs = new object[] { StoreSpecialDayRepository };
            Type storeSpecialDayblType = typeof(Winit.Modules.Store.BL.Classes.StoreSpecialDayBL);
            Winit.Modules.Store.BL.Interfaces.IStoreSpecialDayBL storeSpecialDayService = (Winit.Modules.Store.BL.Interfaces.IStoreSpecialDayBL)Activator.CreateInstance(storeSpecialDayblType, storeSpecialDayRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeSpecialDayController = new StoreSpecialDayController(storeSpecialDayService, cacheService);
        }

        [Test]
        public async Task GetStoreSpecialDayDetails_WithValidData_ReturnsStoreSpecialDayDetails()
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
                      Name = @"CreatedBy",
                      Value = "John Doe",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreSpecialDay>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeSpecialDayList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeSpecialDayList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeSpecialDayList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreSpecialDayDetails_WithEmptyFilterCriteria_ReturnsStoreSpecialDayDetails()
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
            var result = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreSpecialDay>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreSpecialDayDetails_WithEmptySortCriteria_ReturnsStoreSpecialDayDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreSpecialDayName",
                Value = "New Zealand StoreSpecialDay",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreSpecialDay>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreSpecialDayDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreSpecialDay NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Special Day  Details", result.Value);
        }

        [Test]
        public async Task GetStoreSpecialDayDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreSpecialDayDetails()
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
                  Name = "StoreSpecialDayName",
                  Value = "ABN AMRO StoreSpecialDay NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Special Day  Details", result.Value);
        }

        [Test]
        public async Task GetStoreSpecialDayDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreSpecialDayName",
                   Value = "ABN AMRO StoreSpecialDay NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeSpecialDayController.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreSpecialDayDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "example_uidd";
            IActionResult result = await _storeSpecialDayController.SelectStoreSpecialDayByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreSpecialDay storeSpecialDay = okObjectResult.Value as StoreSpecialDay;
                StoreSpecialDay storeSpecialDay = okObjectResult.Value as StoreSpecialDay;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeSpecialDay);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreSpecialDay Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreSpecialDayDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeSpecialDayController.SelectStoreSpecialDayByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreSpecialDay_ReturnsCreatedResultWithstoreSpecialDayObject()
        {
            var storeSpecialDay = new StoreSpecialDay
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreUID = "hfgwefyew",
                DayType = "cbiewgf",
                SpecialDay = DateTime.Now
            };
            var result = await _storeSpecialDayController.CreateStoreSpecialDay(storeSpecialDay) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreSpecialDay_ReturnsConflictResultWhenStoreSpecialDayUIDAlreadyExists()
        {
            var existingStoreSpecialDay = new StoreSpecialDay
            {
                UID = "example_uidd",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreUID = "hfgwefyew",
                DayType = "cbiewgf",
                SpecialDay = DateTime.Now
            };
            var actionResult = await _storeSpecialDayController.CreateStoreSpecialDay(existingStoreSpecialDay) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreSpecialDay_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreSpecialDay = new StoreSpecialDay
            {
                // Missing required fields
            };
            var actionResult = await _storeSpecialDayController.CreateStoreSpecialDay(invalidStoreSpecialDay) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreSpecialDay_SuccessfulUpdate_ReturnsOkWithUpdatedstoreSpecialDaydetails()
        {
            var storeSpecialDay = new StoreSpecialDay
            {
                UID = "example_uidd",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                StoreUID = "hfgwefyew",
                DayType = "cbiewgf",
                SpecialDay = DateTime.Now
            };
            var result = await _storeSpecialDayController.UpdateStoreSpecialDay(storeSpecialDay) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreSpecialDaydetails_NotFound_ReturnsNotFound()
        {
            var storeSpecialDay = new StoreSpecialDay
            {
                UID = "NDFHN343",
            };
            var result = await _storeSpecialDayController.UpdateStoreSpecialDay(storeSpecialDay);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreSpecialDayDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeSpecialDayController.DeleteStoreSpecialDay(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreSpecialDayDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeSpecialDayController.DeleteStoreSpecialDay(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















