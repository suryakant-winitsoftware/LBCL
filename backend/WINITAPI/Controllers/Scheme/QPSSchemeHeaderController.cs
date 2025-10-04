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
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Scheme.Model.Interfaces;
using WinIt.Models.Customers;
using Winit.Modules.SKU.Model.Classes;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class QPSSchemeHeaderController : WINITBaseController
    {

        private readonly IQPSSchemeBL _qpsSchemeHeaderBL;

        public QPSSchemeHeaderController(IServiceProvider serviceProvider, 
            IQPSSchemeBL qpsSchemeHeaderBL) : base(serviceProvider)
        {
            _qpsSchemeHeaderBL = qpsSchemeHeaderBL;
        }

        [HttpPost("GetQPSSchemesByStoreUIDAndSKUUID")]
        public async Task<IActionResult> GetQPSSchemesByStoreUIDAndSKUUID([FromQuery] string storeUID, [FromQuery] DateTime order_date, [FromBody] List<SKUFilter> filters)
        {
            try
            {
                var details = await _qpsSchemeHeaderBL.GetQPSSchemesByStoreUIDAndSKUUID(storeUID, order_date, filters);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sell in scheme by orguid and skus : {@orgUID}, {@skus}", storeUID, filters);
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
        [HttpPost("GetQPSSchemesByPOUID")]
        public async Task<IActionResult> GetQPSSchemesByPOUID([FromQuery] string pouid,[FromBody]List<SKUFilter> filters)
        {
            try
            {
                var details = await _qpsSchemeHeaderBL.GetQPSSchemesByPOUID(pouid, filters);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sell in scheme by orguid and skus : {@orgUID}, {@skus}",  filters);
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
    }
}
