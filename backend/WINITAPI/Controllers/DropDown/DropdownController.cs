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
using Winit.Shared.Models.Common;




namespace WINITAPI.Controllers.Emp
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DropdownController: WINITBaseController
    {
        private readonly Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL _dropDownsBL;

        public DropdownController(IServiceProvider serviceProvider, 
            Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL dropDownsBL) 
            : base(serviceProvider)
        {
            _dropDownsBL = dropDownsBL;
        }

        [HttpPost]
        [Route("GetEmpDropDown")]
        public async Task<ActionResult> GetEmpDropDown(string orgUID, bool getDataByLoginId = false)
        {
            try
            {
                IEnumerable<ISelectionItem>empList = await _dropDownsBL.GetEmpDropDown(orgUID, getDataByLoginId);
                if (empList != null)
                {
                    return CreateOkApiResponse(empList);
                }
                else 
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve EmpDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetRouteDropDown")]
        public async Task<ActionResult> GetRouteDropDown(string orgUID)
        {
            try
            {
                IEnumerable<ISelectionItem> routeList = await _dropDownsBL.GetRouteDropDown(orgUID);
                if (routeList != null)
                {
                    return CreateOkApiResponse(routeList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Route Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetVehicleDropDown")]
        public async Task<ActionResult> GetVehicleDropDown(string parentUID)
        {
            try
            {
                IEnumerable<ISelectionItem> vehicleList = await _dropDownsBL.GetVehicleDropDown(parentUID);
                if (vehicleList != null)
                {
                    return CreateOkApiResponse(vehicleList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Vehicle Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetRequestFromDropDown")]
        public async Task<ActionResult> GetRequestFromDropDown(string parentUID)
        {
            try
            {
                IEnumerable<ISelectionItem> requestFromList = await _dropDownsBL.GetRequestFromDropDown(parentUID);
                if (requestFromList != null)
                {
                    return CreateOkApiResponse(requestFromList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve RequestFrom Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetDistributorDropDown")]
        public async Task<ActionResult> GetDistributorDropDown()
        {
            try
            {
                IEnumerable<ISelectionItem> distributorList = await _dropDownsBL.GetDistributorDropDown();
                if (distributorList != null)
                {
                    return CreateOkApiResponse(distributorList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Distributor Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("GetCustomerDropDown")]
        public async Task<ActionResult> GetCustomerDropDown(string franchiseeOrgUID)
        {
            try
            {
                IEnumerable<ISelectionItem> customerList = await _dropDownsBL.GetCustomerDropDown(franchiseeOrgUID);
                if (customerList != null)
                {
                    return CreateOkApiResponse(customerList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Customer Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetDistributorChannelDropDown")]
        public async Task<ActionResult> GetDistributorChannelDropDown(string parentUID)
        {
            try
            {
                IEnumerable<ISelectionItem> distributorChannelList = await _dropDownsBL.GetDistributorChannelDropDown(parentUID);
                if (distributorChannelList != null)
                {
                    return CreateOkApiResponse(distributorChannelList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve DistributorChannelList Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("GetWareHouseTypeDropDown")]
        public async Task<ActionResult> GetWareHouseTypeDropDown(string parentUID)
        {
            try
            {
                IEnumerable<ISelectionItem> selectionItems = await _dropDownsBL.GetWareHouseTypeDropDown(parentUID);
                if (selectionItems != null)
                {
                    return CreateOkApiResponse(selectionItems);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve WareHouseType Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetCustomersByRouteUIDDropDown")]
        public async Task<ActionResult> GetCustomersByRouteUIDDropDown(string routeUID)
        {
            try
            {
                IEnumerable<ISelectionItem> selectionItems = await _dropDownsBL.GetCustomersByRouteUIDDropDown(routeUID);
                if (selectionItems != null)
                {
                    return CreateOkApiResponse(selectionItems);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Customers Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



    }
}
