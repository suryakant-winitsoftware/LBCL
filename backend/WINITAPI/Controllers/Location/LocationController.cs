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
    //[Authorize]
    public class LocationController : WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ILocationBL _LocationBL;

        public LocationController(IServiceProvider serviceProvider, 
            Winit.Modules.Location.BL.Interfaces.ILocationBL LocationBL) : base(serviceProvider)
        {
            _LocationBL = LocationBL;
        }
        [HttpPost]
        [Route("SelectAllLocationDetails")]
        public async Task<ActionResult> SelectAllLocationDetails(PagingRequest pagingRequest)
        {

            try
            {
                var hed = Request.Headers;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation> PagedResponseLocationList = null;
                PagedResponseLocationList = await _LocationBL.SelectAllLocationDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseLocationList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseLocationList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve LocationDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetLocationByUID")]
        public async Task<ActionResult> GetLocationByUID(string UID)
        {
            try
            {
                Winit.Modules.Location.Model.Interfaces.ILocation LocationDetails = await _LocationBL.GetLocationByUID(UID);
                if (LocationDetails != null)
                {
                    return CreateOkApiResponse(LocationDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("GetCountryAndRegionByState")]
        public async Task<ActionResult> GetCountryAndRegionByState(string UID, string Type)
        {
            try
            {
                Winit.Modules.Location.Model.Interfaces.ILocation LocationDetails = await _LocationBL.GetCountryAndRegionByState(UID, Type);
                if (LocationDetails != null)
                {
                    return CreateOkApiResponse(LocationDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("GetLocationByTypes")]
        public async Task<ActionResult> GetLocationByTypes(List<string> locationTypes)
        {
            try
            {
                var locationList = await _LocationBL.GetLocationByType(locationTypes);
                if (locationList != null)
                {
                    return CreateOkApiResponse(locationList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationDetails with Types: {@UID}", locationTypes);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }


        [HttpPost]
        [Route("CreateLocation")]
        public async Task<ActionResult> CreateLocation([FromBody] Winit.Modules.Location.Model.Classes.Location location)
        {
            try
            {
                location.ServerAddTime = DateTime.Now;
                location.ServerModifiedTime = DateTime.Now;
                var retVal = await _LocationBL.CreateLocation(location);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Location details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPut]
        [Route("UpdateLocationDetails")]
        public async Task<ActionResult> UpdateLocationDetails([FromBody] Winit.Modules.Location.Model.Classes.Location updateLocation)
        {
            try
            {
                var existingLocationDetails = await _LocationBL.GetLocationByUID(updateLocation.UID);
                if (existingLocationDetails != null)
                {  
                    updateLocation.ServerModifiedTime = DateTime.Now;
                    var retVal = await _LocationBL.UpdateLocationDetails(updateLocation);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating LocationDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        [HttpDelete]
        [Route("DeleteLocationDetails")]
        public async Task<ActionResult> DeleteLocationDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _LocationBL.DeleteLocationDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetCityandLoaclityByUIDs")]
        public async Task<ActionResult> GetCityandLoaclityByUIDs(List<string> uids)
        {
            try
            {
                var locationList = await _LocationBL.GetCityandLoaclityByUIDs(uids);
                if (locationList != null)
                {
                    return CreateOkApiResponse(locationList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationDetails with Types: {@UID}", uids);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
    }
}
