using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Diagnostics;
using Winit.Shared.Models.Common;


namespace WINITAPI.Controllers.Vehicle
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController:WINITBaseController
    {
        private readonly Winit.Modules.Vehicle.BL.Interfaces.IVehicleBL _VehicleBL;

        public VehicleController(IServiceProvider serviceProvider, 
            Winit.Modules.Vehicle.BL.Interfaces.IVehicleBL VehicleBL) : base(serviceProvider)
        {
            _VehicleBL = VehicleBL;
        }
        [HttpPost]
        [Route("SelectAllVehicleDetails")]
        public async Task<ActionResult> SelectAllVehicleDetails(PagingRequest pagingRequest, [FromQuery] string OrgUID)
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
                PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> PagedResponseVehicleList = null;
                PagedResponseVehicleList = await _VehicleBL.SelectAllVehicleDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, OrgUID);
                if (PagedResponseVehicleList == null)
                {
                    return NotFound();
                }
                
                return CreateOkApiResponse(PagedResponseVehicleList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve VehicleDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetVehicleByUID")]
        public async Task<ActionResult<ApiResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>>> GetVehicleByUID(string UID)
        {
            try
            {
                Winit.Modules.Vehicle.Model.Interfaces.IVehicle VehicleDetails = await _VehicleBL.GetVehicleByUID(UID);
                if (VehicleDetails != null)
                {
                    return CreateOkApiResponse(VehicleDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve VehicleDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateVehicle")]
        public async Task<ActionResult> CreateVehicle([FromBody] Winit.Modules.Vehicle.Model.Classes.Vehicle createVehicle)
        {
            try
            {
                createVehicle.ServerAddTime = DateTime.Now;
                createVehicle.ServerModifiedTime = DateTime.Now;
                var retVal = await _VehicleBL.CreateVehicle(createVehicle);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Vehicle details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPut]
        [Route("UpdateVehicleDetails")]
        public async Task<ActionResult> UpdateVehicleDetails([FromBody] Winit.Modules.Vehicle.Model.Classes.Vehicle updateVehicle)
        {
            try
            {
                var existingVehicleDetails = await _VehicleBL.GetVehicleByUID(updateVehicle.UID);
                if (existingVehicleDetails != null)
                {
                    updateVehicle.ServerModifiedTime = DateTime.Now;
                    var retVal = await _VehicleBL.UpdateVehicleDetails(updateVehicle);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating VehicleDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpDelete]
        [Route("DeleteVehicleDetails")]
        public async Task<ActionResult> DeleteVehicleDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _VehicleBL.DeleteVehicleDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
