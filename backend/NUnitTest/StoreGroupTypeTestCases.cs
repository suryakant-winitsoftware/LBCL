
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
    public class StoreGroupTypeTestCases
    {
        private StoreGroupTypeController _storeGroupTypeController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreGroupTypeTestCases()
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
            services.AddSingleton<IStoreGroupType, StoreGroupType>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreGroupTypedltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreGroupTypeDL);
            Winit.Modules.Store.DL.Interfaces.IStoreGroupTypeDL StoreGroupTypeRepository = (Winit.Modules.Store.DL.Interfaces.IStoreGroupTypeDL)Activator.CreateInstance(StoreGroupTypedltype, configurationArgs);
            object[] storeGroupTypeRepositoryArgs = new object[] { StoreGroupTypeRepository };
            Type storeGroupTypeblType = typeof(Winit.Modules.Store.BL.Classes.StoreGroupTypeBL);
            Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL storeGroupTypeService = (Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL)Activator.CreateInstance(storeGroupTypeblType, storeGroupTypeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeGroupTypeController = new StoreGroupTypeController(storeGroupTypeService, cacheService);
        }

        [Test]
        public async Task GetStoreGroupTypeDetails_WithValidData_ReturnsStoreGroupTypeDetails()
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
            var result = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreGroupType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeGroupTypeList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeGroupTypeList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeGroupTypeList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreGroupTypeDetails_WithEmptyFilterCriteria_ReturnsStoreGroupTypeDetails()
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
            var result = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreGroupType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreGroupTypeDetails_WithEmptySortCriteria_ReturnsStoreGroupTypeDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreGroupTypeName",
                Value = "New Zealand StoreGroupType",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreGroupType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreGroupTypeDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreGroupType NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store GroupType Details", result.Value);
        }

        [Test]
        public async Task GetStoreGroupTypeDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreGroupTypeDetails()
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
                  Name = "StoreGroupTypeName",
                  Value = "ABN AMRO StoreGroupType NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store GroupType Details", result.Value);
        }

        [Test]
        public async Task GetStoreGroupTypeDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreGroupTypeName",
                   Value = "ABN AMRO StoreGroupType NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeGroupTypeController.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreGroupTypeDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "unique_id_1231";
            IActionResult result = await _storeGroupTypeController.SelectStoreGroupTypeByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreGroupType storeGroupType = okObjectResult.Value as StoreGroupType;
                StoreGroupType storeGroupType = okObjectResult.Value as StoreGroupType;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeGroupType);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreGroupType Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreGroupTypeDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeGroupTypeController.SelectStoreGroupTypeByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreGroupType_ReturnsCreatedResultWithstoreGroupTypeObject()
        {
            var storeGroupType = new StoreGroupType
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Name = "Srinadh",
                ParentUID = "lwqyr923rh2gr29yrwd",
                CompanyUID ="oqwrguqwg",
                OrgUID = "bi9wqgfweif",
                DistributionChannelUID = "fliweysahdfvpwyeyfaakcrshnfbgvfiy",
                LevelNo = 7
            };
            var result = await _storeGroupTypeController.CreateStoreGroupType(storeGroupType) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreGroupType_ReturnsConflictResultWhenStoreGroupTypeUIDAlreadyExists()
        {
            var existingStoreGroupType = new StoreGroupType
            {
                UID = "449f67f0-93df-48c5-ae23-56dfd3251cb0",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Name = "Srinadh",
                ParentUID = "lwqyr923rh2gr29yrwd",
                CompanyUID = "oqwrguqwg",
                OrgUID = "bi9wqgfweif",
                DistributionChannelUID = "fliweysahdfvpwyeyfaakcrshnfbgvfiy",
                LevelNo = 7
            };
            var actionResult = await _storeGroupTypeController.CreateStoreGroupType(existingStoreGroupType) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreGroupType_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreGroupType = new StoreGroupType
            {
                // Missing required fields
            };
            var actionResult = await _storeGroupTypeController.CreateStoreGroupType(invalidStoreGroupType) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreGroupType_SuccessfulUpdate_ReturnsOkWithUpdatedstoreGroupTypedetails()
        {
            var storeGroupType = new StoreGroupType
            {
                UID = "unique_id_1231",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                Name = "Srinadh",
                ParentUID = "lwqyr923rh2gr29yrwd",
                CompanyUID = "oqwrguqwg",
                OrgUID = "bi9wqgfweif",
                DistributionChannelUID = "fliweysahdfvpwyeyfaakcrshnfbgvfiy",
                LevelNo = 7
            };
            var result = await _storeGroupTypeController.UpdateStoreGroupType(storeGroupType) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreGroupTypedetails_NotFound_ReturnsNotFound()
        {
            var storeGroupType = new StoreGroupType
            {
                UID = "NDFHN343",
            };
            var result = await _storeGroupTypeController.UpdateStoreGroupType(storeGroupType);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreGroupTypeDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeGroupTypeController.DeleteStoreGroupType(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreGroupTypeDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeGroupTypeController.DeleteStoreGroupType(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















