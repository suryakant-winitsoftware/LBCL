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
    public class ProductController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductBL _productBL;
        public ProductController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductBL productBl) : base(serviceProvider)
        {
            _productBL = productBl;
        }
        [HttpPost]
        [Route("SelectProductsAll")]
        public async Task<ActionResult> SelectProductsAll(PagingRequest pagingRequest)
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
                var cacheKey = CacheConstants.ALL_Product;
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProduct> pagedResponseProductList = null;
                pagedResponseProductList = null;// _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (pagedResponseProductList != null)
                {
                    return CreateOkApiResponse(pagedResponseProductList);
                }
                pagedResponseProductList = await _productBL.SelectProductsAll(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseProductList == null)
                {
                    return NotFound();
                }
                else
                {
                    //productListResponse = pagedResponseProductList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, productListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(pagedResponseProductList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Product  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectProductByUID")]
        public async Task<ActionResult> SelectProductByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Product.Model.Interfaces.IProduct product = await _productBL.SelectProductByUID(UID);
                if (product != null)
                {
                    return CreateOkApiResponse(product);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("CreateProduct")]
        public async Task<ActionResult> CreateProduct([FromBody] Winit.Modules.Product.Model.Classes.Product createpProduct)
        {
            try
            {
                createpProduct.ServerAddTime = DateTime.Now;
                createpProduct.ServerModifiedTime = DateTime.Now;
                var retVal = await _productBL.CreateProduct(createpProduct);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Product details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpPut]
        [Route("UpdateProduct")]
        public async Task<ActionResult> UpdateProduct([FromBody] Winit.Modules.Product.Model.Classes.Product productDetails)
        {
            try
            {
                var existingProductDetails = await _productBL.SelectProductByUID(productDetails.UID);
                if (existingProductDetails != null)
                {
                    productDetails.ServerModifiedTime = DateTime.Now;
                    var retVal = await _productBL.UpdateProduct(productDetails);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Product Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteProduct")]
        public async Task<ActionResult> DeleteProduct([FromQuery] string UID)
        {
            try
            {
                var retVal = await _productBL.DeleteProduct(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductsFiltered")]
        public async Task<ActionResult> GetProductsFiltered([FromQuery] string product_code, [FromQuery] string product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var products = await _productBL.GetProductsFiltered(product_code, product_name, CreatedTime, ModifiedTime);
                if (products.Any())
                {
                    return Ok(products);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter products data");
                throw;
            }
        }
        [HttpGet]
        [Route("GetProductsPaged")]
        public async Task<ActionResult> GetProductsPaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }

            try
            {
                var products = await _productBL.GetProductsPaged(pageNumber, pageSize);
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetProductsSorted")]
        public async Task<ActionResult> GetProductsSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var products = await _productBL.GetProductsSorted(sortCriterias);
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
        [HttpGet]
        [Route("SelectAllProduct")]
        public async Task<ActionResult> SelectAllProduct([FromQuery] List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        {
            try
            {
                if (pageNumber < 0 || pageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                var cacheKey = CacheConstants.ALL_Products;
                object product = null;
                product = _cacheService.Get<object>(cacheKey);
                if (product != null)
                {
                    return Ok(product);
                }
                product = await _productBL.SelectAllProduct(sortCriterias, pageNumber, pageSize, filterCriterias);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, product, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Product  Details");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve Product  Details");
            }
        }
    }
}