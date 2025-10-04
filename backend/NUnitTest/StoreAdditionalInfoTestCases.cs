
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
using WINITAPI.Controllers.Store;
using Winit.Shared.Models.Enums;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.BL.Classes;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using StackExchange.Redis;

namespace NunitTest
{
    [TestFixture]
    public class StoreAdditionalInfoTestCases
    {
        private StoreAdditionalInfoController _storeAdditionalInfoController;
        public readonly string _connectionString;
        public string UID = Guid.NewGuid().ToString();
        public StoreAdditionalInfoTestCases()
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
            services.AddSingleton<IStoreAdditionalInfo, StoreAdditionalInfo>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            object[] configurationArgs = new object[] { serviceProvider, configuration };
            Type StoreAdditionalInfodltype = typeof(Winit.Modules.Store.DL.Classes.PGSQLStoreAdditionalInfoDL);
            Winit.Modules.Store.DL.Interfaces.IStoreAdditionalInfoDL StoreAdditionalInfoRepository = (Winit.Modules.Store.DL.Interfaces.IStoreAdditionalInfoDL)Activator.CreateInstance(StoreAdditionalInfodltype, configurationArgs);
            object[] storeAdditionalInfoRepositoryArgs = new object[] { StoreAdditionalInfoRepository };
            Type storeAdditionalInfoblType = typeof(Winit.Modules.Store.BL.Classes.StoreAdditionalInfoBL);
            Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL storeAdditionalInfoService = (Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL)Activator.CreateInstance(storeAdditionalInfoblType, storeAdditionalInfoRepositoryArgs);
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };
            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);
            _storeAdditionalInfoController = new StoreAdditionalInfoController(storeAdditionalInfoService, cacheService);
        }

        [Test]
        public async Task GetStoreAdditionalInfoDetails_WithValidData_ReturnsStoreAdditionalInfoDetails()
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
            var result = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAdditionalInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                var storeAdditionalInfoList = (IEnumerable<object>)okObjectResult.Value;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeAdditionalInfoList);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
                // Assert.AreEqual(1, storeAdditionalInfoList.Count());
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
        [Test]
        public async Task GetStoreAdditionalInfoDetails_WithEmptyFilterCriteria_ReturnsStoreAdditionalInfoDetails()
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
            var result = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAdditionalInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreAdditionalInfoDetails_WithEmptySortCriteria_ReturnsStoreAdditionalInfoDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
            {
              new FilterCriteria
              {
                  Name = @"StoreAdditionalInfoName",
                Value = "New Zealand StoreAdditionalInfo",
                Type = Winit.Shared.Models.Enums.FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<IStoreAdditionalInfo>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
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
        public async Task GetStoreAdditionalInfoDetails_WithInvalidFilterCriteria_ReturnsNoResults()
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
                    Value = "ABN AMRO StoreAdditionalInfo NV",
                    Type = FilterType.Equal
                }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Additional Info  Details", result.Value);
        }

        [Test]
        public async Task GetStoreAdditionalInfoDetails_WithInvalidSortCriteria_ReturnsUnsortedStoreAdditionalInfoDetails()
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
                  Name = "StoreAdditionalInfoName",
                  Value = "ABN AMRO StoreAdditionalInfo NV",
                  Type = FilterType.Equal
              }
            };
            var pageNumber = 1;
            var pageSize = 10;
            // Act
            var actionResult = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Fail to retrieve Store Additional Info  Details", result.Value);
        }

        [Test]
        public async Task GetStoreAdditionalInfoDetails_WithInvalidPagination_ReturnsNoResults()
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
                   Name = "StoreAdditionalInfoName",
                   Value = "ABN AMRO StoreAdditionalInfo NV",
                   Type = FilterType.Equal
               }
            };
            var pageNumber = -1; // Provide an invalid page number
            var pageSize = 10;
            // Act
            var actionResult = await _storeAdditionalInfoController.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid page size or page number", result.Value);
        }

        [Test]
        public async Task GetStoreAdditionalInfoDetailsByUID_WhenReturnSuccessDetails()
        {
            string UID  = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F";
            IActionResult result = await _storeAdditionalInfoController.SelectStoreAdditionalInfoByUID(UID);
            if (result is OkObjectResult okObjectResult)
            {
                //StoreAdditionalInfo storeAdditionalInfo = okObjectResult.Value as StoreAdditionalInfo;
                StoreAdditionalInfo storeAdditionalInfo = okObjectResult.Value as StoreAdditionalInfo;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(storeAdditionalInfo);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else if (result is NotFoundResult)
            {
                Assert.Fail("StoreAdditionalInfo Detail not found.");
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }

        [Test]
        public async Task GetStoreAdditionalInfoDetail_ReturnsNotFound()
        {
            string UID = "ABNRET";
            var result = await _storeAdditionalInfoController.SelectStoreAdditionalInfoByUID(UID);
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreatestoreAdditionalInfo_ReturnsCreatedResultWithstoreAdditionalInfoObject()
        {
            var storeAdditionalInfo = new StoreAdditionalInfo
            {
                UID = this.UID,
                ModifiedBy = "Srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreUID = "hkbefiywe",
                OrderType = "food",
                IsPromotionsBlock  = false,
                CustomerStartDate  = DateTime.Now,
                CustomerEndDate = DateTime.Now,
                PurchaseOrderNumber ="wke80943o49803ou4rnn",
                DeliveryDocketIsPurchaseOrderRequired  = 4,
                IsWithPrintedInvoices = true,
                IsAlwaysPrinted  = 4,
                BuildingDeliveryCode  = "wvbeyifv",
                DeliveryInformation  = "wiygkbi",
                IsStopDelivery = true, 
                IsForeCastTopUpQty  = true,
                IsTemperatureCheck =true,
                InvoiceStartDate = DateTime.Now,
                InvoiceEndDate = DateTime.Now,
                InvoiceFormat = "print",
                InvoiceDeliveryMethod = "cash",
                DisplayDeliveryDocket = true,
                DisplayPrice = true,
                ShowCustPO = true,
                InvoiceText = "8043o4n39",
                InvoiceFrequency = "iywebff",
                StockCreditIsPurchaseOrderRequired = true,
                AdminFeePerBillingCycle  = 2312,
                AdminFeePerDelivery =2323,
                LatePayementFee =445,
                Drawer ="hifgiwe",
                BankUID = "khfelgbei9ewf",
                BankAccount = "2380o345223234",
                MandatoryPONumber = true,
                IsStoreCreditCaptureSignatureRequired = true,
                StoreCreditAlwaysPrinted = 3,
                IsDummyCustomer  = true,
                DefaultRun = "Yes",
                ProspectEmpUID = "kjb494b",
                IsFOCCustomer = true,
                RSSShowPrice =true,
                RSSShowPayment= true,
                RSSShowCredit = true,
                RSSShowInvoice = true,
                RSSIsActive = true,
                RSSDeliveryInstructionStatus = "good",
                RSSTimeSpentOnRSSPortal =234,
                RSSOrderPlacedInRSS = 6,
                RSSAvgOrdersPerWeek = 5,
                RSSTotalOrderValue = 32,
                AllowForceCheckIn = true,
                IsManaualEditAllowed = true,
                CanUpdateLatLong = true,
                IsTaxApplicable = true,
                AllowGoodReturn = true,
                AllowBadReturn = true,
                EnableAsset = true,
                EnableSurvey = true,
                AllowReplacement = true,
                IsInvoiceCancellationAllowed = true,
                IsDeliveryNoteRequired = true,
                EInvoicingEnabled = true,
                ImageRecognizationEnabled = true,
                MaxOutstandingInvoices = 6,
                NegativeInvoiceAllowed = true
            };
            var result = await _storeAdditionalInfoController.CreateStoreAdditionalInfo(storeAdditionalInfo) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var createdResult = result.Result as ObjectResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(1, createdResult.Value);
        }

        [Test]
        public async Task CreateStoreAdditionalInfo_ReturnsConflictResultWhenStoreAdditionalInfoUIDAlreadyExists()
        {
            var existingStoreAdditionalInfo = new StoreAdditionalInfo
            {
                UID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                CreatedBy = "srinadh",
                CreatedTime = DateTime.Now,
                StoreUID = "hkbefiywe",
                OrderType = "food",
                IsPromotionsBlock = false,
                CustomerStartDate = DateTime.Now,
                CustomerEndDate = DateTime.Now,
                PurchaseOrderNumber = "wke80943o49803ou4rnn",
                DeliveryDocketIsPurchaseOrderRequired = 4,
                IsWithPrintedInvoices = true,
                IsAlwaysPrinted = 4,
                BuildingDeliveryCode = "wvbeyifv",
                DeliveryInformation = "wiygkbi",
                IsStopDelivery = true,
                IsForeCastTopUpQty = true,
                IsTemperatureCheck = true,
                InvoiceStartDate = DateTime.Now,
                InvoiceEndDate = DateTime.Now,
                InvoiceFormat = "print",
                InvoiceDeliveryMethod = "cash",
                DisplayDeliveryDocket = true,
                DisplayPrice = true,
                ShowCustPO = true,
                InvoiceText = "8043o4n39",
                InvoiceFrequency = "iywebff",
                StockCreditIsPurchaseOrderRequired = true,
                AdminFeePerBillingCycle = 2312,
                AdminFeePerDelivery = 2323,
                LatePayementFee = 445,
                Drawer = "hifgiwe",
                BankUID = "khfelgbei9ewf",
                BankAccount = "2380o345223234",
                MandatoryPONumber = true,
                IsStoreCreditCaptureSignatureRequired = true,
                StoreCreditAlwaysPrinted = 3,
                IsDummyCustomer = true,
                DefaultRun = "Yes",
                ProspectEmpUID = "kjb494b",
                IsFOCCustomer = true,
                RSSShowPrice = true,
                RSSShowPayment = true,
                RSSShowCredit = true,
                RSSShowInvoice = true,
                RSSIsActive = true,
                RSSDeliveryInstructionStatus = "good",
                RSSTimeSpentOnRSSPortal = 234,
                RSSOrderPlacedInRSS = 6,
                RSSAvgOrdersPerWeek = 5,
                RSSTotalOrderValue = 32,
                AllowForceCheckIn = true,
                IsManaualEditAllowed = true,
                CanUpdateLatLong = true,
                IsTaxApplicable = true,
                AllowGoodReturn = true,
                AllowBadReturn = true,
                EnableAsset = true,
                EnableSurvey = true,
                AllowReplacement = true,
                IsInvoiceCancellationAllowed = true,
                IsDeliveryNoteRequired = true,
                EInvoicingEnabled = true,
                ImageRecognizationEnabled = true,
                MaxOutstandingInvoices = 6,
                NegativeInvoiceAllowed = true
            };
            var actionResult = await _storeAdditionalInfoController.CreateStoreAdditionalInfo(existingStoreAdditionalInfo) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            // var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
            var expectedErrorMessage = "UNIQUE";
            Assert.IsTrue(result.Value.ToString().ToUpper().Contains(expectedErrorMessage), "Error message does not match expected");
        }
        [Test]
        public async Task CreateStoreAdditionalInfo_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
        {
            var invalidStoreAdditionalInfo = new StoreAdditionalInfo
            {
                // Missing required fields
            };
            var actionResult = await _storeAdditionalInfoController.CreateStoreAdditionalInfo(invalidStoreAdditionalInfo) as ActionResult<int>;
            Assert.IsNotNull(actionResult);
            var result = actionResult.Result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            var expectedErrorMessage = "Parameter";
            Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
        }

        [Test]
        public async Task UpdatestoreAdditionalInfo_SuccessfulUpdate_ReturnsOkWithUpdatedstoreAdditionalInfodetails()
        {
            var storeAdditionalInfo = new StoreAdditionalInfo
            {
                UID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F",
                ModifiedBy = "srinadhupdate",
                ModifiedTime = DateTime.Now,
                StoreUID = "hkbefiywe",
                OrderType = "food",
                IsPromotionsBlock = false,
                CustomerStartDate = DateTime.Now,
                CustomerEndDate = DateTime.Now,
                PurchaseOrderNumber = "wke80943o49803ou4rnn",
                DeliveryDocketIsPurchaseOrderRequired = 4,
                IsWithPrintedInvoices = true,
                IsAlwaysPrinted = 4,
                BuildingDeliveryCode = "wvbeyifv",
                DeliveryInformation = "wiygkbi",
                IsStopDelivery = true,
                IsForeCastTopUpQty = true,
                IsTemperatureCheck = true,
                InvoiceStartDate = DateTime.Now,
                InvoiceEndDate = DateTime.Now,
                InvoiceFormat = "print",
                InvoiceDeliveryMethod = "cash",
                DisplayDeliveryDocket = true,
                DisplayPrice = true,
                ShowCustPO = true,
                InvoiceText = "8043o4n39",
                InvoiceFrequency = "iywebff",
                StockCreditIsPurchaseOrderRequired = true,
                AdminFeePerBillingCycle = 2312,
                AdminFeePerDelivery = 2323,
                LatePayementFee = 445,
                Drawer = "hifgiwe",
                BankUID = "khfelgbei9ewf",
                BankAccount = "2380o345223234",
                MandatoryPONumber = true,
                IsStoreCreditCaptureSignatureRequired = true,
                StoreCreditAlwaysPrinted = 3,
                IsDummyCustomer = true,
                DefaultRun = "Yes",
                ProspectEmpUID = "kjb494b",
                IsFOCCustomer = true,
                RSSShowPrice = true,
                RSSShowPayment = true,
                RSSShowCredit = true,
                RSSShowInvoice = true,
                RSSIsActive = true,
                RSSDeliveryInstructionStatus = "good",
                RSSTimeSpentOnRSSPortal = 234,
                RSSOrderPlacedInRSS = 6,
                RSSAvgOrdersPerWeek = 5,
                RSSTotalOrderValue = 32,
                AllowForceCheckIn = true,
                IsManaualEditAllowed = true,
                CanUpdateLatLong = true,
                IsTaxApplicable = true,
                AllowGoodReturn = true,
                AllowBadReturn = true,
                EnableAsset = true,
                EnableSurvey = true,
                AllowReplacement = true,
                IsInvoiceCancellationAllowed = true,
                IsDeliveryNoteRequired = true,
                EInvoicingEnabled = true,
                ImageRecognizationEnabled = true,
                MaxOutstandingInvoices = 6,
                NegativeInvoiceAllowed = true
            };
            var result = await _storeAdditionalInfoController.UpdateStoreAdditionalInfo(storeAdditionalInfo) as ActionResult<int>;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var updateResult = result.Result as ObjectResult;
            Assert.IsNotNull(updateResult);
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.AreEqual(1, updateResult.Value);
        }

        [Test]
        public async Task UpdateStoreAdditionalInfodetails_NotFound_ReturnsNotFound()
        {
            var storeAdditionalInfo = new StoreAdditionalInfo
            {
                UID = "NDFHN343",
            };
            var result = await _storeAdditionalInfoController.UpdateStoreAdditionalInfo(storeAdditionalInfo);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task DeleteStoreAdditionalInfoDetail_ShouldReturnOk_WhenExists()
        {
            string UID = this.UID;
            var result = await _storeAdditionalInfoController.DeleteStoreAdditionalInfo(UID);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var deletedResult = result.Result as ObjectResult;
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(202, deletedResult.StatusCode);
            Assert.AreEqual(1, deletedResult.Value);
        }
        [Test]
        public async Task DeleteStoreAdditionalInfoDetail_NotFound_ReturnsNotFound()
        {
            string UID = "BNZBDCBC";
            var result = await _storeAdditionalInfoController.DeleteStoreAdditionalInfo(UID);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var invalidData = result.Result as ObjectResult;
            Assert.IsNotNull(invalidData);
            Assert.AreEqual(500, invalidData.StatusCode);
        }
    }
}


















