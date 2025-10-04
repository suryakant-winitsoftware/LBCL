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
    public class SKUTemplateController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUTemplateBL _sKUTemplateBL;
        public SKUTemplateController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUTemplateBL sKUTemplateBL) : base(serviceProvider)
        {
            _sKUTemplateBL = sKUTemplateBL;
        }
        [HttpPost]
        [Route("SelectAllSKUTemplateDetails")]
        public async Task<ActionResult> SelectAllSKUTemplateDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate> PagedResponse = null;
                PagedResponse = await _sKUTemplateBL.SelectAllSKUTemplateDetails(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Fail to retrieve SKUTemplate Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUTemplateByUID")]
        public async Task<ActionResult> SelectSKUTemplateByUID(string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate = await _sKUTemplateBL.SelectSKUTemplateByUID(UID);
                if (sKUTemplate != null)
                {
                    return CreateOkApiResponse(sKUTemplate);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUTemplate with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateSKUTemplate")]
        public async Task<ActionResult> CreateSKUTemplateLine([FromBody]Winit.Modules.SKU.Model.Classes.SKUTemplate sKUTemplate)
        {
            try
            {
                var retVal = await _sKUTemplateBL.CreateSKUTemplate(sKUTemplate);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUTemplates details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUTemplate")]
        public async Task<ActionResult> UpdateSKUTemplate([FromBody] Winit.Modules.SKU.Model.Classes.SKUTemplate sKUTemplate)
        {
            try
            {
                var existingRec = await _sKUTemplateBL.SelectSKUTemplateByUID(sKUTemplate.UID);
                if (existingRec != null)
                {
                    var retVal = await _sKUTemplateBL.UpdateSKUTemplate(sKUTemplate);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUTemplate Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUTemplate")]
        public async Task<ActionResult> DeleteSKUTemplate([FromQuery] string UID)
        {
            try
            {
                var retVal = await _sKUTemplateBL.DeleteSKUTemplate(UID);
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