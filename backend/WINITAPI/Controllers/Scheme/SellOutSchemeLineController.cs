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
using Winit.Modules.Scheme.DL.Classes;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SellOutSchemeLineController : WINITBaseController
    {

        private readonly ISellOutSchemeLineBL _sellOutSchemeLineBL;
       

        public SellOutSchemeLineController(IServiceProvider serviceProvider, 
            ISellOutSchemeLineBL sellOutSchemeLineBL):base(serviceProvider) 
        {
            _sellOutSchemeLineBL = sellOutSchemeLineBL;
        }

        [HttpPost]
        [Route("SelectAllSellOutSchemeLine")]
        public async Task<ActionResult> SelectAllSellOutSchemeLine(PagingRequest pagingRequest)
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

                var pagedResponse = await _sellOutSchemeLineBL.SelectAllSellOutSchemeLine(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Failed to retrieve SellOutSchemeLines");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSellOutSchemeLineByUID")]
        public async Task<ActionResult> GetSellOutSchemeLineByUID(string UID)
        {
            try
            {
                var details = await _sellOutSchemeLineBL.GetSellOutSchemeLineByUID(UID);
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
                Log.Error(ex, "Failed to retrieve SellOutSchemeLine with UID: {@UID}", UID);
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSellOutSchemeLine")]
        public async Task<ActionResult> CreateSellOutSchemeLine([FromBody] SellOutSchemeLine sellOutSchemeLine)
        {
            try
            {
                sellOutSchemeLine.ServerAddTime = DateTime.Now;
                sellOutSchemeLine.ServerModifiedTime = DateTime.Now;
                var retVal = await _sellOutSchemeLineBL.CreateSellOutSchemeLine(sellOutSchemeLine);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SellOutSchemeLine ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateSellOutSchemeLine")]
        public async Task<ActionResult> UpdateSellOutSchemeLine([FromBody] SellOutSchemeLine sellOutSchemeLine)
        {
            try
            {
                var existing = await _sellOutSchemeLineBL.GetSellOutSchemeLineByUID(sellOutSchemeLine.UID);
                if (existing != null)
                {
                    sellOutSchemeLine.ServerModifiedTime = DateTime.Now;
                    var retVal = await _sellOutSchemeLineBL.UpdateSellOutSchemeLine(sellOutSchemeLine);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SellOutSchemeLine ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSellOutSchemeLine")]
        public async Task<ActionResult> DeleteSellOutSchemeLine([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sellOutSchemeLineBL.DeleteSellOutSchemeLine(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetPreviousOrdersByChannelPartnerUID")]
        public async Task<ActionResult> GetPreviousOrdersByChannelPartnerUID([FromQuery] string UID)
        {
            try
            {
                if (UID == null) return BadRequest();
                var details = await _sellOutSchemeLineBL.GetPreviousOrdersByChannelPartnerUID(UID);
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
                Log.Error(ex, "Failed to retrieve SellOutSchemeHeader with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}