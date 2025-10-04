using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WINITServices.Interfaces.CacheHandler;
using Serilog;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Winit.Modules.Tally.Model.Interfaces;

namespace WINITAPI.Controllers.Tally
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TallyMasterController : WINITBaseController
    {
        private readonly Winit.Modules.Tally.BL.Interfaces.ITallyMasterBL _tallyMasterBL;

        public TallyMasterController(IServiceProvider serviceProvider, 
            Winit.Modules.Tally.BL.Interfaces.ITallyMasterBL _TallyMasterBL) : base(serviceProvider)
        {
            _tallyMasterBL = _TallyMasterBL;
        }
        
        [HttpPost]
        [Route("GetDealerMasterByDistByUID/{UID}")]
        public async Task<ActionResult> GetDealerMasterByDistByUID(PagingRequest pagingRequest, string UID)
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
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster> pagedResponseList = null;
                pagedResponseList = await _tallyMasterBL.GetTallyDealerMasterDataByUID(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, UID);
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

        [HttpPost]
        [Route("GetInventoryMasterByDistByUID/{UID}")]
        public async Task<ActionResult> GetInventoryMasterByDistByUID(PagingRequest pagingRequest, string UID)
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
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster> pagedResponseList = null;
                pagedResponseList = await _tallyMasterBL.GetTallyInventoryMasterDataByUID(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, UID);
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

        [HttpPost]
        [Route("GetSalesInvoiceMasterByDistByUID/{UID}")]
        public async Task<ActionResult> GetSalesInvoiceMasterByDistByUID(PagingRequest pagingRequest, string UID)
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
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster> pagedResponseList = null;
                pagedResponseList = await _tallyMasterBL.GetTallySalesInvoiceMasterDataByUID(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, UID);
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

        [HttpPost]
        [Route("GetSalesInvoiceLineMasterByDistByUID/{UID}")]
        public async Task<ActionResult> GetSalesInvoiceLineMasterByDistByUID(PagingRequest pagingRequest, string UID)
        {
            try
            {
                string decodedUid = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(UID));
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster> pagedResponseList = null;
                pagedResponseList = await _tallyMasterBL.GetTallySalesInvoiceLineMasterDataByUID(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, decodedUid);
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
        [Route("GetDealerMasterItemDetails/{UID}")]
        public async Task<ActionResult> GetDealerMasterItemDetails(string UID)
        {
            try
            {
                Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster tallyDealerMaster = await _tallyMasterBL.GetTallyDealerMasterItem(UID);
                if (tallyDealerMaster != null)
                {
                    return CreateOkApiResponse(tallyDealerMaster);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TemporaryCredit with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpGet]
        [Route("GetInventoryMasterItemDetails/{UID}")]
        public async Task<ActionResult> GetInventoryMasterItemDetails(string UID)
        {
            try
            {
                Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster tallyInventoryMaster = await _tallyMasterBL.GetInventoryMasterItem(UID);
                if (tallyInventoryMaster != null)
                {
                    return CreateOkApiResponse(tallyInventoryMaster);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TemporaryCredit with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpGet]
        [Route("GetSalesInvoiceMasterItemDetails/{UID}")]
        public async Task<ActionResult> GetSalesInvoiceMasterItemDetails(string UID)
        {
            try
            {
                Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster tallySalesInvoiceMaster = await _tallyMasterBL.GetSalesInvoiceMasterItem(UID);
                if (tallySalesInvoiceMaster != null)
                {
                    return CreateOkApiResponse(tallySalesInvoiceMaster);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TemporaryCredit with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("GetTallySalesInvoiceData/{UID}")]
        public async Task<ActionResult> GetTallySalesInvoiceData(PagingRequest pagingRequest , string UID)
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
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult> pagedResponseList = null;
                pagedResponseList = await _tallyMasterBL.GetTallySalesInvoiceData(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize,
                    pagingRequest.FilterCriterias, pagingRequest.IsCountRequired, UID);
                
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

        [HttpPut]
        [Route("UpdateTallyMasterData")]
        public async Task<ActionResult> UpdateTallyMasterData([FromBody] List<ITallyDealerMaster> tallyDBDetails)
        {
            try
            {
                bool retValue = false;
                foreach (var data in tallyDBDetails)
                {
                    data.ModifiedTime = DateTime.Now;
                    data.ServerModifiedTime = DateTime.Now;
                    retValue = await _tallyMasterBL.UpdateTallyMasterData(data);
                    if (!retValue)
                    {
                        return StatusCode(500, retValue);
                    }
                }
                return CreateOkApiResponse(retValue);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details " + ex.ToString());
            }
        }
    }
}
