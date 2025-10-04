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
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Mobile
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppVersionController : WINITBaseController
    {
        private readonly Winit.Modules.Mobile.BL.Interfaces.IAppVersionUserBL _appVersionUserBL;
        public AppVersionController(IServiceProvider serviceProvider, 
            Winit.Modules.Mobile.BL.Interfaces.IAppVersionUserBL appVersionUserBL) 
            : base(serviceProvider)
        {
            _appVersionUserBL = appVersionUserBL;
        }
        [HttpPost]
        [Route("GetAppVersionDetails")]
        public async Task<ActionResult> GetAppVersionDetails(PagingRequest pagingRequest,String OrgUID)
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
                PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> PagedResponse = null;
                PagedResponse = await _appVersionUserBL.GetAppVersionDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, OrgUID);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve AppVersionUser Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAppVersionDetailsByUID")]
        public async Task<ActionResult> GetAppVersionDetailsByUID(String UID)
        {
            try
            {
                Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser appVersionUser = null;
                appVersionUser = await _appVersionUserBL.GetAppVersionDetailsByUID(UID);
                if (appVersionUser == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(appVersionUser);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve AppVersionDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetAppVersionDetailsByEmpUID")]
        public async Task<ActionResult> GetAppVersionDetailsByEmpUID(string empUID)
        {
            try
            {
                Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser appVersionUser = null;
                appVersionUser = await _appVersionUserBL.GetAppVersionDetailsByEmpUID(empUID);
                if (appVersionUser == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(appVersionUser);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve AppVersionDetails with UID: {@UID}", empUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("UpdateAppVersionDetails")]
        public async Task<ActionResult> UpdateAppVersionDetails([FromBody] Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {
            try
            {
                var existingDetails = await _appVersionUserBL.GetAppVersionDetailsByUID(appVersionUser.UID);
                if (existingDetails != null)
                {
                    var retVal = await _appVersionUserBL.UpdateAppVersionDetails(appVersionUser);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to  update the Operations");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        /// <summary>
        /// Inserts a new app version user record for device registration
        /// </summary>
        /// <param name="appVersionUser">App version user data to insert</param>
        /// <returns>Success result if insert was successful</returns>
        [HttpPost]
        [Route("InsertAppVersionUser")]
        public async Task<ActionResult> InsertAppVersionUser([FromBody] Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {
            try
            {
                if (appVersionUser == null)
                {
                    return BadRequest("Invalid app version user data");
                }

                if (string.IsNullOrEmpty(appVersionUser.EmpUID))
                {
                    return BadRequest("Employee UID is required");
                }

                if (string.IsNullOrEmpty(appVersionUser.DeviceId))
                {
                    return BadRequest("Device ID is required");
                }

                // Check if user already exists to prevent duplicates
                var existingUser = await _appVersionUserBL.GetAppVersionDetailsByEmpUID(appVersionUser.EmpUID);
                if (existingUser != null)
                {
                    return Conflict("App version user already exists for this employee");
                }

                // Set default values for required fields
                if (string.IsNullOrEmpty(appVersionUser.UID))
                {
                    appVersionUser.UID = Guid.NewGuid().ToString();
                }

                if (appVersionUser.CreatedTime == default)
                {
                    appVersionUser.CreatedTime = DateTime.UtcNow;
                }

                if (string.IsNullOrEmpty(appVersionUser.CreatedBy))
                {
                    appVersionUser.CreatedBy = "System";
                }

                var retVal = await _appVersionUserBL.InsertAppVersionUser(appVersionUser);
                
                if (retVal > 0)
                {
                    return CreateOkApiResponse(true);
                }
                else
                {
                    return CreateErrorResponse("Failed to insert app version user record");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to insert app version user with EmpUID: {@EmpUID}", appVersionUser?.EmpUID);
                return CreateErrorResponse("An error occurred while processing the request: " + ex.Message);
            }
        }

    }
}