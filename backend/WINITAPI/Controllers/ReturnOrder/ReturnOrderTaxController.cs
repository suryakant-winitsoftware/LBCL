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
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
namespace WINITAPI.Controllers.ReturnOrderTax
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReturnOrderTaxController: WINITBaseController
    {
        private readonly Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderTaxBL _ReturnOrderTaxBL;

        public ReturnOrderTaxController(IServiceProvider serviceProvider, 
            Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderTaxBL ReturnOrderTaxBL) : base(serviceProvider)
        {
            _ReturnOrderTaxBL = ReturnOrderTaxBL;
        }
        [HttpPost]
        [Route("SelectAllReturnOrderTaxDetails")]
        public async Task<ActionResult> SelectAllReturnOrderTaxDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax> PagedResponseReturnOrderTaxList = null;
                PagedResponseReturnOrderTaxList = null;

                if (PagedResponseReturnOrderTaxList != null)
                {
                    return CreateOkApiResponse(PagedResponseReturnOrderTaxList);
                }
                PagedResponseReturnOrderTaxList = await _ReturnOrderTaxBL.SelectAllReturnOrderTaxDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseReturnOrderTaxList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseReturnOrderTaxList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ReturnOrderTaxDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetReturnOrderTaxByUID")]
        public async Task<ActionResult> GetReturnOrderTaxByUID(string UID)
        {
            try
            {
                Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax ReturnOrderTaxDetails = await _ReturnOrderTaxBL.SelectReturnOrderTaxByUID(UID);
                if (ReturnOrderTaxDetails != null)
                {
                    return CreateOkApiResponse(ReturnOrderTaxDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ReturnOrderTaxDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateReturnOrderTax")]
        public async Task<ActionResult> CreateReturnOrderTax([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderTax ReturnOrderTax)
        {
            try
            {
                ReturnOrderTax.ServerAddTime = DateTime.Now;
                ReturnOrderTax.ServerModifiedTime = DateTime.Now;
                var retVal = await _ReturnOrderTaxBL.CreateReturnOrderTax(ReturnOrderTax);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ReturnOrderTax details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateReturnOrderTaxDetails")]
        public async Task<ActionResult> UpdateReturnOrderTaxDetails([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderTax updateReturnOrderTax)
        {
            try
            {
                var existingReturnOrderTaxDetails = await _ReturnOrderTaxBL.SelectReturnOrderTaxByUID(updateReturnOrderTax.UID);
                if (existingReturnOrderTaxDetails != null)
                {
                    updateReturnOrderTax.ServerModifiedTime = DateTime.Now;
                    var retVal = await _ReturnOrderTaxBL.UpdateReturnOrderTax(updateReturnOrderTax);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ReturnOrderTaxDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteReturnOrderTaxDetails")]
        public async Task<ActionResult> DeleteReturnOrderTaxDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _ReturnOrderTaxBL.DeleteReturnOrderTax(UID);
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
