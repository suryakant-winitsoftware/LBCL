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
    public class SKUTemplateLineController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUTemplateLineBL _sKUTemplateLineBL;
        public SKUTemplateLineController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUTemplateLineBL sKUTemplateLineBL) : base(serviceProvider)
        {
            _sKUTemplateLineBL = sKUTemplateLineBL;
        }
        [HttpPost]
        [Route("SelectSKUTemplateLineDetails")]
        public async Task<ActionResult> SelectSKUTemplateLineDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine> PagedResponse = null;

                PagedResponse = await _sKUTemplateLineBL.SelectSKUTemplateLineDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUTemplateLine Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUTemplateLineByUID")]
        public async Task<ActionResult> SelectSKUTemplateLineByUID(string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine = await _sKUTemplateLineBL.SelectSKUTemplateLineByUID(UID);
                if (sKUTemplateLine != null)
                {
                    return CreateOkApiResponse(sKUTemplateLine);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUTemplateLine with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateSKUTemplateLine")]
        public async Task<ActionResult> CreateSKUTemplateLine([FromBody]Winit.Modules.SKU.Model.Classes.SKUTemplateLine sKUTemplateLine)
        {
            try
            {
                var retVal = await _sKUTemplateLineBL.CreateSKUTemplateLine(sKUTemplateLine);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUTemplateLine details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUTemplateLine")]
        public async Task<ActionResult> UpdateAwayPeriodDetails([FromBody] Winit.Modules.SKU.Model.Classes.SKUTemplateLine sKUTemplateLine)
        {
            try
            {
                var existingRec = await _sKUTemplateLineBL.SelectSKUTemplateLineByUID(sKUTemplateLine.UID);
                if (existingRec != null)
                {
                    var retVal = await _sKUTemplateLineBL.UpdateSKUTemplateLine(sKUTemplateLine);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUTemplateLine Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUTemplateLine")]
        public async Task<ActionResult> DeleteSKUTemplateLine([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sKUTemplateLineBL.DeleteSKUTemplateLine(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CUDSKUTemplateAndLine")]
        public async Task<ActionResult> CUDSKUTemplateAndLine([FromBody] Winit.Modules.SKU.Model.Classes.SKUTemplateMaster sKUTemplateMaster)
        {
            try
            {
                var retVal = await _sKUTemplateLineBL.CUDSKUTemplateAndLine(sKUTemplateMaster);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUTemplate and Line details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUTemplateLines")]
        public async Task<ActionResult> DeleteSKUTemplateLines(List<string> UIDs)
        {
            try
            {
                var retVal = await _sKUTemplateLineBL.DeleteSKUTemplateLines(UIDs);
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