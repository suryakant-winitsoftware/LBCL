using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model.Classes;

namespace WINITAPI.Controllers.Syncing
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class MobileDataSyncController : WINITBaseController
    {
        private readonly Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL _mobileDataSyncBL;
        public MobileDataSyncController(IServiceProvider serviceProvider, 
            Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL mobileDataSyncBL) : base(serviceProvider)
        {
            _mobileDataSyncBL = mobileDataSyncBL;
        }
        [HttpPost]
        [Route("DownloadServerDataInSqlite")]
        public async Task<ActionResult> DownloadServerDataInSqlite(SyncRequest syncRequest)
        {
            try
            {
                string filePath = await _mobileDataSyncBL.DownloadServerDataInSqlite(syncRequest.GroupName, syncRequest.TableName, syncRequest.EmployeeUID, syncRequest.JobPositionUID, syncRequest.RoleUID,  syncRequest.VehicleUID, syncRequest.OrgUID, null);
                if (!string.IsNullOrEmpty(filePath))
                {
                    return CreateOkApiResponse(filePath);
                }
                else
                {
                    return NotFound("Not able to generate sqlite path");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"Failed to download Sqlite with employeeUID: {@employeeUID}, 
                                jobPositionUID: {@jobPositionUID}, roleUID: {@roleUID}, vehicleUID: {@vehicleUID}",
                                syncRequest.EmployeeUID, syncRequest.JobPositionUID, syncRequest.RoleUID, syncRequest.VehicleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("DownloadServerDataInSync")]
        public async Task<ActionResult> DownloadServerDataInSync(SyncRequest syncRequest)
        {
            try
            {
                DateTime currentServerTime = DateTime.Now;
                Dictionary<string, object> syncData = await _mobileDataSyncBL.DownloadServerDataInSync(syncRequest.GroupName, syncRequest.TableName, syncRequest.EmployeeUID, syncRequest.JobPositionUID, syncRequest.RoleUID, syncRequest.VehicleUID, syncRequest.OrgUID, syncRequest.EmployeeCode, syncRequest.LastSyncTime);
                if (syncData != null)
                {
                    return CreateOkApiResponse(syncData, currentServerTime);
                }
                else
                {
                    return NotFound("No data found in sync");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"Failed to Sync with employeeUID: {@employeeUID}, 
                                jobPositionUID: {@jobPositionUID}, roleUID: {@roleUID}, vehicleUID: {@vehicleUID}",
                                syncRequest.EmployeeUID, syncRequest.JobPositionUID, syncRequest.RoleUID, syncRequest.VehicleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
