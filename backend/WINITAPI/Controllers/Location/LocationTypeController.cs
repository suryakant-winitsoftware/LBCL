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
    public class LocationTypeController:WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ILocationTypeBL _LocationTypeBL;

        public LocationTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.Location.BL.Interfaces.ILocationTypeBL LocationTypeBL) 
            : base(serviceProvider)
        {
            _LocationTypeBL = LocationTypeBL;
        }
        [HttpPost]
        [Route("SelectAllLocationTypeDetails")]
        public async Task<ActionResult> SelectAllLocationTypeDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType> PagedResponseLocationTypeList = null;
                PagedResponseLocationTypeList = await _LocationTypeBL.SelectAllLocationTypeDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseLocationTypeList != null)
                {
                    return CreateOkApiResponse(PagedResponseLocationTypeList);
                }
                
                if (PagedResponseLocationTypeList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseLocationTypeList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve LocationTypeDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetLocationTypeByUID")]
        public async Task<ActionResult> GetLocationTypeByUID(string UID)
        {
            try
            {
                Winit.Modules.Location.Model.Interfaces.ILocationType LocationTypeDetails = await _LocationTypeBL.GetLocationTypeByUID(UID);
                if (LocationTypeDetails != null)
                {
                    return CreateOkApiResponse(LocationTypeDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LocationTypeDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateLocationType")]
        public async Task<ActionResult> CreateLocationType([FromBody] Winit.Modules.Location.Model.Classes.LocationType createLocationType)
        {
            try
            {
                createLocationType.ServerAddTime = DateTime.Now;
                createLocationType.ServerModifiedTime = DateTime.Now;
                var retVal = await _LocationTypeBL.CreateLocationType(createLocationType);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create LocationType details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateLocationTypeDetails")]
        public async Task<ActionResult> UpdateLocationTypeDetails([FromBody] Winit.Modules.Location.Model.Classes.LocationType updateLocationType)
        {
            try
            {
                var existingLocationTypeDetails = await _LocationTypeBL.GetLocationTypeByUID(updateLocationType.UID);
                if (existingLocationTypeDetails != null)
                {
                    updateLocationType.ServerModifiedTime = DateTime.Now;
                    var retVal = await _LocationTypeBL.UpdateLocationTypeDetails(updateLocationType);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating LocationTypeDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpDelete]
        [Route("DeleteLocationTypeDetails")]
        public async Task<ActionResult> DeleteLocationTypeDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _LocationTypeBL.DeleteLocationTypeDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
    }
}
