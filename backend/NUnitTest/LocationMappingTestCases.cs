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
using WINITAPI.Controllers.Location;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NUnitTest
{
    [TestFixture]
    public class LocationMappingTestCases
    {
        private LocationMappingController _locationMappingController;
        public readonly string _connectionString;

        public LocationMappingTestCases()
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
            services.AddSingleton<ILocationMapping, LocationMapping>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type locationMappingdltype = typeof(Winit.Modules.Location.DL.Classes.PGSQLLocationMappingDL);
            Winit.Modules.Location.DL.Interfaces.ILocationMappingDL locationMappingRepository = (Winit.Modules.Location.DL.Interfaces.ILocationMappingDL)Activator.CreateInstance(locationMappingdltype, configurationArgs);
            object[] locationMappingRepositoryArgs = new object[] { locationMappingRepository };

            Type locationMappingblType = typeof(Winit.Modules.Location.BL.Classes.LocationMappingBL);
            Winit.Modules.Location.BL.Interfaces.ILocationMappingBL locationMappingService = (Winit.Modules.Location.BL.Interfaces.ILocationMappingBL)Activator.CreateInstance(locationMappingblType, locationMappingRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _locationMappingController = new LocationMappingController(locationMappingService, cacheService);

        }
        [Test]
        public async Task GetLocationMappingDetails_WithValidData_ReturnsLocationMappingDetails()
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
                      Name = @"LinkedItemType",
                      Value = "maltsel",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"LocationTypeUID",
                     Value ="2d893d792",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ILocationMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var LocationMappingList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(LocationMappingList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetLocationMappingDetails_WithLocationMappingtyFilterCriteria_ReturnsLocationMappingDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // LocationMappingty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ILocationMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetLocationMappingDetails_WithLocationMappingtySortCriteria_ReturnsLocationMappingDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // LocationMappingty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"LinkedItemType",
                   Value = "maltsel",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ILocationMapping>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetLocationMappingDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO LocationMapping No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve LocationMappingDetails", result.Value);
        }
        [Test]
        public async Task GetLocationMappingDetails_WithInvalidSortCriteria_ReturnsUnsortedLocationMappingDetails()
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
                  Name = "LinkedItemType",
                  Value = "ABN AMRO LocationMapping",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve LocationMappingDetails", result.Value);
        }
        [Test]
        public async Task GetLocationMappingDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "LinkedItemType",
                   Value = "ABN AMRO LocationMapping",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _locationMappingController.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetLocationMappingDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e3978";
            IActionResult result = await _locationMappingController.GetLocationMappingByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                LocationMapping LocationMapping = okObjectResult.Value as LocationMapping;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(LocationMapping);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("LocationMapping Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetLocationMappingDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _locationMappingController.GetLocationMappingByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateLocationMapping_ReturnsCreatedResultWithLocationMappingObject()
        {
            var LocationMapping = new LocationMapping
            {
                UID = Guid.NewGuid().ToString(),
                LocationTypeUID = "fggghhg",
                LocationUID = "njbhjbjh",
                LinkedItemType = "jhbjh",
                LinkedItemUID = "hjhjrt",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _locationMappingController.CreateLocationMapping(LocationMapping) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateLocationMapping_ReturnsConflictResultWhenLocationMappingUIDAlreadyExists()
        {
            var existingLocationMapping = new LocationMapping
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900e3978",
                LocationTypeUID = "fgtrgghhg",
                LocationUID = "njbhgfjbjh",
                LinkedItemType = "jhfgfbjh",
                LinkedItemUID = "hjhfhghjrt",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _locationMappingController.CreateLocationMapping(existingLocationMapping) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateLocationMapping_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidLocationMapping = new LocationMapping
            {
                // Missing required fields
            };
            var actionResult = await _locationMappingController.CreateLocationMapping(invalidLocationMapping) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateLocationMapping_SuccessfulUpdate_ReturnsOkWithUpdatedLocationMappingdetails()
        {
            var LocationMapping = new LocationMapping
            {
                UID = "55768899003452345671324245",
                LocationTypeUID = "fggdfdghhg",
                LocationUID = "njbhfgfjbjh",
                LinkedItemType = "jhbgbjh",
                LinkedItemUID = "hjhjrt",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _locationMappingController.UpdateLocationMappingDetails(LocationMapping) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateLocationMappingdetails_NotFound_ReturnsNotFound()
        {
            var LocationMapping = new LocationMapping
            {
                UID = "NDFHN343",
            };
            var result = await _locationMappingController.UpdateLocationMappingDetails(LocationMapping);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteLocationMappingDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "557688990034523456713242478905";
            var result = await _locationMappingController.DeleteLocationMappingDetails(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteLocationMappingDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _locationMappingController.DeleteLocationMappingDetails(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
