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
    /// Get all Delivery Loading Tracking records by Status
    /// </summary>
    /// <param name="status">Status (SHIPPED or RECEIVED)</param>
    /// <returns>List of Delivery Loading Tracking data with WH Stock Request details</returns>
    [HttpGet("GetByStatus/{status}")]
    public async Task<ActionResult> GetByStatus(string status)
    {
        try
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Status is required");
            }

            var result = await _deliveryLoadingTrackingDL.GetByStatusAsync(status);

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get Delivery Loading Tracking by WH Stock Request UID
    /// </summary>
    /// <param name="whStockRequestUID">WH Stock Request UID</param>
    /// <returns>Delivery Loading Tracking data</returns>
    [HttpGet("GetByWHStockRequestUID/{whStockRequestUID}")]
    public async Task<ActionResult> GetByWHStockRequestUID(string whStockRequestUID)
    {
        try
        {
            if (string.IsNullOrEmpty(whStockRequestUID))
            {
                return BadRequest("WH Stock Request UID is required");
            }

            var result = await _deliveryLoadingTrackingDL.GetByWHStockRequestUIDAsync(whStockRequestUID);

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

            if (string.IsNullOrEmpty(deliveryLoadingTracking.WHStockRequestUID))
            {
                return BadRequest("WH Stock Request UID is required");
            }

            // Check if record already exists for this WH stock request
            var existing = await _deliveryLoadingTrackingDL.GetByWHStockRequestUIDAsync(deliveryLoadingTracking.WHStockRequestUID);

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
                try
                {
                    if (!string.IsNullOrWhiteSpace(deliveryLoadingTracking.SecurityOfficerUID))
                    {
                        await _deliveryLoadingTrackingDL.UpdateWHStockRequestStatusAsync(deliveryLoadingTracking.WHStockRequestUID, "SHIPPED");
                        Console.WriteLine($"✅ WH Stock Request {deliveryLoadingTracking.WHStockRequestUID} status updated to SHIPPED");
                    }
                    else
                    {
                        await _deliveryLoadingTrackingDL.UpdateWHStockRequestStatusAsync(deliveryLoadingTracking.WHStockRequestUID, "Approved");
                        Console.WriteLine($"✅ WH Stock Request {deliveryLoadingTracking.WHStockRequestUID} status updated to Approved");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to update WH stock request status: {ex.Message}");
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
