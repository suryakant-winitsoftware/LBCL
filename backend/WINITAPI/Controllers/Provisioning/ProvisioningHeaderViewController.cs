using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WINITServices.Interfaces.CacheHandler;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Modules.Provisioning.Model.Interfaces;
using System.Collections.Generic;
using Google.Api.Gax.Grpc;
using WINITAPI.Controllers.SKU;
using Winit.Modules.Provisioning.Model.Classes;
using System.Linq;

namespace WINITAPI.Controllers.Provisioning
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProvisioningHeaderViewController : WINITBaseController
    {
        private readonly Winit.Modules.Provisioning.BL.Interfaces.IProvisioningHeaderViewBL _provisioningHeaderViewBL;
        public ProvisioningHeaderViewController(IServiceProvider serviceProvider, 
            Winit.Modules.Provisioning.BL.Interfaces.IProvisioningHeaderViewBL provisioningHeaderViewBL) 
            : base(serviceProvider)
        {
            _provisioningHeaderViewBL = provisioningHeaderViewBL;
        }
        [HttpGet]
        [Route("GetProvisioningHeaderViewByUID/{UID}")]
        public async Task<ActionResult> GetProvisioningHeaderViewByUID(string UID)
        {
            try
            {
                Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView provisioningHeaderView = await _provisioningHeaderViewBL.GetProvisioningHeaderViewByUID(UID);
                if (provisioningHeaderView != null)
                {
                    return CreateOkApiResponse(provisioningHeaderView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve provisioningHeaderView with Code: {@code}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("SelectProvisionByUID")]
        public async Task<ActionResult> SelectProvisionByUID(string UID)
        {
            try
            {
                Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval provisioningHeaderView = await _provisioningHeaderViewBL.SelectProvisionByUID(UID);
                if (provisioningHeaderView != null)
                {
                    return CreateOkApiResponse(provisioningHeaderView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Provision Date with Code: {@code}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("InsertProvisionRequestHistory")]
        public async Task<ActionResult> InsertProvisionRequestHistory([FromBody] List<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval> provisionApproval)
        {
            try
            {
                int retValue = await _provisioningHeaderViewBL.InsertProvisionRequestHistory(provisionApproval);
                if (retValue > 0)
                {
                    return CreateOkApiResponse(retValue);
                }
                else
                {
                    throw new Exception("Insert Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Create ProvisionRequestHistory details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectProvisionRequestHistoryByProvisionIds")]
        public async Task<ActionResult> SelectProvisionRequestHistoryByProvisionIds([FromBody]List<string> ProvisionIds)
        {
            try
            {
                List<IProvisionApproval> provisioningHeaderView = await _provisioningHeaderViewBL.SelectProvisionRequestHistoryByProvisionIds(ProvisionIds);
                if (provisioningHeaderView != null)
                {
                    return CreateOkApiResponse(provisioningHeaderView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Provision Date with Code: {@code}", ProvisionIds);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        //Provision Approval

        [HttpPost]
        [Route("GetProvisionApprovalSummary")]
        public async Task<ActionResult> GetProvisionApprovalSummary(PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<IProvisionApproval> provisionSummary = await _provisioningHeaderViewBL.GetProvisionApprovalSummaryDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (provisionSummary == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(provisionSummary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve provisioningHeaderView with Code: {@code}");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("GetProvisionApprovalDetail")]
        public async Task<ActionResult> GetProvisionApprovalDetail(PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<IProvisionApproval> provisionDetail = await _provisioningHeaderViewBL.GetProvisionApprovalDetailViewDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (provisionDetail == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(provisionDetail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve provisioningHeaderView with Code: {@code}");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("GetProvisionRequestHistoryDetails")]
        public async Task<ActionResult> GetProvisionRequestHistoryDetails(PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<IProvisionApproval> provisionDetail = await _provisioningHeaderViewBL.GetProvisionRequestHistoryDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (provisionDetail == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(provisionDetail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ProvisionRequestHistory with Code: {@code}");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPut]
        [Route("UpdateProvisionData")]
        public async Task<ActionResult<int>> UpdateProvisionData([FromBody] List<string> provisionData)
        {
            try
            {
                int retVal = await _provisioningHeaderViewBL.UpdateProvisionData(provisionData);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Update Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Provisioning Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
