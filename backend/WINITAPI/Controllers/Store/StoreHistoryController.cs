using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace WINITAPI.Controllers.Store;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StoreHistoryController : WINITBaseController
{
    private readonly Winit.Modules.Store.BL.Interfaces.IStoreHistoryBL _storeHistoryBl;
    public StoreHistoryController(IServiceProvider serviceProvider, 
        Winit.Modules.Store.BL.Interfaces.IStoreHistoryBL storeHistoryBl) : base(serviceProvider)
    {
        _storeHistoryBl = storeHistoryBl;
    }


    [HttpGet]
    [Route("GetStoreHistoryByRouteUIDVisitDateAndStoreUID")]
    public async Task<IActionResult> GetStoreHistoryByRouteUIDVisitDateAndStoreUID(string routeUID, string visitDate, string storeUID)
    {
        try
        {
            IStoreHistory storeHistory = await _storeHistoryBl.GetStoreHistoryByRouteUIDVisitDateAndStoreUID(routeUID, visitDate, storeUID);
            return storeHistory is not null ? CreateOkApiResponse(storeHistory) : (IActionResult)NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve StoreHistory with UID: {@UID}", routeUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

}

