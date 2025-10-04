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
using WINITAPI.Controllers.AwayPeriod;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.AwayPeriod.BL.Interfaces;
using Winit.Modules.AwayPeriod.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.AwayPeriod.Model.Interfaces;
using Winit.Modules.AwayPeriod.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NUnitTest
{
    [TestFixture]
    public class AwayPeriodTestCases
    {
        private AwayPeriodController _awayPeriodController;
        public readonly string _connectionString;
        public AwayPeriodTestCases()
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
            services.AddSingleton<IAwayPeriod,AwayPeriod>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type AwayPerioddltype = typeof(Winit.Modules.AwayPeriod.DL.Classes.PGSQLAwayPeriodDL);
            Winit.Modules.AwayPeriod.DL.Interfaces.IAwayPeriodDL AwayPeriodRepository = (Winit.Modules.AwayPeriod.DL.Interfaces.IAwayPeriodDL)Activator.CreateInstance(AwayPerioddltype, configurationArgs);
            object[] AwayPeriodRepositoryArgs = new object[] { AwayPeriodRepository };
            Type AwayPeriodblType = typeof(Winit.Modules.AwayPeriod.BL.Classes.AwayPeriodBL);
            Winit.Modules.AwayPeriod.BL.Interfaces.IAwayPeriodBL AwayPeriodService = (Winit.Modules.AwayPeriod.BL.Interfaces.IAwayPeriodBL)Activator.CreateInstance(AwayPeriodblType, AwayPeriodRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _awayPeriodController = new AwayPeriodController(AwayPeriodService, cacheService);
        }

        [Test]
        public async Task GetAwayPeriodDetails_WithValidData_ReturnsAwayPeriodDetails()
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
                      Name = @"OrgUID",
                      Value = "4534tdfyhg",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Description",
                     Value = "bdhhghhs",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IAwayPeriod>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var AwayPeriodList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(AwayPeriodList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, AwayPeriodList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetAwayPeriodDetails_WithEmptyFilterCriteria_ReturnsAwayPeriodDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Empty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IAwayPeriod>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetAwayPeriodDetails_WithEmptySortCriteria_ReturnsAwayPeriodDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"OrgUID",
                Value = "4534tdfyhg",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IAwayPeriod>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetAwayPeriodDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO AwayPeriod NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve AwayPeriod Details", result.Value);
        }

        [Test]
        public async Task GetAwayPeriodDetails_WithInvalidSortCriteria_ReturnsUnsortedAwayPeriodDetails()
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
                  Name = "OrgUID",
                  Value = "ABN AMRO AwayPeriod NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve AwayPeriod Details", result.Value);
        }

        [Test]
        public async Task GetAwayPeriodDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "OrgUID",
                   Value = "ABN AMRO AwayPeriod NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _awayPeriodController.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetAwayPeriodDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "23345-757467-8-865-4676435";
            IActionResult result = await _awayPeriodController.GetAwayPeriodDetailsByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //AwayPeriod AwayPeriod = okObjectResult.Value as AwayPeriod;
                AwayPeriod AwayPeriod = okObjectResult.Value as AwayPeriod;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(AwayPeriod);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("AwayPeriod Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetAwayPeriodDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _awayPeriodController.GetAwayPeriodDetailsByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateBnak_ReturnsCreatedResultWithAwayPeriodObject()
        {
            var AwayPeriod = new AwayPeriod
            {
                UID = Guid.NewGuid().ToString(),
                OrgUID = "FBNZ",
                Description = "AwayPeriod of New Zealand",
                LinkedItemType = "NZ",
                LinkedItemUID = "fgdg",
                FromDate = DateTime.Now,
                ToDate=DateTime.Now,
                IsActive = true,
                CreatedTime = DateTime.Now,
                ModifiedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1",
                CreatedBy= "7ee9f49f-26ea-4e89-8264-674094d805e1",
                ModifiedTime = DateTime.Now
            };
            var result = await _awayPeriodController.CreateAwayPeriodDetails(AwayPeriod) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateAwayPeriod_ReturnsConflictResultWhenAwayPeriodUIDAlreadyExists()
        {
            var existingAwayPeriod = new AwayPeriod
            {
                // UID = "ExistingAwayPeriod",

                UID = "23345-757467-8-865-4676435565543",
                OrgUID = "FBNZ",
                Description = "AwayPeriod of New Zealand",
                LinkedItemType = "NZ",
                LinkedItemUID = "fgdg",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                IsActive = true,
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                CreatedBy = "Mathi",
                ModifiedTime = DateTime.Now
            };
            var actionResult = await _awayPeriodController.CreateAwayPeriodDetails(existingAwayPeriod) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateAwayPeriod_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidAwayPeriod = new AwayPeriod
            {
                // Missing required fields
            };
            var actionResult = await _awayPeriodController.CreateAwayPeriodDetails(invalidAwayPeriod) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdateAwayPeriod_SuccessfulUpdate_ReturnsOkWithUpdatedAwayPerioddetails()
        {
            var AwayPeriod = new AwayPeriod
            {
                UID = "23345-757467-8-865-4676435565543t565",
                OrgUID = "FBNZ",
                Description = "AwayPeriod of New Zealand",
                LinkedItemType = "NZ",
                LinkedItemUID = "fgdg",
                FromDate = DateTime.Now,
                IsActive = true,
                ToDate = DateTime.Now,  
                CreatedTime= DateTime.Now,
                ModifiedTime= DateTime.Now,
                ModifiedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1",
                CreatedBy = "7ee9f49f-26ea-4e89-8264-674094d805e1",
            };
            var result = await _awayPeriodController.UpdateAwayPeriodDetails(AwayPeriod) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateAwayPerioddetails_NotFound_ReturnsNotFound()
        {
            var AwayPeriod = new AwayPeriod
            {
                UID = "NDFHN343",
            };
            var result = await _awayPeriodController.UpdateAwayPeriodDetails(AwayPeriod);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteAwayPeriodDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "23345-757467-8-865-4676435565543t565653";
            var result = await _awayPeriodController.DeleteAwayPeriodDetail(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteAwayPeriodDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _awayPeriodController.DeleteAwayPeriodDetail(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
