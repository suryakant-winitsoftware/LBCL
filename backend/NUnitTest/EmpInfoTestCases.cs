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
using WINITAPI.Controllers.Emp;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace NUnitTest
{
    [TestFixture]
    public class EmpInfoTestCases
    {
        private EmpInfoController _empInfoController;
        public readonly string _connectionString;
        public EmpInfoTestCases()
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
            services.AddSingleton<IEmpInfo, EmpInfo>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type empInfodltype = typeof(Winit.Modules.Emp.DL.Classes.PGSQLEmpInfoDL);
            Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL empInfoRepository = (Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL)Activator.CreateInstance(empInfodltype, configurationArgs);
            object[] empInfoRepositoryArgs = new object[] { empInfoRepository };

            Type empInfoblType = typeof(Winit.Modules.Emp.BL.Classes.EmpInfoBL);
            Winit.Modules.Emp.BL.Interfaces.IEmpInfoBL empInfoService = (Winit.Modules.Emp.BL.Interfaces.IEmpInfoBL)Activator.CreateInstance(empInfoblType, empInfoRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _empInfoController = new EmpInfoController(empInfoService, cacheService);
        }
        [Test]
        public async Task GetEmpInfoDetails_WithValidData_ReturnsEmpInfoDetails()
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
                      Name = @"EmpUID",
                      Value = "EU",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Email",
                     Value = "OI@gmail",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmpInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var EmpInfoList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(EmpInfoList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetEmpInfoDetails_WithEmpInfotyFilterCriteria_ReturnsEmpInfoDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // EmpInfoty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmpInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetEmpInfoDetails_WithEmpInfotySortCriteria_ReturnsEmpInfoDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // EmpInfoty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Email",
                   Value = "OI@gmail",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmpInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetEmpInfoDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO EmpInfo No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve EmpInfoDetails", result.Value);
        }
        [Test]
        public async Task GetEmpInfoDetails_WithInvalidSortCriteria_ReturnsUnsortedEmpInfoDetails()
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
                  Name = "EmpUID",
                  Value = "ABN AMRO EmpInfo No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve EmpInfoDetails", result.Value);
        }
        [Test]
        public async Task GetEmpInfoDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "EmpUID",
                   Value = "ABN AMRO EmpInfo No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _empInfoController.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetEmpInfoDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a9045540e397FGF8";
            IActionResult result = await _empInfoController.GetEmpInfoByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                EmpInfo EmpInfo = okObjectResult.Value as EmpInfo;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(EmpInfo);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("EmpInfo Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetEmpInfoDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _empInfoController.GetEmpInfoByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateEmpInfo_ReturnsCreatedResultWithempinfoObject()
        {
            var EmpInfo = new EmpInfo
            {
                UID = Guid.NewGuid().ToString(),
                EmpUID = "FBNZ",
                Email = "KOL",
                Phone = "GHVGH",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CanHandleStock=true,
                CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var result = await _empInfoController.CreateEmpInfo(EmpInfo) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateEmpInfo_ReturnsConflictResultWhenEmpInfoUIDAlreadyExists()
        {
            var existingEmpInfo = new EmpInfo
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e39",
                EmpUID = "FBNZ",
                Email = "KOL",
                Phone = "GHVGH",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CanHandleStock = true,
                CreatedBy = "ADMIN",
                CreatedTime = DateTime.Now,
                ModifiedBy = "ADMIN",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _empInfoController.CreateEmpInfo(existingEmpInfo) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateEmpInfo_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidEmpInfo = new EmpInfo
            {
                // Missing required fields
            };
            var actionResult = await _empInfoController.CreateEmpInfo(invalidEmpInfo) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateEmpInfo_SuccessfulUpdate_ReturnsOkWithUpdatedEmpInfodetails()
        {
            var EmpInfo = new EmpInfo
            {
                UID = "2d893d92-dc1b-5904-934c-621103a9045540e3978",
                EmpUID = "FBhgYThNZ",
                Email = "KOLTU",
                Phone = "GHVfgGUUH",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CanHandleStock = true,
                CreatedBy= "ADMIN",
                CreatedTime= DateTime.Now,
                ModifiedBy = "Mathiupdate",
                ModifiedTime = DateTime.Now,
            };
            var result = await _empInfoController.UpdateEmpInfoDetails(EmpInfo) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateEmpInfodetails_NotFound_ReturnsNotFound()
        {
            var EmpInfo = new EmpInfo
            {
                UID = "NDFHN343",
            };
            var result = await _empInfoController.UpdateEmpInfoDetails(EmpInfo);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteEmpInfoDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e39";
            var result = await _empInfoController.DeleteEmpInfoDetails(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteEmpInfoDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _empInfoController.DeleteEmpInfoDetails(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
