using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using WINITAPI.Controllers;
using WINITServices.Interfaces;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace NunitTest
{
    [TestFixture]
    public class OrgCurrencyTestCases
    {

        private OrgCurrencyController _orgcurrencyController;
        public readonly string _connectionString;
        private readonly IOrgCurrencyService _orgcurrencyServiceMock;
        private readonly ICacheService _cacheServiceMock;



        public OrgCurrencyTestCases()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = configuration.GetConnectionString("SqlServer");


            object[] configurationArgs = new object[] { configuration };

            Type orgcurrencyRepositoryType = typeof(WINITRepository.Classes.OrgCurrency.SQLServerOrgCurrencyRepository);
            WINITRepository.Interfaces.OrgCurrency.IOrgCurrencyRepository orgcurrencyRepository = (WINITRepository.Interfaces.OrgCurrency.IOrgCurrencyRepository)Activator.CreateInstance(orgcurrencyRepositoryType, configurationArgs);


            object[] orgcurrencyRepositoryArgs = new object[] { orgcurrencyRepository };
            
            Type orgcurrencyServiceType = typeof(WINITServices.Classes.OrgCurrency.OrgCurrencyService);
            WINITServices.Interfaces.IOrgCurrencyService orgcurrencyService = (WINITServices.Interfaces.IOrgCurrencyService)Activator.CreateInstance(orgcurrencyServiceType, orgcurrencyRepositoryArgs);

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            object[] memoryCacheArgs = new object[] { memoryCache };

            Type cacheServiceType = typeof(WINITServices.Classes.CacheHandler.CacheService);
            WINITServices.Interfaces.CacheHandler.ICacheService cacheService = (WINITServices.Interfaces.CacheHandler.ICacheService)Activator.CreateInstance(cacheServiceType, memoryCacheArgs);

            _orgcurrencyController = new OrgCurrencyController(orgcurrencyService, cacheService);

        }
        [Test]
        public async Task GetOrgCurrencyDetails_WithValidData_ReturnsOrgCurrencyDetails()
        {
            var sortCriterias = new List<SortCriteria>
             {
          new SortCriteria
    {
        SortParameter = "OrgUID",
        Direction = SortDirection.Asc
    },
    
};

            var filterCriterias = new List<FilterCriteria>
{
    new FilterCriteria
    {
        Name = "CurrencyUID",
        Value = "SAR",
        Type = FilterType.Equal
    },
   
};

            var pageNumber = 1;
            var pageSize = 10;


            var result = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<OrgCurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                IEnumerable<OrgCurrency> data = okObjectResult.Value as IEnumerable<OrgCurrency>;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(data);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
            else
            {
                Assert.Fail("Unexpected result type.");
            }
        }
      [Test]
        public async Task GetOrgCurrencyDetails_WithEmptyFilterCriteria_ReturnsOrgCurrencyDetails()
        {

            var sortCriterias = new List<SortCriteria>
         {
             new SortCriteria
             {
                  SortParameter = "OrgUID",
                  Direction = SortDirection.Asc
             }
         };

            var filterCriterias = new List<FilterCriteria>(); // Empty list

            var pageNumber = 1;
            var pageSize = 10;


            var result = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
            Assert.IsNotNull(result);
            if (result is ActionResult<IEnumerable<OrgCurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                IEnumerable<OrgCurrency> data = okObjectResult.Value as IEnumerable<OrgCurrency>;
                var expectedStatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedStatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(data);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }

        }


          [Test]
        public async Task GetOrgCurrencyDetails_WithEmptySortCriteria_ReturnsOrgCurrencyDetails()
        {
            var sortCriterias = new List<SortCriteria>(); // Empty list
            var filterCriterias = new List<FilterCriteria>
          {
              new FilterCriteria
              {
                  Name = "CurrencyUID",
                  Value = "SAR",
                  Type = FilterType.Equal
              }
          };
            var pageNumber = 1;
            var pageSize = 10;
            var result = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

            Assert.IsNotNull(result);

            if (result is ActionResult<IEnumerable<OrgCurrency>> actionResult && actionResult.Result is OkObjectResult okObjectResult)
            {
                IEnumerable<OrgCurrency> data = okObjectResult.Value as IEnumerable<OrgCurrency>;
                var expectedstatusCode = 200;
                var statusCode = okObjectResult.StatusCode;
                Assert.AreEqual(expectedstatusCode, statusCode);
                var responseBody = JsonConvert.SerializeObject(data);
                var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                Assert.AreEqual(expectedResponseBody, responseBody);
            }
        }


         [Test]
         public async Task GetOrgCurrencyDetails_WithInvalidFilterCriteria_ReturnsNoResults()
         {
             // Arrange
             var sortCriterias = new List<SortCriteria>
      {
          new SortCriteria
          {
              SortParameter = "OrgUID",
              Direction = SortDirection.Asc
          }
      };

             var filterCriterias = new List<FilterCriteria>
      {
          new FilterCriteria
          {
              Name = "InvalidColumnName", // Provide an invalid column name
              Value = "SAR",
              Type = FilterType.Equal
          }
      };

             var pageNumber = 1;
             var pageSize = 10;

             // Act
             var actionResult = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
             var result = actionResult.Result as ObjectResult;

             // Assert
             Assert.IsNotNull(result);
             Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
             Assert.AreEqual("Fail to retrieve CurrencyDetails", result.Value);
         }




           [Test]
           public async Task GetOrgCurrencyDetails_WithInvalidSortCriteria_ReturnsUnsortedOrgCurrencyDetails()
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
                      Name = "CurrencyUID",
                      Value = "SAR",
                      Type = FilterType.Equal
                  }
              };

               var pageNumber = 1;
               var pageSize = 10;

               // Act
               var actionResult = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

               var result = actionResult.Result as ObjectResult;
               Assert.IsNotNull(result);
               Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
               Assert.AreEqual("Fail to retrieve CurrencyDetails", result.Value);
           }


           [Test]
           public async Task GetOrgCurrencyDetails_WithInvalidPagination_ReturnsNoResults()
           {
               // Arrange
               var sortCriterias = new List<SortCriteria>
                 {
                     new SortCriteria
                     {
                         SortParameter = "Name",
                         Direction = SortDirection.Asc
                     }
                 };

               var filterCriterias = new List<FilterCriteria>
                 {
                     new FilterCriteria
                     {
                         Name = "Name",
                         Value = "Newzealand Dollor",
                         Type = FilterType.Equal
                     }
                 };

               var pageNumber = -1; // Provide an invalid page number
               var pageSize = 10;

               // Act
               var actionResult = await _orgcurrencyController.GetOrgCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
               var result = actionResult.Result as ObjectResult;
               Assert.IsNotNull(result);
               Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
               Assert.AreEqual("Invalid page size or page number", result.Value);


           }



           [Test]
           public async Task GetOrgCurrencyDetailsById_WhenReturnSuccessDetails()
           {
               string orgUID = "2d893d92-dc1b-5904-934c-621103a900e3";
               IActionResult result = await _orgcurrencyController.GetOrgCurrencyByOrgUID(orgUID);
               if (result is OkObjectResult okObjectResult)
               {
                   WINITSharedObjects.Models.OrgCurrency settings = okObjectResult.Value as WINITSharedObjects.Models.OrgCurrency;
                   var expectedStatusCode = 200;
                   var statusCode = okObjectResult.StatusCode;
                   Assert.AreEqual(expectedStatusCode, statusCode);
                   var responseBody = JsonConvert.SerializeObject(settings);
                   var expectedResponseBody = JsonConvert.SerializeObject(okObjectResult.Value);
                   Assert.AreEqual(expectedResponseBody, responseBody);
               }
               else if (result is NotFoundResult)
               {
                   Assert.Fail("Org Currency Detail not found.");
               }
               else
               {
                   Assert.Fail("Unexpected result type.");
               }
           }


           [Test]
           public async Task GetOrgCurrencyDetail_ReturnsNotFound()
           {
            string orgUID = "2d893d92-dc1b-5904-934c-621103a900e3gfdgfdgd";

            var result = await _orgcurrencyController.GetOrgCurrencyByOrgUID(orgUID);

               Assert.IsInstanceOf<NotFoundResult>(result);
               var notFoundResult = result as NotFoundResult;
               Assert.AreEqual(404, notFoundResult.StatusCode);
           }


           [Test]
           public async Task CreateOrgCurrency_ReturnsCreatedResultWithOrgCurrencyObject()
           {
               var cratteOrgcurrency = new WINITSharedObjects.Models.OrgCurrency
               {
                   CurrencyUID = "Indian Rupee",
                   IsPrimary = true,
                   SS = 0,
                   


               };

               var result = await _orgcurrencyController.CreateOrgCurrency(cratteOrgcurrency) as ActionResult<WINITSharedObjects.Models.OrgCurrency>;
               Assert.IsNotNull(result);
               Assert.IsInstanceOf<CreatedResult>(result.Result);
               var createdResult = result.Result as CreatedResult;
               Assert.IsNotNull(createdResult);
               Assert.AreEqual(201, createdResult.StatusCode);
               var createdorgCurrencyType = createdResult.Value as WINITSharedObjects.Models.OrgCurrency;
               Assert.IsNotNull(createdorgCurrencyType);
               Assert.AreEqual(cratteOrgcurrency.OrgUID, createdorgCurrencyType.OrgUID);
               Assert.AreEqual(cratteOrgcurrency.CurrencyUID, createdorgCurrencyType.CurrencyUID);
               Assert.AreEqual(cratteOrgcurrency.IsPrimary, createdorgCurrencyType.IsPrimary);
               Assert.AreEqual(cratteOrgcurrency.SS, createdorgCurrencyType.SS);
               Assert.AreEqual(cratteOrgcurrency.CreatedTime, createdorgCurrencyType.CreatedTime);
               Assert.AreEqual(cratteOrgcurrency.ModifiedTime, createdorgCurrencyType.ModifiedTime);
               Assert.AreEqual(cratteOrgcurrency.ServerAddTime, createdorgCurrencyType.ServerAddTime);
               Assert.AreEqual(cratteOrgcurrency.ServerModifiedTime, createdorgCurrencyType.ServerModifiedTime);

           }




        /*    [Test]
             public async Task CreateOrgCurrency_ReturnsConflictResultWhenCurrencyUIDAlreadyExists()
             {
                 var existingOrgCurrency = new WINITSharedObjects.Models.OrgCurrency
                 {
                     CurrencyUID = "SAR",
                     IsPrimary = true,
                     SS = 0,
                 };

                 var actionResult = await _orgcurrencyController.CreateOrgCurrency(existingOrgCurrency) as ActionResult<WINITSharedObjects.Models.OrgCurrency>;
                 Assert.IsNotNull(actionResult);
                 var result = actionResult.Result as ObjectResult;
                 Assert.IsNotNull(result);
                 Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
                 var expectedErrorMessage = "Violation of UNIQUE KEY constraint";
                 Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
             }*/


           [Test]
              public async Task CreateOrgCurrency_ReturnsBadRequestResultWhenRequiredFieldsAreMissing()
              {

                  var invalidOrgCurrency = new WINITSharedObjects.Models.OrgCurrency
                  {
                      // Missing required fields
                  };


                  var actionResult = await _orgcurrencyController.CreateOrgCurrency(invalidOrgCurrency) as ActionResult<WINITSharedObjects.Models.OrgCurrency>;
                  Assert.IsNotNull(actionResult);
                  var result = actionResult.Result as ObjectResult;

                  Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
                  var expectedErrorMessage = "expects the parameter";
                  Assert.IsTrue(result.Value.ToString().Contains(expectedErrorMessage), "Error message does not match expected");
              }


          [Test]
         public async Task UpdateOrgCurrency_SuccessfulUpdate_ReturnsOkWithUpdatedOrgCurrencydetails()
         {
             var orgcurrency = new WINITSharedObjects.Models.OrgCurrency
             {
                 OrgUID= "c5dc4e43-bb86-454f-b3a0-fe45255265fb",
                 CurrencyUID = "SAR",
                 IsPrimary = false,
                 SS = 3,
                

             };


             var result = await _orgcurrencyController.UpdateOrgCurrency(orgcurrency) as ActionResult<WINITSharedObjects.Models.OrgCurrency>;
             Assert.IsNotNull(result);
             Assert.IsInstanceOf<ActionResult<WINITSharedObjects.Models.OrgCurrency>>(result);
             var actionResult = result as ActionResult<WINITSharedObjects.Models.OrgCurrency>;
             Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
             var okResult = actionResult.Result as OkObjectResult;
             Assert.IsNotNull(okResult);
             Assert.AreEqual(200, okResult.StatusCode);
             Assert.AreEqual("Update successfully", okResult.Value);

         }

        [Test]
         public async Task UpdateOrgCurrencydetails_NotFound_ReturnsNotFound()
         {
             var orgcurrecny = new WINITSharedObjects.Models.OrgCurrency
             {
                 OrgUID= "c5dc4e43-bb86-454f-b3a0-fe45255265fbfjfjfj",
             };

             var result = await _orgcurrencyController.UpdateOrgCurrency(orgcurrecny);

             Assert.IsInstanceOf<NotFoundResult>(result.Result);
             var notFoundResult = result.Result as NotFoundResult;
             Assert.AreEqual(404, notFoundResult.StatusCode);
         }


         [Test]
         public async Task DeleteOrgCurrencyDetail_ShouldReturnOk_WhenExists()
         {
              string orguid = "a0f1bb78-e592-4ee2-a71d-6159c6fc9ea1";

             var result = await _orgcurrencyController.DeleteOrgCurrency(orguid);
             Assert.IsInstanceOf<OkObjectResult>(result);
             var okResult = (OkObjectResult)result;
             Assert.AreEqual("Deleted successfully.", okResult.Value);
         }

          [Test]
         public async Task DeleteOrgCurrencyDetail_NotFound_ReturnsNotFound()
         {
            String orguid = "a0f1bb78";

             var result = await _orgcurrencyController.DeleteOrgCurrency(orguid);

             Assert.IsInstanceOf<NotFoundResult>(result);
             var notFoundResult = result as NotFoundResult;
             Assert.AreEqual(404, notFoundResult.StatusCode);
         }

    }


}


















