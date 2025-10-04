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

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AwayPeriodController : WINITBaseController
    {
        private readonly Winit.Modules.AwayPeriod.BL.Interfaces.IAwayPeriodBL _awayPeriodBL;
        public AwayPeriodController(IServiceProvider serviceProvider, 
            Winit.Modules.AwayPeriod.BL.Interfaces.IAwayPeriodBL awayPeriodBL) 
            : base(serviceProvider)
        {
            _awayPeriodBL = awayPeriodBL;
        }
        [HttpPost]
        [Route("GetAwayPeriodDetails")]
        public async Task<ActionResult> GetAwayPeriodDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> pagedResponseAwayPeriodList = null;
                pagedResponseAwayPeriodList = await _awayPeriodBL.GetAwayPeriodDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseAwayPeriodList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseAwayPeriodList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve AwayPeriodDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAwayPeriodDetailsByUID")]
        public async Task<ActionResult> GetAwayPeriodDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod AwayPeriodDetails = await _awayPeriodBL.GetAwayPeriodDetailsByUID(UID);
                if (AwayPeriodDetails != null)
                {
                    return CreateOkApiResponse(AwayPeriodDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve AwayPeriodDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateAwayPeriodDetails")]
        public async Task<ActionResult> CreateAwayPeriodDetails([FromBody]Winit.Modules.AwayPeriod.Model.Classes.AwayPeriod awayPeriod)
        {
            try
            {
                awayPeriod.ServerAddTime = DateTime.Now;
                awayPeriod.ServerModifiedTime = DateTime.Now;
                var retVal = await _awayPeriodBL.CreateAwayPeriodDetails(awayPeriod);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create AwayPeriod details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateAwayPeriodDetails")]
        public async Task<ActionResult> UpdateAwayPeriodDetails([FromBody] Winit.Modules.AwayPeriod.Model.Classes.AwayPeriod awayPeriod)
        {
            try
            {
                var existingProductDetails = await _awayPeriodBL.GetAwayPeriodDetailsByUID(awayPeriod.UID);
                if (existingProductDetails != null)
                {
                    awayPeriod.ServerModifiedTime = DateTime.Now;
                    var retVal = await _awayPeriodBL.UpdateAwayPeriodDetails(awayPeriod);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating AwayPeriod Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteAwayPeriodDetail")]
        public async Task<ActionResult> DeleteAwayPeriodDetail([FromQuery] string UID)
        {
            try
            {
                var retVal = await _awayPeriodBL.DeleteAwayPeriodDetail(UID);
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