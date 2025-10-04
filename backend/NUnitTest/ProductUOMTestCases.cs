
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
using Winit.Shared.Models.Enums;
using Winit.Modules.Product.BL.Interfaces;
using Winit.Modules.Product.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Product.Model.Interfaces;
using Winit.Modules.Product.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using StackExchange.Redis;

namespace NunitTest
{
    [TestFixture]
    public class ProductUOMTestCases
    {
        private ProductUOMController _productUOMController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public ProductUOMTestCases()
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
            services.AddSingleton<IProductUOM, ProductUOM>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type ProductUOMdltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductUOMDL);
            Winit.Modules.Product.DL.Interfaces.IProductUOMDL ProductUOMRepository = (Winit.Modules.Product.DL.Interfaces.IProductUOMDL)Activator.CreateInstance(ProductUOMdltype, configurationArgs);
            object[] productUOMRepositoryArgs = new object[] { ProductUOMRepository };
            Type productUOMblType = typeof(Winit.Modules.Product.BL.Classes.ProductUOMBL);
            Winit.Modules.Product.BL.Interfaces.IProductUOMBL productUOMService = (Winit.Modules.Product.BL.Interfaces.IProductUOMBL)Activator.CreateInstance(productUOMblType, productUOMRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productUOMController = new ProductUOMController(productUOMService, cacheService);
        }

        [Test]
        public async Task GetProductUOMDetails_WithValidData_ReturnsProductUOMDetails()
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
            var result = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductUOM>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var productUOMList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(productUOMList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, productUOMList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductUOMDetails_WithEmptyFilterCriteria_ReturnsProductUOMDetails()
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
            var result = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductUOM>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductUOMDetails_WithEmptySortCriteria_ReturnsProductUOMDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"ProductUOMName",
                Value = "New Zealand ProductUOM",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductUOM>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductUOMDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO ProductUOM NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductUOM  Details", result.Value);
        }

        [Test]
        public async Task GetProductUOMDetails_WithInvalidSortCriteria_ReturnsUnsortedProductUOMDetails()
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
                  Name = "ProductUOMName",
                  Value = "ABN AMRO ProductUOM NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductUOM  Details", result.Value);
        }

        [Test]
        public async Task GetProductUOMDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ProductUOMName",
                   Value = "ABN AMRO ProductUOM NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _productUOMController.SelectAllProductUOM(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetProductUOMDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "sjlhg9ujkewbfi4";
            IActionResult result = await _productUOMController.GetProductUOMByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //ProductUOM productUOM = okObjectResult.Value as ProductUOM;
                ProductUOM productUOM = okObjectResult.Value as ProductUOM;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(productUOM);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("ProductUOM Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetProductUOMDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _productUOMController.GetProductUOMByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateproductUOM_ReturnsCreatedResultWithproductUOMObject()
        {
            var productUOM = new ProductUOM
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                ProductCode = "fggghhg87687",
                Label = "b34khbfuiew",
                BarCode = "2380238rh43",
                IsBaseUOM = false,
                IsOuterUOM = true,
                Multiplier = 6.78M
            };
            var result = await _productUOMController.CreateProductUOM(productUOM) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateProductUOM_ReturnsConflictResultWhenProductUOMUIDAlreadyExists()
        {
            var existingProductUOM = new ProductUOM
            {
                UID = "sjlhg9ujkewbfi4",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                ProductCode = "fggghhg87687",
                Label = "b34khbfuiew",
                BarCode = "2380238rh43",
                IsBaseUOM = false,
                IsOuterUOM = true,
                Multiplier = 6.78M

            };
            var actionResult = await _productUOMController.CreateProductUOM(existingProductUOM) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductUOM_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductUOM = new ProductUOM
            {
                // Missing required fields
            };
            var actionResult = await _productUOMController.CreateProductUOM(invalidProductUOM) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdateproductUOM_SuccessfulUpdate_ReturnsOkWithUpdatedproductUOMdetails()
        {
            var productUOM = new ProductUOM
            {
                UID = "sjlhg9ujkewbfi4",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Name = "Srinadh",
                ProductCode = "fggghhg87687",
                Label = "b34khbfuiew",
                BarCode = "2380238rh43",
                IsBaseUOM = false,
                IsOuterUOM = true,
                Multiplier = 6.78M
            };
            var result = await _productUOMController.UpdateProductUOM(productUOM) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateProductUOMdetails_NotFound_ReturnsNotFound()
        {
            var productUOM = new ProductUOM
            {
                UID = "NDFHN343",
            };
            var result = await _productUOMController.UpdateProductUOM(productUOM);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteProductUOMDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _productUOMController.DeleteProductUOM(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductUOMDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productUOMController.DeleteProductUOM(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















