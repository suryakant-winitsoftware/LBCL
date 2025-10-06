using System;
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
public class StockReceivingTrackingController : WINITBaseController
{
    private readonly IStockReceivingTrackingDL _stockReceivingTrackingDL;

    public StockReceivingTrackingController(
        IServiceProvider serviceProvider,
        IStockReceivingTrackingDL stockReceivingTrackingDL) : base(serviceProvider)
    {
        _stockReceivingTrackingDL = stockReceivingTrackingDL;
    }

    /// <summary>
    /// Get all Stock Receiving Tracking records
    /// </summary>
    /// <returns>List of Stock Receiving Tracking data</returns>
    [HttpGet("GetAll")]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var result = await _stockReceivingTrackingDL.GetAllAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get Stock Receiving Tracking by Purchase Order UID
    /// </summary>
    /// <param name="purchaseOrderUID">Purchase Order UID</param>
    /// <returns>Stock Receiving Tracking data</returns>
    [HttpGet("GetByPurchaseOrderUID/{purchaseOrderUID}")]
    public async Task<ActionResult> GetByPurchaseOrderUID(string purchaseOrderUID)
    {
        try
        {
            if (string.IsNullOrEmpty(purchaseOrderUID))
            {
                return BadRequest("Purchase Order UID is required");
            }

            var result = await _stockReceivingTrackingDL.GetByPurchaseOrderUIDAsync(purchaseOrderUID);

            if (result == null)
            {
                return Ok(new { success = false, message = "No stock receiving tracking found" });
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Save Stock Receiving Tracking data
    /// </summary>
    /// <param name="stockReceivingTracking">Stock Receiving Tracking data</param>
    /// <returns>Success status</returns>
    [HttpPost("SaveStockReceivingTracking")]
    public async Task<ActionResult> SaveStockReceivingTracking([FromBody] StockReceivingTracking stockReceivingTracking)
    {
        try
        {
            if (stockReceivingTracking == null)
            {
                return BadRequest("Invalid request data");
            }

            if (string.IsNullOrEmpty(stockReceivingTracking.PurchaseOrderUID))
            {
                return BadRequest("Purchase Order UID is required");
            }

            // Check if record already exists for this purchase order
            var existing = await _stockReceivingTrackingDL.GetByPurchaseOrderUIDAsync(stockReceivingTracking.PurchaseOrderUID);

            bool success;
            string message;

            if (existing != null)
            {
                // Update existing record - don't create duplicate
                stockReceivingTracking.UID = existing.UID;
                stockReceivingTracking.ModifiedBy = GetCurrentUserId();
                success = await _stockReceivingTrackingDL.UpdateStockReceivingTrackingAsync(stockReceivingTracking);
                message = success ? "Stock receiving tracking updated successfully" : "Failed to update stock receiving tracking";
            }
            else
            {
                // Create new record
                stockReceivingTracking.CreatedBy = GetCurrentUserId();
                stockReceivingTracking.IsActive = true;
                success = await _stockReceivingTrackingDL.SaveStockReceivingTrackingAsync(stockReceivingTracking);
                message = success ? "Stock receiving tracking created successfully" : "Failed to create stock receiving tracking";
            }

            if (success)
            {
                // Update purchase order status to RECEIVED
                try
                {
                    await _stockReceivingTrackingDL.UpdatePurchaseOrderStatusAsync(stockReceivingTracking.PurchaseOrderUID, "RECEIVED");
                    Console.WriteLine($"✅ Purchase order {stockReceivingTracking.PurchaseOrderUID} status updated to RECEIVED");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to update purchase order status: {ex.Message}");
                    // Continue anyway since stock receiving tracking was saved
                }

                return Ok(new { success = true, message });
            }
            else
            {
                return StatusCode(500, new { success = false, message });
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
