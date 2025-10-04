
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
    public class StoreCreditTestCases
    {
        private StoreCreditController _storeCreditController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreCreditTestCases()
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
            services.AddSingleton<IStoreCredit, StoreCredit>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreCreditdltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreCreditDL);
            Winit.Modules.Store.DL.Interfaces.IStoreCreditDL StoreCreditRepository = (Winit.Modules.Store.DL.Interfaces.IStoreCreditDL)Activator.CreateInstance(StoreCreditdltype, configurationArgs);
            object[] storeCreditRepositoryArgs = new object[] { StoreCreditRepository };
            Type storeCreditblType = typeof(Winit.Modules.Store.BL.Classes.StoreCreditBL);
            Winit.Modules.Store.BL.Interfaces.IStoreCreditBL storeCreditService = (Winit.Modules.Store.BL.Interfaces.IStoreCreditBL)Activator.CreateInstance(storeCreditblType, storeCreditRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeCreditController = new StoreCreditController(storeCreditService, cacheService);
        }

        [Test]
        public async Task GetStoreCreditDetails_WithValidData_ReturnsStoreCreditDetails()
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
            var result = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreCredit>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeCreditList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeCreditList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeCreditList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreCreditDetails_WithEmptyFilterCriteria_ReturnsStoreCreditDetails()
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
            var result = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreCredit>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreCreditDetails_WithEmptySortCriteria_ReturnsStoreCreditDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreCreditName",
                Value = "New Zealand StoreCredit",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreCredit>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreCreditDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreCredit NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Credit  Details", result.Value);
        }

        [Test]
        public async Task GetStoreCreditDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreCreditDetails()
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
                  Name = "StoreCreditName",
                  Value = "ABN AMRO StoreCredit NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Credit  Details", result.Value);
        }

        [Test]
        public async Task GetStoreCreditDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreCreditName",
                   Value = "ABN AMRO StoreCredit NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeCreditController.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreCreditDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "449f67f0-93df-48c5-ae23-56dfd3251cb0";
            IActionResult result = await _storeCreditController.SelectStoreCreditByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreCredit storeCredit = okObjectResult.Value as StoreCredit;
                StoreCredit storeCredit = okObjectResult.Value as StoreCredit;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeCredit);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreCredit Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreCreditDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeCreditController.SelectStoreCreditByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreCredit_ReturnsCreatedResultWithstoreCreditObject()
        {
            var storeCredit = new StoreCredit
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                IsActive = true,
                IsBlocked = false,
                StoreUID = "wyvasfqw",
                PaymentTermUID = 7,
                CreditType = "Quaterly",
                CreditLimit = 40000,
                TemporaryCredit = 3000,
                OrgUID = "wokfosdfb",
                DistributionChannelUID = "yfgkfbyivbogqnkh",
                PreferredPaymentMode = "Cash",
                BlockingReasonCode = "siudgfwiefwei",
                BlockingReasonDescription = "Very Bad Service"
            };
            var result = await _storeCreditController.CreateStoreCredit(storeCredit) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreCredit_ReturnsConflictResultWhenStoreCreditUIDAlreadyExists()
        {
            var existingStoreCredit = new StoreCredit
            {
                UID = "449f67f0-93df-48c5-ae23-56dfd3251cb0",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                IsActive = true,
                IsBlocked = false,
                StoreUID = "wyvasfqw",
                PaymentTermUID = 7,
                CreditType = "Quaterly",
                CreditLimit = 40000,
                TemporaryCredit = 3000,
                OrgUID = "wokfosdfb",
                DistributionChannelUID = "yfgkfbyivbogqnkh",
                PreferredPaymentMode = "Cash",
                BlockingReasonCode = "siudgfwiefwei",
                BlockingReasonDescription = "Very Bad Service"
            };
            var actionResult = await _storeCreditController.CreateStoreCredit(existingStoreCredit) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreCredit_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreCredit = new StoreCredit
            {
                // Missing required fields
            };
            var actionResult = await _storeCreditController.CreateStoreCredit(invalidStoreCredit) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreCredit_SuccessfulUpdate_ReturnsOkWithUpdatedstoreCreditdetails()
        {
            var storeCredit = new StoreCredit
            {
                UID = "449f67f0-93df-48c5-ae23-56dfd3251cb0",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                IsActive = true,
                IsBlocked = false,
                StoreUID = "wyvasfqw",
                PaymentTermUID = 7,
                CreditType = "Quaterly",
                CreditLimit = 40000,
                TemporaryCredit = 3000,
                OrgUID = "wokfosdfb",
                DistributionChannelUID = "yfgkfbyivbogqnkh",
                PreferredPaymentMode = "Cash",
                BlockingReasonCode = "siudgfwiefwei",
                BlockingReasonDescription = "Very Bad Service"
            };
            var result = await _storeCreditController.UpdateStoreCredit(storeCredit) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreCreditdetails_NotFound_ReturnsNotFound()
        {
            var storeCredit = new StoreCredit
            {
                UID = "NDFHN343",
            };
            var result = await _storeCreditController.UpdateStoreCredit(storeCredit);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreCreditDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeCreditController.DeleteStoreCredit(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreCreditDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeCreditController.DeleteStoreCredit(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















