
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
    public class StoreWeekOffTestCases
    {
        private StoreWeekOffController _storeWeekOffController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreWeekOffTestCases()
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
            services.AddSingleton<IStoreWeekOff, StoreWeekOff>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreWeekOffdltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreWeekOffDL);
            Winit.Modules.Store.DL.Interfaces.IStoreWeekOffDL StoreWeekOffRepository = (Winit.Modules.Store.DL.Interfaces.IStoreWeekOffDL)Activator.CreateInstance(StoreWeekOffdltype, configurationArgs);
            object[] storeWeekOffRepositoryArgs = new object[] { StoreWeekOffRepository };
            Type storeWeekOffblType = typeof(Winit.Modules.Store.BL.Classes.StoreWeekOffBL);
            Winit.Modules.Store.BL.Interfaces.IStoreWeekOffBL storeWeekOffService = (Winit.Modules.Store.BL.Interfaces.IStoreWeekOffBL)Activator.CreateInstance(storeWeekOffblType, storeWeekOffRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeWeekOffController = new StoreWeekOffController(storeWeekOffService, cacheService);
        }

        [Test]
        public async Task GetStoreWeekOffDetails_WithValidData_ReturnsStoreWeekOffDetails()
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
                      Value = "admin257",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreWeekOff>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeWeekOffList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeWeekOffList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeWeekOffList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreWeekOffDetails_WithEmptyFilterCriteria_ReturnsStoreWeekOffDetails()
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
            var result = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreWeekOff>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreWeekOffDetails_WithEmptySortCriteria_ReturnsStoreWeekOffDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreWeekOffName",
                Value = "New Zealand StoreWeekOff",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreWeekOff>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreWeekOffDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreWeekOff NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Group  Details", result.Value);
        }

        [Test]
        public async Task GetStoreWeekOffDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreWeekOffDetails()
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
                  Name = "StoreWeekOffName",
                  Value = "ABN AMRO StoreWeekOff NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Group  Details", result.Value);
        }

        [Test]
        public async Task GetStoreWeekOffDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreWeekOffName",
                   Value = "ABN AMRO StoreWeekOff NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeWeekOffController.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreWeekOffDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "08508797-d6eb-425c-abe0-58eee1dd177d";
            IActionResult result = await _storeWeekOffController.SelectStoreWeekOffByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreWeekOff storeWeekOff = okObjectResult.Value as StoreWeekOff;
                StoreWeekOff storeWeekOff = okObjectResult.Value as StoreWeekOff;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeWeekOff);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreWeekOff Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreWeekOffDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeWeekOffController.SelectStoreWeekOffByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreWeekOff_ReturnsCreatedResultWithstoreWeekOffObject()
        {
            var storeWeekOff = new StoreWeekOff
            {
                UID = this.UID,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                StoreUID = "e1f50b65-4a20-433d-8239-6320abebc24a",
                Sun = true,
                Mon = false,
                Tue = true,
                Wed = false,
                Thu = true,
                Fri = false,
                Sat = true
            };
            var result = await _storeWeekOffController.CreateStoreWeekOff(storeWeekOff) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreWeekOff_ReturnsConflictResultWhenStoreWeekOffUIDAlreadyExists()
        {
            var existingStoreWeekOff = new StoreWeekOff
            {
                UID = "08508797-d6eb-425c-abe0-58eee1dd177d",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                StoreUID = "e1f50b65-4a20-433d-8239-6320abebc24a",
                Sun = true,
                Mon = false,
                Tue = true,
                Wed = false,
                Thu = true,
                Fri = false,
                Sat = true
            };
            var actionResult = await _storeWeekOffController.CreateStoreWeekOff(existingStoreWeekOff) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreWeekOff_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreWeekOff = new StoreWeekOff
            {
                // Missing required fields
            };
            var actionResult = await _storeWeekOffController.CreateStoreWeekOff(invalidStoreWeekOff) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreWeekOff_SuccessfulUpdate_ReturnsOkWithUpdatedstoreWeekOffdetails()
        {
            var storeWeekOff = new StoreWeekOff
            {
                UID = "08508797-d6eb-425c-abe0-58eee1dd177d",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                StoreUID = "e1f50b65-4a20-433d-8239-6320abebc24a",
                Sun = true,
                Mon = false,
                Tue = true,
                Wed = false,
                Thu = true,
                Fri = false,
                Sat = true
            };
            var result = await _storeWeekOffController.UpdateStoreWeekOff(storeWeekOff) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreWeekOffdetails_NotFound_ReturnsNotFound()
        {
            var storeWeekOff = new StoreWeekOff
            {
                UID = "NDFHN343",
            };
            var result = await _storeWeekOffController.UpdateStoreWeekOff(storeWeekOff);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreWeekOffDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeWeekOffController.DeleteStoreWeekOff(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreWeekOffDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeWeekOffController.DeleteStoreWeekOff(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















