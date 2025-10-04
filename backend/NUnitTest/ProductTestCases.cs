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
    public class ProductTestCases
    {
        private ProductController _productController;
        public readonly string _connectionString;

        public ProductTestCases()
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
            services.AddSingleton<IProduct, Product>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type productdltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductDL);
            Winit.Modules.Product.DL.Interfaces.IProductDL productRepository = (Winit.Modules.Product.DL.Interfaces.IProductDL)Activator.CreateInstance(productdltype, configurationArgs);
            object[] productRepositoryArgs = new object[] { productRepository };

            Type productblType = typeof(Winit.Modules.Product.BL.Classes.ProductBL);
            Winit.Modules.Product.BL.Interfaces.IProductBL productService = (Winit.Modules.Product.BL.Interfaces.IProductBL)Activator.CreateInstance(productblType, productRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productController = new ProductController(productService, cacheService);

        }
        [Test]
        public async Task GetProductDetails_WithValidData_ReturnsProductDetails()
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
                      Name = @"ProductName",
                      Value = "Sample Product",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"LongName",
                     Value ="Sample Product Long Name",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProduct>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var ProductList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(ProductList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductDetails_WithProducttyFilterCriteria_ReturnsProductDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Productty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProduct>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductDetails_WithProducttySortCriteria_ReturnsProductDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Productty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"ProductName",
                   Value = "Sample Product",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProduct>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Product No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Product Details", result.Value);
        }
        [Test]
        public async Task GetProductDetails_WithInvalidSortCriteria_ReturnsUnsortedProductDetails()
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
                  Name = "ProductName",
                  Value = "ABN AMRO Product",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Product Details", result.Value);
        }
        [Test]
        public async Task GetProductDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ProductName",
                   Value = "ABN AMRO Product",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _productController.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetProductDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1324634636srew3456sdsgg234sf8989";
            IActionResult result = await _productController.SelectProductByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Product Product = okObjectResult.Value as Product;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(Product);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Product Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _productController.SelectProductByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateProduct_ReturnsCreatedResultWithProductObject()
        {
            var Product = new Product
            {
                UID = Guid.NewGuid().ToString(),
                OrgUID="jbjkbekjbjh",
                ProductCode = "fggghhgggtgth",
                ProductAliasName = "$@@",
                ProductName = "kjbnfkj",
                LongName = "efkjbwkr",
                DisplayName="khfjwef",
                FromDate=DateTime.Now,
                ToDate=DateTime.Now,
                IsActive=true,
                BaseUOM="PCS",
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productController.CreateProduct(Product) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateProduct_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingProduct = new Product
            {
                UID = "1324634636srew3456sdsgg234sf8989hjhu788",
                OrgUID = "jbjkbekjbjh",
                ProductCode = "fggghhg",
                ProductAliasName = "$@@",
                ProductName = "kjbnfkj",
                LongName = "efkjbwkr",
                DisplayName = "khfjwef",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                IsActive = true,
                BaseUOM = "PCS",
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _productController.CreateProduct(existingProduct) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProduct_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProduct = new Product
            {
                // Missing required fields
            };
            var actionResult = await _productController.CreateProduct(invalidProduct) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateProduct_SuccessfulUpdate_ReturnsOkWithUpdatedProductdetails()
        {
            var Product = new Product
            {
                UID = "1324634636srew3456sdsgg234sf8989hjhu7889998",
                OrgUID = "jbjkbekjbjh",
                ProductCode = "fggghhg",
                ProductAliasName = "$@@",
                ProductName = "kjbnfkj",
                LongName = "efkjbwkr",
                DisplayName = "khfjwef",
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                IsActive = true,
                BaseUOM = "PCS",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productController.UpdateProduct(Product) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateProductdetails_NotFound_ReturnsNotFound()
        {
            var Product = new Product
            {
                UID = "NDFHN343",
            };
            var result = await _productController.UpdateProduct(Product);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteProductDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "1324634636srew3456sdsgg234sf8989hjhu7889998987";
            var result = await _productController.DeleteProduct(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productController.DeleteProduct(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
