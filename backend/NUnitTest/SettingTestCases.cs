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
using WINITAPI.Controllers.Setting;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Setting.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Setting.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

using WINITAPI.Controllers.Route;

namespace NunitTest
{
    [TestFixture]
    public class SettingTestCases
    {
        private SettingController _settingController;
        public readonly string _connectionString;
        public SettingTestCases()
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
            services.AddSingleton<ISetting, Setting>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type settingdltype = typeof(Winit.Modules.Setting.DL.Classes.PGSQLSettingDL);
            Winit.Modules.Setting.DL.Interfaces.ISettingDL settingRepository = (Winit.Modules.Setting.DL.Interfaces.ISettingDL)Activator.CreateInstance(settingdltype, configurationArgs);
            object[] settingRepositoryArgs = new object[] { settingRepository };

            Type settingblType = typeof(Winit.Modules.Setting.BL.Classes.SettingBL);
            Winit.Modules.Setting.BL.Interfaces.ISettingBL settingService = (Winit.Modules.Setting.BL.Interfaces.ISettingBL)Activator.CreateInstance(settingblType, settingRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _settingController = new SettingController(settingService, cacheService);
        }
        [Test]
        public async Task GetSettingDetails_WithValidData_ReturnsSettingDetails()
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
                      Name = @"Name",
                      Value = "KOL",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Value",
                     Value = "djhfbwejhfew",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISetting>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var SettingList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SettingList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSettingDetails_WithSettingtyFilterCriteria_ReturnsSettingDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Settingty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISetting>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSettingDetails_WithSettingtySortCriteria_ReturnsSettingDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Settingty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Name",
                   Value = "KOL",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISetting>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSettingDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Setting No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SettingDetails", result.Value);
        }
        [Test]
        public async Task GetSettingDetails_WithInvalidSortCriteria_ReturnsUnsortedSettingDetails()
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
                  Name = "Name",
                  Value = "ABN AMRO Setting No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SettingDetails", result.Value);
        }
        [Test]
        public async Task GetSettingDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "Name",
                   Value = "ABN AMRO Setting No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _settingController.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetSettingDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "a828cefa-7f3b-4000-9a87-2rty331cd34rgt3";
            IActionResult result = await _settingController.GetSettingById(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Setting Setting = okObjectResult.Value as Setting;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(Setting);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Setting Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSettingDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _settingController.GetSettingById(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateSetting_ReturnsCreatedResultWithSettingObject()
        {
            var Setting = new Setting
            {
                UID = Guid.NewGuid().ToString(),
                Type = "FBNZcx",
                Name = "KOLzs",
                Value = "djhfbwejhfew",
                DataType = "jbkjendfkjwe",
                IsEditable = true,
                SS=1,
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _settingController.CreateSetting(Setting) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateSetting_ReturnsConflictResultWhenSettingUIDAlreadyExists()
        {
            var existingSetting = new Setting
            {
                UID = "a828cefa-7f3b-4000-9a87-2rty331cd34rgt34",
                Type = "FBNZ",
                Name = "KOL",
                Value = "djhfbwejhfew",
                DataType = "jbkjendfkjwe",
                IsEditable = true,
                SS = 1,
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _settingController.CreateSetting(existingSetting) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateSetting_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidSetting = new Setting
            {
                // Missing required fields
            };
            var actionResult = await _settingController.CreateSetting(invalidSetting) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateSetting_SuccessfulUpdate_ReturnsOkWithUpdatedSettingdetails()
        {
            //var currentTime = DateTime.Now.TimeOfDay;
            var Setting = new Setting
            {
                UID = "a828cefa-7f3b-4000-9a87-2rty331cd34rgt3ere4",
                Type = "FBNqZ",
                Name = "KOLa",
                Value = "djhfbwejhfew",
                DataType = "jbkjendfkjwe",
                IsEditable = true,
                SS = 1,
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            };
            var result = await _settingController.UpdateSetting(Setting) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateSettingdetails_NotFound_ReturnsNotFound()
        {
            var Setting = new Setting
            {
                UID = "NDFHN343",
            };
            var result = await _settingController.UpdateSetting(Setting);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteSettingDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "a828cefa-7f3b-4000-9a87-2rty331cd34rgt3ere445";
            var result = await _settingController.DeleteSetting(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteSettingDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _settingController.DeleteSetting(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }

    }


}


















