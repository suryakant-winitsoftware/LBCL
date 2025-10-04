
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
    public class ProductDimensionBridgeTestCases
    {
        private ProductDimensionBridgeController _productDimensionBridgeController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public ProductDimensionBridgeTestCases()
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
            services.AddSingleton<IProductDimensionBridge, ProductDimensionBridge>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type ProductDimensionBridgedltype = typeof(Winit.Modules.Product.DL.Classes.PGSQLProductDimensionBridgeDL);
            Winit.Modules.Product.DL.Interfaces.IProductDimensionBridgeDL ProductDimensionBridgeRepository = (Winit.Modules.Product.DL.Interfaces.IProductDimensionBridgeDL)Activator.CreateInstance(ProductDimensionBridgedltype, configurationArgs);
            object[] productDimensionBridgeRepositoryArgs = new object[] { ProductDimensionBridgeRepository };
            Type productDimensionBridgeblType = typeof(Winit.Modules.Product.BL.Classes.ProductDimensionBridgeBL);
            Winit.Modules.Product.BL.Interfaces.IProductDimensionBridgeBL productDimensionBridgeService = (Winit.Modules.Product.BL.Interfaces.IProductDimensionBridgeBL)Activator.CreateInstance(productDimensionBridgeblType, productDimensionBridgeRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _productDimensionBridgeController = new ProductDimensionBridgeController(productDimensionBridgeService, cacheService);
        }

        

        [Test]
        public async Task CreateproductDimensionBridge_ReturnsCreatedResultWithproductDimensionBridgeObject()
        {
            var productDimensionBridge = new ProductDimensionBridge
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                ProductCode = "fggghhg87687988977",
                ProductDimensionUID = "sjlhg9ujkewbfi4"
            };
            var result = await _productDimensionBridgeController.CreateProductDimensionBridge(productDimensionBridge) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateProductDimensionBridge_ReturnsConflictResultWhenProductDimensionBridgeUIDAlreadyExists()
        {
            var existingProductDimensionBridge = new ProductDimensionBridge
            {
                UID = "sjlhg9ujkewsdbfi4",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                ProductCode = "fggghhg87687988977",
                ProductDimensionUID = "sjlhg9ujkewbfi4"
            };
            var actionResult = await _productDimensionBridgeController.CreateProductDimensionBridge(existingProductDimensionBridge) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateProductDimensionBridge_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidProductDimensionBridge = new ProductDimensionBridge
            {
                // Missing required fields
            };
            var actionResult = await _productDimensionBridgeController.CreateProductDimensionBridge(invalidProductDimensionBridge) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task DeleteProductDimensionBridgeDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _productDimensionBridgeController.DeleteProductDimensionBridge(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteProductDimensionBridgeDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _productDimensionBridgeController.DeleteProductDimensionBridge(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















