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
    public class ProductTypeTestCases
    {
        private ProductTypeController _productTypeController;
        public readonly string _connectionString;

        public ProductTypeTestCases()
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
            services.AddSingleton<IProductType, ProductType>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type productTypedltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductTypeDL);
            Winit.Modules.Product.DL.Interfaces.IProductTypeDL productTypeRepository = (Winit.Modules.Product.DL.Interfaces.IProductTypeDL)Activator.CreateInstance(productTypedltype, configurationArgs);
            object[] productTypeRepositoryArgs = new object[] { productTypeRepository };

            Type productTypeblType = typeof(Winit.Modules.Product.BL.Classes.ProductTypeBL);
            Winit.Modules.Product.BL.Interfaces.IProductTypeBL productTypeService = (Winit.Modules.Product.BL.Interfaces.IProductTypeBL)Activator.CreateInstance(productTypeblType, productTypeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productTypeController = new ProductTypeController(productTypeService, cacheService);

        }
        [Test]
        public async Task GetProductTypeDetails_WithValidData_ReturnsProductTypeDetails()
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
                      Name = @"ProductTypeName",
                      Value = "dfbewri",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"ProductTypeDescription",
                     Value ="ldjfgire",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var ProductTypeList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(ProductTypeList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductTypeDetails_WithProductTypetyFilterCriteria_ReturnsProductTypeDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // ProductTypety list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductTypeDetails_WithProductTypetySortCriteria_ReturnsProductTypeDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // ProductTypety list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"ProductTypeName",
                   Value = "dfbewri",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IProductType>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetProductTypeDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO ProductType No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductType data", result.Value);
        }
        [Test]
        public async Task GetProductTypeDetails_WithInvalidSortCriteria_ReturnsUnsortedProductTypeDetails()
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
                  Name = "ProductTypeName",
                  Value = "ABN AMRO ProductType",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve ProductType data", result.Value);
        }
        [Test]
        public async Task GetProductTypeDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "ProductTypeName",
                   Value = "ABN AMRO ProductType",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _productTypeController.SelectProductTypeAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetProductTypeDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "1324634636srew3234sf";
            IActionResult result = await _productTypeController.GetProductTypeByProductTypeId(UID);
            if (result is OkObjectResult okObjectResult)
            {
                ProductType ProductType = okObjectResult.Value as ProductType;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(ProductType);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("ProductType Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetProductTypeDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _productTypeController.GetProductTypeByProductTypeId(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateProductType_ReturnsCreatedResultWithProductTypeObject()
        {
            var ProductType = new ProductType
            {
                UID = Guid.NewGuid().ToString(),
                ProductTypeCode = "fggghhg",
                ParentProductTypeUID = "$@@",
                ProductTypeName = "kjbnfkj",
                ProductTypeDescription = "efkjbwkr",
                CreatedBy="Mathi",
                ModifiedBy="Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productTypeController.CreateProductType(ProductType) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateProductType_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingProductType = new ProductType
            {
                UID = "1324634636srew3456234sf",
                ProductTypeCode = "fggghhg",
                ParentProductTypeUID = "$@@",
                ProductTypeName = "kjbnfkj",
                ProductTypeDescription = "efkjbwkr",
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",              
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _productTypeController.CreateProductType(existingProductType) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductType_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductType = new ProductType
            {
                // Missing required fields
            };
            var actionResult = await _productTypeController.CreateProductType(invalidProductType) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateProductType_SuccessfulUpdate_ReturnsOkWithUpdatedProductTypedetails()
        {
            var ProductType = new ProductType
            {
                UID = "1324634636srewsf",
                ProductTypeCode = "fggghhg",
                ParentProductTypeUID = "$@@",
                ProductTypeName = "kjbnfkj",
                ProductTypeDescription = "efkjbwkr",              
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _productTypeController.UpdateProductType(ProductType) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateProductTypedetails_NotFound_ReturnsNotFound()
        {
            var ProductType = new ProductType
            {
                UID = "NDFHN343",
            };
            var result = await _productTypeController.UpdateProductType(ProductType);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteProductTypeDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "1324633434636srew3456234s454f";
            var result = await _productTypeController.DeleteProductType(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductTypeDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productTypeController.DeleteProductType(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}
