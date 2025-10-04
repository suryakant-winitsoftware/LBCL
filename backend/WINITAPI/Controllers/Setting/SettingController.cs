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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;
using WINITRepository.Interfaces.Customers;


namespace WINITAPI.Controllers.Setting
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingController : WINITBaseController
    {
        private readonly Winit.Modules.Setting.BL.Interfaces.ISettingBL _SettingBL;
        private readonly DataPreparationController _dataPreparationController;
        public SettingController(IServiceProvider serviceProvider, 
            Winit.Modules.Setting.BL.Interfaces.ISettingBL settingBL, 
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _SettingBL = settingBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllSettingDetails")]
        public async Task<ActionResult> SelectAllSettingDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting> pagedResponseSettingList = null;
                pagedResponseSettingList = await _SettingBL.SelectAllSettingDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (pagedResponseSettingList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSettingList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Setting  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSettingByUID")]
        public async Task<ActionResult> GetSettingByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Setting.Model.Interfaces.ISetting setting = await _SettingBL.GetSettingByUID(UID);
                if (setting != null)
                {
                    return CreateOkApiResponse(setting);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SettingList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSetting")]
        public async Task<ActionResult> CreateSetting([FromBody] Winit.Modules.Setting.Model.Classes.Setting createSetting)
        {
            try
            {
                createSetting.ServerAddTime = DateTime.Now;
                createSetting.ServerModifiedTime = DateTime.Now;
                var bankDetails = await _SettingBL.CreateSetting(createSetting);
                _ = _dataPreparationController.PopulateSettings();
                return (bankDetails > 0) ? CreateOkApiResponse(bankDetails) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Setting details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateSetting")]
        public async Task<ActionResult> UpdateSetting([FromBody] Winit.Modules.Setting.Model.Classes.Setting updateSetting)
        {
            try
            {
                var existingSettingDetails = await _SettingBL.GetSettingByUID(updateSetting.UID);
                if (existingSettingDetails != null)
                {
                    updateSetting.ServerModifiedTime = DateTime.Now;
                    var ratValue = await _SettingBL.UpdateSetting(updateSetting);
                    _ = _dataPreparationController.PopulateSettings();
                    return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Setting Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSetting")]
        public async Task<ActionResult> DeleteSetting([FromQuery] string UID)
        {
            try
            {
                var result = await _SettingBL.DeleteSetting(UID);
                _ = _dataPreparationController.PopulateSettings();
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
