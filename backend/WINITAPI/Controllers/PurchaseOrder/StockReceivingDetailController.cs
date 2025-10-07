using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace WINITAPI.Controllers.PurchaseOrder;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StockReceivingDetailController : WINITBaseController
{
    private readonly IStockReceivingDetailDL _stockReceivingDetailDL;

    public StockReceivingDetailController(
        IServiceProvider serviceProvider,
        IStockReceivingDetailDL stockReceivingDetailDL) : base(serviceProvider)
    {
        _stockReceivingDetailDL = stockReceivingDetailDL;
    }

    /// <summary>
    /// Get Stock Receiving Details by WH Stock Request UID
    /// </summary>
    /// <param name="whStockRequestUID">WH Stock Request UID</param>
    /// <returns>List of Stock Receiving Detail data</returns>
    [HttpGet("GetByWHStockRequestUID/{whStockRequestUID}")]
    public async Task<ActionResult> GetByWHStockRequestUID(string whStockRequestUID)
    {
        try
        {
            if (string.IsNullOrEmpty(whStockRequestUID))
            {
                return BadRequest("WH Stock Request UID is required");
            }

            var result = await _stockReceivingDetailDL.GetByWHStockRequestUIDAsync(whStockRequestUID);

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get Stock Receiving Details by Purchase Order UID (Deprecated - use GetByWHStockRequestUID)
    /// </summary>
    [HttpGet("GetByPurchaseOrderUID/{purchaseOrderUID}")]
    public async Task<ActionResult> GetByPurchaseOrderUID(string purchaseOrderUID)
    {
        return await GetByWHStockRequestUID(purchaseOrderUID);
    }

    /// <summary>
    /// Save Stock Receiving Detail data
    /// </summary>
    /// <param name="details">List of Stock Receiving Detail data</param>
    /// <returns>Success status</returns>
    [HttpPost("SaveStockReceivingDetails")]
    public async Task<ActionResult> SaveStockReceivingDetails([FromBody] List<StockReceivingDetail> details)
    {
        try
        {
            if (details == null || details.Count == 0)
            {
                return BadRequest("Invalid request data");
            }

            var userId = GetCurrentUserId();

            // Set CreatedBy for all details
            foreach (var detail in details)
            {
                detail.CreatedBy = userId;
            }

            // Delete existing details for this WH stock request (soft delete)
            var whStockRequestUID = details[0].WHStockRequestUID;
            await _stockReceivingDetailDL.DeleteByWHStockRequestUIDAsync(whStockRequestUID);

            // Save new details
            bool success = await _stockReceivingDetailDL.SaveStockReceivingDetailsAsync(details);

            if (success)
            {
                return Ok(new { success = true, message = "Stock receiving details saved successfully" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Failed to save stock receiving details" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    private string? GetCurrentUserId()
    {
        try
        {
            var userIdClaim = User?.FindFirst("UserUID");
            return userIdClaim?.Value;
        }
        catch
        {
            return null;
        }
    }
}
