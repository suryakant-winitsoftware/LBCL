
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

namespace NunitTest
{
    [TestFixture]
    public class StoreTestCases
    {
        private StoreController _storeController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreTestCases()
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
            services.AddSingleton<IStore, Store>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type Storedltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreDL);
            Winit.Modules.Store.DL.Interfaces.IStoreDL StoreRepository = (Winit.Modules.Store.DL.Interfaces.IStoreDL)Activator.CreateInstance(Storedltype, configurationArgs);
            object[] storeRepositoryArgs = new object[] { StoreRepository };
            Type storeblType = typeof(Winit.Modules.Store.BL.Classes.StoreBL);
            Winit.Modules.Store.BL.Interfaces.IStoreBL storeService = (Winit.Modules.Store.BL.Interfaces.IStoreBL)Activator.CreateInstance(storeblType, storeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeController = new StoreController(storeService, cacheService);
        }

        [Test]
        public async Task GetStoreDetails_WithValidData_ReturnsStoreDetails()
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
            var result = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStore>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreDetails_WithEmptyFilterCriteria_ReturnsStoreDetails()
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
            var result = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStore>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreDetails_WithEmptySortCriteria_ReturnsStoreDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreName",
                Value = "New Zealand Store",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStore>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Store NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store  Details", result.Value);
        }

        [Test]
        public async Task GetStoreDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreDetails()
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
                  Name = "StoreName",
                  Value = "ABN AMRO Store NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store  Details", result.Value);
        }

        [Test]
        public async Task GetStoreDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreName",
                   Value = "ABN AMRO Store NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeController.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "976655f0-01fe-404e-a5bf-efd0fc7f6732";
            IActionResult result = await _storeController.SelectStoreByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //Store store = okObjectResult.Value as Store;
                Store store = okObjectResult.Value as Store;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(store);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Store Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeController.SelectStoreByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task Createstore_ReturnsCreatedResultWithstoreObject()
        {
            var store = new Store
            {
                UID = this.UID,
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                CompanyUID = "fufwe",
                Code = "jbu34u",
                Number = "rfgsdff",
                AliasName = "Srinadh",
                BillToStoreUID = "wrfbdffiylw",
                ShipToStoreUID = "fwekgb;wef",
                SoldToStoreUID = "jebgfyivgfbiwe",
                Status = 1,
                IsActive = true,
                StoreClass = "fwuiwe",
                StoreRating = "good",
                IsBlocked = false,
                BlockedReasonCode = "ouwgwkefi",
                BlockedReasonDescription = "very bad service",
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                CountryUID = "12312jqw",
                RegionUID = "fnwruig;w",
                CityUID = "uwfsudfg",
                Source = "fgwieueiw;fgagf",
                storeCredit = new List<string>(new string[] { "Srinadh", "Ram,ana", "selava" }),
            };
            var result = await _storeController.CreateStore(store) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStore_ReturnsConflictResultWhenStoreUIDAlreadyExists()
        {
            var existingStore = new Store
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                CompanyUID = "fufwe",
                Code = "jbu34u",
                Number = "rfgsdff",
                AliasName = "Srinadh",
                BillToStoreUID = "wrfbdffiylw",
                ShipToStoreUID = "fwekgb;wef",
                SoldToStoreUID = "jebgfyivgfbiwe",
                Status = 1,
                IsActive = true,
                StoreClass = "fwuiwe",
                StoreRating = "good",
                IsBlocked = false,
                BlockedReasonCode = "ouwgwkefi",
                BlockedReasonDescription = "very bad service",
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                CountryUID = "12312jqw",
                RegionUID = "fnwruig;w",
                CityUID = "uwfsudfg",
                Source = "fgwieueiw;fgagf",
                storeCredit = new List<string> (new string[]{ "Srinadh", "Ram,ana", "selava" })
            };
            var actionResult = await _storeController.CreateStore(existingStore) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStore_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStore = new Store
            {
                // Missing required fields
            };
            var actionResult = await _storeController.CreateStore(invalidStore) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task Updatestore_SuccessfulUpdate_ReturnsOkWithUpdatedstoredetails()
        {
            var store = new Store
            {
                UID = "976655f0-01fe-404e-a5bf-efd0fc7f6732",
                Type = "NZ",
                Name = "paid",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CompanyUID = "fufwe",
                Code = "jbu34u",
                Number = "rfgsdff",
                AliasName = "Srinadh",
                BillToStoreUID = "wrfbdffiylw",
                ShipToStoreUID = "fwekgb;wef",
                SoldToStoreUID = "jebgfyivgfbiwe",
                Status = 1,
                IsActive = true,
                StoreClass = "fwuiwe",
                StoreRating = "good",
                IsBlocked = false,
                BlockedReasonCode = "ouwgwkefi",
                BlockedReasonDescription = "very bad service",
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                CountryUID = "12312jqw",
                RegionUID = "fnwruig;w",
                CityUID = "uwfsudfg",
                Source = "fgwieueiw;fgagf",
                storeCredit = new List<string>(new string[] { "Srinadh", "Ram,ana", "selava" })
            };
            var result = await _storeController.UpdateStore(store) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoredetails_NotFound_ReturnsNotFound()
        {
            var store = new Store
            {
                UID = "NDFHN343",
            };
            var result = await _storeController.UpdateStore(store);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeController.DeleteStore(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeController.DeleteStore(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















