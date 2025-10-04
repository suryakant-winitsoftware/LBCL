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
using WINITAPI.Controllers.Holiday;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Modules.Holiday.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Modules.Holiday.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WINITRepository.Interfaces.Holiday;

namespace NunitTest
{
    [TestFixture]
    public class HolidayListRoleTestCases
    {
        private HolidayListRoleController _holidayListRoleController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public HolidayListRoleTestCases()
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
            services.AddSingleton<IHolidayListRole, HolidayListRole>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type holidayListRoledltype = typeof(Winit.Modules.Holiday.DL.Classes.PGSQLHolidayListRoleDL);
            Winit.Modules.Holiday.DL.Interfaces.IHolidayListRoleDL holidayListRoleRepository = (Winit.Modules.Holiday.DL.Interfaces.IHolidayListRoleDL)Activator.CreateInstance(holidayListRoledltype, configurationArgs);
            object[] holidayListRoleRepositoryArgs = new object[] { holidayListRoleRepository };
            Type holidayListRoleblType = typeof(Winit.Modules.Holiday.BL.Classes.HolidayListRoleBL);
            Winit.Modules.Holiday.BL.Interfaces.IHolidayListRoleBL holidayListRoleService = (Winit.Modules.Holiday.BL.Interfaces.IHolidayListRoleBL)Activator.CreateInstance(holidayListRoleblType, holidayListRoleRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _holidayListRoleController = new HolidayListRoleController(holidayListRoleService, cacheService);
        }

        [Test]
        public async Task GetHolidayListRoleDetails_WithValidData_ReturnsHolidayListRoleDetails()
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
                      Name = @"CompanyUID",
                      Value = "jkefhv9unsdfg",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHolidayListRole>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var holidayListRole = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(holidayListRole);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, holidayListRole.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetHolidayListRoleDetails_WithEmptyFilterCriteria_ReturnsHolidayListRoleDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = @"UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Empty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHolidayListRole>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetHolidayListRoleDetails_WithEmptySortCriteria_ReturnsHolidayListRoleDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"HolidayName",
                Value = "New Zealand Holiday",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IHolidayListRole>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetHolidayListRoleDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO HolidayList NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve HolidayListRole Details", result.Value);
        }

        [Test]
        public async Task GetHolidayListRoleDetails_WithInvalidSortCriteria_ReturnsUnsortedHolidayListRoleDetails()
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
                  Name = "HolidayListName",
                  Value = "ABN AMRO HolidayList NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve HolidayListRole Details", result.Value);
        }

        [Test]
        public async Task GetHolidayListRoleDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "HolidayListName",
                   Value = "ABN AMRO HolidayList NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _holidayListRoleController.SelectAllHolidayListRole(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetHolidayListRoleDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1c086069-514c-4d22-9fe2-94af9c1beef2";
            IActionResult result = await _holidayListRoleController.GetHolidayListRoleByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //HolidayListRole holidayListRole = okObjectResult.Value as HolidayListRole;
                HolidayListRole holidayListRole = okObjectResult.Value as HolidayListRole;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(holidayListRole);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("HolidayListRole Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetHolidayListRoleDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _holidayListRoleController.GetHolidayListRoleByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateholidayListRole_ReturnsCreatedResultWithholidayListRoleObject()
        {
            var holidayListRole = new HolidayListRole
            {
                UID =this.UID ,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                HolidayListUID = "EJFGUWV",
                UserRoleUID = "BWEFWE"
            };
            var result = await _holidayListRoleController.CreateHolidayListRole(holidayListRole) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateHolidayListRole_ReturnsConflictResultWhenHolidayListRoleUIDAlreadyExists()
        {
            var existingHolidayListRole = new HolidayListRole
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                HolidayListUID = "EJFGUWV",
                UserRoleUID = "BWEFWE"
            };
            var actionResult = await _holidayListRoleController.CreateHolidayListRole(existingHolidayListRole) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateHolidayListRole_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidHolidayListRole = new HolidayListRole
            {
                // Missing required fields
            };
            var actionResult = await _holidayListRoleController.CreateHolidayListRole(invalidHolidayListRole) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdateholidayListRole_SuccessfulUpdate_ReturnsOkWithUpdatedholidayListRoledetails()
        {
            var holidayListRole = new HolidayListRole
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                HolidayListUID = "EJFGUWV",
                UserRoleUID = "BWEFWE"
            };
            var result = await _holidayListRoleController.UpdateHolidayListRole(holidayListRole) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateHolidayListRoledetails_NotFound_ReturnsNotFound()
        {
            var holidayListRole = new HolidayListRole
            {
                UID = "NDFHN343",
            };
            var result = await _holidayListRoleController.UpdateHolidayListRole(holidayListRole);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteHolidayListRoleDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _holidayListRoleController.DeleteHolidayListRole(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteHolidayListRoleDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _holidayListRoleController.DeleteHolidayListRole(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















