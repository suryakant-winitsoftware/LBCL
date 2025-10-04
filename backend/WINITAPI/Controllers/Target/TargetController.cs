using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Modules.Target.Model.Classes;
using Winit.Modules.Target.Model.Interfaces;
using Winit.Modules.Target.BL.Interfaces;
using WINITAPI.Common;

namespace WINITAPI.Controllers.Target
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TargetController : WINITBaseController
    {
        private readonly ITargetBL _targetBL;

        public TargetController(IServiceProvider serviceProvider, ITargetBL targetBL) : base(serviceProvider)
        {
            _targetBL = targetBL;
        }

        /// <summary>
        /// Get all targets with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllTargets([FromQuery] TargetFilter filter)
        {
            try
            {
                var targets = await _targetBL.GetAllTargetsAsync(filter ?? new TargetFilter());
                return CreateOkApiResponse(targets);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting targets");
                return CreateErrorResponse($"Error retrieving targets: {ex.Message}");
            }
        }

        /// <summary>
        /// Get paged targets with filtering
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedTargets([FromQuery] TargetFilter filter)
        {
            try
            {
                var result = await _targetBL.GetPagedTargetsAsync(filter ?? new TargetFilter());
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting paged targets");
                return CreateErrorResponse($"Error retrieving targets: {ex.Message}");
            }
        }

        /// <summary>
        /// Get target by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetTargetById(long id)
        {
            try
            {
                var target = await _targetBL.GetTargetByIdAsync(id);
                if (target == null)
                {
                    return NotFound($"Target with ID {id} not found");
                }
                return CreateOkApiResponse(target);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error getting target {id}");
                return CreateErrorResponse($"Error retrieving target: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new target
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateTarget([FromBody] Winit.Modules.Target.Model.Classes.Target target)
        {
            try
            {
                if (target == null)
                {
                    return BadRequest("Target data is required");
                }

                // Set created by from session
                target.CreatedBy = GetCurrentUserName();
                target.ModifiedBy = GetCurrentUserName();

                var createdTarget = await _targetBL.CreateTargetAsync(target);
                return CreateOkApiResponse(createdTarget);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating target");
                return CreateErrorResponse($"Error creating target: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing target
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTarget(long id, [FromBody] Winit.Modules.Target.Model.Classes.Target target)
        {
            try
            {
                if (target == null)
                {
                    return BadRequest("Target data is required");
                }

                if (id != target.Id)
                {
                    return BadRequest("ID mismatch");
                }

                // Set modified by from session
                target.ModifiedBy = GetCurrentUserName();

                var updatedTarget = await _targetBL.UpdateTargetAsync(target);
                return CreateOkApiResponse(updatedTarget);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating target {id}");
                return CreateErrorResponse($"Error updating target: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a target by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTarget(long id)
        {
            try
            {
                var result = await _targetBL.DeleteTargetAsync(id);
                if (result)
                {
                    return CreateOkApiResponse(new { message = "Target deleted successfully" });
                }
                else
                {
                    return NotFound($"Target with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error deleting target {id}");
                return CreateErrorResponse($"Error deleting target: {ex.Message}");
            }
        }

        /// <summary>
        /// Get target summary for a user
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult> GetTargetSummary([FromQuery] string userLinkedUid, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                if (string.IsNullOrEmpty(userLinkedUid))
                {
                    return BadRequest("UserLinkedUid is required");
                }

                var summary = await _targetBL.GetTargetSummaryAsync(userLinkedUid, year, month);
                return CreateOkApiResponse(summary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting target summary");
                return CreateErrorResponse($"Error retrieving target summary: {ex.Message}");
            }
        }



        /// <summary>
        /// Bulk create targets
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateTargets([FromBody] List<Winit.Modules.Target.Model.Classes.Target> targets)
        {
            try
            {
                if (targets == null || targets.Count == 0)
                {
                    return BadRequest("Target list is required");
                }

                var createdBy = GetCurrentUserName();
                foreach (var target in targets)
                {
                    target.CreatedBy = createdBy;
                    target.ModifiedBy = createdBy;
                }

                var count = await _targetBL.BulkCreateTargetsAsync(targets);
                return CreateOkApiResponse(new { message = $"Successfully created {count} targets", count = count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error bulk creating targets");
                return CreateErrorResponse($"Error creating targets: {ex.Message}");
            }
        }

        /// <summary>
        /// Get targets by employee
        /// </summary>
        [HttpGet("by-employee/{employeeId}")]
        public async Task<ActionResult> GetTargetsByEmployee(string employeeId)
        {
            try
            {
                var filter = new TargetFilter
                {
                    UserLinkedType = "Employee",
                    UserLinkedUid = employeeId
                };
                
                var targets = await _targetBL.GetAllTargetsAsync(filter);
                return CreateOkApiResponse(targets);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error getting targets for employee {employeeId}");
                return CreateErrorResponse($"Error retrieving targets: {ex.Message}");
            }
        }

        /// <summary>
        /// Get targets by customer
        /// </summary>
        [HttpGet("by-customer/{customerId}")]
        public async Task<ActionResult> GetTargetsByCustomer(string customerId)
        {
            try
            {
                var filter = new TargetFilter
                {
                    CustomerLinkedUid = customerId
                };
                
                var targets = await _targetBL.GetAllTargetsAsync(filter);
                return CreateOkApiResponse(targets);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error getting targets for customer {customerId}");
                return CreateErrorResponse($"Error retrieving targets: {ex.Message}");
            }
        }

        private string GetCurrentUserName()
        {
            // Get from session or claims
            return HttpContext.User?.Identity?.Name ?? "SYSTEM";
        }

    }
}