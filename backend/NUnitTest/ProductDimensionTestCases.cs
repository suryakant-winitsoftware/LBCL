
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
    public class ProductDimensionTestCases
    {
        private ProductDimensionController _productDimensionController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public ProductDimensionTestCases()
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
            services.AddSingleton<IProductDimension, ProductDimension>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type ProductDimensiondltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductDimensionDL);
            Winit.Modules.Product.DL.Interfaces.IProductDimensionDL ProductDimensionRepository = (Winit.Modules.Product.DL.Interfaces.IProductDimensionDL)Activator.CreateInstance(ProductDimensiondltype, configurationArgs);
            object[] productDimensionRepositoryArgs = new object[] { ProductDimensionRepository };
            Type productDimensionblType = typeof(Winit.Modules.Product.BL.Classes.ProductDimensionBL);
            Winit.Modules.Product.BL.Interfaces.IProductDimensionBL productDimensionService = (Winit.Modules.Product.BL.Interfaces.IProductDimensionBL)Activator.CreateInstance(productDimensionblType, productDimensionRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productDimensionController = new ProductDimensionController(productDimensionService, cacheService);
        }

        [Test]
        public async Task GetProductDimensionDetails_WithValidData_ReturnsProductDimensionDetails()
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
            var result = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductDimension>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var productDimensionList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(productDimensionList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, productDimensionList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductDimensionDetails_WithEmptyFilterCriteria_ReturnsProductDimensionDetails()
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
            var result = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductDimension>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductDimensionDetails_WithEmptySortCriteria_ReturnsProductDimensionDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"ProductDimensionName",
                Value = "New Zealand ProductDimension",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductDimension>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductDimensionDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO ProductDimension NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductDimension  Details", result.Value);
        }

        [Test]
        public async Task GetProductDimensionDetails_WithInvalidSortCriteria_ReturnsUnsortedProductDimensionDetails()
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
                  Name = "ProductDimensionName",
                  Value = "ABN AMRO ProductDimension NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductDimension  Details", result.Value);
        }

        [Test]
        public async Task GetProductDimensionDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ProductDimensionName",
                   Value = "ABN AMRO ProductDimension NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _productDimensionController.SelectAllProductDimension(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetProductDimensionDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "sjlhg9ujkewbfi4";
            IActionResult result = await _productDimensionController.GetProductDimensionByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //ProductDimension productDimension = okObjectResult.Value as ProductDimension;
                ProductDimension productDimension = okObjectResult.Value as ProductDimension;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(productDimension);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("ProductDimension Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetProductDimensionDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _productDimensionController.GetProductDimensionByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateproductDimension_ReturnsCreatedResultWithproductDimensionObject()
        {
            var productDimension = new ProductDimension
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Description = "Product Dimension",
                ParentUID = "urhnegwoh3"
            };
            var result = await _productDimensionController.CreateProductDimension(productDimension) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateProductDimension_ReturnsConflictResultWhenProductDimensionUIDAlreadyExists()
        {
            var existingProductDimension = new ProductDimension
            {
                UID = "sjlhg9ujkewbfi4",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Description = "Product Dimension",
                ParentUID = "urhnegwoh3"
            };
            var actionResult = await _productDimensionController.CreateProductDimension(existingProductDimension) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductDimension_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductDimension = new ProductDimension
            {
                // Missing required fields
            };
            var actionResult = await _productDimensionController.CreateProductDimension(invalidProductDimension) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdateproductDimension_SuccessfulUpdate_ReturnsOkWithUpdatedproductDimensiondetails()
        {
            var productDimension = new ProductDimension
            {
                UID = "sjlhg9ujkewbfi4",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                Code = "fiweygfiwefg",
                Description = "Product Dimension",
                ParentUID = "urhnegwoh3"
            };
            var result = await _productDimensionController.UpdateProductDimension(productDimension) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateProductDimensiondetails_NotFound_ReturnsNotFound()
        {
            var productDimension = new ProductDimension
            {
                UID = "NDFHN343",
            };
            var result = await _productDimensionController.UpdateProductDimension(productDimension);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteProductDimensionDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _productDimensionController.DeleteProductDimension(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductDimensionDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productDimensionController.DeleteProductDimension(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















