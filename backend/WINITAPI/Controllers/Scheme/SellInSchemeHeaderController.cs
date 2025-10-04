using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Scheme.Model.Interfaces;
using WinIt.Models.Customers;
using Winit.Modules.SKU.Model.Classes;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SellInSchemeHeaderController : WINITBaseController
    {

        private readonly ISellInSchemeBL _sellInSchemeHeaderBL;

        public SellInSchemeHeaderController(IServiceProvider serviceProvider,
            ISellInSchemeBL sellInSchemeHeaderBL) : base(serviceProvider)
        {
            _sellInSchemeHeaderBL = sellInSchemeHeaderBL;
        }

        [HttpPost]
        [Route("SelectAllSellInHeader")]
        public async Task<ActionResult> SelectAllSellInHeader(PagingRequest pagingRequest)
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

                var pagedResponse = await _sellInSchemeHeaderBL.SelectAllSellInHeader(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SellInSchemeHeaderDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectAllSellInDetails")]
        public async Task<ActionResult> SelectAllSellInDetails(PagingRequest pagingRequest)
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

                var pagedResponse = await _sellInSchemeHeaderBL.SelectAllSellInDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SellInSchemeHeaderDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSellInHeaderByUID")]
        public async Task<ActionResult> GetSellInHeaderByUID(string UID)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetSellInHeaderByUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SellInSchemeHeaderwith UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSellInDetailByUID")]
        public async Task<ActionResult> GetSellInDetailByUID(string UID)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetSellInDetailByUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Sell In Scheme Detail with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSellInHeader")]
        public async Task<ActionResult> CreateSellInHeader([FromBody] SellInSchemeHeader sellInSchemeHeader)
        {
            try
            {
                sellInSchemeHeader.ServerAddTime = DateTime.Now;
                sellInSchemeHeader.ServerModifiedTime = DateTime.Now;
                var retVal = await _sellInSchemeHeaderBL.CreateSellInHeader(sellInSchemeHeader);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SellInSchemeHeader details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUSellInHeader")]
        public async Task<ActionResult> CUSellInHeader([FromBody] ISellInSchemeDTO sellInScheme)
        {
            if (sellInScheme == null || sellInScheme.SellInHeader == null || sellInScheme.SellInSchemeLines == null || sellInScheme.SellInSchemeLines.Count == 0)
            {
                return BadRequest("Data should not be null");
            }
            try
            {
                var retVal = await _sellInSchemeHeaderBL.CUSellInHeader(sellInScheme);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Sell In Scheme Master details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateSellInScheme")]
        public async Task<ActionResult> CreateSellInScheme([FromBody] ISellInSchemeDTO sellInScheme)
        {
            if (sellInScheme == null || sellInScheme.SellInHeader is null || sellInScheme.SellInSchemeLines is null
                || sellInScheme.SellInSchemeLines.Count == 0)
            {
                return BadRequest("Data should not be null");
            }
            try
            {
                var retVal = await _sellInSchemeHeaderBL.CreateSellInScheme(sellInScheme);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Sell In Scheme Master details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSellInMasterByHeaderUID")]
        public async Task<ActionResult> GetSellInMasterByHeaderUID([FromQuery] string UID)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetSellInMasterByHeaderUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Sell In Scheme Master Detail with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateSellInDetail")]
        public async Task<ActionResult> CreateSellInDetail([FromBody] SellInSchemeLine sellInSchemeHeader)
        {
            try
            {
                sellInSchemeHeader.ServerAddTime = DateTime.Now;
                sellInSchemeHeader.ServerModifiedTime = DateTime.Now;
                var retVal = await _sellInSchemeHeaderBL.CreateSellInDetail(sellInSchemeHeader);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SellInSchemeHeader details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateSellInSchemeHeader")]
        public async Task<ActionResult> UpdateSellInSchemeHeader([FromBody] SellInSchemeHeader sellInSchemeHeader)
        {
            try
            {
                var existingDetails = await _sellInSchemeHeaderBL.GetSellInHeaderByUID(sellInSchemeHeader.UID);
                if (existingDetails != null)
                {
                    sellInSchemeHeader.ServerModifiedTime = DateTime.Now;
                    var retVal = await _sellInSchemeHeaderBL.UpdateSellInHeader(sellInSchemeHeader);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SellInSchemeHeader details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSellInDetail")]
        public async Task<ActionResult> UpdateSellInDetail([FromBody] SellInSchemeLine sellInSchemeHeader)
        {
            try
            {
                var existingDetails = await _sellInSchemeHeaderBL.GetSellInHeaderByUID(sellInSchemeHeader.UID);
                if (existingDetails != null)
                {
                    sellInSchemeHeader.ServerModifiedTime = DateTime.Now;
                    var retVal = await _sellInSchemeHeaderBL.UpdateSellInDetail(sellInSchemeHeader);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SellInSchemeHeader details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSellInHeader")]
        public async Task<ActionResult> DeleteSellInHeader([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sellInSchemeHeaderBL.DeleteSellInHeader(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSellInDetail")]
        public async Task<ActionResult> DeleteSellInDetail([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sellInSchemeHeaderBL.DeleteSellInDetail(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSellInSchemeByOrgUid")]
        public async Task<ActionResult> GetSellInSchemeByOrgUid([FromQuery] string OrgUid, [FromQuery] DateTime OrderDate)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetSellInSchemeByOrgUid(OrgUid, OrderDate);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Sell In Scheme Master Detail with OrgUID: {@OrgUid}", OrgUid);
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
        [HttpGet]
        [Route("GetStandingProvisionAmountByChannelPartnerUID")]
        public async Task<ActionResult> GetStandingProvisionAmountByChannelPartnerUID([FromQuery] string channelPartnerUID)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetStandingProvisionAmountByChannelPartnerUID(channelPartnerUID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Standing provision Amount By Channel Partner : {@GetStandingProvisionAmountByChannelPartnerUID}", GetStandingProvisionAmountByChannelPartnerUID);
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }

        [HttpPost("GetSellInSchemesByOrgUidAndSKUUid")]
        public async Task<IActionResult> GetSellInSchemesByOrgUidAndSKUUid([FromQuery] string orgUID, [FromBody] List<string> skus)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetSellInSchemesByOrgUidAndSKUUid(orgUID, skus);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sell in scheme by orguid and skus : {@orgUID}, {@skus}", orgUID, skus);
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
        [HttpGet("GetExistSellInSchemesByPOUid")]
        public async Task<IActionResult> GetExistSellInSchemesByPOUid([FromQuery] string POHeaderUID)
        {
            try
            {
                var details = await _sellInSchemeHeaderBL.GetExistSellInSchemesByPOUid(POHeaderUID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sell in scheme by orguid and skus : {@orgUID}");
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
    }
}
