using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreToGroupMappingController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreToGroupMappingBL _storeToGroupMappingBL;
        public StoreToGroupMappingController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreToGroupMappingBL storeToGroupMappingService) 
            : base(serviceProvider)
        {
            _storeToGroupMappingBL = storeToGroupMappingService;
        }
        [HttpPost]
        [Route("CreateStoreToGroupMapping")]
        public async Task<ActionResult> CreateStoreToGroupMapping([FromBody] Winit.Modules.Store.Model.Classes.StoreToGroupMapping storeGroupAttributes)
        {
            try
            {
                storeGroupAttributes.ServerAddTime = DateTime.Now;
                storeGroupAttributes.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeToGroupMappingBL.CreateStoreToGroupMapping(storeGroupAttributes);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Insert Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create storeGroupAttributes details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("SelectAllStoreToGroupMapping")]
        public async Task<ActionResult> SelectAllStoreToGroupMapping([FromBody] PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping> pagedResponseStoreToGroupMappingList = null;
                pagedResponseStoreToGroupMappingList = await _storeToGroupMappingBL.SelectAllStoreToGroupMapping(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreToGroupMappingList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreToGroupMappingList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreToGroupMapping  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreToGroupMappingByUID")]
        public async Task<ActionResult> SelectStoreToGroupMappingByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping storeToGroupMapping = await _storeToGroupMappingBL.SelectAllStoreToGroupMappingByStoreUID(UID);
                if (storeToGroupMapping != null)
                {
                    return CreateOkApiResponse(storeToGroupMapping);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreToGroupMappingList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreToGroupMapping")]
        public async Task<ActionResult> UpdateStoreToGroupMapping([FromBody] Winit.Modules.Store.Model.Classes.StoreToGroupMapping StoreToGroupMapping)
        {
            try
            {
                var existingStoreList = await _storeToGroupMappingBL.SelectAllStoreToGroupMappingByStoreUID(StoreToGroupMapping.UID);
                if (existingStoreList != null)
                {
                    StoreToGroupMapping.ServerAddTime = DateTime.Now;
                    StoreToGroupMapping.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeToGroupMappingBL.UpdateStoreToGroupMapping(StoreToGroupMapping);
                    if (retVal > 0)
                    {
                        return CreateOkApiResponse(retVal);
                    }
                    else
                    {
                        throw new Exception("Update Failed");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating StoreToGroupMapping Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreToGroupMapping")]
        public async Task<ActionResult> DeleteStoreToGroupMapping(string UID)
        {
            try
            {
                var retVal = await _storeToGroupMappingBL.DeleteStoreToGroupMapping(UID);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Delete Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectAllChannelMasterData")]
        public async Task<ActionResult> SelectAllChannelMasterData()
        {
            try
            {
                var retVal = await _storeToGroupMappingBL.SelectAllChannelMasterData();
                return CreateOkApiResponse<IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData>>(retVal);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fetching data Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}