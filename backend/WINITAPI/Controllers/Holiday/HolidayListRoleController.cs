using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Winit.Shared.Models.Enums;
using Microsoft.AspNetCore.Http;
using Winit.Shared.Models.Constants;
using System.Linq;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Holiday
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HolidayListRoleController:WINITBaseController
    {
        private readonly Winit.Modules.Holiday.BL.Interfaces.IHolidayListRoleBL _holidayListRoleBL;
        public HolidayListRoleController(IServiceProvider serviceProvider, 
            Winit.Modules.Holiday.BL.Interfaces.IHolidayListRoleBL holidayListRoleBL) 
            : base(serviceProvider)
        {
            _holidayListRoleBL = holidayListRoleBL;
        }
        [HttpPost]
        [Route("GetHolidayListRoleDetails")]
        public async Task<ActionResult> GetHolidayListRoleDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> pagedResponseHolidayListRole = null;
                    pagedResponseHolidayListRole = await _holidayListRoleBL.GetHolidayListRoleDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                
                if (pagedResponseHolidayListRole == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseHolidayListRole);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve HolidayListRoleDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetHolidayListRoleByUID")]
        public async Task<ActionResult> GetHolidayListRoleByUID(string UID)
        {
            try
            {
                Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole HolidayListRoleDetails = await _holidayListRoleBL.GetHolidayListRoleByUID(UID);
                if (HolidayListRoleDetails != null)
                {
                    return CreateOkApiResponse(HolidayListRoleDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayListRoleDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateHolidayListRole")]
        public async Task<ActionResult> CreateHolidayListRole([FromBody] Winit.Modules.Holiday.Model.Classes.HolidayListRole CreateHolidayListRole)
        {
            try
            {
                CreateHolidayListRole.ServerAddTime = DateTime.Now;
                CreateHolidayListRole.ServerModifiedTime = DateTime.Now;
                var resultValue = await _holidayListRoleBL.CreateHolidayListRole(CreateHolidayListRole);
                return (resultValue > 0) ? CreateOkApiResponse(resultValue) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create HolidayListRole details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateHolidayListRole")]
        public async Task<ActionResult> UpdateHolidayListRole([FromBody] Winit.Modules.Holiday.Model.Classes.HolidayListRole updateHolidayListRole)
        {
            try
            {
                var existingDetails = await _holidayListRoleBL.GetHolidayListRoleByUID(updateHolidayListRole.UID);
                if (existingDetails != null)
                {
                    updateHolidayListRole.ServerModifiedTime = DateTime.Now;
                    var resultValue = await _holidayListRoleBL.UpdateHolidayListRole(updateHolidayListRole);
                    return (resultValue > 0)? CreateOkApiResponse(resultValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating HolidayListRole Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteHolidayListRole")]
        public async Task<ActionResult> DeleteHolidayListRole([FromQuery] string UID)
        {
            try
            {
                var result = await _holidayListRoleBL.DeleteHolidayListRole(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
