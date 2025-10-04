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
using WINITAPI.Controllers.Vehicle;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Vehicle.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using WINITAPI.Controllers.Emp;

namespace NUnitTest
{
    public class VehicleTestCases
    {
        private VehicleController _vehicleController;
        public readonly string _connectionString;
        public VehicleTestCases()
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
            services.AddSingleton<IVehicle, Vehicle>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type vehicledltype = typeof(Winit.Modules.Vehicle.DL.Classes.PGSQLVehicleDL);
            Winit.Modules.Vehicle.DL.Interfaces.IVehicleDL vehicleRepository = (Winit.Modules.Vehicle.DL.Interfaces.IVehicleDL)Activator.CreateInstance(vehicledltype, configurationArgs);
            object[] vehicleRepositoryArgs = new object[] { vehicleRepository };

            Type VehicleblType = typeof(Winit.Modules.Vehicle.BL.Classes.VehicleBL);
            Winit.Modules.Vehicle.BL.Interfaces.IVehicleBL vehicleService = (Winit.Modules.Vehicle.BL.Interfaces.IVehicleBL)Activator.CreateInstance(VehicleblType, vehicleRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _vehicleController = new VehicleController(vehicleService, cacheService);
        }
        [Test]
        public async Task GetVehicleDetails_WithValidData_ReturnsVehicleDetails()
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
                      Value = "267dfd6d893d92",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"VehicleNo",
                     Value = "TN72hn53426",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IVehicle>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var VehicleList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(VehicleList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetVehicleDetails_WithVehicletyFilterCriteria_ReturnsVehicleDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Vehiclety list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IVehicle>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetVehicleDetails_WithVehicletySortCriteria_ReturnsVehicleDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Vehiclety list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"CompanyUID",
                   Value = "267dfd6d893d92",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IVehicle>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetVehicleDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Vehicle No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve VehicleDetails", result.Value);
        }
        [Test]
        public async Task GetVehicleDetails_WithInvalidSortCriteria_ReturnsUnsortedVehicleDetails()
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
                  Name = "CompanyUID",
                  Value = "ABN AMRO Vehicle",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve VehicleDetails", result.Value);
        }
        [Test]
        public async Task GetVehicleDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "CompanyUID",
                   Value = "ABN AMRO Vehicle",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _vehicleController.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetVehicleDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900e3";
            IActionResult result = await _vehicleController.GetVehicleByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Vehicle Vehicle = okObjectResult.Value as Vehicle;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(Vehicle);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Vehicle Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetVehicleDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _vehicleController.GetVehicleByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateVehicle_ReturnsCreatedResultWithvehicleObject()
        {
            var Vehicle = new Vehicle
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "FBrrfdNZ",
                OrgUID = "KOLgrt",
                VehicleNo = "GHVgfhhgGH",
                RegistrationNo = "TN69gh786690",
                Model = "splenthydor",
                Type = "cchh",
                IsActive=true,
                TruckSIDate=DateTime.Now,
                RoadTaxExpiryDate=DateTime.Now,
                InspectionDate=DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
            };
            var result = await _vehicleController.CreateVehicle(Vehicle) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateVehicle_ReturnsConflictResultWhenVehicleUIDAlreadyExists()
        {
            var existingVehicle = new Vehicle
            {
                UID = "2d893d92-dc1b-5904-934c-621103a900eghgh3",
                CompanyUID = "FBrrNZ",
                OrgUID = "KOLrt",
                VehicleNo = "GHVgfgGH",
                RegistrationNo = "TN69gh7890",
                Model = "splendor",
                Type = "cc",
                IsActive = false,
                TruckSIDate = DateTime.Now,
                RoadTaxExpiryDate = DateTime.Now,
                InspectionDate = DateTime.Now,
                CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _vehicleController.CreateVehicle(existingVehicle) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateVehicle_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidVehicle = new Vehicle
            {
                // Missing required fields
            };
            var actionResult = await _vehicleController.CreateVehicle(invalidVehicle) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateVehicle_SuccessfulUpdate_ReturnsOkWithUpdatedVehicledetails()
        {
            var Vehicle = new Vehicle
            {
                UID = "5270b4de-57e5-428c-9c44-8394dc765476",
                CompanyUID = "FBrrNgfZ",
                OrgUID = "KOLrfgt",
                VehicleNo = "GHVgffhgGH",
                RegistrationNo = "TN69gh7840",
                Model = "splendorplus",
                Type = "ccc",
                IsActive = true,
                TruckSIDate = DateTime.Now,
                RoadTaxExpiryDate = DateTime.Now,
                InspectionDate = DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
            };
            var result = await _vehicleController.UpdateVehicleDetails(Vehicle) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateVehicledetails_NotFound_ReturnsNotFound()
        {
            var Vehicle = new Vehicle
            {
                UID = "NDFHN343",
            };
            var result = await _vehicleController.UpdateVehicleDetails(Vehicle);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteVehicleDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934c-621103a900enjnghmkkgh39089";
            var result = await _vehicleController.DeleteVehicleDetails(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteVehicleDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _vehicleController.DeleteVehicleDetails(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
