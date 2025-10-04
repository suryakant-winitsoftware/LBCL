using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductDimensionController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductDimensionBL _productDimensionBL;
        public ProductDimensionController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductDimensionBL productDimensionBL) 
            : base(serviceProvider)
        {
            _productDimensionBL = productDimensionBL;
        }
        [HttpGet]
        [Route("SelectProductDimensionAll")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>>> SelectProductDimensionAll()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProductDimension.ALL_ProductDimension;
                IEnumerable<Winit.Modules.Base.Model.IBaseModel> productDimensionList = null;
                IEnumerable<object> productDimensionResponse = null;
                productDimensionResponse = _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (productDimensionResponse != null)
                {
                    return Ok(productDimensionResponse);
                }
                productDimensionList = await _productDimensionBL.SelectProductDimensionAll();
                if (productDimensionList == null)
                {
                    return NotFound();
                }
                else
                {
                    productDimensionResponse = productDimensionList.OfType<object>().ToList();
                    _cacheService.Set(cacheKey, productDimensionResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(productDimensionResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductDimension data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductDimension data");
            }
        }

        [HttpGet]
        [Route("GetProductDimensionByUID")]
        public async Task<ActionResult> GetProductDimensionByUID(string UID)
        {
            try
            {
                Winit.Modules.Product.Model.Interfaces.IProductDimension ProductDimensionDetails = await _productDimensionBL.GetProductDimensionByUID(UID);
                if (ProductDimensionDetails != null)
                {
                    return CreateOkApiResponse(ProductDimensionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductDimensionDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateProductDimension")]
        public async Task<ActionResult> CreateProductDimension([FromBody] Winit.Modules.Product.Model.Classes.ProductDimension CreateProductDimension)
        {
            try
            {
                CreateProductDimension.ServerAddTime = DateTime.Now;
                CreateProductDimension.ServerModifiedTime = DateTime.Now;
                var returnValue = await _productDimensionBL.CreateProductDimension(CreateProductDimension);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductDimension");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateProductDimension")]
        public async Task<ActionResult> UpdateProductDimension([FromBody] Winit.Modules.Product.Model.Classes.ProductDimension ProductDimension)
        {
            try
            {
                var existingCustomer = await _productDimensionBL.GetProductDimensionByUID(ProductDimension.UID);
                if (existingCustomer != null)
                {
                    ProductDimension.ModifiedTime = DateTime.Now;
                    ProductDimension.ServerModifiedTime = DateTime.Now;
                    var retvalue = await _productDimensionBL.UpdateProductDimension(ProductDimension);
                    return (retvalue > 0) ? CreateOkApiResponse(retvalue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductDimensionList");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteProductDimension")]
        public async Task<ActionResult> DeleteProductDimension(string UID)
        {
            try
            {
                var result = await _productDimensionBL.DeleteProductDimension(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductDimensionFiltered")]
        public async Task<ActionResult> GetProductDimensionFiltered([FromQuery] string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductDimensionList = await _productDimensionBL.GetProductDimensionFiltered(product_dimension_code, CreatedTime, ModifiedTime);
                if (ProductDimensionList.Any())
                {
                    return Ok(ProductDimensionList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter ProductDimensionList data");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductDimensionPaged")]
        public async Task<ActionResult> GetProductDimensionPaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductDimensionList = await _productDimensionBL.GetProductDimensionPaged(pageNumber, pageSize);
                return Ok(ProductDimensionList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagaination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductDimensionSorted")]
        public async Task<ActionResult> GetProductDimensionSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var ProductDimensionList = await _productDimensionBL.GetProductDimensionSorted(sortCriterias);
                return Ok(ProductDimensionList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
        [HttpPost]
        [Route("SelectAllProductDimension")]
        public async Task<ActionResult> SelectAllProductDimension(PagingRequest pagingRequest)
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
                var cacheKey = CacheConstants.ALL_ProductDimension;
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension> PagedResponseProductDimensionList = null;
                PagedResponseProductDimensionList = null;

                if (PagedResponseProductDimensionList != null)
                {
                    return CreateOkApiResponse(PagedResponseProductDimensionList);
                }
                PagedResponseProductDimensionList = await _productDimensionBL.SelectAllProductDimension(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseProductDimensionList == null)
                {
                    return NotFound();
                }
                else
                {
                    //currencyListResponse = currencyList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, currencyListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(PagedResponseProductDimensionList);

            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductDimensionList");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}