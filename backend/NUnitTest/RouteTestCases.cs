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
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using WINITAPI.Controllers.Route;

namespace NUnitTest
{
    [TestFixture]
    public class RouteTestCases
    {
        private RouteController _routeController;
        public readonly string _connectionString;
        public RouteTestCases()
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
            services.AddSingleton<IRoute, Route>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type routedltype = typeof(Winit.Modules.Route.DL.Classes.PGSQLRoute);
            Winit.Modules.Route.DL.Interfaces.IRouteDL routeRepository = (Winit.Modules.Route.DL.Interfaces.IRouteDL)Activator.CreateInstance(routedltype, configurationArgs);
            object[] routeRepositoryArgs = new object[] { routeRepository };

            Type routeblType = typeof(Winit.Modules.Route.BL.Classes.RouteBL);
            Winit.Modules.Route.BL.Interfaces.IRouteBL routeService = (Winit.Modules.Route.BL.Interfaces.IRouteBL)Activator.CreateInstance(routeblType, routeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _routeController = new RouteController(routeService, cacheService);
        }
        [Test]
        public async Task GetRouteDetails_WithValidData_ReturnsRouteDetails()
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
                      Value = "example_code",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Name",
                     Value = "example_name",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRoute>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var RouteList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(RouteList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteDetails_WithRoutetyFilterCriteria_ReturnsRouteDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Routety list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRoute>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteDetails_WithRoutetySortCriteria_ReturnsRouteDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Routety list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Code",
                   Value = "example_code",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRoute>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Route No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Route Details", result.Value);
        }
        [Test]
        public async Task GetRouteDetails_WithInvalidSortCriteria_ReturnsUnsortedRouteDetails()
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
                  Value = "ABN AMRO Route No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Route Details", result.Value);
        }
        [Test]
        public async Task GetRouteDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Value = "ABN AMRO Route No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _routeController.SelectRouteAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetRouteDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a9045540e397FGF8";
            IActionResult result = await _routeController.SelectRouteDetailByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Route Route = okObjectResult.Value as Route;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(Route);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Route Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _routeController.SelectRouteDetailByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateRoute_ReturnsCreatedResultWithRouteObject()
        {
            var Route = new Route
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "FBNZ",
                Code = "KOL",
                Name = "GHVGH",
                Type = "weafs",
                OrgUID = "wAFDZX",
                WHOrgUID = "sdz",
                VehicleUID="AWDFCFG",
                JobPositionUID="ERETR",
                LocationUID="DGERR",
                IsActive=false,
                Status="ETRTRT",
                ValidFrom=DateTime.Now,
                ValidUpto=DateTime.Now,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _routeController.CreateRouteDetails(Route) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateRoute_ReturnsConflictResultWhenRouteUIDAlreadyExists()
        {
            var existingRoute = new Route
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e396rtr563435",
                CompanyUID = "FBNZ",
                Code = "KOL",
                Name = "GHVGH",
                Type = "weafs",
                OrgUID = "wAFDZX",
                WHOrgUID = "sdz",
                VehicleUID = "AWDFCFG",
                JobPositionUID = "ERETR",
                LocationUID = "DGERR",
                IsActive = false,
                Status = "ETRTRT",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,  
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _routeController.CreateRouteDetails(existingRoute) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateRoute_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidRoute = new Route
            {
                // Missing required fields
            };
            var actionResult = await _routeController.CreateRouteDetails(invalidRoute) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateRoute_SuccessfulUpdate_ReturnsOkWithUpdatedRoutedetails()
        {
            var Route = new Route
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e396rtr56343535465",
                CompanyUID = "FBNZ",
                Code = "KOgfhL",
                Name = "GHVdgGH",
                Type = "weafs",
                OrgUID = "wAFDZX",
                WHOrgUID = "sdz",
                VehicleUID = "AWDFCFG",
                JobPositionUID = "ERETR",
                LocationUID = "DGERR",
                IsActive = false,
                Status = "ETRTRT",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _routeController.UpdateRouteDetails(Route) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateRoutedetails_NotFound_ReturnsNotFound()
        {
            var Route = new Route
            {
                UID = "NDFHN343",
            };
            var result = await _routeController.UpdateRouteDetails(Route);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteRouteDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934c";
            var result = await _routeController.DeleteRouteDetail(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteRouteDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _routeController.DeleteRouteDetail(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
