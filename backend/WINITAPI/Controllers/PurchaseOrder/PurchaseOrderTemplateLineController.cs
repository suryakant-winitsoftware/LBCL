using Google.Api.Gax;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
namespace WINITAPI.Controllers.PurchaseOrder;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchaseOrderTemplateLineController : WINITBaseController
{
    private readonly Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateLineBL _purchaseOrderTemplateLineBL;
    public PurchaseOrderTemplateLineController(IServiceProvider serviceProvider, 
        Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateLineBL purchaseOrderTemplateLineBL) 
        : base(serviceProvider)
    {
        _purchaseOrderTemplateLineBL = purchaseOrderTemplateLineBL;
    }

    [HttpPost("GetAllPurchaseOrderTemplateLines")]
    public async Task<IActionResult> GetAllPurchaseOrderTemplateLines([FromBody] PagingRequest pagingRequest)
    {
        try
        {
            if (pagingRequest is null)
            {
                return BadRequest();
            }
            PagedResponse<IPurchaseOrderTemplateLine> pagedResponse = await _purchaseOrderTemplateLineBL
                .GetAllPurchaseOrderTemplateLines(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize,
                pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
            if (pagedResponse == null)
            {
                _ = CreateErrorResponse("Error occured while retiving the PurchaseOrderTemplateLines");
            }
            return CreateOkApiResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive PurchaseOrderTemplateLines");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    
    [HttpDelete("DeletePurchaseOrderTemplateLinesByUIDs")]
    public async Task<IActionResult> DeletePurchaseOrderTemplateLinesByUIDs([FromBody] List<string> purchaseOrderTemplateLineUids)
    {
        try
        {
            if (purchaseOrderTemplateLineUids is null || !purchaseOrderTemplateLineUids.Any())
            {
                return BadRequest();
            }
            int response = await _purchaseOrderTemplateLineBL
                .DeletePurchaseOrderTemplateLinesByUIDs(purchaseOrderTemplateLineUids);
            if (response == 0)
            {
                _ = CreateErrorResponse("Error occured while deleting the PurchaseOrderTemplateLines");
            }
            return CreateOkApiResponse(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to deleting PurchaseOrderTemplateLines");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
