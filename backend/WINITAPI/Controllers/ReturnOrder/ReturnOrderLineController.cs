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
namespace WINITAPI.Controllers.ReturnOrderLine
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReturnOrderLineController: WINITBaseController
    {
        private readonly Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderLineBL _ReturnOrderLineBL;

        public ReturnOrderLineController(IServiceProvider serviceProvider, 
            Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderLineBL ReturnOrderLineBL) : base(serviceProvider)
        {
            _ReturnOrderLineBL = ReturnOrderLineBL;
        }
        [HttpPost]
        [Route("SelectAllReturnOrderLineDetails")]
        public async Task<ActionResult> SelectAllReturnOrderLineDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> PagedResponseReturnOrderLineList = null;
                PagedResponseReturnOrderLineList = await _ReturnOrderLineBL.SelectAllReturnOrderLineDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseReturnOrderLineList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseReturnOrderLineList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve ReturnOrderLineDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetReturnOrderLineByUID")]
        public async Task<ActionResult> GetReturnOrderLineByUID(string UID)
        {
            try
            {
                Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine ReturnOrderLineDetails = await _ReturnOrderLineBL.SelectReturnOrderLineByUID(UID);
                if (ReturnOrderLineDetails != null)
                {
                    return CreateOkApiResponse(ReturnOrderLineDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve ReturnOrderLineDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }


        [HttpPost]
        [Route("CreateReturnOrderLine")]
        public async Task<ActionResult> CreateReturnOrderLine([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine ReturnOrderLine)
        {
            try
            {
                ReturnOrderLine.ServerAddTime = DateTime.Now;
                ReturnOrderLine.ServerModifiedTime = DateTime.Now;
                var retVal = await _ReturnOrderLineBL.CreateReturnOrderLine(ReturnOrderLine);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ReturnOrderLine details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }


        [HttpPut]
        [Route("UpdateReturnOrderLineDetails")]
        public async Task<ActionResult> UpdateReturnOrderLineDetails([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine updateReturnOrderLine)
        {
            try
            {
                var existingReturnOrderLineDetails = await _ReturnOrderLineBL.SelectReturnOrderLineByUID(updateReturnOrderLine.UID);
                if (existingReturnOrderLineDetails != null)
                {
                    updateReturnOrderLine.ServerModifiedTime = DateTime.Now;
                    var retVal = await _ReturnOrderLineBL.UpdateReturnOrderLine(updateReturnOrderLine);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ReturnOrderLineDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteReturnOrderLineDetails")]
        public async Task<ActionResult> DeleteReturnOrderLineDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _ReturnOrderLineBL.DeleteReturnOrderLine(UID);
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
