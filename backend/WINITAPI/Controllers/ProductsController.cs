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
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Proucts.PostgreSQLProductsRepository;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProductsController : WINITBaseController
    {
        private readonly WINITServices.Interfaces.IProductService _productService;

        public ProductsController(IServiceProvider serviceProvider, 
            WINITServices.Interfaces.IProductService productService) : base(serviceProvider)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("GetProductsAll")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsAll()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProducts.ALL_Products;
                object products = null;
                products = _cacheService.Get<object>(cacheKey);
                if (products != null)
                {
                    return Ok(products);
                }

                products = await _productService.GetProductsAll();
                if (products == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, products, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve products data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve products data");
            }
        }

        [HttpGet]
        [Route("GetProductsByProductCode")]
        public async Task<ActionResult> GetProductsByProductCode([FromQuery] string productCode)
        {
            try
            {
                WINITSharedObjects.Models.Product Product = await _productService.GetProductByProductCode(productCode);
                if (Product != null)
                {
                    return Ok(Product);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve products with productCode: {@productCode}", productCode);
                throw;
            }
        }

        [HttpPost]
        [Route("CreateProduct")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                product.CreatedTime = DateTime.Now;
                product.ModifiedTime = DateTime.Now;
                product.ServerAddTime = DateTime.Now;
                product.ServerModifiedTime = DateTime.Now;
                var createdproducts = await _productService.CreateProduct(product);
                return Created("", createdproducts);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create prodcts details");
                return StatusCode(500, new { success = false, message = "Error creating products", error = ex.Message });
            }

        }



        //[HttpPut]
        //[Route("UpdateProduct")]
        //public async Task<ActionResult<Product>> UpdateProducts([FromQuery] string productCode, [FromBody] Product UpdateProduct)
        //{
        //    try
        //    {
        //        var existingCustomer = await _productService.GetProductByProductCode(productCode);
        //        if (existingCustomer != null)
        //        {
        //            UpdateProduct.ModifiedTime = DateTime.Now;
        //            UpdateProduct.ServerModifiedTime = DateTime.Now;
        //            var updateProduct = await _productService.UpdateProduct(productCode, UpdateProduct);
        //            return Ok("Update successfully");
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error updating Products");
        //        return StatusCode(500, new { success = false, message = "Error updating Products", error = ex.Message });
        //    }

        //}

        [HttpPut]
        [Route("UpdateProduct")]
        public async Task<ActionResult<Product>> UpdateProducts( [FromBody] Product UpdateProduct)
        {
            try
            {
                var existingProduct = await _productService.GetProductByProductCode(UpdateProduct.product_code);
                if (existingProduct != null)
                {
                    UpdateProduct.ModifiedTime = DateTime.Now;
                    UpdateProduct.ServerModifiedTime = DateTime.Now;
                    var updateProduct = await _productService.UpdateProduct(UpdateProduct);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Products");
                return StatusCode(500, new { success = false, message = "Error updating Products", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteProduct")]
        public async Task<ActionResult> DeleteProduct([FromQuery] string productCode)
        {
            try
            {              

                var result = await _productService.DeleteProduct(productCode);
                if(result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }

        [HttpGet]
        [Route("GetProductsFiltered")]
        public async Task<ActionResult> GetProductsFiltered([FromQuery] string product_code, [FromQuery] string product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var products = await _productService.GetProductsFiltered(product_code, product_name, CreatedTime, ModifiedTime);
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
                var products = await _productService.GetProductsPaged(pageNumber, pageSize);
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
                var products = await _productService.GetProductsSorted(sortCriterias);
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }
        // ProductConfig All EndPoints

        [HttpGet]
        [Route("GetProductConfigAll")]
        public async Task<ActionResult<IEnumerable<ProductConfig>>> GetProductConfigAll()
        {
            try
            {
          
                var cacheKey = WINITSharedObjects.Constants.CacheProductsConfig.ALL_ProductsConfig;
                object ProductConfiglist = null;

                ProductConfiglist = _cacheService.Get<object>(cacheKey);
                if (ProductConfiglist != null)
                {
                    return Ok(ProductConfiglist);
                }

                ProductConfiglist = await _productService.GetProductConfigAll();
                if (ProductConfiglist == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, ProductConfiglist, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(ProductConfiglist);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductConfiglist data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductConfiglist data");
            }
        }

        [HttpGet]
        [Route("GetProductConfigByskuConfigId")]
        public async Task<ActionResult> GetProductConfigByskuConfigId([FromQuery] int skuConfigId)
        {
            try
            {
                WINITSharedObjects.Models.ProductConfig ProductConfigList = await _productService.GetProductConfigByskuConfigId(skuConfigId);

                if (ProductConfigList != null)
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
                Log.Error(ex, "Failed to retrieve ProductConfigList with skuConfigId: {@skuConfigId}", skuConfigId);
                throw;
            }
        }

        [HttpPost]
        [Route("CreateProductConfig")]
        public async Task<ActionResult<ProductConfig>> CreateProductConfig([FromBody] ProductConfig CreateProductConfig)
        {
            try
            {
                CreateProductConfig.CreatedTime = DateTime.Now;
                CreateProductConfig.ModifiedTime = DateTime.Now;
                CreateProductConfig.ServerAddTime = DateTime.Now;
                CreateProductConfig.ServerModifiedTime = DateTime.Now;
                
                var ProductConfigList = await _productService.CreateProductConfig(CreateProductConfig);
                return Created("", ProductConfigList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductConfig details");
                return StatusCode(500, new { success = false, message = "Error creating ProductConfig", error = ex.Message });
            }

        }

        [HttpPut]
        [Route("UpdateProductConfig")]
        public async Task<ActionResult<ProductConfig>> UpdateProductConfig( [FromBody] ProductConfig UpdateProductConfig)
        {
            try
            {
                long skuConfigId = UpdateProductConfig.SKUConfigId;
                 int convertedId = (int)skuConfigId;
                var existingProductConfig = await _productService.GetProductConfigByskuConfigId(convertedId);
                if (existingProductConfig != null)
                {
                    UpdateProductConfig.ModifiedTime = DateTime.Now;
                    UpdateProductConfig.ServerModifiedTime = DateTime.Now;
                    var ProductConfigList = await _productService.UpdateProductConfig(UpdateProductConfig);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductConfig");
                return StatusCode(500, new { success = false, message = "Error updating ProductConfig", error = ex.Message });
            }

        }

        [HttpDelete]
        [Route("DeleteProductConfig")]
        public async Task<ActionResult> DeleteProductConfig(int skuConfigId)
        {
            try
            {
              
                var result = await _productService.DeleteProductConfig(skuConfigId);
                if (result == 0)
                {
                   return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }
        [HttpGet]
        [Route("GetProductConfigFiltered")]
        public async Task<ActionResult> GetProductConfigFiltered([FromQuery] string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductConfigList = await _productService.GetProductConfigFiltered(ProductCode, CreatedTime, ModifiedTime);
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

            if(pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductConfigList = await _productService.GetProductConfigPaged(pageNumber, pageSize);
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
                var ProductConfigList = await _productService.GetProductConfigSorted(sortCriterias);
                return Ok(ProductConfigList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }

        // ProductUOM All EndPoints

        [HttpGet]
        [Route("GetProductUOMAll")]
        public async Task<ActionResult<IEnumerable<ProductUOM>>> GetProductUOMAll()
        {
            try
            {
               

                var cacheKey = WINITSharedObjects.Constants.CacheProductUOMAll.ALL_ProductUOM;
                object ProductUOMlist = null;
                ProductUOMlist = _cacheService.Get<object>(cacheKey);
                if (ProductUOMlist != null)
                {
                    return Ok(ProductUOMlist);
                }
                ProductUOMlist = await _productService.GetProductUOMAll();
                if (ProductUOMlist == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, ProductUOMlist, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(ProductUOMlist);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductUOMList data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductUOMList data");
            }
        }

        [HttpGet]
        [Route("GetProductUOMByProductUomId")]
        public async Task<ActionResult> GetProductUOMByProductUomId([FromQuery] int productUomId)
        {
            try
            {
                WINITSharedObjects.Models.ProductUOM ProductUOMlist = await _productService.GetProductUOMByProductUomId(productUomId);

                if (ProductUOMlist != null)
                {
                    return Ok(ProductUOMlist);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductUOMlist with productUomId: {@productUomId}", productUomId);
                throw;
            }
        }

        [HttpPost]
        [Route("CreateProductUOM")]
        public async Task<ActionResult<ProductUOM>> CreateProductUOM([FromBody] ProductUOM CreateProductUOM)
        {
            try
            {
                CreateProductUOM.CreatedTime = DateTime.Now;
                CreateProductUOM.ModifiedTime = DateTime.Now;
                CreateProductUOM.ServerAddTime = DateTime.Now;
                CreateProductUOM.ServerModifiedTime = DateTime.Now;
                var ProductUOMList = await _productService.CreateProductUOM(CreateProductUOM);
                return Created("", ProductUOMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductUOM");
                return StatusCode(500, new { success = false, message = "Error creating ProductUOM", error = ex.Message });
            }

        }

        [HttpPut]
        [Route("UpdateProductUOM")]
        public async Task<ActionResult<ProductUOM>> UpdateProductUOM( [FromBody] ProductUOM UpdateProductUOM)
        {
            try
            {

                long productUOMId = UpdateProductUOM.ProductUOMId;
                int convertedId = (int)productUOMId;
                var existingProductUOM = await _productService.GetProductUOMByProductUomId(convertedId);
                if (existingProductUOM != null)
                {
                    UpdateProductUOM.ModifiedTime = DateTime.Now;
                    UpdateProductUOM.ServerModifiedTime = DateTime.Now;
                    var ProductUOMList = await _productService.UpdateProductUOM(UpdateProductUOM);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductUOM");
                return StatusCode(500, new { success = false, message = "Error updating ProductUOM", error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteProductUOM")]
        public async Task<ActionResult> DeleteProductUOM(int productUomId)
        {
            try
            {
                var result = await _productService.DeleteProductUOM(productUomId);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }

        [HttpGet]
        [Route("GetProductUOMFiltered")]
        public async Task<ActionResult> GetProductUOMFiltered([FromQuery] string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var GetProductUOMList = await _productService.GetProductUOMFiltered(ProductCode, CreatedTime, ModifiedTime);
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

            if(pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductUOMList = await _productService.GetProductUOMPaged(pageNumber, pageSize);
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
                var ProductUOMList = await _productService.GetProductUOMSorted(sortCriterias);
                return Ok(ProductUOMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }

       

        // ProductType All EndPoints

        [HttpGet]
        [Route("GetProductTypeAll")]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetProductTypeAll()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProductType.ALL_ProductType;
                object ProductTypelist = null;
                ProductTypelist = _cacheService.Get<object>(cacheKey);
                if (ProductTypelist != null)
                {
                    return Ok(ProductTypelist);
                }
                ProductTypelist = await _productService.GetProductTypeAll();
                if (ProductTypelist == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, ProductTypelist, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(ProductTypelist);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductType data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductType data");
            }
        }

        [HttpGet]
        [Route("GetProductTypeByProductTypeId")]
        public async Task<ActionResult> GetProductTypeByProductTypeId([FromQuery] int productTypeId)
        {
            try
            {
                WINITSharedObjects.Models.ProductType ProductTypelist = await _productService.GetProductTypeByProductTypeId(productTypeId);

                if (ProductTypelist != null)
                {
                    return Ok(ProductTypelist);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductTypelist with productTypeId: {@productTypeId}", productTypeId);
                throw;
            }
        }

        [HttpPost]
        [Route("CreateProductType")]
        public async Task<ActionResult<ProductType>> CreateProductType([FromBody] ProductType CreateProductType)
        {
            try
            {
                CreateProductType.CreatedTime = DateTime.Now;
                CreateProductType.ModifiedTime = DateTime.Now;
                CreateProductType.ServerAddTime = DateTime.Now;
                CreateProductType.ServerModifiedTime = DateTime.Now;
                var ProductTypeList = await _productService.CreateProductType(CreateProductType);
                return Created("", ProductTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductType");
                return StatusCode(500, new { success = false, message = "Error creating ProductType", error = ex.Message });
            }

        }
        [HttpPut]
        [Route("UpdateProductType")]
        public async Task<ActionResult<ProductType>> UpdateProductType( [FromBody] ProductType UpdateProductType)
        {
            try
            {
                long producttypeid = UpdateProductType.product_type_id;
                int convertedId = (int)producttypeid;
                var existingProductType = await _productService.GetProductTypeByProductTypeId(convertedId);
                if (existingProductType != null)
                {
                    UpdateProductType.ModifiedTime = DateTime.Now;
                    UpdateProductType.ServerModifiedTime = DateTime.Now;
                    var ProductTypeList = await _productService.UpdateProductType(UpdateProductType);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductType");
                return StatusCode(500, new { success = false, message = "Error updating ProductType", error = ex.Message });
            }

        }

        [HttpDelete]
        [Route("DeleteProductType")]
        public async Task<ActionResult> DeleteProductType(int productTypeId)
        {
            try
            {
                var result = await _productService.DeleteProductType(productTypeId);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }



        [HttpGet]
        [Route("GetProductTypeFiltered")]
        public async Task<ActionResult> GetProductTypeFiltered([FromQuery] string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductTypeList = await _productService.GetProductTypeFiltered(product_type_code, CreatedTime, ModifiedTime);
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

            if(pageNumber < 1 || pageSize < 1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductTypeList = await _productService.GetProductTypePaged(pageNumber, pageSize);
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
                var ProductTypeList = await _productService.GetProductTypeSorted(sortCriterias);
                return Ok(ProductTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }



        [HttpGet]
        [Route("GetProductDimensionAll")]
        public async Task<ActionResult<IEnumerable<ProductDimension>>> GetProductDimensionAll()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProductDimension.ALL_ProductDimension;
                object ProductDimensionlist = null;
                ProductDimensionlist = _cacheService.Get<object>(cacheKey);
                if (ProductDimensionlist != null)
                {
                    return Ok(ProductDimensionlist);
                }

                ProductDimensionlist = await _productService.GetProductDimensionAll();
                if (ProductDimensionlist == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, ProductDimensionlist, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(ProductDimensionlist);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductDimension data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductDimension data");
            }
        }

        [HttpGet]
        [Route("GetProductDimensionByProductDimensionId")]
        public async Task<ActionResult> GetProductDimensionByProductDimensionId([FromQuery] int productDimensionId)
        {
            try
            {
                WINITSharedObjects.Models.ProductDimension ProductDimensionlist = await _productService.GetProductDimensionByProductDimensionId(productDimensionId);

                if (ProductDimensionlist != null)
                {
                    return Ok(ProductDimensionlist);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProductDimension with productDimensionId: {@productDimensionId}", productDimensionId);
                throw;
            }
        }

        [HttpPost]
        [Route("CreateProductDimension")]
        public async Task<ActionResult<ProductDimension>> CreateProductDimension([FromBody] ProductDimension CreateProductDimension)
        {
            try
            {
                CreateProductDimension.CreatedTime = DateTime.Now;
                CreateProductDimension.ModifiedTime = DateTime.Now;
                CreateProductDimension.ServerAddTime = DateTime.Now;
                CreateProductDimension.ServerModifiedTime = DateTime.Now;
                var ProductDimensionList = await _productService.CreateProductDimensionList(CreateProductDimension);
                return Created("", ProductDimensionList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductDimension");
                return StatusCode(500, new { success = false, message = "Error creating ProductDimension", error = ex.Message });
            }

        }
        [HttpPut]
        [Route("UpdateProductDimension")]
        public async Task<ActionResult<ProductDimension>> UpdateProductDimension( [FromBody] ProductDimension UpdateProductDimension)
        {
            try
            {
                long productDimensionId = UpdateProductDimension.product_dimension_id;
                int convertedId = (int)productDimensionId;
                var existingCustomer = await _productService.GetProductDimensionByProductDimensionId(convertedId);
                if (existingCustomer != null)
                {
                    UpdateProductDimension.ModifiedTime = DateTime.Now;
                    UpdateProductDimension.ServerModifiedTime = DateTime.Now;
                    var ProductDimensionList = await _productService.UpdateProductDimension(UpdateProductDimension);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ProductDimensionList");
                return StatusCode(500, new { success = false, message = "Error updating ProductDimension", error = ex.Message });
            }

        }

        [HttpDelete]
        [Route("DeleteProductDimension")]
        public async Task<ActionResult> DeleteProductDimension(int productDimensionId)
        {
            try
            {          
                var result = await _productService.DeleteProductDimension(productDimensionId);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }



        [HttpGet]
        [Route("GetProductDimensionFiltered")]
        public async Task<ActionResult> GetProductDimensionFiltered([FromQuery] string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            try
            {
                var ProductDimensionList = await _productService.GetProductDimensionFiltered(product_dimension_code, CreatedTime, ModifiedTime);
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
            if(pageNumber < 1 || pageSize <1)
            {
                return StatusCode(400);
            }
            try
            {
                var ProductDimensionList = await _productService.GetProductDimensionPaged(pageNumber, pageSize);
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
                var ProductDimensionList = await _productService.GetProductDimensionSorted(sortCriterias);
                return Ok(ProductDimensionList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }


       

        [HttpGet]
        [Route("AllGetProduct")]
        public async Task<ActionResult<IEnumerable<Product>>> AllGetProduct([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProducts.ALL_Products;
                object products = null;
                products = _cacheService.Get<object>(cacheKey);
                if (products != null)
                {
                    return Ok(products);
                }
                products = await _productService.AllGetProduct(sortCriterias, pageNumber, pageSize, filterCriterias);
                if (products == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, products, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve products data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve products data");
            }
        }


        //ProductAttributes
        [HttpGet]
        [Route("GetProductAttributes")]
        public async Task<ActionResult<IEnumerable<ProductAttributes>>> GetProductAttributes()
        {
            try
            {
                var cacheKey = WINITSharedObjects.Constants.CacheProductAttributes.ALL_ProductAttributes;
                object ProductAttributesList = null;
                ProductAttributesList = _cacheService.Get<object>(cacheKey);
                if (ProductAttributesList != null)
                {
                    return Ok(ProductAttributesList);
                }
                ProductAttributesList = await _productService.GetProductAttributes();
                if (ProductAttributesList == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, ProductAttributesList, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(ProductAttributesList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ProductAttributes data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve ProductAttributes data");
            }
        }

        //GetProductMasterData

        [HttpGet("GetProductsMasterData")]
        public async Task<ActionResult<ProductMaster>> GetProductsMasterData(bool isProductConfigRequired = false, bool isProductUOMRequired = false)
        {
            var Products = await _productService.GetProductsAll();
            var response = new ProductMaster
            {
                Products = (List<Product>)Products
            };

            if (isProductConfigRequired)
            {
                var ProductConfigs = await _productService.GetProductConfigAll();
                response.ProductConfigs = (List<ProductConfig>)ProductConfigs;
            }

            if (isProductUOMRequired)
            {
                var ProductUOMs = await _productService.GetProductUOMAll();
                response.ProductUOMs = (List<ProductUOM>)ProductUOMs;
            }

            return Ok(response);
        }



        //ProductDimensionBridge 

        [HttpPost]
        [Route("CreateProductDimensionBridge")]
        public async Task<ActionResult<ProductDimensionBridge>> CreateProductDimensionBridge([FromBody] ProductDimensionBridge productDimensionBridge)
        {
            try
            {
                productDimensionBridge.CreatedTime = DateTime.Now;
                productDimensionBridge.ModifiedTime = DateTime.Now;
                productDimensionBridge.ServerAddTime = DateTime.Now;
                productDimensionBridge.ServerModifiedTime = DateTime.Now;
                var createdproducts = await _productService.CreateProductDimensionBridge(productDimensionBridge);
                return Created("", productDimensionBridge);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductDimensionBridge details");
                return StatusCode(500, new { success = false, message = "Error creating ProductDimensionBridge", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteProductDimensionBridge")]
        public async Task<ActionResult> DeleteProductDimensionBridge([FromQuery] int product_dimension_bridge_id)
        {
            try
            {

                var result = await _productService.DeleteProductDimensionBridge(product_dimension_bridge_id);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }

        //ProductTypeBridge 

        [HttpPost]
        [Route("CreateProductTypeBridge")]
        public async Task<ActionResult<ProductTypeBridge>> CreateProductTypeBridge([FromBody] ProductTypeBridge productTypeBridge)
        {
            try
            {
                productTypeBridge.CreatedTime = DateTime.Now;
                productTypeBridge.ModifiedTime = DateTime.Now;
                productTypeBridge.ServerAddTime = DateTime.Now;
                productTypeBridge.ServerModifiedTime = DateTime.Now;
                var createdproducts = await _productService.CreateProductTypeBridge(productTypeBridge);
                return Created("", productTypeBridge);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductTypeBridge details");
                return StatusCode(500, new { success = false, message = "Error creating ProductTypeBridge", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteProductTypeBridge")]
        public async Task<ActionResult> DeleteProductTypeBridge([FromQuery] int product_type_bridge_id)
        {
            try
            {

                var result = await _productService.DeleteProductTypeBridge(product_type_bridge_id);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }



        /*  public async Task<ActionResult> GetProductsMasterData(bool isProductConfigRequired = false, bool isProductUOMRequired = false)
          {

              var productList = await _productService.GetProductsAll();
              var response = new ProductMaster
              {
                  Products = (List<Product>)productList
              };

              if (isProductConfigRequired)
              {
                  var productConfigList = await _productService.GetProductConfigAll();
                  response.ProductConfigs = (List<ProductConfig>)productConfigList;
              }

              if (isProductUOMRequired)
              {
                  var productUOMList = await _productService.GetProductUOMAll();
                  response.ProductUOMs = (List<ProductUOM>)productUOMList;
              }

              var filteredResponse = new ProductMaster
              {
                  Products = response.Products,
                  ProductConfigs = response.ProductConfigs != null ? response.ProductConfigs : null,
                  ProductUOMs = response.ProductUOMs != null ? response.ProductUOMs : null
              };

              return Ok(filteredResponse);

          }*/

    }
}
