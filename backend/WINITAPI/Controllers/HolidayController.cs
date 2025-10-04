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
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class HolidayController : WINITBaseController
    {
        private readonly WINITServices.Interfaces.IHolidayService _holidayService;

        public HolidayController(IServiceProvider serviceProvider, WINITServices.Interfaces.IHolidayService holidayService) 
            : base(serviceProvider)
        {
            _holidayService = holidayService;
        }


        [HttpGet]
        [Route("GetHolidayDetails")]
        public async Task<ActionResult<IEnumerable<HolidayDetails>>> GetHolidayDetails([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber, int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        {
            var holidayDetails = await _holidayService.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

            if (holidayDetails != null && holidayDetails.Count() > 0)
            {
                foreach (var objholidayList in holidayDetails)
                {
                    var holidayList = await _holidayService.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
                    var holidayListRole = await _holidayService.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);

                    if (holidayList != null)
                    {
                        objholidayList.Holidays = holidayList.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
                    }

                    if (holidayListRole != null)
                    {
                        objholidayList.HolidayListRoleList = holidayListRole.Where(e => e.HolidayListUID == objholidayList.UID).ToList();
                    }
                }
                
            }



            //var result = new
            //{
            //    HolidayDetails = holidayDetails,
            //    Holidays = holidayDetails.SelectMany(h => h.Holidays),
            //    HolidayListRoleList = holidayDetails.SelectMany(h => h.HolidayListRoleList)
            //};

            return Ok(holidayDetails);
        }


        [HttpGet]
        [Route("GetHolidayByOrgUID")]
        public async Task<ActionResult> GetHolidayByOrgUID([FromQuery] string holidayListUID)
        {
            try
            {
                WINITSharedObjects.Models.Holiday HolidayDetails = await _holidayService.GetHolidayByOrgUID(holidayListUID);
                if (HolidayDetails != null)
                {
                    return Ok(HolidayDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayDetails with holidayListUID: {@holidayListUID}", holidayListUID);
                throw;
            }
        }




        [HttpPost]
        [Route("CreateHoliday")]
        public async Task<ActionResult<Holiday>> CreateHoliday([FromBody] Holiday createHoliday)
        {
            try
            {
                createHoliday.UID = Guid.NewGuid().ToString();
                createHoliday.HolidayListUID = Guid.NewGuid().ToString();
                createHoliday.CreatedTime = DateTime.Now;
                createHoliday.ModifiedTime = DateTime.Now;
                createHoliday.ServerAddTime = DateTime.Now;
                createHoliday.ServerModifiedTime = DateTime.Now;
                var holidayDetails = await _holidayService.CreateHoliday(createHoliday);
                return Created("", createHoliday);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Holiday details");
                return StatusCode(500, new { success = false, message = "Error creating Holiday Details", error = ex.Message });
            }

        }


        [HttpPut]
        [Route("UpdateHoliday")]
        public async Task<ActionResult<Holiday>> UpdateHoliday([FromBody] Holiday updateHoliday)
        {
            try
            {
                var existingDetails = await _holidayService.GetHolidayByOrgUID(updateHoliday.HolidayListUID);
                if (existingDetails != null)
                {
                    updateHoliday.ModifiedTime = DateTime.Now;
                    updateHoliday.ServerModifiedTime = DateTime.Now;
                    var updatehOlidayDetails = await _holidayService.UpdateHoliday(updateHoliday);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Holiday Details");
                return StatusCode(500, new { success = false, message = "Error updating Holiday Details", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteHoliday")]
        public async Task<ActionResult> DeleteHoliday([FromQuery] string holidayListUID)
        {
            try
            {

                var result = await _holidayService.DeleteHoliday(holidayListUID);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }



        //HolidayListRole




        [HttpGet]
        [Route("GetHolidayListRoleByHolidayListUID")]
        public async Task<ActionResult> GetHolidayListRoleByHolidayListUID([FromQuery] string holidayListUID)
        {
            try
            {
                WINITSharedObjects.Models.HolidayListRole HolidayListRoleDetails = await _holidayService.GetHolidayListRoleByHolidayListUID(holidayListUID);
                if (HolidayListRoleDetails != null)
                {
                    return Ok(HolidayListRoleDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayListRole with holidayListUID: {@holidayListUID}", holidayListUID);
                throw;
            }
        }




        [HttpPost]
        [Route("CreateHolidayListRole")]
        public async Task<ActionResult<HolidayListRole>> CreateHolidayListRole([FromBody] HolidayListRole CreateHolidayListRole)
        {
            try
            {
                CreateHolidayListRole.UID = Guid.NewGuid().ToString();
                CreateHolidayListRole.HolidayListUID = Guid.NewGuid().ToString();
                CreateHolidayListRole.CreatedTime = DateTime.Now;
                CreateHolidayListRole.ModifiedTime = DateTime.Now;
                CreateHolidayListRole.ServerAddTime = DateTime.Now;
                CreateHolidayListRole.ServerModifiedTime = DateTime.Now;
                var bankDetails = await _holidayService.CreateHolidayListRole(CreateHolidayListRole);
                return Created("", CreateHolidayListRole);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create HolidayListRole details");
                return StatusCode(500, new { success = false, message = "Error creating HolidayListRole Details", error = ex.Message });
            }

        }


        [HttpPut]
        [Route("UpdateHolidayListRole")]
        public async Task<ActionResult<HolidayListRole>> UpdateHolidayListRole([FromBody] HolidayListRole updateHolidayListRole)
        {
            try
            {
                var existingDetails = await _holidayService.GetHolidayListRoleByHolidayListUID(updateHolidayListRole.HolidayListUID);
                if (existingDetails != null)
                {
                    updateHolidayListRole.ModifiedTime = DateTime.Now;
                    updateHolidayListRole.ServerModifiedTime = DateTime.Now;
                    var updatehOlidayDetails = await _holidayService.UpdateHolidayListRole(updateHolidayListRole);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating HolidayListRole Details");
                return StatusCode(500, new { success = false, message = "Error updating HolidayListRole Details", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteHoliday")]
        public async Task<ActionResult> DeleteHolidayListRole([FromQuery] string holidayListUID)
        {
            try
            {

                var result = await _holidayService.DeleteHolidayListRole(holidayListUID);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }






        //HolidayList




        [HttpGet]
        [Route("GetHolidayListByHolidayListUID")]
        public async Task<ActionResult> GetHolidayListByHolidayListUID([FromQuery] string uID)
        {
            try
            {
                WINITSharedObjects.Models.HolidayList HolidayListDetails = await _holidayService.GetHolidayListByHolidayListUID(uID);
                if (HolidayListDetails != null)
                {
                    return Ok(HolidayListDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayList with uID: {@uID}", uID);
                throw;
            }
        }




        [HttpPost]
        [Route("CreateHolidayList")]
        public async Task<ActionResult<HolidayList>> CreateHolidayList([FromBody] HolidayList createHolidayList)
        {
            try
            {
                createHolidayList.UID = Guid.NewGuid().ToString();
                createHolidayList.OrgUID = Guid.NewGuid().ToString();
                createHolidayList.CreatedTime = DateTime.Now;
                createHolidayList.ModifiedTime = DateTime.Now;
                createHolidayList.ServerAddTime = DateTime.Now;
                createHolidayList.ServerModifiedTime = DateTime.Now;
                var bankDetails = await _holidayService.CreateHolidayList(createHolidayList);
                return Created("", createHolidayList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create HolidayList details");
                return StatusCode(500, new { success = false, message = "Error creating HolidayList Details", error = ex.Message });
            }

        }


        [HttpPut]
        [Route("UpdateHolidayList")]
        public async Task<ActionResult<HolidayList>> UpdateHolidayList([FromBody] HolidayList updateHolidayList)
        {
            try
            {
                var existingDetails = await _holidayService.GetHolidayListByHolidayListUID(updateHolidayList.UID);
                if (existingDetails != null)
                {
                    updateHolidayList.ModifiedTime = DateTime.Now;
                    updateHolidayList.ServerModifiedTime = DateTime.Now;
                    var updatehOlidayDetails = await _holidayService.UpdateHolidayList(updateHolidayList);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating HolidayList Details");
                return StatusCode(500, new { success = false, message = "Error updating HolidayList Details", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteHolidayList")]
        public async Task<ActionResult> DeleteHolidayList([FromQuery] string uID)
        {
            try
            {

                var result = await _holidayService.DeleteHolidayList(uID);
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }

        }


        //public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidayDetails([FromQuery] List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        // int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        //{
        //    try
        //    {
        //        var cacheKey = WINITSharedObjects.Constants.CacheSettingsDetails.ALL_SettingsDetails;
        //        object holidayDetails = null;
        //        holidayDetails = _cacheService.Get<object>(cacheKey);
        //        if (holidayDetails != null)
        //        {
        //            return Ok(holidayDetails);
        //        }
        //        holidayDetails = await _holidayService.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //        if (holidayDetails == null)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _cacheService.Set(cacheKey, holidayDetails, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
        //        }
        //        return Ok(holidayDetails);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Fail to retrieve Holiday Details ");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve Holiday Details ");
        //    }
        //}
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