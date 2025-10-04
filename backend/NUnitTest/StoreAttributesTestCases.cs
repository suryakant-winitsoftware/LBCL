
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
    public class StoreAttributesTestCases
    {
        private StoreAttributesController _storeAttributesController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreAttributesTestCases()
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
            services.AddSingleton<IStoreAttributes, StoreAttributes>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreAttributesdltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreAttributesDL);
            Winit.Modules.Store.DL.Interfaces.IStoreAttributesDL StoreAttributesRepository = (Winit.Modules.Store.DL.Interfaces.IStoreAttributesDL)Activator.CreateInstance(StoreAttributesdltype, configurationArgs);
            object[] storeAttributesRepositoryArgs = new object[] { StoreAttributesRepository };
            Type storeAttributesblType = typeof(Winit.Modules.Store.BL.Classes.StoreAttributesBL);
            Winit.Modules.Store.BL.Interfaces.IStoreAttributesBL storeAttributesService = (Winit.Modules.Store.BL.Interfaces.IStoreAttributesBL)Activator.CreateInstance(storeAttributesblType, storeAttributesRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeAttributesController = new StoreAttributesController(storeAttributesService, cacheService);
        }

        [Test]
        public async Task GetStoreAttributesDetails_WithValidData_ReturnsStoreAttributesDetails()
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
            var result = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAttributes>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeAttributesList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeAttributesList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeAttributesList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreAttributesDetails_WithEmptyFilterCriteria_ReturnsStoreAttributesDetails()
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
            var result = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAttributes>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreAttributesDetails_WithEmptySortCriteria_ReturnsStoreAttributesDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreAttributesName",
                Value = "New Zealand StoreAttributes",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAttributes>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreAttributesDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreAttributes NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Attributes Details", result.Value);
        }

        [Test]
        public async Task GetStoreAttributesDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreAttributesDetails()
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
                  Name = "StoreAttributesName",
                  Value = "ABN AMRO StoreAttributes NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Attributes Details", result.Value);
        }

        [Test]
        public async Task GetStoreAttributesDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreAttributesName",
                   Value = "ABN AMRO StoreAttributes NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeAttributesController.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreAttributesDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "33477ad8-4d93-42cf-a813-6748116fc159";
            IActionResult result = await _storeAttributesController.SelectStoreAttributesByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreAttributes storeAttributes = okObjectResult.Value as StoreAttributes;
                StoreAttributes storeAttributes = okObjectResult.Value as StoreAttributes;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeAttributes);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreAttributes Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreAttributesDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeAttributesController.SelectStoreAttributesByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreAttributes_ReturnsCreatedResultWithstoreAttributesObject()
        {
            var storeAttributes = new StoreAttributes
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                CompanyUID = "ofhfouoeu",
                OrgUID = "0834yf304g3",
                DistributionChannelUID = "034534jf304yt0f",
                StoreUID = "gwrrwer",
                Value = "jefbofjs",
                ParentName = "jibeubfg"
            };
            var result = await _storeAttributesController.CreateStoreAttributes(storeAttributes) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreAttributes_ReturnsConflictResultWhenStoreAttributesUIDAlreadyExists()
        {
            var existingStoreAttributes = new StoreAttributes
            {
                UID = "33477ad8-4d93-42cf-a813-6748116fc159",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                CompanyUID = "ofhfouoeu",
                OrgUID = "0834yf304g3",
                DistributionChannelUID = "034534jf304yt0f",
                StoreUID = "gwrrwer",
                Value = "jefbofjs",
                ParentName = "jibeubfg"
            };
            var actionResult = await _storeAttributesController.CreateStoreAttributes(existingStoreAttributes) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreAttributes_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreAttributes = new StoreAttributes
            {
                // Missing required fields
            };
            var actionResult = await _storeAttributesController.CreateStoreAttributes(invalidStoreAttributes) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreAttributes_SuccessfulUpdate_ReturnsOkWithUpdatedstoreAttributesdetails()
        {
            var storeAttributes = new StoreAttributes
            {
                UID = "33477ad8-4d93-42cf-a813-6748116fc159",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                CompanyUID = "ofhfouoeu",
                OrgUID = "0834yf304g3",
                DistributionChannelUID = "034534jf304yt0f",
                StoreUID = "gwrrwer",
                Value = "jefbofjs",
                ParentName = "jibeubfg"
            };
            var result = await _storeAttributesController.UpdateStoreAttributes(storeAttributes) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreAttributesdetails_NotFound_ReturnsNotFound()
        {
            var storeAttributes = new StoreAttributes
            {
                UID = "NDFHN343",
            };
            var result = await _storeAttributesController.UpdateStoreAttributes(storeAttributes);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreAttributesDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeAttributesController.DeleteStoreAttributes(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreAttributesDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeAttributesController.DeleteStoreAttributes(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















