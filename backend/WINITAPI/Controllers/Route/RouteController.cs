using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Winit.Shared.Models.Common;
using Winit.Modules.Route.Model.Interfaces;

namespace WINITAPI.Controllers.Route
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class RouteController : WINITBaseController
    {
        private readonly Winit.Modules.Route.BL.Interfaces.IRouteBL _routeBL;
        public RouteController(IServiceProvider serviceProvider, 
            Winit.Modules.Route.BL.Interfaces.IRouteBL routeBL) : base(serviceProvider)
        {
            _routeBL = routeBL;
        }
        [HttpPost]
        [Route("SelectAllRouteDetails")]
        public async Task<ActionResult> SelectRouteAllDetails(
            PagingRequest pagingRequest,string OrgUID)
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
                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute> pagedResponseRouteList = null;
                pagedResponseRouteList = await _routeBL.SelectRouteAllDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, OrgUID);
                if (pagedResponseRouteList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseRouteList);
            }
            catch (OperationCanceledException)
            {
                Log.Warning("[TIMEOUT] SelectAllRouteDetails operation cancelled due to timeout (1 minute exceeded)");
                return CreateErrorResponse("Route data retrieval is taking longer than expected. Please try again or contact support if this persists.");
            }
            catch (TimeoutException ex)
            {
                Log.Warning(ex, "[TIMEOUT] SelectAllRouteDetails database timeout");
                return CreateErrorResponse("Database query timeout. Please try again with more specific filters or contact support.");
            }
            catch (Npgsql.NpgsqlException ex) when (ex.Message.Contains("timeout") || ex.Message.Contains("lock"))
            {
                Log.Warning(ex, "[DATABASE LOCK] SelectAllRouteDetails blocked by database lock");
                return CreateErrorResponse("Route data temporarily unavailable due to system maintenance. Please try again in 1-2 minutes.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Route  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("SelectRouteChangeLogAllDetails")]
        public async Task<ActionResult> SelectRouteChangeLogAllDetails(
           PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog> pagedResponseRouteChangeLogList = null;
                pagedResponseRouteChangeLogList = await _routeBL.SelectRouteChangeLogAllDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseRouteChangeLogList == null)
                {
                    return NotFound();
                }
              
                return CreateOkApiResponse(pagedResponseRouteChangeLogList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Route Change Log  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectRouteDetailByUID")]
        public async Task<ActionResult> SelectRouteDetailByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Route.Model.Interfaces.IRoute route = await _routeBL.SelectRouteDetailByUID(UID);
                if (route != null)
                {
                    return CreateOkApiResponse(route);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RouteList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateRouteDetails")]
        public async Task<ActionResult> CreateRouteDetails([FromBody] Winit.Modules.Route.Model.Classes.Route routedetails)
        {
            try
            {
                routedetails.ServerAddTime = DateTime.Now;
                routedetails.ServerModifiedTime = DateTime.Now;
                var retVal = await _routeBL.CreateRouteDetails(routedetails);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Created failed");
               
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Route details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateRouteDetails")]
        public async Task<ActionResult> UpdateRouteDetails([FromBody] Winit.Modules.Route.Model.Classes.Route routedetails)
        {
            try
            {
                var existingDetails = await _routeBL.SelectRouteDetailByUID(routedetails.UID);
                if (existingDetails != null)
                {
                    routedetails.ServerModifiedTime = DateTime.Now;
                     int  retVal= await _routeBL.UpdateRouteDetails(routedetails);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Route Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpDelete]
        [Route("DeleteRouteDetail")]
        public async Task<ActionResult> DeleteRouteDetail([FromQuery] string UID)
        {
            try
            {
                int retVal = await _routeBL.DeleteRouteDetail(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateRouteMaster")]
        public async Task<ActionResult> CreateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
        {
            // Set API-level timeout to 4 minutes (240 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));

            try
            {
                // Debug logging for incoming data
                Log.Information("[DEBUG API] ========== CreateRouteMaster API Called ==========");
                Log.Information($"[DEBUG API] Route UID: {routeMasterDetails.Route?.UID}");
                Log.Information($"[DEBUG API] RouteSchedule UID: {routeMasterDetails.RouteSchedule?.UID}");
                Log.Information($"[DEBUG API] RouteCustomersList count: {routeMasterDetails.RouteCustomersList?.Count ?? 0}");
                Log.Information($"[DEBUG API] RouteScheduleCustomerMappings count: {routeMasterDetails.RouteScheduleCustomerMappings?.Count ?? 0}");
                
                // Log customer details
                if (routeMasterDetails.RouteCustomersList != null)
                {
                    foreach (var customer in routeMasterDetails.RouteCustomersList)
                    {
                        Log.Information($"[DEBUG API] Customer: {customer.StoreUID}, Frequency: {customer.Frequency ?? "NULL"}");
                    }
                }
                
                // Log schedule customer mappings
                if (routeMasterDetails.RouteScheduleCustomerMappings != null)
                {
                    foreach (var mapping in routeMasterDetails.RouteScheduleCustomerMappings)
                    {
                        Log.Information($"[DEBUG API] Mapping - RouteScheduleUID: {mapping.RouteScheduleUID}, ConfigUID: {mapping.RouteScheduleConfigUID}, CustomerUID: {mapping.CustomerUID}");
                    }
                }
                
                int retVal = await _routeBL.CreateRouteMaster(routeMasterDetails);
                Log.Information($"[DEBUG API] CreateRouteMaster returned: {retVal}");
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert failed");
            }
            catch (OperationCanceledException)
            {
                Log.Warning("[TIMEOUT] CreateRouteMaster operation cancelled due to timeout (4 minutes exceeded)");
                return CreateErrorResponse("Route creation is taking longer than expected. This may be due to database maintenance. Please try again in a few minutes.");
            }
            catch (TimeoutException ex)
            {
                Log.Warning(ex, "[TIMEOUT] CreateRouteMaster database timeout");
                return CreateErrorResponse("Database operation timeout. This may be due to system maintenance. Please try again shortly.");
            }
            catch (Npgsql.NpgsqlException ex) when (ex.Message.Contains("timeout") || ex.Message.Contains("lock"))
            {
                Log.Warning(ex, "[DATABASE LOCK] CreateRouteMaster blocked by database lock");
                return CreateErrorResponse("Route creation temporarily unavailable due to database maintenance. Please try again in 2-3 minutes.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateRouteMaster")]
        public async Task<ActionResult> UpdateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
        {
            try
            {
                int retVal = await _routeBL.UpdateRouteMaster(routeMasterDetails);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        
        [HttpGet]
        [Route("SelectRouteMasterViewByUID")]
        public async Task<ActionResult> SelectRouteMasterViewByUID(string UID)
        {
            try
            {
                IRouteMasterView routeMasterViewDetails = null;
                routeMasterViewDetails = await _routeBL.SelectRouteMasterViewByUID(UID);
                if (routeMasterViewDetails != null)
                {
                    return CreateOkApiResponse(routeMasterViewDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RouteLoadTruckTemplateViewDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetVehicleDDL")]
        public async Task<ActionResult> GetVehicleDDL(string orgUID)
        {
            try
            {
                var response= await _routeBL.GetVehicleDDL(orgUID);
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Dropdowns data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetWareHouseDDL")]
        public async Task<ActionResult> GetWareHouseDDL(string OrgTypeUID, string ParentUID)
        {
            try
            {
                var response = await _routeBL.GetWareHouseDDL(OrgTypeUID, ParentUID);
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Dropdowns data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetUserDDL")]
        public async Task<ActionResult> GetUserDDL(string OrgUID)
        {
            try
            {
                var response = await _routeBL.GetUserDDL(OrgUID);
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Dropdowns data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetRoutesByStoreUID")]
        public async Task<ActionResult> GetRoutesByStoreUID([FromQuery]string OrgUID,string StoreUID=null)
        {
            try
            {
                var response = await _routeBL.GetRoutesByStoreUID(OrgUID, StoreUID);
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Dropdowns data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllRouteScheduleConfigs")]
        public async Task<ActionResult> GetAllRouteScheduleConfigs()
        {
            try
            {
                var response = await _routeBL.GetAllRouteScheduleConfigs();
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Route Schedule Configs");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}
