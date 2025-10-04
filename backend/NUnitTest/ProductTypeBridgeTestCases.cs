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
    public class ProductTypeBridgeTestCases
    {
        private ProductTypeBridgeController _ProductTypeBridgeController;
        public readonly string _connectionString;

        public ProductTypeBridgeTestCases()
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
            services.AddSingleton<IProductTypeBridge, ProductTypeBridge>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type ProductTypeBridgedltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductTypeBridgeDL);
            Winit.Modules.Product.DL.Interfaces.IProductTypeBridgeDL ProductTypeBridgeRepository = (Winit.Modules.Product.DL.Interfaces.IProductTypeBridgeDL)Activator.CreateInstance(ProductTypeBridgedltype, configurationArgs);
            object[] ProductTypeBridgeRepositoryArgs = new object[] { ProductTypeBridgeRepository };

            Type ProductTypeBridgeblType = typeof(Winit.Modules.Product.BL.Classes.ProductTypeBridgeBL);
            Winit.Modules.Product.BL.Interfaces.IProductTypeBridgeBL ProductTypeBridgeService = (Winit.Modules.Product.BL.Interfaces.IProductTypeBridgeBL)Activator.CreateInstance(ProductTypeBridgeblType, ProductTypeBridgeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _ProductTypeBridgeController = new ProductTypeBridgeController(ProductTypeBridgeService, cacheService);

        }
        
        [Test]
        public async Task CreateProductTypeBridge_ReturnsCreatedResultWithProductTypeBridgeObject()
        {
            var ProductTypeBridge = new ProductTypeBridge
            {
                UID = Guid.NewGuid().ToString(),
                ProductCode = "fggghhg",
                ProductTypeUID = "1324634636srew3456234sf",               
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var result = await _ProductTypeBridgeController.CreateProductTypeBridge(ProductTypeBridge) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateProductTypeBridge_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingProductTypeBridge = new ProductTypeBridge
            {
                UID = "1324634636srew3456234sfrgrdfd3453gr",
                ProductCode = "fggghhg",
                ProductTypeUID = "1324634636srew3456234sf",
                CreatedBy = "Mathi",
                ModifiedBy = "Mathi",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _ProductTypeBridgeController.CreateProductTypeBridge(existingProductTypeBridge) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductTypeBridge_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductTypeBridge = new ProductTypeBridge
            {
                // Missing required fields
            };
            var actionResult = await _ProductTypeBridgeController.CreateProductTypeBridge(invalidProductTypeBridge) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task DeleteProductTypeBridge_ShouldReturnOk_WhenExists()
        {
            string UID = "1324634636srew3456234sfrgrdfd3453gr6565333";
            var result = await _ProductTypeBridgeController.DeleteProductTypeBridgeByUID(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductTypeBridgeDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _ProductTypeBridgeController.DeleteProductTypeBridgeByUID(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }

    }
}
