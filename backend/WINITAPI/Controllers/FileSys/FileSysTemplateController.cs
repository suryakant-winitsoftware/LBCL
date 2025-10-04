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




namespace WINITAPI.Controllers.FileSysTemplate
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileSysTemplateController:WINITBaseController
    {
        private readonly Winit.Modules.FileSys.BL.Interfaces.IFileSysTemplateBL _FileSysTemplateBL;

        public FileSysTemplateController(IServiceProvider serviceProvider,
            Winit.Modules.FileSys.BL.Interfaces.IFileSysTemplateBL FileSysTemplateBL)
            : base(serviceProvider)
        {
            _FileSysTemplateBL = FileSysTemplateBL;
        }
        [HttpGet]
        [Route("GetFileSysTemplateDetails")]
        public async Task<ActionResult> SelectAllFileSysTemplateDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate> PagedResponseFileSysTemplateList = null;
                
                PagedResponseFileSysTemplateList = await _FileSysTemplateBL.SelectAllFileSysTemplateDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseFileSysTemplateList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseFileSysTemplateList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve FileSysTemplateDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetFileSysTemplateByUID")]
        public async Task<ActionResult> GetFileSysTemplateByUID(string UID)
        {
            try
            {
                Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate FileSysTemplateDetails = await _FileSysTemplateBL.GetFileSysTemplateByUID(UID);
                if (FileSysTemplateDetails != null)
                {
                    return CreateOkApiResponse(FileSysTemplateDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve FileSysTemplateDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateFileSysTemplate")]
        public async Task<ActionResult> CreateFileSysTemplate([FromBody] Winit.Modules.FileSys.Model.Classes.FileSysTemplate fileSysTemplate)
        {
            try
            {
                fileSysTemplate.ServerAddTime = DateTime.Now;
                fileSysTemplate.ServerModifiedTime = DateTime.Now;
                var retVal = await _FileSysTemplateBL.CreateFileSysTemplate(fileSysTemplate);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create FileSysTemplate details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateFileSysTemplate")]
        public async Task<ActionResult> UpdateFileSysTemplate([FromBody] Winit.Modules.FileSys.Model.Classes.FileSysTemplate updateFileSysTemplate)
        {
            try
            {
                var existingFileSysTemplateDetails = await _FileSysTemplateBL.GetFileSysTemplateByUID(updateFileSysTemplate.UID);
                if (existingFileSysTemplateDetails != null)
                {
                    updateFileSysTemplate.ServerModifiedTime = DateTime.Now;
                    var retVal = await _FileSysTemplateBL.UpdateFileSysTemplate(updateFileSysTemplate);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating FileSysTemplateDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteFileSysTemplate")]
        public async Task<ActionResult> DeleteFileSysTemplate([FromQuery] string UID)
        {
            try
            {
                var retVal = await _FileSysTemplateBL.DeleteFileSysTemplate(UID);
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
