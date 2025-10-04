using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using WINITRepository.Interfaces.Customers;


using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Security.Cryptography;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Holiday
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HolidayController : WINITBaseController
    {
        private readonly Winit.Modules.Holiday.BL.Interfaces.IHolidayBL _holidayBL;
        public HolidayController(IServiceProvider serviceProvider, 
            Winit.Modules.Holiday.BL.Interfaces.IHolidayBL holidayBL) : base(serviceProvider)
        {
            _holidayBL = holidayBL;
        }
        [HttpPost]
        [Route("GetHolidayDetails")]
        public async Task<ActionResult> GetHolidayDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday> pagedResponseHolidayList = null;
                pagedResponseHolidayList = await _holidayBL.GetHolidayDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseHolidayList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseHolidayList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve HolidayDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetHolidayByOrgUID")]
        public async Task<ActionResult> GetHolidayByOrgUID(string UID)
        {
            try
            {
                Winit.Modules.Holiday.Model.Interfaces.IHoliday HolidayDetails = await _holidayBL.GetHolidayByOrgUID(UID);
                if (HolidayDetails != null)
                {
                    return CreateOkApiResponse(HolidayDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateHoliday")]
        public async Task<ActionResult> CreateHoliday([FromBody] Winit.Modules.Holiday.Model.Classes.Holiday createHoliday)
        {
            try
            {
                createHoliday.ServerAddTime = DateTime.Now;
                createHoliday.ServerModifiedTime = DateTime.Now;
                int returnValue = await _holidayBL.CreateHoliday(createHoliday);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Holiday details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateHoliday")]
        public async Task<ActionResult> UpdateHoliday([FromBody] Winit.Modules.Holiday.Model.Classes.Holiday updateHoliday)
        {
            try
            {
                var existingDetails = await _holidayBL.GetHolidayByOrgUID(updateHoliday.UID);
                if (existingDetails != null)
                {
                    updateHoliday.ServerModifiedTime = DateTime.Now;
                    var retValue = await _holidayBL.UpdateHoliday(updateHoliday);
                    return (retValue > 0)? CreateOkApiResponse(retValue) :throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Holiday Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteHoliday")]
        public async Task<ActionResult> DeleteHoliday([FromQuery] string UID)
        {
            try
            {
                var result = await _holidayBL.DeleteHoliday(UID);
                return (result > 0) ?CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
       
        //[HttpGet]
        //[Route("GetHolidayDetails")]
        //public async Task<ActionResult<HolidayDetails>> GetHolidayDetails([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber, int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        //{

        //    var holidayDetails = await _holidayService.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //    if (holidayDetails != null && holidayDetails.Count() > 0)
        //    {
        //        var holidayList = await _holidayService.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //        var holidayListRole = await _holidayService.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

        //        foreach (HolidayDetails objholidayList in holidayDetails)
        //        {
        //            if (holidayList != null)
        //            {
        //                objholidayList.Holidays = holidayList.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
        //            }
        //            if (holidayListRole != null)
        //            {
        //                objholidayList.HolidayListRoleList = holidayListRole.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
        //            }
        //        }
        //    }


        //    return Ok(holidayDetails);
        //}

        //[HttpGet]
        //[Route("GetHolidayDetails")]
        //public async Task<ActionResult<HolidayDetails>> GetHolidayDetails([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber, int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        //{
        //    var holidayDetails = await _holidayService.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

        //    if (holidayDetails != null && holidayDetails.Tables.Count > 0)
        //    {
        //        var holidayTable = holidayDetails.Tables[0];

        //        if (holidayTable != null && holidayTable.Rows.Count > 0)
        //        {
        //            if (!holidayTable.Columns.Contains("Holidays"))
        //            {
        //                holidayTable.Columns.Add("Holidays", typeof(List<Holiday>));
        //            }

        //            if (!holidayTable.Columns.Contains("HolidayListRoleList"))
        //            {
        //                holidayTable.Columns.Add("HolidayListRoleList", typeof(List<HolidayListRole>));
        //            }

        //            foreach (DataRow row in holidayTable.Rows)
        //            {
        //                var uid = row["UID"].ToString();

        //                var holidayList = await _holidayService.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //                if (holidayList != null)
        //                {
        //                    var holidays = holidayList.Where(e => e.HolidayListUID == uid).ToList();
        //                    row["Holidays"] = holidays;
        //                }

        //                var holidayListRole = await _holidayService.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //                if (holidayListRole != null)
        //                {
        //                    var holidayListRoles = holidayListRole.Where(e => e.HolidayListUID == uid).ToList();
        //                    row["HolidayListRoleList"] = holidayListRoles;
        //                }
        //            }
        //        }
        //    }

        //    var jsonOptions = new JsonSerializerOptions
        //    {
        //        ReferenceHandler = ReferenceHandler.Preserve,
        //        MaxDepth = 32
        //    };

        //    return Ok(JsonSerializer.Serialize(holidayDetails, jsonOptions));
        //}

        //[HttpGet]
        //[Route("GetHolidayDetails")]
        //public async Task<ActionResult<HolidayDetails>> GetHolidayDetails([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber, int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        //{

        //    var holidayDetails = await _holidayService.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //    if (holidayDetails != null && holidayDetails.Count() > 0)
        //    {
        //        foreach (var objholidayList in holidayDetails)
        //        {
        //            var holidayList = await _holidayService.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //            var holidayListRole = await _holidayService.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

        //            if (holidayList != null)
        //            {
        //                objholidayList.Holidays = holidayList.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
        //            }

        //            if (holidayListRole != null)
        //            {
        //                objholidayList.HolidayListRoleList = holidayListRole.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
        //            }
        //        }
        //    }

        //    return Ok(holidayDetails);
        //}
    }
}