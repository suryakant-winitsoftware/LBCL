using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductConfigController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductConfigBL _productConfigBL;
        public ProductConfigController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductConfigBL productConfigBL) 
            : base(serviceProvider)
        {
            _productConfigBL = productConfigBL;
        }
        [HttpPost]
        [Route("SelectProductConfigAll")]
        public async Task<ActionResult> SelectProductConfigAll(PagingRequest pagingRequest)
        {

            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                var cacheKey = CacheConstants.ALL_ProductsConfig;
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig> PagedResponseProductConfigList = null;
                PagedResponseProductConfigList = null;

                if (PagedResponseProductConfigList != null)
                {
                    return CreateOkApiResponse(PagedResponseProductConfigList);
                }
                PagedResponseProductConfigList = await _productConfigBL.SelectProductConfigAll(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseProductConfigList == null)
                {
                    return NotFound();
                }
                else
                {
                    //currencyListResponse = currencyList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, currencyListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(PagedResponseProductConfigList);

            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectProductConfigByUID")]
        public async Task<ActionResult> SelectProductConfigByUID(string UID)
        {
            try
            {
                Winit.Modules.Product.Model.Interfaces.IProductConfig PagedResponseProductConfig = await _productConfigBL.SelectProductConfigByUID(UID);
                if (PagedResponseProductConfig != null)
                {
                    return CreateOkApiResponse(PagedResponseProductConfig);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductConfigDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateProductConfig")]
        public async Task<ActionResult> CreateProductConfig([FromBody] Winit.Modules.Product.Model.Classes.ProductConfig createpProductConfig)
        {
            try
            {
                createpProductConfig.ServerAddTime = DateTime.Now;
                createpProductConfig.ServerModifiedTime = DateTime.Now;
                var retVal = await _productConfigBL.CreateProductConfig(createpProductConfig);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Product Config details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpPut]
        [Route("UpdateProductConfig")]
        public async Task<ActionResult> UpdateProductConfig([FromBody] Winit.Modules.Product.Model.Classes.ProductConfig productConfigDetails)
        {
            try
            {
                var existingProductConfigDetails = await _productConfigBL.SelectProductConfigByUID(productConfigDetails.UID);
                if (existingProductConfigDetails != null)
                {
                    productConfigDetails.ServerModifiedTime = DateTime.Now;
                    var retVal = await _productConfigBL.UpdateProductConfig(productConfigDetails);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Product Config Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteProductConfig")]
        public async Task<ActionResult> DeleteProductConfig([FromQuery] string UID)
        {
            try
            {
                var retVal = await _productConfigBL.DeleteProductConfig(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetProductConfigFiltered")]
        public async Task<ActionResult> GetProductConfigFiltered([FromQuery] string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductConfigList = await _productConfigBL.GetProductConfigFiltered(ProductCode, CreatedTime, ModifiedTime);
                if (ProductConfigList.Any())
                {
                    return Ok(ProductConfigList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter ProductConfigList data");
                throw;
            }
        }


        [HttpGet]
        [Route("GetProductConfigPaged")]
        public async Task<ActionResult> GetProductConfigPaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            if (pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductConfigList = await _productConfigBL.GetProductConfigPaged(pageNumber, pageSize);
                return Ok(ProductConfigList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagaination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductConfigSorted")]
        public async Task<ActionResult> GetProductConfigSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var ProductConfigList = await _productConfigBL.GetProductConfigSorted(sortCriterias);
                return Ok(ProductConfigList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
    }
}