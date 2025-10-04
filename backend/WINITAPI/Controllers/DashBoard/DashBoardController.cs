using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Modules.Distributor.BL.Interfaces;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Modules.Emp.BL.Interfaces;
using System.Collections.Generic;
using Winit.Modules.Address.BL.Interfaces;
using Winit.Modules.Contact.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.StoreDocument.BL.Interfaces;
using Winit.Shared.Models.Enums;
using System.Linq;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Serilog;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
using WINITAPI.Common;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Distributor.Model.Interfaces;
using Winit.Modules.JobPosition.BL.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Shared.CommonUtilities.Common;

namespace WINITAPI.Controllers.DashBoard
{

    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class DashBoardController : WINITBaseController
    {
        IDashBoardBL _dashBoardBL;

        public DashBoardController(IServiceProvider serviceProvider,
            IDashBoardBL dashBoardBL) : base(serviceProvider)
        {
            _dashBoardBL = dashBoardBL;
        }

        [HttpGet]
        [Route("GetSalesPerformance")]
        public async Task<ActionResult> CreateDistributor([FromQuery] int month, [FromQuery] int year,
            [FromQuery] int count)
        {
            try
            {
                var result = await _dashBoardBL.GetSalesPerformance(month, year, count);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch DashBoard details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTopChanelPartners")]
        public async Task<ActionResult> GetTopChanelPartners([FromQuery] int LastYear, [FromQuery] int CurrentYear,
            [FromQuery] int count)
        {
            try
            {
                var result = await _dashBoardBL.GetTopChanelPartners(LastYear, CurrentYear, count);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Channel Partner details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetTopChanelPartnersByCategoryAndGroup")]
        public async Task<ActionResult> GetTopChanelPartnersByCategoryAndGroup([FromBody] CategoryWiseTopChhannelPartnersRequest request)
        {
            try
            {
                var result = await _dashBoardBL.GetTopChanelPartnersByCategoryAndGroup(request);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Channel Partner details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCategorySalesPerformance")]
        public async Task<ActionResult> GetCategorySalesPerformance([FromQuery] int month, [FromQuery] int year,
            [FromQuery] int count)
        {
            try
            {
                var result = await _dashBoardBL.GetCategorySalesPerformance(month, year, count);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Category details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetGrowthVsDeGrowth")]
        public async Task<ActionResult> GetGrowthVsDeGrowth([FromQuery] int LastYear, [FromQuery] int CurrentYear,
            [FromQuery] int count)
        {
            try
            {
                var result = await _dashBoardBL.GetGrowthVsDeGrowth(LastYear, CurrentYear, count);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Category details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpGet]
        [Route("GetTargetVsAchievement")]
        public async Task<ActionResult> GetTargetVsAchievement([FromQuery] int Year, [FromQuery] int Month,
            [FromQuery] int count)
        {
            try
            {
                var result = await _dashBoardBL.GetTargetVsAchievement(Year, Month, count);
                return CreateOkApiResponse(result);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Category details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetBranchSalesReport")]
        public async Task<ActionResult> GetBranchSalesReport([FromBody] PagingRequest pagingRequest, [FromQuery] bool isForExport = false)
        {
            try
            {
                var result = await _dashBoardBL.GetBranchSalesReport(pagingRequest.SortCriterias, pageNumber: pagingRequest.PageNumber,
                    pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired,isForExport);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Category details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetAsmViewByBranchCode")]
        public async Task<IActionResult> GetAsmViewByBranchCode([FromQuery] string branchCode)
        {
            try
            {
                string str = CommonFunctions.GetDecodedStringValue(branchCode);
                var result = await _dashBoardBL.GetAsmViewByBranchCode(branchCode);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch Asm view details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetOrgViewByBranchCode")]
        public async Task<IActionResult> GetOrgViewByBranchCode([FromQuery] string branchCode)
        {
            try
            {
                string str = CommonFunctions.GetDecodedStringValue(branchCode);
                var result = await _dashBoardBL.GetOrgViewByBranchCode(branchCode);
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch org view details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}