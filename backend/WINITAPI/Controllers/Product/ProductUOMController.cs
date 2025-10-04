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
    public class ProductUOMController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductUOMBL _productUOMBL;
        public ProductUOMController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductUOMBL productUOMBl) 
            : base(serviceProvider)
        {
            _productUOMBL = productUOMBl;
        }
        [HttpGet]
        [Route("SelectProductUOMAll")]
        public async Task<ActionResult> SelectProductUOMAll()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProductUOMAll.ALL_ProductUOM;
                IEnumerable<Winit.Modules.Base.Model.IBaseModel> productUOMList = null;
                IEnumerable<object> productUOMListResponse = null;
                productUOMListResponse = _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (productUOMListResponse != null)
                {
                    return Ok(productUOMListResponse);
                }
                productUOMList = await _productUOMBL.SelectProductUOMAll();
                if (productUOMList == null)
                {
                    return NotFound();
                }
                else
                {
                    productUOMListResponse = productUOMList.OfType<object>().ToList();  
                    _cacheService.Set(cacheKey, productUOMListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(productUOMListResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductUOMList data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductUOMList data");
            }
        }

        [HttpGet]
        [Route("GetProductUOMByUID")]
        public async Task<ActionResult> GetProductUOMByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Product.Model.Interfaces.IProductUOM productUOM = await _productUOMBL.GetProductUOMByUID(UID);
                if (productUOM != null)
                {
                    return CreateOkApiResponse(productUOM);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductUOMList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateProductUOM")]
        public async Task<ActionResult> CreateProductUOM([FromBody] Winit.Modules.Product.Model.Classes.ProductUOM CreateProductUOM)
        {
            try
            {
                //CreateProductUOM.CreatedTime = DateTime.Now;
                //CreateProductUOM.ModifiedTime = DateTime.Now;
                CreateProductUOM.ServerAddTime = DateTime.Now;
                CreateProductUOM.ServerModifiedTime = DateTime.Now;
                var returnValue = await _productUOMBL.CreateProductUOM(CreateProductUOM);
                return (returnValue>0)? CreateOkApiResponse(returnValue) :throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductUOM");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateProductUOM")]
        public async Task<ActionResult> UpdateProductUOM([FromBody] Winit.Modules.Product.Model.Classes.ProductUOM UpdateProductUOM)
        {
            try
            {
                var existingProductUOM = await _productUOMBL.GetProductUOMByUID(UpdateProductUOM.UID);
                if (existingProductUOM != null)
                {
                    UpdateProductUOM.ModifiedTime = DateTime.Now;
                    UpdateProductUOM.ServerModifiedTime = DateTime.Now;
                    var returnValue = await _productUOMBL.UpdateProductUOM(UpdateProductUOM);
                    return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductUOM");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteProductUOM")]
        public async Task<ActionResult> DeleteProductUOM(string UID)
        {
            try
            {
                var returnvalue = await _productUOMBL.DeleteProductUOM(UID);
                return (returnvalue > 0) ? CreateOkApiResponse(returnvalue) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        [HttpGet]
        [Route("GetProductUOMFiltered")]
        public async Task<ActionResult> GetProductUOMFiltered([FromQuery] string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var GetProductUOMList = await _productUOMBL.GetProductUOMFiltered(ProductCode, CreatedTime, ModifiedTime);
                if (GetProductUOMList.Any())
                {
                    return Ok(GetProductUOMList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter GetProductUOMList data");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductUOMPaged")]
        public async Task<ActionResult> GetProductUOMPaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            if (pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductUOMList = await _productUOMBL.GetProductUOMPaged(pageNumber, pageSize);
                return Ok(ProductUOMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagaination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductUOMSorted")]
        public async Task<ActionResult> GetProductUOMSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var ProductUOMList = await _productUOMBL.GetProductUOMSorted(sortCriterias);
                return Ok(ProductUOMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
        [HttpPost]
        [Route("SelectAllProductUOM")]
        public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM>>>> SelectAllProductUOM(
            PagingRequest pagingRequest)
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
                var cacheKey = CacheConstants.ALL_ProductUOM;
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM> pagedResponseProductUOMList = null;
                pagedResponseProductUOMList = null;// _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (pagedResponseProductUOMList != null)
                {
                    return CreateOkApiResponse(pagedResponseProductUOMList);
                }
                pagedResponseProductUOMList = await _productUOMBL.SelectAllProductUOM(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseProductUOMList == null)
                {
                    return NotFound();
                }
                else
                {
                    //productUOMListResponse = pagedResponseProductUOMList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, productUOMListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(pagedResponseProductUOMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductUOM  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}