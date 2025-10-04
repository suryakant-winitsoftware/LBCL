using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductDimensionBridgeController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductDimensionBridgeBL _productdimensionbridgeBl;
        public ProductDimensionBridgeController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductDimensionBridgeBL productdimensionbridgeBl) 
            : base(serviceProvider)
        {
            _productdimensionbridgeBl = productdimensionbridgeBl;
        }
        [HttpPost]
        [Route("CreateProductDimensionBridge")]
        public async Task<ActionResult> CreateProductDimensionBridge([FromBody] Winit.Modules.Product.Model.Classes.ProductDimensionBridge productDimensionBridge)
        {
            try
            {
                productDimensionBridge.ServerAddTime = DateTime.Now;
                productDimensionBridge.ServerModifiedTime = DateTime.Now;
                var retValue = await _productdimensionbridgeBl.CreateProductDimensionBridge(productDimensionBridge);
                return (retValue>0)? CreateOkApiResponse(retValue) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ProductDimensionBridge details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteProductDimensionBridge")]
        public async Task<ActionResult> DeleteProductDimensionBridge([FromQuery] string UID)
        {
            try
            {
                var result = await _productdimensionbridgeBl.DeleteProductDimensionBridge(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}