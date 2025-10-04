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
    public class RouteCustomerTestCases
    {
        private RouteCustomerController _routeCustomerController;
        public readonly string _connectionString;
        public RouteCustomerTestCases()
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
            services.AddSingleton<IRouteCustomer, RouteCustomer>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type routeCustomerdltype = typeof(Winit.Modules.Route.DL.Classes.PGSQLRouteCustomer);
            Winit.Modules.Route.DL.Interfaces.IRouteCustomerDL routeCustomerRepository = (Winit.Modules.Route.DL.Interfaces.IRouteCustomerDL)Activator.CreateInstance(routeCustomerdltype, configurationArgs);
            object[] routeCustomerRepositoryArgs = new object[] { routeCustomerRepository };

            Type routeCustomerblType = typeof(Winit.Modules.Route.BL.Classes.RouteCustomerBL);
            Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL routeCustomerService = (Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL)Activator.CreateInstance(routeCustomerblType, routeCustomerRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _routeCustomerController = new RouteCustomerController(routeCustomerService, cacheService);
        }
        [Test]
        public async Task GetRouteCustomerDetails_WithValidData_ReturnsRouteCustomerDetails()
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
                     Name = @"StoreUID",
                     Value = "KOL",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteCustomer>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var RouteCustomerList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(RouteCustomerList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteCustomerDetails_WithRouteCustomertyFilterCriteria_ReturnsRouteCustomerDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // RouteCustomerty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteCustomer>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteCustomerDetails_WithRouteCustomertySortCriteria_ReturnsRouteCustomerDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // RouteCustomerty list
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
            var result = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IRouteCustomer>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetRouteCustomerDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO RouteCustomer No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Route Customer Details", result.Value);
        }
        [Test]
        public async Task GetRouteCustomerDetails_WithInvalidSortCriteria_ReturnsUnsortedRouteCustomerDetails()
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
                  Value = "ABN AMRO RouteCustomer No",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Route Customer Details", result.Value);
        }
        [Test]
        public async Task GetRouteCustomerDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Value = "ABN AMRO RouteCustomer No",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _routeCustomerController.SelectRouteCustomerAllDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetRouteCustomerDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e396rtr563435";
            IActionResult result = await _routeCustomerController.SelectRouteCustomerDetailByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                RouteCustomer RouteCustomer = okObjectResult.Value as RouteCustomer;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(RouteCustomer);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("RouteCustomer Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetRouteCustomerDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _routeCustomerController.SelectRouteCustomerDetailByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateRouteCustomer_ReturnsCreatedResultWithRouteCustomerObject()
        {
            var RouteCustomer = new RouteCustomer
            {
                UID = Guid.NewGuid().ToString(),
                RouteUID = "FBNZ",
                StoreUID = "KOL",
                SeqNo = 2,
                VisitDuration = "weafs",
                VisitTime = new TimeSpan(14, 30, 0),
                IsDeleted = true,
                EndTime = new TimeSpan(14, 30, 0),               
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _routeCustomerController.CreateRouteCustomerDetails(RouteCustomer) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateRouteCustomer_ReturnsConflictResultWhenRouteCustomerUIDAlreadyExists()
        {
            var existingRouteCustomer = new RouteCustomer
            {
                UID = "a828cefa-7f3b-4000-9a87-2331cde5544f",
                RouteUID = "FBNZ",
                StoreUID = "KOL",
                SeqNo = 2,
                VisitDuration = "weafs",
                VisitTime = new TimeSpan(14, 30, 0),
                IsDeleted = true,
                EndTime = new TimeSpan(14, 30, 0),
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _routeCustomerController.CreateRouteCustomerDetails(existingRouteCustomer) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateRouteCustomer_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidRouteCustomer = new RouteCustomer
            {
                // Missing required fields
            };
            var actionResult = await _routeCustomerController.CreateRouteCustomerDetails(invalidRouteCustomer) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateRouteCustomer_SuccessfulUpdate_ReturnsOkWithUpdatedRouteCustomerdetails()
        {
            //var currentTime = DateTime.Now.TimeOfDay;
            var RouteCustomer = new RouteCustomer
            {
                UID = "2d893d92-dc1b-5904-934c-621103a55454",
                SeqNo = 2,
                VisitDuration = "weafsjj",
                VisitTime = new TimeSpan(14, 30, 0),
                IsDeleted = true,
                EndTime = new TimeSpan(14, 30, 0),
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                ServerModifiedTime=DateTime.Now,
            };
            var result = await _routeCustomerController.UpdateRouteCustomerDetails(RouteCustomer) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateRouteCustomerdetails_NotFound_ReturnsNotFound()
        {
            var RouteCustomer = new RouteCustomer
            {
                UID = "NDFHN343",
            };
            var result = await _routeCustomerController.UpdateRouteCustomerDetails(RouteCustomer);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteRouteCustomerDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a554546788";
            var result = await _routeCustomerController.DeleteRouteDetail(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteRouteCustomerDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _routeCustomerController.DeleteRouteDetail(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
