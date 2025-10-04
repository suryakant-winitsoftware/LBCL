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
using WINITAPI.Controllers.SKUClass;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.SKUClass.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NUnitTest
{
    [TestFixture]
    public class SKUClassGroupItemsTestCases
    {
        private SKUClassGroupItemsController _sKUClassGroupItemsController;
        public readonly string _connectionString;

        public SKUClassGroupItemsTestCases()
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
            services.AddSingleton<ISKUClassGroupItems, SKUClassGroupItems>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type sKUClassGroupItemsdltype = typeof(Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassGroupItemsDL);
            Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupItemsDL sKUClassGroupItemsRepository = (Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupItemsDL)Activator.CreateInstance(sKUClassGroupItemsdltype, configurationArgs);
            object[] sKUClassGroupItemsRepositoryArgs = new object[] { sKUClassGroupItemsRepository };

            Type sKUClassGroupItemsblType = typeof(Winit.Modules.SKUClass.BL.Classes.SKUClassGroupItemsBL);
            Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL sKUClassGroupItemsService = (Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL)Activator.CreateInstance(sKUClassGroupItemsblType, sKUClassGroupItemsRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _sKUClassGroupItemsController = new SKUClassGroupItemsController(sKUClassGroupItemsService, cacheService);

        }
        [Test]
        public async Task GetSKUClassGroupItemsDetails_WithValidData_ReturnsSKUClassGroupItemsDetails()
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
                      Name = @"SKUClassGroupUID",
                      Value ="codefdhhjd",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"SKUCode",
                     Value ="malfgthh",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroupItems>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var SKUClassGroupItemsList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClassGroupItemsList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassGroupItemsDetails_WithSKUClassGroupItemstyFilterCriteria_ReturnsSKUClassGroupItemsDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // SKUClassGroupItemsty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroupItems>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassGroupItemsDetails_WithSKUClassGroupItemstySortCriteria_ReturnsSKUClassGroupItemsDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // SKUClassGroupItemsty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"SKUClassGroupUID",
                   Value = "codefdhhjd",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroupItems>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassGroupItemsDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO SKUClassGroupItems No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClassGroupItems Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupItemsDetails_WithInvalidSortCriteria_ReturnsUnsortedSKUClassGroupItemsDetails()
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
                  Name = "SKUClassGroupUID",
                  Value = "ABN AMRO SKUClassGroupItems",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClassGroupItems Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupItemsDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "SKUClassGroupUID",
                   Value = "ABN AMRO SKUClassGroupItems",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupItemsController.SelectAllSKUClassGroupItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupItemsDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b55556";
            IActionResult result = await _sKUClassGroupItemsController.GetSKUClassGroupItemsByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                SKUClassGroupItems SKUClassGroupItems = okObjectResult.Value as SKUClassGroupItems;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClassGroupItems);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("SKUClassGroupItems Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassGroupItemsDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _sKUClassGroupItemsController.GetSKUClassGroupItemsByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateSKUClassGroupItems_ReturnsCreatedResultWithSKUClassGroupItemsObject()
        {
            var SKUClassGroupItems = new SKUClassGroupItems
            {
                UID = Guid.NewGuid().ToString(),
                SKUClassGroupUID = "1234",
                SKUCode = "6789",
                SerialNumber=4576,
                ModelQty = 3,
                ModelUoM = "sdfasd",
                SupplierOrgUID = "qa",
                LeadTimeInDays = 36,
                DailyCutOffTime = "sdzfx",
                IsExclusive = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassGroupItemsController.CreateSKUClassGroupItems(SKUClassGroupItems) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateSKUClassGroupItems_ReturnsConflictResultWhenSKUClassGroupItemsUIDAlreadyExists()
        {
            var existingSKUClassGroupItems = new SKUClassGroupItems
            {
                UID = "2d893d92-dc1b555567888",
                SKUClassGroupUID = "1234",
                SKUCode = "6789",
                SerialNumber = 4576,
                ModelQty = 3,
                ModelUoM = "sdfasd",
                SupplierOrgUID = "qa",
                LeadTimeInDays = 36,
                DailyCutOffTime = "sdzfx",
                IsExclusive = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _sKUClassGroupItemsController.CreateSKUClassGroupItems(existingSKUClassGroupItems) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateSKUClassGroupItems_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidSKUClassGroupItems = new SKUClassGroupItems
            {
                // Missing required fields
            };
            var actionResult = await _sKUClassGroupItemsController.CreateSKUClassGroupItems(invalidSKUClassGroupItems) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateSKUClassGroupItems_SuccessfulUpdate_ReturnsOkWithUpdatedSKUClassGroupItemsdetails()
        {
            var SKUClassGroupItems = new SKUClassGroupItems
            {
                UID = "2d893d92-dc1b55556788123338",
                SKUClassGroupUID = "1234",
                SKUCode = "6789",
                SerialNumber = 4576,
                ModelQty = 3,
                ModelUoM = "sdfasd",
                SupplierOrgUID = "qa",
                LeadTimeInDays = 36,
                DailyCutOffTime = "sdzfx",
                IsExclusive = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassGroupItemsController.UpdateSKUClassGroupItems(SKUClassGroupItems) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateSKUClassGroupItemsdetails_NotFound_ReturnsNotFound()
        {
            var SKUClassGroupItems = new SKUClassGroupItems
            {
                UID = "NDFHN343",
            };
            var result = await _sKUClassGroupItemsController.UpdateSKUClassGroupItems(SKUClassGroupItems);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteSKUClassGroupItemsDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b5555678812333800976";
            var result = await _sKUClassGroupItemsController.DeleteSKUClassGroupItems(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteSKUClassGroupItemsDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _sKUClassGroupItemsController.DeleteSKUClassGroupItems(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
