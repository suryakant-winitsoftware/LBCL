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
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Route.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WINITAPI.Controllers.Route;

namespace NUnitTest
{
    [TestFixture]
    public class RouteUserTestCases
    {
        private RouteUserController _RouteUserController;
        public readonly string _connectionString;
        public RouteUserTestCases()
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
            services.AddSingleton<IRouteUser, RouteUser>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type routeUserdltype = typeof(Winit.Modules.Route.DL.Classes.PGSQLRouteUserDL);
            Winit.Modules.Route.DL.Interfaces.IRouteUserDL routeUserRepository = (Winit.Modules.Route.DL.Interfaces.IRouteUserDL)Activator.CreateInstance(routeUserdltype, configurationArgs);
            object[] routeUserRepositoryArgs = new object[] { routeUserRepository };

            Type routeUserblType = typeof(Winit.Modules.Route.BL.Classes.RouteUserBL);
            Winit.Modules.Route.BL.Interfaces.IRouteUserBL routeUserService = (Winit.Modules.Route.BL.Interfaces.IRouteUserBL)Activator.CreateInstance(routeUserblType, routeUserRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _RouteUserController = new RouteUserController(routeUserService, cacheService);
        }
        [Test]
        public async Task GetRouteUserDetails_WithValidData_ReturnsRouteUserDetails()
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
                      Name = @"RouteUID",
                      Value = "FBNZ",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"JobPositionUID",
                     Value = "KOL",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteUser>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var RouteUserList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(RouteUserList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteUserDetails_WithRouteUsertyFilterCriteria_ReturnsRouteUserDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // RouteUserty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteUser>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteUserDetails_WithRouteUsertySortCriteria_ReturnsRouteUserDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // RouteUserty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"RouteUID",
                   Value = "FBNZ",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteUser>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteUserDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO RouteUser No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve RouteUser Details", result.Value);
        }
        [Test]
        public async Task GetRouteUserDetails_WithInvalidSortCriteria_ReturnsUnsortedRouteUserDetails()
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
                  Name = "RouteUID",
                  Value = "ABN AMRO RouteUser No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve RouteUser Details", result.Value);
        }
        [Test]
        public async Task GetRouteUserDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "RouteUID",
                   Value = "ABN AMRO RouteUser No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _RouteUserController.SelectAllRouteUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetRouteUserDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "a828cefa-7f3b-4000-9a87-2331cd";
            IActionResult result = await _RouteUserController.SelectRouteUserByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                RouteUser RouteUser = okObjectResult.Value as RouteUser;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(RouteUser);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("RouteUser Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteUserDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _RouteUserController.SelectRouteUserByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateRouteUser_ReturnsCreatedResultWithRouteUserObject()
        {
            var RouteUser = new RouteUser
            {
                UID = Guid.NewGuid().ToString(),
                RouteUID = "FBNZ",
                JobPositionUID = "KOL",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,         
                IsActive = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _RouteUserController.CreateRouteUser(RouteUser) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateRouteUser_ReturnsConflictResultWhenRouteUserUIDAlreadyExists()
        {
            var existingRouteUser = new RouteUser
            {
                UID = "a828cefa-7f3b-4000-9a87-2rty331cd",
                RouteUID = "FBNZ",
                JobPositionUID = "KOL",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                IsActive = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _RouteUserController.CreateRouteUser(existingRouteUser) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateRouteUser_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidRouteUser = new RouteUser
            {
                // Missing required fields
            };
            var actionResult = await _RouteUserController.CreateRouteUser(invalidRouteUser) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateRouteUser_SuccessfulUpdate_ReturnsOkWithUpdatedRouteUserdetails()
        {
            //var currentTime = DateTime.Now.TimeOfDay;
            var RouteUser = new RouteUser
            {
                UID = "a828cefa-7f3b-4000-9a87-2rty331cdgdfd",
                RouteUID = "FBNZ",
                JobPositionUID = "KOL",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                IsActive = true,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            };
            var result = await _RouteUserController.UpdateRouteUser(RouteUser) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateRouteUserdetails_NotFound_ReturnsNotFound()
        {
            var RouteUser = new RouteUser
            {
                UID = "NDFHN343",
            };
            var result = await _RouteUserController.UpdateRouteUser(RouteUser);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteRouteUserDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "a828cefa-7f3b-4000-9a87-2rty331cdgdghhfd";
            var result = await _RouteUserController.DeleteRouteUser(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteRouteUserDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _RouteUserController.DeleteRouteUser(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
