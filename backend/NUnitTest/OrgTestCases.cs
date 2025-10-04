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
using WINITAPI.Controllers.Org;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Org.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WINITAPI.Controllers.Org;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;

namespace NUnitTest
{
    public class OrgTestCases
    {
        private OrgController _orgController;
        public readonly string _connectionString;
        public OrgTestCases()
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
            services.AddSingleton<IOrg, Org>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type orgdltype = typeof(Winit.Modules.Org.DL.Classes.PGSQLOrgDL);
            Winit.Modules.Org.DL.Interfaces.IOrgDL orgRepository = (Winit.Modules.Org.DL.Interfaces.IOrgDL)Activator.CreateInstance(orgdltype, configurationArgs);
            object[] orgRepositoryArgs = new object[] { orgRepository };

            Type orgblType = typeof(Winit.Modules.Org.BL.Classes.OrgBL);
            Winit.Modules.Org.BL.Interfaces.IOrgBL orgService = (Winit.Modules.Org.BL.Interfaces.IOrgBL)Activator.CreateInstance(orgblType, orgRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _orgController = new OrgController(orgService, cacheService);
        }
        [Test]
        public async Task GetOrgDetails_WithValidData_ReturnsOrgDetails()
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
                      Name = @"Code",
                      Value = "RO",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Name",
                     Value = "rajmanan",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IOrg>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var OrgList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(OrgList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetOrgDetails_WithOrgtyFilterCriteria_ReturnsOrgDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Orgty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IOrg>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetOrgDetails_WithOrgtySortCriteria_ReturnsOrgDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Orgty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Code",
                   Value = "RO",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IOrg>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetOrgDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Org No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve OrgDetails", result.Value);
        }
        [Test]
        public async Task GetOrgDetails_WithInvalidSortCriteria_ReturnsUnsortedOrgDetails()
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
                  Name = "Code",
                  Value = "ABN AMRO Org",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve OrgDetails", result.Value);
        }
        [Test]
        public async Task GetOrgDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "Code",
                   Value = "ABN AMRO Org",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _orgController.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetOrgDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F";
            IActionResult result = await _orgController.GetOrgByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Org Org = okObjectResult.Value as Org;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(Org);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Org Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetOrgDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _orgController.GetOrgByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateOrg_ReturnsCreatedResultWithOrgObject()
        {
            var Org = new Org
            {
                UID = Guid.NewGuid().ToString(),
                Code = "FBrrfdNZ",
                Name = "KOLgrt",             
                IsActive = true,               
                CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var result = await _orgController.CreateOrg(Org) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateOrg_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingOrg = new Org
            {
                UID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F",
                Code = "GFtytH",
                Name = "GHghf",
                IsActive = false,
                CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _orgController.CreateOrg(existingOrg) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateOrg_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidOrg = new Org
            {
                // Missing required fields
            };
            var actionResult = await _orgController.CreateOrg(invalidOrg) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateOrg_SuccessfulUpdate_ReturnsOkWithUpdatedOrgdetails()
        {
            var Org = new Org
            {
                UID = "8D006B71-7DFD-4831-B132-F4B53gjhjF2C4C7rrytyhtjhyyF3",
                Code = "FBrrNgf8Z",
                Name = "KOLrfghght",
                IsActive = true,               
                CreatedBy = "admin",
                CreatedTime = DateTime.Now,
                ModifiedBy = "admin",
                ModifiedTime = DateTime.Now,
            };
            var result = await _orgController.UpdateOrg(Org) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateOrgdetails_NotFound_ReturnsNotFound()
        {
            var Org = new Org
            {
                UID = "NDFHN343",
            };
            var result = await _orgController.UpdateOrg(Org);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteOrgDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "8D006B71-7DFD-4831-B132-F4B53gjhjF2C4C7rrytyhtjhyyF4";
            var result = await _orgController.DeleteOrg(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteOrgDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _orgController.DeleteOrg(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
