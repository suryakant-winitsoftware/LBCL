using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.StockUpdater.Model.Classes;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.Stock
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockUpdaterController : WINITBaseController
    {
        private readonly Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL _stockUpdaterBL;
        public StockUpdaterController(IServiceProvider serviceProvider, 
            Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL stockUpdaterBL) : base(serviceProvider)
        {
            _stockUpdaterBL = stockUpdaterBL;
        }
        [HttpPost]
        [Route("UpdateStockAsync")]
        public async Task<ActionResult> UpdateStockAsync([FromBody] List<WHStockLedger> stockLedgers)
        {
            try
            {
                List<IWHStockLedger> iStockLedgers = stockLedgers.Cast<IWHStockLedger>().ToList();
                int retValue = await _stockUpdaterBL.UpdateStockAsync(iStockLedgers);
                if (retValue == 0)
                {
                    return CreateOkApiResponse("No records updated");
                }
                return CreateOkApiResponse(retValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update Stock Ledger");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetWHStockSummary")]
        public async Task<ActionResult> GetWHStockSummary([FromQuery] string orgUID, [FromQuery] string wareHouseUID)
        {
            try
            {
                List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary> wHStockSummaries = await _stockUpdaterBL.GetWHStockSummary(orgUID, wareHouseUID);
                if (wHStockSummaries == null && wHStockSummaries.Count == 0)
                {
                    return CreateOkApiResponse("No records found");
                }
                return CreateOkApiResponse(wHStockSummaries);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve stock details");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
    }
}
