using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Linq;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Route
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class RouteUserController:WINITBaseController
    {
        private readonly Winit.Modules.Route.BL.Interfaces.IRouteUserBL _routeUserBL;

        public RouteUserController(IServiceProvider serviceProvider, 
            Winit.Modules.Route.BL.Interfaces.IRouteUserBL routeUserBL) : base(serviceProvider)
        {
            _routeUserBL = routeUserBL;
        }
        [HttpPost]
        [Route("SelectAllRouteUserDetails")]
        public async Task<ActionResult> SelectAllRouteUserDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Route .Model.Interfaces.IRouteUser> pagedResponseRouteUserList = null;
                pagedResponseRouteUserList = await _routeUserBL.SelectAllRouteUserDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseRouteUserList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseRouteUserList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve RouteUser  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectRouteUserByUID")]
        public async Task<ActionResult> SelectRouteUserByUID([FromBody] List<string> UIDs)
        {
            try
            {
             IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>routeUser = await _routeUserBL.SelectRouteUserByUID(UIDs);
                if (routeUser != null)
                {
                    return CreateOkApiResponse(routeUser);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RouteUserList with UID: {@UID}", UIDs);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateRouteUser")]
        public async Task<ActionResult> CreateRouteUser([FromBody] List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            try
            {
                var retVal = await _routeUserBL.CreateRouteUser(routeUserList);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create RouteUser details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateRouteUser")]
        public async Task<ActionResult> UpdateRouteUser([FromBody] List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            try
            {
              if(routeUserList !=null && routeUserList.Count > 0)
                {
                    var retVal = await _routeUserBL.UpdateRouteUser(routeUserList);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                
                {
                    return CreateErrorResponse("Internal server Error");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating RouteUser Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpDelete]
        [Route("DeleteRouteUser")]
        public async Task<ActionResult> DeleteRouteUser([FromBody] List<string> UIDs)
        {
            try
            {
                var retVal = await _routeUserBL.DeleteRouteUser(UIDs);
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
