using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
namespace WINITAPI.Controllers.PurchaseOrder;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchaseOrderLineController : WINITBaseController
{
    private readonly Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderLineBL _purchaseOrderLineBL;
    public PurchaseOrderLineController(IServiceProvider serviceProvider, 
        Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderLineBL purchaseOrderLineBL) 
        : base(serviceProvider)
    {
        _purchaseOrderLineBL = purchaseOrderLineBL;
    }

    [HttpDelete("DeletePurchaseOrderLinesByUIDs")]
    public async Task<IActionResult> DeletePurchaseOrderLinesByUIDs([FromBody] List<string> purchaseOrderLineUIDs)
    {
        try
        {
            if (purchaseOrderLineUIDs is null || !purchaseOrderLineUIDs.Any())
            {
                return BadRequest();
            }
            int count = await _purchaseOrderLineBL.DeletePurchaseOrderLinesByUIDs(purchaseOrderLineUIDs);
            
            return CreateOkApiResponse(count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to delete PurchaseOrderlines");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
