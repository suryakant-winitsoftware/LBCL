
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
using WINITAPI.Controllers.FileSysTemplate;
using Winit.Shared.Models.Enums;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NunitTest
{
    [TestFixture]
    public class FileSysTemplateTestCases
    {
        private FileSysTemplateController _fileSysTemplateController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public FileSysTemplateTestCases()
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
            services.AddSingleton<IFileSysTemplate, FileSysTemplate>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type FileSysTemplatedltype = typeof(Winit.Modules.FileSys.DL.Classes.PGSQLFileSysTemplateDL);
            Winit.Modules.FileSys.DL.Interfaces.IFileSysTemplateDL FileSysTemplateRepository = (Winit.Modules.FileSys.DL.Interfaces.IFileSysTemplateDL)Activator.CreateInstance(FileSysTemplatedltype, configurationArgs);
            object[] fileSysTemplateRepositoryArgs = new object[] { FileSysTemplateRepository };
            Type fileSysTemplateblType = typeof(Winit.Modules.FileSys.BL.Classes.FileSysTemplateBL);
            Winit.Modules.FileSys.BL.Interfaces.IFileSysTemplateBL fileSysTemplateService = (Winit.Modules.FileSys.BL.Interfaces.IFileSysTemplateBL)Activator.CreateInstance(fileSysTemplateblType, fileSysTemplateRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _fileSysTemplateController = new FileSysTemplateController(fileSysTemplateService, cacheService);
        }

        [Test]
        public async Task GetFileSysTemplateDetails_WithValidData_ReturnsFileSysTemplateDetails()
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
            var result = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSysTemplate>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var fileSysTemplateList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(fileSysTemplateList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, fileSysTemplateList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetFileSysTemplateDetails_WithEmptyFilterCriteria_ReturnsFileSysTemplateDetails()
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
            var result = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSysTemplate>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetFileSysTemplateDetails_WithEmptySortCriteria_ReturnsFileSysTemplateDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"FileSysTemplateName",
                Value = "New Zealand FileSysTemplate",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IFileSysTemplate>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetFileSysTemplateDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO FileSysTemplate NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve FileSysTemplateDetails", result.Value);
        }

        [Test]
        public async Task GetFileSysTemplateDetails_WithInvalidSortCriteria_ReturnsUnsortedFileSysTemplateDetails()
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
                  Name = "FileSysTemplateName",
                  Value = "ABN AMRO FileSysTemplate NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve FileSysTemplateDetails", result.Value);
        }

        [Test]
        public async Task GetFileSysTemplateDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "FileSysTemplateName",
                   Value = "ABN AMRO FileSysTemplate NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _fileSysTemplateController.GetFileSysTemplateDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetFileSysTemplateDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "cbccf13f-0DSF04ayutsc-u4afd-af710-95af1c8ac3fc";
            IActionResult result = await _fileSysTemplateController.GetFileSysTemplateByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //FileSysTemplate fileSysTemplate = okObjectResult.Value as FileSysTemplate;
                FileSysTemplate fileSysTemplate = okObjectResult.Value as FileSysTemplate;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(fileSysTemplate);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("FileSysTemplate Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetFileSysTemplateDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _fileSysTemplateController.GetFileSysTemplateByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatefileSysTemplate_ReturnsCreatedResultWithfileSysTemplateObject()
        {
            var fileSysTemplate = new FileSysTemplate
            {
                UID = this.UID,
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                FileSysType = "video",
                Folder = "frfr",
                RelativePathFormat = "./dwed./ede/",
                IsMobile = true,
                IsServer = false
            };
            var result = await _fileSysTemplateController.CreateFileSysTemplate(fileSysTemplate) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateFileSysTemplate_ReturnsConflictResultWhenFileSysTemplateUIDAlreadyExists()
        {
            var existingFileSysTemplate = new FileSysTemplate
            {
                UID = "cbccf13f-0DSF04ayutsc-u4afd-af710-95af1c8ac3fc",
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                CreatedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                CreatedTime = DateTime.Now,
                FileSysType = "video",
                Folder = "frfr",
                RelativePathFormat = "./dwed./ede/",
                IsMobile = true,
                IsServer = false
            };
            var actionResult = await _fileSysTemplateController.CreateFileSysTemplate(existingFileSysTemplate) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateFileSysTemplate_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidFileSysTemplate = new FileSysTemplate
            {
                // Missing required fields
            };
            var actionResult = await _fileSysTemplateController.CreateFileSysTemplate(invalidFileSysTemplate) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatefileSysTemplate_SuccessfulUpdate_ReturnsOkWithUpdatedfileSysTemplatedetails()
        {
            var fileSysTemplate = new FileSysTemplate
            {
                UID = "cbccf13f-0DSF04ayutsc-u4afd-af710-95af1c8ac3fc",
                ModifiedBy = "f437b533-f66e-49e4-b9d0-2e7b5cc4e23b",
                ModifiedTime = DateTime.Now,
                FileSysType = "video",
                Folder = "frfr",
                RelativePathFormat = "./dwed./ede/",
                IsMobile = true,
                IsServer = false
            };
            var result = await _fileSysTemplateController.UpdateFileSysTemplate(fileSysTemplate) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateFileSysTemplatedetails_NotFound_ReturnsNotFound()
        {
            var fileSysTemplate = new FileSysTemplate
            {
                UID = "NDFHN343",
            };
            var result = await _fileSysTemplateController.UpdateFileSysTemplate(fileSysTemplate);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteFileSysTemplateDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _fileSysTemplateController.DeleteFileSysTemplate(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteFileSysTemplateDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _fileSysTemplateController.DeleteFileSysTemplate(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















