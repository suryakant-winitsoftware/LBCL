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
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;

namespace NUnitTest
{
    [TestFixture]
    public class SKUClassTestCases
    {
        private SKUClassController _sKUClassController;
        public readonly string _connectionString;

        public SKUClassTestCases()
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
            services.AddSingleton<ISKUClass, SKUClass>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type sKUClassdltype = typeof(Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassDL);
            Winit.Modules.SKUClass.DL.Interfaces.ISKUClassDL sKUClassRepository = (Winit.Modules.SKUClass.DL.Interfaces.ISKUClassDL)Activator.CreateInstance(sKUClassdltype, configurationArgs);
            object[] sKUClassRepositoryArgs = new object[] { sKUClassRepository };

            Type sKUClassblType = typeof(Winit.Modules.SKUClass.BL.Classes.SKUClassBL);
            Winit.Modules.SKUClass.BL.Interfaces.ISKUClassBL sKUClassService = (Winit.Modules.SKUClass.BL.Interfaces.ISKUClassBL)Activator.CreateInstance(sKUClassblType, sKUClassRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _sKUClassController = new SKUClassController(sKUClassService, cacheService);

        }
        [Test]
        public async Task GetSKUClassDetails_WithValidData_ReturnsSKUClassDetails()
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
                      Name = @"ClassName",
                      Value = "maltggh",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Description",
                     Value ="Maltcghhode",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClass>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var SKUClassList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClassList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassDetails_WithSKUClasstyFilterCriteria_ReturnsSKUClassDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // SKUClassty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClass>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassDetails_WithSKUClasstySortCriteria_ReturnsSKUClassDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // SKUClassty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"ClassName",
                   Value = "maltggh",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClass>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO SKUClass No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClass Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassDetails_WithInvalidSortCriteria_ReturnsUnsortedSKUClassDetails()
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
                  Name = "ClassName",
                  Value = "ABN AMRO SKUClass",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClass Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ClassName",
                   Value = "ABN AMRO SKUClass",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassController.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetSKUClassDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e3954";
            IActionResult result = await _sKUClassController.GetSKUClassByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                SKUClass SKUClass = okObjectResult.Value as SKUClass;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClass);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("SKUClass Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _sKUClassController.GetSKUClassByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateSKUClass_ReturnsCreatedResultWithSKUClassObject()
        {
            var SKUClass = new SKUClass
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "1234",
                ClassLabel = "6789",
                ClassName = "Mathi",
                Description = "sdfasd",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassController.CreateSKUClass(SKUClass) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateSKUClass_ReturnsConflictResultWhenSKUClassUIDAlreadyExists()
        {
            var existingSKUClass = new SKUClass
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e39543",
                CompanyUID = "134234",
                ClassLabel = "678g9",
                ClassName = "Mathis",
                Description = "sdfaasdfsd",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _sKUClassController.CreateSKUClass(existingSKUClass) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateSKUClass_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidSKUClass = new SKUClass
            {
                // Missing required fields
            };
            var actionResult = await _sKUClassController.CreateSKUClass(invalidSKUClass) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateSKUClass_SuccessfulUpdate_ReturnsOkWithUpdatedSKUClassdetails()
        {
            var SKUClass = new SKUClass
            {
                UID = "2d893d92-dc1b-5904-934c-6211",
                CompanyUID = "12534",
                ClassLabel = "678g9",
                ClassName = "Mathia",
                Description = "sdfasd",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassController.UpdateSKUClass(SKUClass) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateSKUClassdetails_NotFound_ReturnsNotFound()
        {
            var SKUClass = new SKUClass
            {
                UID = "NDFHN343",
            };
            var result = await _sKUClassController.UpdateSKUClass(SKUClass);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteSKUClassDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934c-6211543";
            var result = await _sKUClassController.DeleteSKUClass(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteSKUClassDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _sKUClassController.DeleteSKUClass(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
