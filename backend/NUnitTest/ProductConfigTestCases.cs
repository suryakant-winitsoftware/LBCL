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
using WINITAPI.Controllers.Product;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Product.BL.Interfaces;
using Winit.Modules.Product.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Product.Model.Interfaces;
using Winit.Modules.Product.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NUnitTest
{
    [TestFixture]
    public class ProductConfigTestCases
    {
        private ProductConfigController _productConfigController;
        public readonly string _connectionString;

        public ProductConfigTestCases()
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
            services.AddSingleton<IProductConfig, ProductConfig>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type ProductConfigdltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductConfigDL);
            Winit.Modules.Product.DL.Interfaces.IProductConfigDL ProductConfigRepository = (Winit.Modules.Product.DL.Interfaces.IProductConfigDL)Activator.CreateInstance(ProductConfigdltype, configurationArgs);
            object[] ProductConfigRepositoryArgs = new object[] { ProductConfigRepository };

            Type ProductConfigblType = typeof(Winit.Modules.Product.BL.Classes.ProductConfigBL);
            Winit.Modules.Product.BL.Interfaces.IProductConfigBL ProductConfigService = (Winit.Modules.Product.BL.Interfaces.IProductConfigBL)Activator.CreateInstance(ProductConfigblType, ProductConfigRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productConfigController = new ProductConfigController(ProductConfigService, cacheService);

        }
        [Test]
        public async Task GetProductConfigDetails_WithValidData_ReturnsProductConfigDetails()
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
                      Name = @"ProductCode",
                      Value = "PROD123",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"BuyingUOM",
                     Value ="PCSupdate",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductConfig>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var ProductConfigList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(ProductConfigList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductConfigDetails_WithProductConfigtyFilterCriteria_ReturnsProductConfigDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // ProductConfigty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductConfig>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductConfigDetails_WithProductConfigtySortCriteria_ReturnsProductConfigDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // ProductConfigty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"ProductCode",
                   Value = "PROD123",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductConfig>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductConfigDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO ProductConfig No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Product Config Details", result.Value);
        }
        [Test]
        public async Task GetProductConfigDetails_WithInvalidSortCriteria_ReturnsUnsortedProductConfigDetails()
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
                  Name = "ProductCode",
                  Value = "ABN AMRO ProductConfig",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Product Config Details", result.Value);
        }
        [Test]
        public async Task GetProductConfigDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ProductConfigName",
                   Value = "ABN AMRO ProductConfig",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _productConfigController.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetProductConfigDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1324634636srew3456sdsgg234sf8989hjhu78dfd8";
            IActionResult result = await _productConfigController.SelectProductConfigByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                ProductConfig ProductConfig = okObjectResult.Value as ProductConfig;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(ProductConfig);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("ProductConfig Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductConfigDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _productConfigController.SelectProductConfigByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateProductConfig_ReturnsCreatedResultWithProductConfigObject()
        {
            var ProductConfig = new ProductConfig
            {
                UID = Guid.NewGuid().ToString(),
                ProductCode = "fggghhg",
                DistributionChannelOrgUID = "fggghhgggtgth",
                CanBuy = true,
                CanSell = false,
                BuyingUOM = "efkjbwkr",
                SellingUOM = "khfjwef",
                IsActive = true,
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productConfigController.CreateProductConfig(ProductConfig) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateProductConfig_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingProductConfig = new ProductConfig
            {
                UID = "1324634636srew3456sdsgg234sf8989hjhu78dfd844f4",
                ProductCode = "fggghhg",
                DistributionChannelOrgUID = "fggghhgggtgth",
                CanBuy = true,
                CanSell = false,
                BuyingUOM = "efkjbwkr",
                SellingUOM = "khfjwef",
                IsActive = true,
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _productConfigController.CreateProductConfig(existingProductConfig) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductConfig_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductConfig = new ProductConfig
            {
                // Missing required fields
            };
            var actionResult = await _productConfigController.CreateProductConfig(invalidProductConfig) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateProductConfig_SuccessfulUpdate_ReturnsOkWithUpdatedProductConfigdetails()
        {
            var ProductConfig = new ProductConfig
            {
                UID = "132459hjhu78dfd844f4",
                ProductCode = "fggghhg",
                DistributionChannelOrgUID = "fggghhggg44tgth",
                CanBuy = true,
                CanSell = false,
                BuyingUOM = "efkjbwkr",
                SellingUOM = "khfjwef",
                IsActive = true,
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productConfigController.UpdateProductConfig(ProductConfig) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateProductConfigdetails_NotFound_ReturnsNotFound()
        {
            var ProductConfig = new ProductConfig
            {
                UID = "NDFHN343",
            };
            var result = await _productConfigController.UpdateProductConfig(ProductConfig);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteProductConfigDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "132459hjhu78dfd844f3434";
            var result = await _productConfigController.DeleteProductConfig(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductConfigDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productConfigController.DeleteProductConfig(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
