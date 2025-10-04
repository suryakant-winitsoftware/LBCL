
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
using WINITAPI.Controllers.FileSys;
using Winit.Shared.Models.Enums;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using Serilog.Sinks.File;

namespace NunitTest
{
    [TestFixture]
    public class FileSysTestCases
    {
        private FileSysController _fileSysController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public FileSysTestCases()
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
            services.AddSingleton<IFileSys, FileSys>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type FileSysdltype = typeof(Winit.Modules.FileSys.DL.Classes.PGSQLFileSysDL);
            Winit.Modules.FileSys.DL.Interfaces.IFileSysDL FileSysRepository = (Winit.Modules.FileSys.DL.Interfaces.IFileSysDL)Activator.CreateInstance(FileSysdltype, configurationArgs);
            object[] fileSysRepositoryArgs = new object[] { FileSysRepository };
            Type fileSysblType = typeof(Winit.Modules.FileSys.BL.Classes.FileSysBL);
            Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysService = (Winit.Modules.FileSys.BL.Interfaces.IFileSysBL)Activator.CreateInstance(fileSysblType, fileSysRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _fileSysController = new FileSysController(fileSysService, cacheService);
        }

        [Test]
        public async Task GetFileSysDetails_WithValidData_ReturnsFileSysDetails()
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
                      Name = @"CreatedBy",
                      Value = "admin257",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSys>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var fileSysList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(fileSysList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, fileSysList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetFileSysDetails_WithEmptyFilterCriteria_ReturnsFileSysDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = @"""UID""",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Empty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSys>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetFileSysDetails_WithEmptySortCriteria_ReturnsFileSysDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"FileSysName",
                Value = "New Zealand FileSys",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSys>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetFileSysDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO FileSys NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve FileSysDetails", result.Value);
        }

        [Test]
        public async Task GetFileSysDetails_WithInvalidSortCriteria_ReturnsUnsortedFileSysDetails()
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
                  Name = "FileSysName",
                  Value = "ABN AMRO FileSys NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve FileSysDetails", result.Value);
        }

        [Test]
        public async Task GetFileSysDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "FileSysName",
                   Value = "ABN AMRO FileSys NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysController.GetFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetFileSysDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "cbccf13f-004asc-u4afd-af710-95af1c8ac3fc";
            IActionResult result = await _fileSysController.GetFileSysByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //FileSys fileSys = okObjectResult.Value as FileSys;
                FileSys fileSys = okObjectResult.Value as FileSys;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(fileSys);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("FileSys Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetFileSysDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _fileSysController.GetFileSysByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatefileSys_ReturnsCreatedResultWithfileSysObject()
        {
            var fileSys = new FileSys
            {
                UID = this.UID,
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                LinkedItemType = "e4wrh08293bjew",
                LinkedItemUID = "80392ro",
                FileSysType = "video",
                FileType = "audio",
                ParentFileSysUID = "2308ro3r",
                IsDirectory = true,
                FileName = "Srinadh",
                DisplayName = "Srinadh",
                FileSize = 450,
                RelativePath = "././wwe",
                Latitude = "2.34.22.4",
                Longitude = "564r43643t456",
            };
            var result = await _fileSysController.CreateFileSys(fileSys) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateFileSys_ReturnsConflictResultWhenFileSysUIDAlreadyExists()
        {
            var existingFileSys = new FileSys
            {
                UID = "cbccf13f-004asc-u4afd-af710-95af1c8ac3fc",
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                LinkedItemType = "e4wrh08293bjew",
                LinkedItemUID = "80392ro",
                FileSysType = "video",
                FileType = "audio",
                ParentFileSysUID = "2308ro3r",
                IsDirectory = true,
                FileName = "Srinadh",
                DisplayName = "Srinadh",
                FileSize = 450,
                RelativePath = "././wwe",
                Latitude = "2.34.22.4",
                Longitude = "564r43643t456",
            };
            var actionResult = await _fileSysController.CreateFileSys(existingFileSys) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateFileSys_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidFileSys = new FileSys
            {
                // Missing required fields
            };
            var actionResult = await _fileSysController.CreateFileSys(invalidFileSys) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatefileSys_SuccessfulUpdate_ReturnsOkWithUpdatedfileSysdetails()
        {
            var fileSys = new FileSys
            {
                UID = "cbccf13f-004asc-u4afd-af710-95af1c8ac3fc",
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                CreatedByEmpUID = "iwesdwylef",
                CreatedByJobPositionUID = "fyfnffjkgqwfss,dfw",
                LinkedItemType = "e4wrh08293bjew",
                LinkedItemUID = "80392ro",
                FileSysType = "video",
                FileType = "audio",
                ParentFileSysUID = "2308ro3r",
                IsDirectory = true,
                FileName = "Srinadh",
                DisplayName = "Srinadh",
                FileSize = 450,
                RelativePath = "././wwe",
                Latitude = "2.34.22.4",
                Longitude = "564r43643t456",
            };
            var result = await _fileSysController.UpdateFileSys(fileSys) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateFileSysdetails_NotFound_ReturnsNotFound()
        {
            var fileSys = new FileSys
            {
                UID = "NDFHN343",
            };
            var result = await _fileSysController.UpdateFileSys(fileSys);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteFileSysDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _fileSysController.DeleteFileSys(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteFileSysDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _fileSysController.DeleteFileSys(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















