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
    public class ProductTypeBridgeController : WINITBaseController
    {
        private readonly Winit.Modules.Product.BL.Interfaces.IProductTypeBridgeBL _productTypeBridgeBl;
        public ProductTypeBridgeController(IServiceProvider serviceProvider, 
            Winit.Modules.Product.BL.Interfaces.IProductTypeBridgeBL productTypeBridgeBl) 
            : base(serviceProvider)
        {
            _productTypeBridgeBl = productTypeBridgeBl;
        }
        [HttpPost]
        [Route("CreateProductTypeBridge")]
        public async Task<ActionResult> CreateProductTypeBridge([FromBody] Winit.Modules.Product.Model.Classes.ProductTypeBridge createProductTypeBridge)
        {
            try
            {
                createProductTypeBridge.ServerAddTime = DateTime.Now;
                createProductTypeBridge.ServerModifiedTime = DateTime.Now;
                var retVal = await _productTypeBridgeBl.CreateProductTypeBridge(createProductTypeBridge);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Product Type Bridge details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpDelete]
        [Route("DeleteProductTypeBridgeByUID")]
        public async Task<ActionResult> DeleteProductTypeBridgeByUID([FromQuery] string UID)
        {
            try
            {
                var retVal = await _productTypeBridgeBl.DeleteProductTypeBridgeByUID(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}