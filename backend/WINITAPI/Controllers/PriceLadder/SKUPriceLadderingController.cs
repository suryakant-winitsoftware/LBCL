using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WINITAPI.Controllers.PriceLadder;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class SKUPriceLadderingController : WINITBaseController
{
    private readonly Winit.Modules.PriceLadder.BL.Interfaces.ISKUPriceLadderingBL _sKUPriceLadderingBL;
    public SKUPriceLadderingController(IServiceProvider serviceProvider, 
        Winit.Modules.PriceLadder.BL.Interfaces.ISKUPriceLadderingBL sKUPriceLadderingBL) 
        : base(serviceProvider)
    {
        _sKUPriceLadderingBL = sKUPriceLadderingBL;
    }

    [HttpPost]
    [Route("GetApplicablePriceLaddering")]
    public async Task<ActionResult> DeleteTaxSkuMapByUID([FromQuery] string broadCustomerClassification, [FromQuery] DateTime date,
        [FromBody] List<int> productCategoryIds = null)
    {
        try
        {
            List<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData> result = await _sKUPriceLadderingBL.GetApplicablePriceLaddering(broadCustomerClassification, date, productCategoryIds);
            return result != null ? CreateOkApiResponse(result) : throw new Exception("Retriving GetApplicablePriceLaddering Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetApplicablePriceLaddering Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
