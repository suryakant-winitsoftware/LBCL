using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKU.Model.Classes;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUToGroupMappingController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUToGroupMappingBL _sKUToGroupMappingBL;

        public SKUToGroupMappingController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUToGroupMappingBL sKUToGroupMappingBL) : base(serviceProvider)
        {
            _sKUToGroupMappingBL = sKUToGroupMappingBL;
        }
        [HttpPost]
        [Route("SelectAllSKUToGroupMappingDetails")]
        public async Task<ActionResult> SelectAllSKUToGroupMappingDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping> pagedResponseSKUToGroupMappingList = null;
                pagedResponseSKUToGroupMappingList = await _sKUToGroupMappingBL.SelectAllSKUToGroupMappingDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (pagedResponseSKUToGroupMappingList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUToGroupMappingList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUToGroupMapping  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUToGroupMappingByUID")]
        public async Task<ActionResult> SelectSKUToGroupMappingByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping sKUToGroupMappingList = await _sKUToGroupMappingBL.SelectSKUToGroupMappingByUID(UID);
                if (sKUToGroupMappingList != null)
                {
                    return CreateOkApiResponse(sKUToGroupMappingList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUToGroupMapping with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUToGroupMapping")]
        public async Task<ActionResult> CreateSKUToGroupMapping([FromBody] Winit.Modules.SKU.Model.Classes.SKUToGroupMapping sKUToGroupMapping)
        {
            try
            {
                sKUToGroupMapping.ServerAddTime = DateTime.Now;
                sKUToGroupMapping.ServerModifiedTime = DateTime.Now;
                var ratValue = await _sKUToGroupMappingBL.CreateSKUToGroupMapping(sKUToGroupMapping);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKU To Group Mapping details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUToGroupMapping")]
        public async Task<ActionResult> UpdateSKUToGroupMapping([FromBody] Winit.Modules.SKU.Model.Classes.SKUToGroupMapping sKUToGroupMapping)
        {
            try
            {
                var existingDetails = await _sKUToGroupMappingBL.SelectSKUToGroupMappingByUID(sKUToGroupMapping.UID);
                if (existingDetails != null)
                {
                    sKUToGroupMapping.ServerModifiedTime = DateTime.Now;
                    var ratValue= await _sKUToGroupMappingBL.UpdateSKUToGroupMapping(sKUToGroupMapping);
                    return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKU To Group Mapping Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUToGroupMappingByUID")]
        public async Task<ActionResult> DeleteSKUToGroupMappingByUID([FromQuery] string UID)
        {
            try
            {
                var result = await _sKUToGroupMappingBL.DeleteSKUToGroupMappingByUID(UID);
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
