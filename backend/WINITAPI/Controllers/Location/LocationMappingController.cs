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
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationMappingController:WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ILocationMappingBL _LocationMappingBL;

        public LocationMappingController(IServiceProvider serviceProvider, 
            Winit.Modules.Location.BL.Interfaces.ILocationMappingBL LocationMappingBL) 
            : base(serviceProvider)
        {
            _LocationMappingBL = LocationMappingBL;
        }

        [HttpPost]
        [Route("SelectAllLocationMappingDetails")]
        public async Task<ActionResult> SelectAllLocationMappingDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping> PagedResponseLocationMappingList = null;
                PagedResponseLocationMappingList = await _LocationMappingBL.SelectAllLocationMappingDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseLocationMappingList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseLocationMappingList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve LocationMappingDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetLocationMappingByUID")]
        public async Task<ActionResult> GetLocationMappingByUID(string UID)
        {
            try
            {
                Winit.Modules.Location.Model.Interfaces.ILocationMapping LocationMappingDetails = await _LocationMappingBL.GetLocationMappingByUID(UID);
                if (LocationMappingDetails != null)
                {
                    return CreateOkApiResponse(LocationMappingDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationMappingDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateLocationMapping")]
        public async Task<ActionResult> CreateLocationMapping([FromBody] Winit.Modules.Location.Model.Classes.LocationMapping createLocationMapping)
        {
            try
            {
                createLocationMapping.ServerAddTime = DateTime.Now;
                createLocationMapping.ServerModifiedTime = DateTime.Now;
                var retVal = await _LocationMappingBL.CreateLocationMapping(createLocationMapping);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create LocationMapping details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("InsertLocationHierarchy")]
        public async Task<ActionResult> InsertLocationHierarchy(string type, string uid)
        {
            try
            {
                var retVal = await _LocationMappingBL.InsertLocationHierarchy(type, uid);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create LocationHierarchy details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateLocationMappingDetails")]
        public async Task<ActionResult> UpdateLocationMappingDetails([FromBody] Winit.Modules.Location.Model.Classes.LocationMapping updateLocationMapping)
        {
            try
            {
                var existingLocationMappingDetails = await _LocationMappingBL.GetLocationMappingByUID(updateLocationMapping.UID);
                if (existingLocationMappingDetails != null)
                {                  
                    updateLocationMapping.ServerModifiedTime = DateTime.Now;
                    var retVal = await _LocationMappingBL.UpdateLocationMappingDetails(updateLocationMapping);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating LocationMappingDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteLocationMappingDetails")]
        public async Task<ActionResult> DeleteLocationMappingDetails([FromQuery] string UID)
        {
            try
            {

                var retVal = await _LocationMappingBL.DeleteLocationMappingDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetLocationMasterByLocationName")]
        public async Task<ActionResult> GetLocationMaster()
        {
            try
            {
                var retVal = await _LocationMappingBL.GetLocationMaster();
                return (retVal != null) ? CreateOkApiResponse(retVal) : NotFound($"There is no location with ");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fetch Failed");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
