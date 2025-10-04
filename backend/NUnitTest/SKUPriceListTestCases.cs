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
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WINITAPI.Controllers.SKU;

namespace NunitTest
{
    [TestFixture]
    public class SKUPriceListTestCases
    {
        private SKUPriceListController _sKUPriceListController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public SKUPriceListTestCases()
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
            services.AddSingleton<ISKUPriceList, SKUPriceList>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type sKUPriceListdltype = typeof(Winit.Modules.SKU.DL.Classes.PGSQLSKUPriceListDL);
            Winit.Modules.SKU.DL.Interfaces.ISKUPriceListDL sKUPriceListRepository = (Winit.Modules.SKU.DL.Interfaces.ISKUPriceListDL)Activator.CreateInstance(sKUPriceListdltype, configurationArgs);
            object[] sKUPriceListRepositoryArgs = new object[] { sKUPriceListRepository };
            Type sKUPriceListblType = typeof(Winit.Modules.SKU.BL.Classes.SKUPriceListBL);
            Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL sKUPriceListService = (Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL)Activator.CreateInstance(sKUPriceListblType, sKUPriceListRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _sKUPriceListController = new SKUPriceListController(sKUPriceListService, cacheService);
        }

        [Test]
        public async Task GetSKUPriceListDetails_WithValidData_ReturnsSKUPriceListDetails()
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
                      Value = "2d893d92-dc1b-5904-934c-621103a900e39",
                      Type = Winit.Shared.Models.Enums.FilterType.Like
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUPriceList>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var sKUPriceList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(sKUPriceList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, sKUPriceList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetSKUPriceListDetails_WithEmptyFilterCriteria_ReturnsSKUPriceListDetails()
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
            var result = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUPriceList>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUPriceListDetails_WithEmptySortCriteria_ReturnsSKUPriceListDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"SKUPriceListName",
                Value = "New Zealand SKUPriceList",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ISKUPriceList>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetSKUPriceListDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO SKUPriceList NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUPriceList Details ", result.Value);
        }

        [Test]
        public async Task GetSKUPriceListDetails_WithInvalidSortCriteria_ReturnsUnsortedSKUPriceListDetails()
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
                  Name = "SKUPriceListName",
                  Value = "ABN AMRO SKUPriceList NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve SKUPriceList Details ", result.Value);
        }

        [Test]
        public async Task GetSKUPriceListDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "SKUPriceListName",
                   Value = "ABN AMRO SKUPriceList NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _sKUPriceListController.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetSKUPriceListDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1c086069-514c-4d22-9fe2-94af9c1beef2";
            IActionResult result = await _sKUPriceListController.SelectSKUPriceListByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //SKUPriceList sKUPriceList = okObjectResult.Value as SKUPriceList;
                SKUPriceList sKUPriceList = okObjectResult.Value as SKUPriceList;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(sKUPriceList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("SKUPriceList Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetSKUPriceListDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _sKUPriceListController.SelectSKUPriceListByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateSKUPriceList_ReturnsCreatedResultWithSKUPriceListObject()
        {
            var sKUPriceList = new SKUPriceList
            {
                UID =this.UID ,
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                Status = "ojsfj",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now,
                IsActive = true,
                CompanyUID = "gyreb",
                Code = "huwfgfwe",
                Name = "srinadh",
                Type = "fiwefi",
                OrgUID = "bfwieyfg",
                DistributionChannelUID = "9236fwet",
                Priority = 1,
                SelectionGroup = "fbweif",
                SelectionType = "yfbwe",
                SelectionUID = "SelectionUID",
            };
            var result = await _sKUPriceListController.CreateSKUPriceList(sKUPriceList) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateSKUPriceList_ReturnsConflictResultWhenSKUPriceListUIDAlreadyExists()
        {
            var existingSKUPriceList = new SKUPriceList
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                CreatedTime = DateTime.Now,
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                Status = "ojsfj",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now,
                IsActive = true,
                CompanyUID = "gyreb",
                Code = "huwfgfwe",
                Name = "srinadh",
                Type = "fiwefi",
                OrgUID = "bfwieyfg",
                DistributionChannelUID = "9236fwet",
                Priority = 1,
                SelectionGroup = "fbweif",
                SelectionType = "yfbwe",
                SelectionUID = "SelectionUID",
            };
            var actionResult = await _sKUPriceListController.CreateSKUPriceList(existingSKUPriceList) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateSKUPriceList_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidSKUPriceList = new SKUPriceList
            {
                // Missing required fields
            };
            var actionResult = await _sKUPriceListController.CreateSKUPriceList(invalidSKUPriceList) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdateSKUPriceList_SuccessfulUpdate_ReturnsOkWithUpdatedSKUPriceListdetails()
        {
            var sKUPriceList = new SKUPriceList
            {
                UID = "1c086069-514c-4d22-9fe2-94af9c1beef2",
                ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39",
                ModifiedTime = DateTime.Now,
                Status = "ojsfj",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now,
                IsActive = true,
                CompanyUID = "gyreb",
                Code = "huwfgfwe",
                Name = "srinadh",
                Type = "fiwefi",
                OrgUID="bfwieyfg",
                DistributionChannelUID="9236fwet",
                Priority=1,
                SelectionGroup = "fbweif",
                SelectionType="yfbwe",
                SelectionUID= "SelectionUID",
            };
            var result = await _sKUPriceListController.UpdateSKUPriceList(sKUPriceList) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateSKUPriceListdetails_NotFound_ReturnsNotFound()
        {
            var sKUPriceList = new SKUPriceList
            {
                UID = "NDFHN343",
            };
            var result = await _sKUPriceListController.UpdateSKUPriceList(sKUPriceList);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteSKUPriceListDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _sKUPriceListController.DeleteSKUPriceList(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteSKUPriceListDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _sKUPriceListController.DeleteSKUPriceList(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















