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
using WINITAPI.Controllers.SKUClass;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.SKUClass.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace NUnitTest
{
    [TestFixture]
    public class SKUClassGroupTestCases
    {
        private SKUClassGroupController _sKUClassGroupController;
        public readonly string _connectionString;

        public SKUClassGroupTestCases()
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
            services.AddSingleton<ISKUClassGroup, SKUClassGroup>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type sKUClassGroupdltype = typeof(Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassGroupDL);
            Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupDL sKUClassGroupRepository = (Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupDL)Activator.CreateInstance(sKUClassGroupdltype, configurationArgs);
            object[] sKUClassGroupRepositoryArgs = new object[] { sKUClassGroupRepository };

            Type sKUClassGroupblType = typeof(Winit.Modules.SKUClass.BL.Classes.SKUClassGroupBL);
            Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupBL sKUClassGroupService = (Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupBL)Activator.CreateInstance(sKUClassGroupblType, sKUClassGroupRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _sKUClassGroupController = new SKUClassGroupController(sKUClassGroupService, cacheService);

        }
        [Test]
        public async Task GetSKUClassGroupDetails_WithValidData_ReturnsSKUClassGroupDetails()
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
                      Name = @"Name",
                      Value ="codjkemalt",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"Description",
                     Value ="Maltcfhode",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroup>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var SKUClassGroupList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClassGroupList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassGroupDetails_WithSKUClassGrouptyFilterCriteria_ReturnsSKUClassGroupDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // SKUClassGroupty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroup>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassGroupDetails_WithSKUClassGrouptySortCriteria_ReturnsSKUClassGroupDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // SKUClassGroupty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Name",
                   Value = "codjkemalt",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUClassGroup>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUClassGroupDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO SKUClassGroup No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClassGroup Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupDetails_WithInvalidSortCriteria_ReturnsUnsortedSKUClassGroupDetails()
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
                  Name = "Name",
                  Value = "ABN AMRO SKUClassGroup",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUClassGroup Details", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "Name",
                   Value = "ABN AMRO SKUClassGroup",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _sKUClassGroupController.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetSKUClassGroupDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "2d893d92-dc1b-5904-934c";
            IActionResult result = await _sKUClassGroupController.GetSKUClassGroupByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                SKUClassGroup SKUClassGroup = okObjectResult.Value as SKUClassGroup;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(SKUClassGroup);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("SKUClassGroup Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUClassGroupDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _sKUClassGroupController.GetSKUClassGroupByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateSKUClassGroup_ReturnsCreatedResultWithSKUClassGroupObject()
        {
            var SKUClassGroup = new SKUClassGroup
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "1234",
                SKUClassUID = "6789",
                Name = "Mathi",
                Description = "sdfasd",
                OrgUID="",
                DistributionChannelUID="asfd",
                FranchiseeOrgUID="sdzfx",
                IsActive=true,
                FromDate= DateTime.Now,
                ToDate= DateTime.Now,
                SourceType="qwadf",
                SourceDate= DateTime.Now,
                Priority =1,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassGroupController.CreateSKUClassGroup(SKUClassGroup) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateSKUClassGroup_ReturnsConflictResultWhenSKUClassGroupUIDAlreadyExists()
        {
            var existingSKUClassGroup = new SKUClassGroup
            {
                UID = "2d893d92-dc1b-5904-934454c",
                CompanyUID = "1234",
                SKUClassUID = "6789",
                Name = "Mathi",
                Description = "sdfasd",
                OrgUID = "",
                DistributionChannelUID = "asfd",
                FranchiseeOrgUID = "sdzfx",
                IsActive = true,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                SourceType = "qwadf",
                SourceDate = DateTime.Now,
                Priority = 1,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _sKUClassGroupController.CreateSKUClassGroup(existingSKUClassGroup) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateSKUClassGroup_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidSKUClassGroup = new SKUClassGroup
            {
                // Missing required fields
            };
            var actionResult = await _sKUClassGroupController.CreateSKUClassGroup(invalidSKUClassGroup) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateSKUClassGroup_SuccessfulUpdate_ReturnsOkWithUpdatedSKUClassGroupdetails()
        {
            var SKUClassGroup = new SKUClassGroup
            {
                UID = "2d893d92-dc1b-5904-934454445646c",
                CompanyUID = "1234",
                SKUClassUID = "6789",
                Name = "Mathi",
                Description = "sdfasd",
                OrgUID = "",
                DistributionChannelUID = "asfd",
                FranchiseeOrgUID = "sdzfx",
                IsActive = true,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                SourceType = "qwadf",
                SourceDate = DateTime.Now,
                Priority = 1,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _sKUClassGroupController.UpdateSKUClassGroup(SKUClassGroup) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateSKUClassGroupdetails_NotFound_ReturnsNotFound()
        {
            var SKUClassGroup = new SKUClassGroup
            {
                UID = "NDFHN343",
            };
            var result = await _sKUClassGroupController.UpdateSKUClassGroup(SKUClassGroup);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteSKUClassGroupDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "2d893d92-dc1b-5904-934454445646c1234";
            var result = await _sKUClassGroupController.DeleteSKUClassGroup(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteSKUClassGroupDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _sKUClassGroupController.DeleteSKUClassGroup(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
