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
using System.Security.Principal;

namespace WINITAPI.Controllers.Route
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class RouteCustomerController : WINITBaseController
    {
        private readonly Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL _routecustomerBL;

        public RouteCustomerController(IServiceProvider serviceProvider, 
            Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL routecustomerBL) : base(serviceProvider)
        {
            _routecustomerBL = routecustomerBL;
        }
        [HttpPost]
        [Route("SelectAllRouteCustomerDetails")]
        public async Task<ActionResult> SelectRouteCustomerAllDetails(
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
                PagedResponse<Winit.Modules.Route .Model.Interfaces.IRouteCustomer> pagedResponseRouteCustomerList = null;
                if (pagedResponseRouteCustomerList != null)
                {
                    return CreateOkApiResponse(pagedResponseRouteCustomerList);
                }
                pagedResponseRouteCustomerList = await _routecustomerBL.SelectRouteCustomerAllDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseRouteCustomerList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseRouteCustomerList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve RouteCustomer  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectRouteCustomerDetailByUID")]
        public async Task<ActionResult> SelectRouteCustomerDetailByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Route.Model.Interfaces.IRouteCustomer routeCustomer = await _routecustomerBL.SelectRouteCustomerDetailByUID(UID);
                if (routeCustomer != null)
                {
                    return CreateOkApiResponse(routeCustomer);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RouteCustomerList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetRouteByStoreUID")]
        public async Task<ActionResult> GetRouteByStoreUID([FromQuery] string storeUID)
        {
            try
            {
                IEnumerable<SelectionItem> routeCustomers = await _routecustomerBL.GetRouteByStoreUID(storeUID);
                if (routeCustomers != null)
                {
                    return CreateOkApiResponse(routeCustomers);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RouteByStoreUID with storeUID: {@UID}", storeUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateRouteCustomerDetails")]
        public async Task<ActionResult> CreateRouteCustomerDetails([FromBody] Winit.Modules.Route.Model.Classes.RouteCustomer routecustomerdetails)
        {
            try
            {
                routecustomerdetails.ServerAddTime = DateTime.Now;
                routecustomerdetails.ServerModifiedTime = DateTime.Now;
                var retVal = await _routecustomerBL.CreateRouteCustomerDetails(routecustomerdetails);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Route Customer details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateRouteCustomerDetails")]
        public async Task<ActionResult> UpdateRouteCustomerDetails([FromBody] Winit.Modules.Route.Model.Classes.RouteCustomer routeCustomerdetails)
        {
            try
            {
                var existingDetails = await _routecustomerBL.SelectRouteCustomerDetailByUID(routeCustomerdetails.UID);
                if (existingDetails != null)
                {
                    routeCustomerdetails.ServerModifiedTime = DateTime.Now;
                    var retVal = await _routecustomerBL.UpdateRouteCustomerDetails(routeCustomerdetails);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Route Customer Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteRouteCustomerDetails")]
        public async Task<ActionResult> DeleteRouteCustomerDetails(List<string> UIDS)
        {
            try
            {
                var retVal = await _routecustomerBL.DeleteRouteCustomerDetails(UIDS);
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
