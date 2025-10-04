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
public class PurchaseOrderTemplateHeaderController : WINITBaseController
{
    private readonly Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateHeaderBL _purchaseOrderTemplateHeaderBL;
    public PurchaseOrderTemplateHeaderController(IServiceProvider serviceProvider, 
        Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateHeaderBL purchaseOrderTemplateHeaderBL) 
        : base(serviceProvider)
    {
        _purchaseOrderTemplateHeaderBL = purchaseOrderTemplateHeaderBL;
    }
    [HttpPost]
    [Route("GetAllPurchaseOrderTemplateHeaders")]
    public async Task<ActionResult> GetAllPurchaseOrderTemplateHeaders(PagingRequest pagingRequest)
    {
        try
        {
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaderPagedResponse = null;
            purchaseOrderTemplateHeaderPagedResponse = await _purchaseOrderTemplateHeaderBL.GetAllPurchaseOrderTemplateHeaders(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired);
            return purchaseOrderTemplateHeaderPagedResponse == null ? NotFound() : CreateOkApiResponse(purchaseOrderTemplateHeaderPagedResponse);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve PurchaseOrder");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }


    [HttpPost("CUD_PurchaseOrderTemplate")]
    public async Task<IActionResult> CUD_PurchaseOrderTemplate([FromBody] IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster)
    {
        try
        {
            return purchaseOrderTemplateMaster == null
                ? BadRequest()
                : await _purchaseOrderTemplateHeaderBL.CUD_PurchaseOrderTemplate(purchaseOrderTemplateMaster)
                    ? CreateOkApiResponse("Created Successfully")
                    : (IActionResult)CreateErrorResponse("Fail to Create PurchaseOrderTemplateMaster");
        }
        catch (Exception ex)
        {

            Log.Error(ex, "Fail to Create PurchaseOrderTemplateMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet("GetPurchaseOrderTemplateMasterByUID")]
    public async Task<IActionResult> GetPurchaseOrderTemplateMasterByUID([FromQuery] string uID)
    {
        try
        {
            if (string.IsNullOrEmpty(uID))
            {
                return BadRequest();
            }
            IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster = await _purchaseOrderTemplateHeaderBL.GetPurchaseOrderTemplateMasterByUID(uID);
            if (purchaseOrderTemplateMaster == null)
            {
                _ = CreateErrorResponse("Error occured while retiving the order master");
            }
            return CreateOkApiResponse(purchaseOrderTemplateMaster);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive PurchaseOrderTemplateMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpDelete("DeletePurchaseOrderHeaderByUIDs")]
    public async Task<IActionResult> DeletePurchaseOrderHeaderByUIDs([FromBody] List<string> purchaseOrderHeaderUids)
    {
        try
        {
            if (purchaseOrderHeaderUids == null || !purchaseOrderHeaderUids.Any())
            {
                return BadRequest();
            }
            int count = await _purchaseOrderTemplateHeaderBL.DeletePurchaseOrderHeaderByUID(purchaseOrderHeaderUids);
            if (count == 0)
            {
                _ = CreateErrorResponse("Error occured while deleting the purchase Order header");
            }
            return CreateOkApiResponse(count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to delete PurchaseOrderTemplateMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet("GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby")]
    public async Task<IActionResult> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby([FromQuery] string storeUid, [FromQuery] string createdby)
    {
        try
        {
            if (string.IsNullOrEmpty(storeUid) && string.IsNullOrEmpty(createdby))
            {
                return BadRequest();
            }
            var purchaseOrderTemplates = await _purchaseOrderTemplateHeaderBL.GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(storeUid, createdby);
            if (purchaseOrderTemplates == null)
            {
                _ = CreateErrorResponse("Error occured while retiving the  purchaseOrderTemplates");
            }
            return CreateOkApiResponse(purchaseOrderTemplates);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive purchaseOrderTemplates");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
