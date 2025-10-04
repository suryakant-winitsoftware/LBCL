using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using WINITServices.Interfaces.CacheHandler;
using Serilog;
using Winit.Shared.Models.Common;
namespace WINITAPI.Controllers.Provisioning
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProvisioningLineItemController : WINITBaseController
    {
        private readonly Winit.Modules.Provisioning.BL.Interfaces.IProvisioningItemViewBL _provisioningItemViewBL;
        public ProvisioningLineItemController(IServiceProvider serviceProvider, 
            Winit.Modules.Provisioning.BL.Interfaces.IProvisioningItemViewBL provisioningItemViewBL) 
            : base(serviceProvider)
        {
            _provisioningItemViewBL = provisioningItemViewBL;
        }

        

        [HttpPost]
        [Route("SelectProvisioningLineItemsDetailsByUID/{UID}")]
        public async Task<ActionResult> SelectProvisioningLineItemsDetailsByUID(PagingRequest pagingRequest , string UID)
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
                PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView> pagedResponseList = null;
                pagedResponseList = await _provisioningItemViewBL.SelectProvisioningLineItemsDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired , UID);
                if (pagedResponseList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Provisioning Items. ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetProvisioningLineItemDetailsByUID/{UID}")]
        public async Task<ActionResult> GetProvisioningHeaderViewByUID(string UID)
        {
            try
            {
                Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView provisioningLineItemView = await _provisioningItemViewBL.GetProvisioningLineItemDetailsByUID(UID);
                if (provisioningLineItemView != null)
                {
                    return CreateOkApiResponse(provisioningLineItemView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve provisioningLine Items with Code: {@code}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
    }
}
