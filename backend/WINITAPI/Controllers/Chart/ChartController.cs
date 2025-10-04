using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Chart.BL.Interfaces;
namespace WINITAPI.Controllers.Contact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChartController : WINITBaseController
    {
        private readonly Winit.Modules.Chart.BL.Interfaces.IChartBL _chartBL;
        public ChartController(IServiceProvider serviceProvider, 
            Winit.Modules.Chart.BL.Interfaces.IChartBL chartBL) 
            : base(serviceProvider)
        {
            _chartBL = chartBL;
        }
        [HttpPost]
        [Route("GetPurchaseOrderAndTallyDashBoard")]
        public async Task<ActionResult> GetPurchaseOrderAndTallyDashBoard()
        {
            try
            {
                var poAndTallydashBoard = await _chartBL.GetPurchaseOrderAndTallyDashBoard();
                if (poAndTallydashBoard == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(poAndTallydashBoard);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve dashBoard Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}
