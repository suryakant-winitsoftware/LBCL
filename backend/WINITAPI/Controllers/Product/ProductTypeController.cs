using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITRepository.Interfaces.Customers;
using WINITServices.Interfaces.CacheHandler;



namespace WINITAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductTypeController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductTypeBL _productTypeBL;
        public ProductTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductTypeBL productTypeBLservice) 
            : base(serviceProvider)
        {
            _productTypeBL = productTypeBLservice;
        }

        [HttpPost]
        [Route("SelectProductTypeAll")]
        public async Task<ActionResult> SelectProductTypeAll(PagingRequest pagingRequest)
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
                var cacheKey = CacheConstants.ALL_ProductType;
                PagedResponse<Winit.Modules.Product .Model.Interfaces.IProductType> pagedResponseProductTypeList = null;
                pagedResponseProductTypeList = null;// _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (pagedResponseProductTypeList != null)
                {
                    return CreateOkApiResponse(pagedResponseProductTypeList);
                }
                pagedResponseProductTypeList = await _productTypeBL.SelectProductTypeAll(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseProductTypeList == null)
                {
                    return NotFound();
                }
                else
                {
                    //productTypeListResponse = pagedResponseProductTypeList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, productTypeListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(pagedResponseProductTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductType  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductTypeByUID")]
        public async Task<ActionResult> GetProductTypeByProductTypeId([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Product .Model.Interfaces.IProductType productType = await _productTypeBL.GetProductTypeByUID(UID);
                if (productType != null)
                {
                    return CreateOkApiResponse(productType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductTypeList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateProductType")]
        public async Task<ActionResult> CreateProductType([FromBody] Winit.Modules.Product.Model.Classes.ProductType CreateProductType)
        {
            try
            {
                CreateProductType.ServerAddTime = DateTime.Now;
                CreateProductType.ServerModifiedTime = DateTime.Now;
                var returnValue = await _productTypeBL.CreateProductType(CreateProductType);
                return (returnValue>0) ? CreateOkApiResponse(returnValue) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductType");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateProductType")]
        public async Task<ActionResult> UpdateProductType([FromBody] Winit.Modules.Product.Model.Classes.ProductType productType)
        {
            try
            {
                var existingProductType = await _productTypeBL.GetProductTypeByUID(productType.UID);
                if (existingProductType != null)
                {
                    //productType.ModifiedTime = DateTime.Now;
                    productType.ServerModifiedTime = DateTime.Now;
                    var returnValue = await _productTypeBL.UpdateProductType(productType);
                    return (returnValue>0)? CreateOkApiResponse(returnValue) : throw new Exception("Update failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductType");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteProductType")]
        public async Task<ActionResult> DeleteProductType(string UID)
        {
            try
            {
                var result = await _productTypeBL.DeleteProductType(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductTypeFiltered")]
        public async Task<ActionResult> GetProductTypeFiltered([FromQuery] string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductTypeList = await _productTypeBL.GetProductTypeFiltered(product_type_code, CreatedTime, ModifiedTime);
                if (ProductTypeList.Any())
                {
                    return Ok(ProductTypeList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter ProductTypeList data");
                throw;
            }
        }


        [HttpGet]
        [Route("GetProductTypePaged")]
        public async Task<ActionResult> GetProductTypePaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            if (pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductTypeList = await _productTypeBL.GetProductTypePaged(pageNumber, pageSize);
                return Ok(ProductTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagaination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductTypeSorted")]
        public async Task<ActionResult> GetProductTypeSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var ProductTypeList = await _productTypeBL.GetProductTypeSorted(sortCriterias);
                return Ok(ProductTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
    }
}
