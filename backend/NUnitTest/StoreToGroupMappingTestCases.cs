
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
    public class StoreToGroupMappingTestCases
    {
        private StoreToGroupMappingController _storeToGroupMappingController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreToGroupMappingTestCases()
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
            services.AddSingleton<IStoreToGroupMapping, StoreToGroupMapping>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreToGroupMappingdltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreToGroupMappingDL);
            Winit.Modules.Store.DL.Interfaces.IStoreToGroupMappingDL StoreToGroupMappingRepository = (Winit.Modules.Store.DL.Interfaces.IStoreToGroupMappingDL)Activator.CreateInstance(StoreToGroupMappingdltype, configurationArgs);
            object[] storeToGroupMappingRepositoryArgs = new object[] { StoreToGroupMappingRepository };
            Type storeToGroupMappingblType = typeof(Winit.Modules.Store.BL.Classes.StoreToGroupMappingBL);
            Winit.Modules.Store.BL.Interfaces.IStoreToGroupMappingBL storeToGroupMappingService = (Winit.Modules.Store.BL.Interfaces.IStoreToGroupMappingBL)Activator.CreateInstance(storeToGroupMappingblType, storeToGroupMappingRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeToGroupMappingController = new StoreToGroupMappingController(storeToGroupMappingService, cacheService);
        }

        [Test]
        public async Task GetStoreToGroupMappingDetails_WithValidData_ReturnsStoreToGroupMappingDetails()
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
            var result = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreToGroupMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeToGroupMappingList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeToGroupMappingList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeToGroupMappingList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreToGroupMappingDetails_WithEmptyFilterCriteria_ReturnsStoreToGroupMappingDetails()
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
            var result = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreToGroupMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreToGroupMappingDetails_WithEmptySortCriteria_ReturnsStoreToGroupMappingDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreToGroupMappingName",
                Value = "New Zealand StoreToGroupMapping",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreToGroupMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreToGroupMappingDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreToGroupMapping NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve StoreToGroupMapping  Details", result.Value);
        }

        [Test]
        public async Task GetStoreToGroupMappingDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreToGroupMappingDetails()
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
                  Name = "StoreToGroupMappingName",
                  Value = "ABN AMRO StoreToGroupMapping NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve StoreToGroupMapping  Details", result.Value);
        }

        [Test]
        public async Task GetStoreToGroupMappingDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreToGroupMappingName",
                   Value = "ABN AMRO StoreToGroupMapping NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeToGroupMappingController.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreToGroupMappingDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "49372edf-b49d-43f6-b903-01f5b9a90152";
            IActionResult result = await _storeToGroupMappingController.SelectStoreToGroupMappingByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreToGroupMapping storeToGroupMapping = okObjectResult.Value as StoreToGroupMapping;
                StoreToGroupMapping storeToGroupMapping = okObjectResult.Value as StoreToGroupMapping;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeToGroupMapping);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreToGroupMapping Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreToGroupMappingDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeToGroupMappingController.SelectStoreToGroupMappingByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreToGroupMapping_ReturnsCreatedResultWithstoreToGroupMappingObject()
        {
            var storeToGroupMapping = new StoreToGroupMapping
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreGroupUID = "wbywgeef",
                StoreUID = "jkghfwigf"
            };
            var result = await _storeToGroupMappingController.CreateStoreToGroupMapping(storeToGroupMapping) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreToGroupMapping_ReturnsConflictResultWhenStoreToGroupMappingUIDAlreadyExists()
        {
            var existingStoreToGroupMapping = new StoreToGroupMapping
            {
                UID = "49372edf-b49d-43f6-b903-01f5b9a90152",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreGroupUID = "wbywgeef",
                StoreUID = "jkghfwigf"
            };
            var actionResult = await _storeToGroupMappingController.CreateStoreToGroupMapping(existingStoreToGroupMapping) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreToGroupMapping_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreToGroupMapping = new StoreToGroupMapping
            {
                // Missing required fields
            };
            var actionResult = await _storeToGroupMappingController.CreateStoreToGroupMapping(invalidStoreToGroupMapping) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreToGroupMapping_SuccessfulUpdate_ReturnsOkWithUpdatedstoreToGroupMappingdetails()
        {
            var storeToGroupMapping = new StoreToGroupMapping
            {
                UID = "49372edf-b49d-43f6-b903-01f5b9a90152",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                StoreGroupUID = "wbywgeef",
                StoreUID = "jkghfwigf"
            };
            var result = await _storeToGroupMappingController.UpdateStoreToGroupMapping(storeToGroupMapping) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreToGroupMappingdetails_NotFound_ReturnsNotFound()
        {
            var storeToGroupMapping = new StoreToGroupMapping
            {
                UID = "NDFHN343",
            };
            var result = await _storeToGroupMappingController.UpdateStoreToGroupMapping(storeToGroupMapping);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreToGroupMappingDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeToGroupMappingController.DeleteStoreToGroupMapping(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreToGroupMappingDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeToGroupMappingController.DeleteStoreToGroupMapping(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















