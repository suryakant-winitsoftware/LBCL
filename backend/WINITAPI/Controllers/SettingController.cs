using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SettingController : WINITBaseController
    {
        private readonly Winit.Modules.Setting.BL.Interfaces.ISettingBL _SettingBL;

        public SettingController(IServiceProvider serviceProvider, 
            Winit.Modules.Setting.BL.Interfaces.ISettingBL settingBL) : base(serviceProvider)
        {
            _SettingBL = settingBL;
        }
        [HttpGet]
        [Route("GetSettingDetails")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting>>> GetSettingDetails([FromQuery] List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        {
            try
            {
                if (pageNumber < 0 || pageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                var cacheKey = WINITSharedObjects.Constants.CacheSettingDetails.ALL_SettingDetails;
                object settingDetails = null;
                settingDetails = _cacheService.Get<object>(cacheKey);
                if (settingDetails != null)
                {
                    return Ok(settingDetails);
                }
                settingDetails = await _SettingBL.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
                if (settingDetails == null)
                {
                    return NotFound();
                }
                else
                {
                    _cacheService.Set(cacheKey, settingDetails, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(settingDetails);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SettingDetails");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve SettingDetails");
            }
        }


        [HttpGet]
        [Route("GetSettingById")]
        public async Task<ActionResult> GetSettingById([FromQuery] int Id)
        {
            try
            {
                Winit.Modules.Setting.Model.Interfaces.ISetting settingDetails = await _SettingBL.GetSettingById(Id);
                if (settingDetails != null)
                {
                    return Ok(settingDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve settingDetails with Id: {@Id}", Id);
                throw;
            }
        }


        [HttpPost]
        [Route("CreateSetting")]
        public async Task<ActionResult<Winit.Modules.Setting.Model.Interfaces.ISetting>> CreateSetting([FromBody] Winit.Modules.Setting.Model.Interfaces.ISetting createSetting)
        {
            try
            {
                createSetting.CreatedTime = DateTime.Now;
                createSetting.ModifiedTime = DateTime.Now;
                createSetting.ServerAddTime = DateTime.Now;
                createSetting.ServerModifiedTime = DateTime.Now;
                var bankDetails = await _SettingBL.CreateSetting(createSetting);
                return Created("", createSetting);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Setting details");
                return StatusCode(500, new { success = false, message = "Error creating Setting Details", error = ex.Message });
            }
             
        }


        [HttpPut]
        [Route("UpdateSetting")]
        public async Task<ActionResult<Winit.Modules.Setting.Model.Interfaces.ISetting>> UpdateSetting([FromBody] Winit.Modules.Setting.Model.Interfaces.ISetting updateSetting)
        {
            try
            {
                var existingSettingDetails = await _SettingBL.GetSettingById(updateSetting.Id);
                if (existingSettingDetails != null)
                {
                    updateSetting.ModifiedTime = DateTime.Now;
                    updateSetting.ServerModifiedTime = DateTime.Now;
                    var updateBankDetails = await _SettingBL.UpdateSetting(updateSetting);
                    return Ok("Update successfully");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Setting Details");
                return StatusCode(500, new { success = false, message = "Error updating SettingDetails", error = ex.Message });
            }

        }


        [HttpDelete]
        [Route("DeleteSetting")]
        public async Task<ActionResult> DeleteSetting([FromQuery] int  Id)
        {
            try
            {

                var result = await _SettingBL.DeleteSetting(Id);
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
    }
}