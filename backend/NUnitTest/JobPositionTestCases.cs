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
using WINITAPI.Controllers.JobPosition;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.JobPosition.BL.Interfaces;
using Winit.Modules.JobPosition.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NUnitTest
{
    [TestFixture]
    public class JobPositionTestCases
    {
        private JobPositionController _jobPositionController;
        public readonly string _connectionString;

        public JobPositionTestCases()
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
            services.AddSingleton<IJobPosition, JobPosition>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type JobPositiondltype = typeof(Winit.Modules.JobPosition.DL.Classes.PGSQLJobPositionDL);
            Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL JobPositionRepository = (Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL)Activator.CreateInstance(JobPositiondltype, configurationArgs);
            object[] JobPositionRepositoryArgs = new object[] { JobPositionRepository };

            Type JobPositionblType = typeof(Winit.Modules.JobPosition.BL.Classes.JobPositionBL);
            Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL JobPositionService = (Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL)Activator.CreateInstance(JobPositionblType, JobPositionRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _jobPositionController = new JobPositionController(JobPositionService, cacheService);

        }
        [Test]
        public async Task GetJobPositionDetails_WithValidData_ReturnsJobPositionDetails()
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
                      Name = @"Designation",
                      Value = "hyd",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Department",
                     Value ="goods",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IJobPosition>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var JobPositionList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(JobPositionList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetJobPositionDetails_WithJobPositiontyFilterCriteria_ReturnsJobPositionDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // JobPositionty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IJobPosition>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetJobPositionDetails_WithJobPositiontySortCriteria_ReturnsJobPositionDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // JobPositionty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Designation",
                   Value = "hyd",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IJobPosition>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetJobPositionDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO JobPosition No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve JobPosition Details", result.Value);
        }
        [Test]
        public async Task GetJobPositionDetails_WithInvalidSortCriteria_ReturnsUnsortedJobPositionDetails()
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
                  Name = "Designation",
                  Value = "ABN AMRO JobPosition",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve JobPosition Details", result.Value);
        }
        [Test]
        public async Task GetJobPositionDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "Designation",
                   Value = "ABN AMRO JobPosition",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _jobPositionController.GetJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetJobPositionDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1324634636srewsf76767kuhu687766788";
            IActionResult result = await _jobPositionController.GetJobPositionByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                JobPosition JobPosition = okObjectResult.Value as JobPosition;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(JobPosition);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("JobPosition Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetJobPositionDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _jobPositionController.GetJobPositionByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateJobPosition_ReturnsCreatedResultWithJobPositionObject()
        {
            var JobPosition = new JobPosition
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "634346",
                Designation = "hyd",
                EmpUID = "364",
                Department = "sales",
                UserRoleUID = "543",
                LocationUID = "5656",
                OrgUID = "7899",
                SeqCode = "1234",
                HasEOT = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _jobPositionController.CreateJobPosition(JobPosition) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateJobPosition_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingJobPosition = new JobPosition
            {
                UID = "1324634636srewsf76767kuhu6877667886778",
                CompanyUID = "634346",
                Designation = "hyd",
                EmpUID = "364",
                Department = "sales",
                UserRoleUID = "543",
                LocationUID = "5656",
                OrgUID = "7899",
                SeqCode = "1234",
                HasEOT = true,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _jobPositionController.CreateJobPosition(existingJobPosition) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateJobPosition_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidJobPosition = new JobPosition
            {
                // Missing required fields
            };
            var actionResult = await _jobPositionController.CreateJobPosition(invalidJobPosition) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateJobPosition_SuccessfulUpdate_ReturnsOkWithUpdatedJobPositiondetails()
        {
            var JobPosition = new JobPosition
            {
                UID = "132463463hu6877667886778",
                CompanyUID = "634346",
                Designation = "hyd",
                EmpUID = "364",
                Department = "sales",
                UserRoleUID = "543",
                LocationUID = "5656",
                OrgUID = "7899",
                SeqCode = "1234",
                HasEOT = true,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _jobPositionController.UpdateJobPosition(JobPosition) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateJobPositiondetails_NotFound_ReturnsNotFound()
        {
            var JobPosition = new JobPosition
            {
                UID = "NDFHN343",
            };
            var result = await _jobPositionController.UpdateJobPosition(JobPosition);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteJobPositionDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "132463463hu687766788677789778";
            var result = await _jobPositionController.DeleteJobPosition(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteJobPositionDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _jobPositionController.DeleteJobPosition(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
