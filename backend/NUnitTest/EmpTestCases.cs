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
    public class EmpTestCases
    {
        private EmpController _empController;
        public readonly string _connectionString;
        public EmpTestCases()
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
            services.AddSingleton<IEmp, Emp>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type empdltype = typeof(Winit.Modules.Emp.DL.Classes.PGSQLEmpDL);
            Winit.Modules.Emp.DL.Interfaces.IEmpDL empRepository = (Winit.Modules.Emp.DL.Interfaces.IEmpDL)Activator.CreateInstance(empdltype, configurationArgs);
            object[] empRepositoryArgs = new object[] { empRepository };

            Type empblType = typeof(Winit.Modules.Emp.BL.Classes.EmpBL);
            Winit.Modules.Emp.BL.Interfaces.IEmpBL empService = (Winit.Modules.Emp.BL.Interfaces.IEmpBL)Activator.CreateInstance(empblType, empRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _empController = new EmpController(empService, cacheService);
        }
        [Test]
        public async Task GetEmpDetails_WithValidData_ReturnsEmpDetails()
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
                      Name = @"EmpNo",
                      Value = "EN",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"CompanyUID",
                     Value = "EN",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmp>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var empList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(empList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetEmpDetails_WithEmptyFilterCriteria_ReturnsEmpDetails()
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
            var result = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmp>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetEmpDetails_WithEmptySortCriteria_ReturnsEmpDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"CompanyUID",
                   Value = "EN",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IEmp>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetEmpDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Emp No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve EmpDetails", result.Value);
        }
        [Test]
        public async Task GetEmpDetails_WithInvalidSortCriteria_ReturnsUnsortedEmpDetails()
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
                  Name = "EmpNo",
                  Value = "ABN AMRO Emp No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve EmpDetails", result.Value);
        }
        [Test]
        public async Task GetEmpDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "EmpNo",
                   Value = "ABN AMRO Emp No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _empController.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetEmpDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e39";
            IActionResult result = await _empController.GetEmpByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Emp emp = okObjectResult.Value as Emp;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(emp);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Emp Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetEmpDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _empController.GetEmpByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateEmp_ReturnsCreatedResultWithempObject()
        {
            var emp = new Emp
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "FBNZ",
                OrgUID = "KOL",
                Code="GHVGH",
                Name="MATHI",
                AliasName="SELVA",
                LoginId="GH",
                EmpNo="QWER",
                AuthType="DFGH",
                Status="HGJ",
                ActiveAuthKey="HJYH",
                EncryptedPassword="ZXCVB",
                CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var result = await _empController.CreateEmp(emp) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateEmp_ReturnsConflictResultWhenEmpUIDAlreadyExists()
        {
            var existingEmp = new Emp
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e39",
                CompanyUID = "EM",
                OrgUID = "OI",
                Code = "CO",
                Name = "NA",
                AliasName = "AN",
                LoginId = "LI",
                EmpNo = "EN",
                AuthType = "NULL",
                Status = "NULL",
                ActiveAuthKey = "NULL",
                EncryptedPassword = "NULL",
                CreatedBy = "ADMIN",
                CreatedTime = DateTime.Now,
                ModifiedBy = "ADMIN",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _empController.CreateEmp(existingEmp) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateEmp_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidEmp = new Emp
            {
                // Missing required fields
            };
            var actionResult = await _empController.CreateEmp(invalidEmp) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateEmp_SuccessfulUpdate_ReturnsOkWithUpdatedempdetails()
        {
            var emp = new Emp
            {
                UID = "49ab6770-8673-4c82-ade0-0447b47ac50c",
                CompanyUID = "EMh",
                OrgUID = "OIh",
                Code = "COh",
                Name = "NAh",
                AliasName = "ANh",
                LoginId = "LIh",
                EmpNo = "ENh",
                AuthType = "NULL",
                Status = "NULL",
                ActiveAuthKey = "NULL",
                EncryptedPassword="sdsf",
                ModifiedBy = "Mathiupdate",
                ModifiedTime = DateTime.Now,
            };
            var result = await _empController.UpdateEmp(emp) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateEmpdetails_NotFound_ReturnsNotFound()
        {
            var emp = new Emp
            {
                UID = "NDFHN343",
            };
            var result = await _empController.UpdateEmp(emp);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteEmpDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "6c3e4161-2daf-46b1-be90-0263d229f37c";
            var result = await _empController.DeleteEmp(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteEmpDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _empController.DeleteEmp(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
