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
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Winit.Shared.CommonUtilities.Extensions;


namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationTemplateController : WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ILocationTemplateBL _LocationBL;
        public LocationTemplateController(IServiceProvider serviceProvider, 
            Winit.Modules.Location.BL.Interfaces.ILocationTemplateBL LocationBL) 
            : base(serviceProvider)
        {
            _LocationBL = LocationBL;
        }
        [HttpPost]
        [Route("SelectAllLocationTemplates")]
        public async Task<ActionResult> SelectAllLocationTemplates()
        {
            try
            {
                var data = await _LocationBL.SelectAllLocationTemplates();
                if (data == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(data);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve LocationTemplateDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateLocationTemplate")]
        public async Task<ActionResult> CreateLocationTemplate(LocationTemplate locationTemplate)
        {
            try
            {
                int count = await _LocationBL.CreateLocationTemplate(locationTemplate);
                return CreateOkApiResponse(count);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to creative LocationDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("UpdateLocationTemplate")]
        public async Task<ActionResult> UpdateLocationTemplate(LocationTemplate locationTemplate)
        {
            try
            {
                int count = await _LocationBL.UpdateLocationTemplate(locationTemplate);
                return CreateOkApiResponse(count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update LocationDetails ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllLocationTemplateLinesBytemplateUID")]
        public async Task<ActionResult> GetAllLocationTemplateLinesBytemplateUID([FromQuery]string templateUID)
        {
            try
            {
                var locationList = await _LocationBL.SelectAllLocationTemplatesLineBytemplateUID(templateUID);
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
                Log.Error(ex, "Failed to retrieve LocationDetails with Types: {@Types}", string.Join(", ", templateUID));
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateTemplateLine")]
        public async Task<ActionResult> CreateTemplateLine(List<LocationTemplateLine> templateLines)
        {
            try
            {
                var retVal = await _LocationBL.CreateTemplateLine(templateLines);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Location details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDLocationMappingAndLine")]
        public async Task<ActionResult> CUDLocationMappingAndLine(LocationTemplateMaster locationTemplateMaster)
        {
            try
            {
                int retVal = await _LocationBL.CUDLocationMappingAndLine(locationTemplateMaster);
                return CreateOkApiResponse(retVal);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteLocationTemplateLines")]
        public async Task<ActionResult> DeleteLocationTemplateLines(List<string> uIDs)
        {
            try
            {
                int retVal = await _LocationBL.DeleteLocationTemplateLines(uIDs);
                return CreateOkApiResponse(retVal);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
    }
}
