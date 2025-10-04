using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.PurchaseOrder.BL.Classes;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
namespace WINITAPI.Controllers.PurchaseOrder;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchaseOrderController : WINITBaseController
{
    private readonly Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderHeaderBL _PurchaseOrderBL;
    public PurchaseOrderController(IServiceProvider serviceProvider, 
        Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderHeaderBL PurchaseOrderBL
        ) : base(serviceProvider)
    {
        _PurchaseOrderBL = PurchaseOrderBL;
    }
    [HttpPost]
    [Route("GetPurchaseOrderHeaders")]
    public async Task<ActionResult> GetPurchaseOrderHeadersAsync(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem> PagedResponsePurchaseOrderHeaderlist = null;
            PagedResponsePurchaseOrderHeaderlist = await _PurchaseOrderBL.GetPurchaseOrderHeadersAsync(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired);
            return PagedResponsePurchaseOrderHeaderlist == null ? NotFound() : CreateOkApiResponse(PagedResponsePurchaseOrderHeaderlist);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve PurchaseOrder");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }


    [HttpPost("CUD_PurchaseOrder")]
    public async Task<IActionResult> CUD_PurchaseOrder([FromBody] List<IPurchaseOrderMaster> purchaseOrderMasters)
    {
        try
        {
            if (purchaseOrderMasters == null && !purchaseOrderMasters.Any())
            {
                return BadRequest();
            }
            var response = await _PurchaseOrderBL.CUD_PurchaseOrder(purchaseOrderMasters)
                ? CreateOkApiResponse("Created Successfully")
                : (IActionResult)CreateErrorResponse("Fail to Create PurchaseOrderMaster");

            foreach (IPurchaseOrderMaster purchaseOrderMaster in purchaseOrderMasters)
            {
                PerformAuditTrial(purchaseOrderMaster);
            }

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Create PurchaseOrderMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    private void PerformAuditTrial(IPurchaseOrderMaster purchaseOrderMaster)
    {
        try
        {
            var newData = new Dictionary<string, object>();

            //string auditTrailData = JsonSerializer.Serialize(purchaseOrderMaster, options);

            newData["Header"] = DictionaryConverter.ToDictionary(purchaseOrderMaster.PurchaseOrderHeader);

            // Convert only properties marked with [AuditTrail] in each line item
            newData["Lines"] = purchaseOrderMaster.PurchaseOrderLines?
                .Select(DictionaryConverter.ToDictionary)
                .ToList() ?? new List<Dictionary<string, object>>();

            // Convert only properties marked with [AuditTrail] in each line item
            newData["Provisions"] = purchaseOrderMaster.PurchaseOrderLineProvisions?
                .Select(DictionaryConverter.ToDictionary)
                .ToList() ?? new List<Dictionary<string, object>>();

            string orderNumber = purchaseOrderMaster.PurchaseOrderHeader?.DMSOrderNumber;

            var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                linkedItemType: LinkedItemType.PurchaseOrder,
                linkedItemUID: purchaseOrderMaster.PurchaseOrderHeader?.UID,
                commandType: AuditTrailCommandType.Create,
                docNo: orderNumber,
                jobPositionUID: null,
                empUID: purchaseOrderMaster.PurchaseOrderHeader?.CreatedBy,
                empName: User.FindFirst(ClaimTypes.Name)?.Value,
                newData: newData,
                originalDataId: null,
                changeData: null
            );

            LogAuditTrailInBackground(auditTrailEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(purchaseOrderMaster)}");
        }
    }
    private Dictionary<string, object> ToDictionary(object obj)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new AuditTrailConverter<IPurchaseOrderMaster>() },
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(obj, options);

        return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

    // Convert a list of objects to a List<Dictionary<string, object>>
    private List<Dictionary<string, object>> ToListOfDictionaries<T>(List<T> list)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new AuditTrailConverter<IPurchaseOrderMaster>() },
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(list, options);
        return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
    }
    

    [HttpGet("GetPurchaseOrderMasterByUID")]
    public async Task<IActionResult> GetPurchaseOrderMasterByUID([FromQuery] string uID)
    {
        try
        {
            if (string.IsNullOrEmpty(uID))
            {
                return BadRequest();
            }
            IPurchaseOrderMaster purchaseOrderMaster = await _PurchaseOrderBL.GetPurchaseOrderMasterByUID(uID);
            if (purchaseOrderMaster == null)
            {
                _ = CreateErrorResponse("Error occured while retiving the order master");
            }
            return CreateOkApiResponse(purchaseOrderMaster);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive PurchaseOrderMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost("GetPurchaseOrderSatatusCounts")]
    public async Task<IActionResult> GetPurchaseOrderSatatusCounts([FromBody] List<FilterCriteria>? filters = null)
    {
        try
        {
            var statusDict = await _PurchaseOrderBL.GetPurchaseOrderSatatusCounts(filters);
            if (statusDict == null)
            {
                return CreateErrorResponse("Error occured while retiving the Purchase Order Status Count");
            }
            return CreateOkApiResponse(statusDict);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive Purchase Order Status Count");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPut("UpdatePurchaseOrderHeaderStatusAfterApproval")]
    public async Task<IActionResult> UpdatePurchaseOrderHeaderStatusAfterApproval([FromBody] IPurchaseOrderHeader purchaseOrderHeader)
    {
        if (purchaseOrderHeader == null)
        {
            return BadRequest();
        }
        try
        {
            int cnt = await _PurchaseOrderBL.UpdatePurchaseOrderHeaderStatusAfterApproval(purchaseOrderHeader);
            if (cnt == 0)
            {
                return CreateErrorResponse("Error occured while Updating the Purchase Order Status ");
            }
            return CreateOkApiResponse(cnt);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Retrive Purchase Order Status Count");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost("CreateApproval")]
    public async Task<IActionResult> CreateApproval([FromQuery] string purchaseOrderUid, [FromBody] ApprovalRequestItem approvalRequestItem)
    {
        if (string.IsNullOrEmpty(purchaseOrderUid) || approvalRequestItem == null)
        {
            return BadRequest();
        }
        try
        {
            if (await _PurchaseOrderBL.CreateApproval(purchaseOrderUid, approvalRequestItem))
            {
                return CreateOkApiResponse("Approved");
            }
            return CreateErrorResponse("Error Occurded while Creating Approval");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to create Approval");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
