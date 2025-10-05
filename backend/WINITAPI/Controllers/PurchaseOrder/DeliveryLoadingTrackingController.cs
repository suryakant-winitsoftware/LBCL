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
public class DeliveryLoadingTrackingController : WINITBaseController
{
    private readonly IDeliveryLoadingTrackingDL _deliveryLoadingTrackingDL;

    public DeliveryLoadingTrackingController(
        IServiceProvider serviceProvider,
        IDeliveryLoadingTrackingDL deliveryLoadingTrackingDL) : base(serviceProvider)
    {
        _deliveryLoadingTrackingDL = deliveryLoadingTrackingDL;
    }

    /// <summary>
    /// Get Delivery Loading Tracking by Purchase Order UID
    /// </summary>
    /// <param name="purchaseOrderUID">Purchase Order UID</param>
    /// <returns>Delivery Loading Tracking data</returns>
    [HttpGet("GetByPurchaseOrderUID/{purchaseOrderUID}")]
    public async Task<ActionResult> GetByPurchaseOrderUID(string purchaseOrderUID)
    {
        try
        {
            if (string.IsNullOrEmpty(purchaseOrderUID))
            {
                return BadRequest("Purchase Order UID is required");
            }

            var result = await _deliveryLoadingTrackingDL.GetByPurchaseOrderUIDAsync(purchaseOrderUID);

            if (result == null)
            {
                return Ok(new { success = false, message = "No delivery loading tracking found" });
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Save Delivery Loading Tracking data
    /// </summary>
    /// <param name="deliveryLoadingTracking">Delivery Loading Tracking data</param>
    /// <returns>Success status</returns>
    [HttpPost("SaveDeliveryLoadingTracking")]
    public async Task<ActionResult> SaveDeliveryLoadingTracking([FromBody] DeliveryLoadingTracking deliveryLoadingTracking)
    {
        try
        {
            if (deliveryLoadingTracking == null)
            {
                return BadRequest("Invalid request data");
            }

            if (string.IsNullOrEmpty(deliveryLoadingTracking.PurchaseOrderUID))
            {
                return BadRequest("Purchase Order UID is required");
            }

            // Check if record already exists for this purchase order
            var existing = await _deliveryLoadingTrackingDL.GetByPurchaseOrderUIDAsync(deliveryLoadingTracking.PurchaseOrderUID);

            bool success;
            string message;

            if (existing != null)
            {
                // Update existing record - don't create duplicate
                deliveryLoadingTracking.UID = existing.UID;
                deliveryLoadingTracking.ModifiedBy = GetCurrentUserId();
                success = await _deliveryLoadingTrackingDL.UpdateDeliveryLoadingTrackingAsync(deliveryLoadingTracking);
                message = success ? "Delivery loading tracking updated successfully" : "Failed to update delivery loading tracking";
            }
            else
            {
                // Create new record
                deliveryLoadingTracking.CreatedBy = GetCurrentUserId();
                deliveryLoadingTracking.IsActive = true;
                success = await _deliveryLoadingTrackingDL.SaveDeliveryLoadingTrackingAsync(deliveryLoadingTracking);
                message = success ? "Delivery loading tracking created successfully" : "Failed to create delivery loading tracking";
            }

            if (success)
            {
                // Update purchase order status to SHIPPED
                try
                {
                    await _deliveryLoadingTrackingDL.UpdatePurchaseOrderStatusAsync(deliveryLoadingTracking.PurchaseOrderUID, "SHIPPED");
                    Console.WriteLine($"✅ Purchase order {deliveryLoadingTracking.PurchaseOrderUID} status updated to SHIPPED");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to update purchase order status: {ex.Message}");
                    // Continue anyway since delivery loading tracking was saved
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
