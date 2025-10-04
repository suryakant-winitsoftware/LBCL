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

namespace WINITAPI.Controllers.SKUClass
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUClassController:WINITBaseController
    {
        private readonly Winit.Modules.SKUClass.BL.Interfaces.ISKUClassBL _sKUClassBL;

        public SKUClassController(IServiceProvider serviceProvider, 
            Winit.Modules.SKUClass.BL.Interfaces.ISKUClassBL sKUClassBL) : base(serviceProvider)
        {
            _sKUClassBL = sKUClassBL;
        }
        [HttpPost]
        [Route("SelectAllSKUClassDetails")]
        public async Task<ActionResult> SelectAllSKUClassDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass> pagedResponseSKUClassList = null;
                pagedResponseSKUClassList = await _sKUClassBL.SelectAllSKUClassDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseSKUClassList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUClassList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUClass  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSKUClassByUID")]
        public async Task<ActionResult> GetSKUClassByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKUClass.Model.Interfaces.ISKUClass sKUClass = await _sKUClassBL.GetSKUClassByUID(UID);
                if (sKUClass != null)
                {
                    return CreateOkApiResponse(sKUClass);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUClassList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUClass")]
        public async Task<ActionResult> CreateSKUClass([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClass createSKUClass)
        {
            try
            {
                var now = DateTime.Now;
                createSKUClass.CreatedTime = now;
                createSKUClass.ModifiedTime = now;
                createSKUClass.ServerAddTime = now;
                createSKUClass.ServerModifiedTime = now;
                var retValue = await _sKUClassBL.CreateSKUClass(createSKUClass);
                return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUClass details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUClass")]
        public async Task<ActionResult> UpdateSKUClass([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClass updateSKUClass)
        {
            try
            {
                var existingDetails = await _sKUClassBL.GetSKUClassByUID(updateSKUClass.UID);
                if (existingDetails != null)
                {
                    var now = DateTime.Now;
                    updateSKUClass.ModifiedTime = now;
                    updateSKUClass.ServerModifiedTime = now;
                    // Preserve original created timestamps
                    updateSKUClass.CreatedTime = existingDetails.CreatedTime;
                    updateSKUClass.ServerAddTime = existingDetails.ServerAddTime;
                    var retValue = await _sKUClassBL.UpdateSKUClass(updateSKUClass);
                    return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUClass Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUClass")]
        public async Task<ActionResult> DeleteSKUClass([FromQuery] string UID)
        {
            try
            {
                var result = await _sKUClassBL.DeleteSKUClass(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
