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
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SellOutSchemeHeaderController : WINITBaseController
    {

        private readonly ISellOutSchemeHeaderBL _sellOutSchemeHeaderBL;

        public SellOutSchemeHeaderController(IServiceProvider serviceProvider, 
            ISellOutSchemeHeaderBL sellOutSchemeHeaderBL) : base(serviceProvider)
        {
            _sellOutSchemeHeaderBL = sellOutSchemeHeaderBL;
        }

        [HttpPost]
        [Route("SelectAllSellOutSchemeHeader")]
        public async Task<ActionResult> SelectAllSellOutSchemeHeader(PagingRequest pagingRequest)
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

                var pagedResponse = await _sellOutSchemeHeaderBL.SelectAllSellOutSchemeHeader(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Failed to retrieve SelectAllSellOutSchemeHeader");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSellOutSchemeHeaderByUID")]
        public async Task<ActionResult> GetSellOutSchemeHeaderByUID(string UID)
        {
            try
            {
                var details = await _sellOutSchemeHeaderBL.GetSellOutSchemeHeaderByUID(UID);
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

        [HttpPost]
        [Route("CreateSellOutSchemeHeader")]
        public async Task<ActionResult> CreateSellOutSchemeHeader([FromBody] SellOutSchemeHeader sellOutSchemeHeader)
        {
            try
            {
                sellOutSchemeHeader.ServerAddTime = DateTime.Now;
                sellOutSchemeHeader.ServerModifiedTime = DateTime.Now;
                var retVal = await _sellOutSchemeHeaderBL.CreateSellOutSchemeHeader(sellOutSchemeHeader);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SellOutSchemeHeader ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateSellOutSchemeHeader")]
        public async Task<ActionResult> UpdateSellOutSchemeHeader([FromBody] SellOutSchemeHeader sellOutSchemeHeader)
        {
            try
            {
                var existing = await _sellOutSchemeHeaderBL.GetSellOutSchemeHeaderByUID(sellOutSchemeHeader.UID);
                if (existing != null)
                {
                    sellOutSchemeHeader.ServerModifiedTime = DateTime.Now;
                    var retVal = await _sellOutSchemeHeaderBL.UpdateSellOutSchemeHeader(sellOutSchemeHeader);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SellOutSchemeHeader ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSellOutSchemeHeader")]
        public async Task<ActionResult> DeleteSellOutSchemeHeader([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sellOutSchemeHeaderBL.DeleteSellOutSchemeHeader(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost("CrudSellOutMaster")]
        public async Task<IActionResult> CrudSellOutMaster([FromBody] ISellOutMasterScheme sellOutMasterScheme)
        {
            try
            {
                if (sellOutMasterScheme == null)
                {
                    return BadRequest();
                }
                return await _sellOutSchemeHeaderBL.CrudSellOutMaster(sellOutMasterScheme)
                    ? CreateOkApiResponse("Created Successfully")
                    : (IActionResult)CreateErrorResponse("Fail to Create PurchaseOrderMaster");
            }
            catch (Exception ex)
            {

                Log.Error(ex, "Fail to Create PurchaseOrderMaster");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet("GetSellOutMasterByUID")]
        public async Task<IActionResult> GetSellOutMasterByUID([FromQuery] string UID)
        {
            try
            {
                if (string.IsNullOrEmpty(UID))
                {
                    return BadRequest();
                }
                ISellOutMasterScheme sellOutMasterScheme = await _sellOutSchemeHeaderBL.GetSellOutMasterByUID(UID);
                if (sellOutMasterScheme == null)
                {
                    _ = CreateErrorResponse("Error occured while retiving the order master");
                }
                return CreateOkApiResponse(sellOutMasterScheme);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to Retrive PurchaseOrderMaster");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}