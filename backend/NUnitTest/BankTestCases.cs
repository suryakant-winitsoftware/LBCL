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
using WINITAPI.Controllers.Bank;
using WINITServices.Classes.Bank;
using WINITServices.Classes.CacheHandler;
using WINITServices.Classes.Customer;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Enums;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.Bank.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Bank.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NunitTest
{
    [TestFixture]
    public class BankTestCases
    {
        private BankController _bankController;
        public readonly string _connectionString;
        public BankTestCases()
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
            services.AddSingleton<IBank, Bank>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type bankdltype = typeof(Winit.Modules.Bank.DL.Classes.PGSQLBankDL);
            Winit.Modules.Bank.DL.Interfaces.IBankDL bankRepository = (Winit.Modules.Bank.DL.Interfaces.IBankDL)Activator.CreateInstance(bankdltype, configurationArgs);
            object[] bankRepositoryArgs = new object[] { bankRepository };
            Type bankblType = typeof(Winit.Modules.Bank.BL.Classes.BankBL);
            Winit.Modules.Bank.BL.Interfaces.IBankBL bankService = (Winit.Modules.Bank.BL.Interfaces.IBankBL)Activator.CreateInstance(bankblType, bankRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _bankController = new BankController(bankService, cacheService);
        }

        [Test]
        public async Task GetBankDetails_WithValidData_ReturnsBankDetails()
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
                      Name = @"BankName",
                      Value = "New Zealand Bank",
                      Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
                 new Winit.Shared.Models.Enums.FilterCriteria
                 {
                     Name = @"CountryUID",
                     Value = "NZ",
                     Type = Winit.Shared.Models.Enums.FilterType.Equal
                 },
            };
            var pageNumber = 1;
            var pageSize = 2;
            var result = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IBank>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var bankList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(bankList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, bankList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetBankDetails_WithEmptyFilterCriteria_ReturnsBankDetails()
        {
            var sortCriterias = new List<SortCriteria>
            { 
                new SortCriteria
                {
                    SortParameter = "UID",
                    Direction = SortDirection.Asc
                }
            };
            var filterCriterias = new List<FilterCriteria>(); // Empty list
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IBank>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetBankDetails_WithEmptySortCriteria_ReturnsBankDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"BankName",
                Value = "New Zealand Bank",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IBank>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetBankDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO Bank NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve BankDetails", result.Value);
        }

        [Test]
        public async Task GetBankDetails_WithInvalidSortCriteria_ReturnsUnsortedBankDetails()
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
                  Name = "BankName",
                  Value = "ABN AMRO Bank NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve BankDetails", result.Value);
        }

        [Test]
        public async Task GetBankDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "BankName",
                   Value = "ABN AMRO Bank NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetBankDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID = "BNknzd";
            IActionResult result = await _bankController.GetBankDetailsByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //Bank bank = okObjectResult.Value as Bank;
                Bank bank = okObjectResult.Value as Bank;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(bank);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("Bank Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetBankDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _bankController.GetBankDetailsByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateBnak_ReturnsCreatedResultWithbankObject()
        {
            var bank = new Bank
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = "FBNZ",
                BankName = "Bank of New Zealand",
                CountryUID = "NZ",
                ChequeFee = 0,
                CreatedBy = "Ramana",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Ramana",
                ModifiedTime = DateTime.Now,
            };
            var result = await _bankController.CreateBankDetails(bank) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(202, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateBank_ReturnsConflictResultWhenBankUIDAlreadyExists()
        {
            var existingBank = new Bank
            {
                // UID = "ExistingBank",

                UID = "d4a57390-7980-4959-a7b1-1c5d1de74439",
                CompanyUID = "FBNZ",
                BankName = "Bank of New Zealand",
                CountryUID = "NZ",
                ChequeFee = 0,

                CreatedBy = "Ramana",
                CreatedTime = DateTime.Now,
                ModifiedBy = "Ramana",
                ModifiedTime = DateTime.Now,
            };
            var actionResult = await _bankController.CreateBankDetails(existingBank) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateBank_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidBank = new Bank
            {
                // Missing required fields
            };
            var actionResult = await _bankController.CreateBankDetails(invalidBank) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task Updatebank_SuccessfulUpdate_ReturnsOkWithUpdatedbankdetails()
        {
            var bank = new Bank
            {
                UID = "e5599f10-2df6-4b17-8d86-2d8dc7084e88",
                CompanyUID = "FBNZ",
                BankName = "Bank of New Zealandupdate",
                CountryUID = "NZ",
                ChequeFee = 12,
                ModifiedBy = "Ramanaupdate",
                ModifiedTime = DateTime.Now,
            };
            var result = await _bankController.UpdateBankDetails(bank) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }
        
        [Test]
        public async Task UpdateBankdetails_NotFound_ReturnsNotFound()
        {
            var bank = new Bank
            {
                UID = "NDFHN343",
            };
            var result = await _bankController.UpdateBankDetails(bank);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteBankDetail_ShouldReturnOk_WhenExists()
        {
            string UID = "b1dc203b-31fd-4034-b7a2-e9abf11f7216";
            var result = await _bankController.DeleteBankDetail(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteBankDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _bankController.DeleteBankDetail(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}







//[Test]
//public async Task GetBankDetails_WithValidData_ReturnsBankDetails()
//{
//    // Arrange
//    var sortCriterias = new List<SortCriteria>
//{
//    new SortCriteria
//    {
//        SortParameter = "UID",
//        Direction = SortDirection.Desc
//    }
//};

//    var filterCriterias = new List<FilterCriteria>
//{
//    new FilterCriteria
//    {
//        Name = "BankName",
//        Value = "New Zealand Bank",
//        Type = FilterType.Equal
//    }
//};
//    var pageNumber = 1;
//    var pageSize = 2;
//    var result = await _bankController.GetBankDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
//    Assert.IsNotNull(result);
//    Assert.IsInstanceOf<OkObjectResult>(result.Result);
//    var expectedStatusCode = 200;
//    var okObjectResult = (OkObjectResult)result.Result;
//    var bankList = (IEnumerable<object>)okObjectResult.Value;
//    var responseBody = JsonConvert.SerializeObject(bankList);
//    var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
//    var statusCode = okObjectResult.StatusCode;
//    Assert.AreEqual(expectedStatusCode, statusCode);
//    Assert.AreEqual(expectedResponseBody, responseBody);
//   

//}














