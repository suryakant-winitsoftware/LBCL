using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Winit.Shared.Models.Common;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;

namespace WINITAPI.Controllers.Route
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class RouteLoadTruckTemplateController : WINITBaseController
    {
        private readonly Winit.Modules.Route.BL.Interfaces.IRouteLoadTruckTemplateBL _routeLoadTruckTemplateBL;

        public RouteLoadTruckTemplateController(IServiceProvider serviceProvider, 
            Winit.Modules.Route.BL.Interfaces.IRouteLoadTruckTemplateBL routeLoadTruckTemplateBL) 
            : base(serviceProvider)
        {
            _routeLoadTruckTemplateBL = routeLoadTruckTemplateBL;
        }
        [HttpPost]
        [Route("SelectAllRouteLoadTruckTemplateDetails")]
        public async Task<ActionResult> SelectRouteLoadTruckTemplateAllDetails( PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate> pagedResponseRouteLoadTruckTemplateList = null;
                pagedResponseRouteLoadTruckTemplateList = await _routeLoadTruckTemplateBL.SelectRouteLoadTruckTemplateAllDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseRouteLoadTruckTemplateList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseRouteLoadTruckTemplateList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve RouteLoadTruckTemplate  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("SelectRouteLoadTruckTemplateAndLineByUID")]
        public async Task<ActionResult> SelectRouteLoadTruckTemplateAndLineByUID(string UID)
        {
            try
            {
                IRouteLoadTruckTemplateView RouteLoadTruckTemplateViewDetails = null;
                RouteLoadTruckTemplateViewDetails = await _routeLoadTruckTemplateBL.SelectRouteLoadTruckTemplateAndLineByUID(UID);
                if (RouteLoadTruckTemplateViewDetails != null)
                {
                    return CreateOkApiResponse(RouteLoadTruckTemplateViewDetails);
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


        [HttpPost]
        [Route("CreateRouteLoadTruckTemplateAndLine")]
        public async Task<ActionResult> CreateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
        {
            try
            {
                int retVal = await _routeLoadTruckTemplateBL.CreateRouteLoadTruckTemplateAndLine(routeLoadTruckTemplateViewDTO);
                if (retVal < 0)
                {
                    throw new Exception("Insert failed");
                }
                return CreateOkApiResponse(retVal);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateRouteLoadTruckTemplateAndLine")]
        public async Task<ActionResult> UpdateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
        {
            try
            {
                int retVal = await _routeLoadTruckTemplateBL.UpdateRouteLoadTruckTemplateAndLine(routeLoadTruckTemplateViewDTO);
                if (retVal < 0)
                {
                    throw new Exception("Update failed");
                }
                return CreateOkApiResponse(retVal);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteRouteDetail")]
        public async Task<ActionResult> DeleteRouteLoadTruckTemplate([FromQuery] string UID)
        {
            try
            {
                int retVal = await _routeLoadTruckTemplateBL.DeleteRouteLoadTruckTemplate(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteRouteLoadTruckTemplateLine")]
        public async Task<ActionResult> DeleteRouteLoadTruckTemplateLine(List<string> RouteLoadTruckTemplateUIDs)
        {
            try
            {
                int retVal = await _routeLoadTruckTemplateBL.DeleteRouteLoadTruckTemplateLine(RouteLoadTruckTemplateUIDs);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }




    }
}
