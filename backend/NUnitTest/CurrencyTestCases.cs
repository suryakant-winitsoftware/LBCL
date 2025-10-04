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
using WINITAPI.Controllers.Org;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Currency.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Currency.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;

namespace NunitTest
{
    [TestFixture]
    public class CurrencyTestCases
    {

        private CurrencyController _currencyController;
        public readonly string _connectionString;

        public CurrencyTestCases()
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
            services.AddSingleton<ICurrency, Currency>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };

            Type currencydltype = typeof(Winit.Modules.Currency.DL.Classes.PGSQLCurrencyDL);
            Winit.Modules.Currency.DL.Interfaces.ICurrencyDL currencyRepository = (Winit.Modules.Currency.DL.Interfaces.ICurrencyDL)Activator.CreateInstance(currencydltype, configurationArgs);
            object[] currencyRepositoryArgs = new object[] { currencyRepository };

            Type currencyblType = typeof(Winit.Modules.Currency.BL.Classes.CurrencyBL);
            Winit.Modules.Currency.BL.Interfaces.ICurrencyBL currencyService = (Winit.Modules.Currency.BL.Interfaces.ICurrencyBL)Activator.CreateInstance(currencyblType, currencyRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _currencyController = new CurrencyController(currencyService, cacheService);

        }
        [Test]
        public async Task GetCurrencyDetails_WithValidData_ReturnsCurrencyDetails()
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
                      Value = "Euro",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"FractionName",
                     Value ="Cent",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ICurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var CurrencyList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(CurrencyList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);

            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetCurrencyDetails_WithCurrencytyFilterCriteria_ReturnsCurrencyDetails()
        {
            var sortCriterias = new List<SortCriteria>
            {
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Currencyty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ICurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetCurrencyDetails_WithCurrencytySortCriteria_ReturnsCurrencyDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Currencyty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                   Name = @"Name",
                   Value = "Euro",
                   Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<ICurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetCurrencyDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Currency No",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve CurrencyDetails", result.Value);
        }
        [Test]
        public async Task GetCurrencyDetails_WithInvalidSortCriteria_ReturnsUnsortedCurrencyDetails()
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
                  Value = "ABN AMRO Currency",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve CurrencyDetails", result.Value);
        }
        [Test]
        public async Task GetCurrencyDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Value = "ABN AMRO Currency",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _currencyController.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }
        [Test]
        public async Task GetCurrencyDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "SAR";
            IActionResult result = await _currencyController.GetCurrencyById(UID);
            if (result is OkObjectResult okObjectResult)
            {
                Currency currency = okObjectResult.Value as Currency;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(currency);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Currency Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetCurrencyDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _currencyController.GetCurrencyById(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task CreateCurrency_ReturnsCreatedResultWithCurrencyObject()
        {
            var Currency = new Currency
            {
                UID = Guid.NewGuid().ToString(),
                Name = "fggghhg",
                Symbol = "$@@",
                Digits = 12,
                NumberCode=0,
                FractionName="Zuro",
                SS=1,
                CreatedTime = DateTime.Now,             
                ModifiedTime = DateTime.Now,
            };
            var result = await _currencyController.CreateCurrency(Currency) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }
        [Test]
        public async Task CreateCurrency_ReturnsConflictResultWhenOrgUIDAlreadyExists()
        {
            var existingCurrency = new Currency
            {
                UID = "IND1",
                Name = "fgggghg",
                Symbol = "$@",
                Digits = 1,
                NumberCode = 13,
                FractionName = "Zutro",
                SS = 2,
                // CreatedBy = "Mathi",
                CreatedTime = DateTime.Now,
                //ModifiedBy = "Mathi",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _currencyController.CreateCurrency(existingCurrency) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateCurrency_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidCurrency = new Currency
            {
                // Missing required fields
            };
            var actionResult = await _currencyController.CreateCurrency(invalidCurrency) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task UpdateCurrency_SuccessfulUpdate_ReturnsOkWithUpdatedCurrencydetails()
        {
            var Currency = new Currency
            {
                UID = "INDaee",
                Name = "fgggg",
                Symbol = "$",
                Digits = 12,
                NumberCode = 101,
                FractionName = "Zuro",
                SS = 1,
                CreatedTime = DateTime.Now, 
                ModifiedTime = DateTime.Now,
            };
            var result = await _currencyController.UpdateCurrency(Currency) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        [Test]
        public async Task UpdateCurrencydetails_NotFound_ReturnsNotFound()
        {
            var Currency = new Currency
            {
                UID = "NDFHN343",
            };
            var result = await _currencyController.UpdateCurrency(Currency);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task DeleteCurrencyDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "INDaeehjjh";
            var result = await _currencyController.DeleteCurrency(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteCurrencyDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _currencyController.DeleteCurrency(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }

    }


}


















