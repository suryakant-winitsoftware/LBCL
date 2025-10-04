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
    public class StoreGroupTypeController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL _storeGroupTypeBL;
        public StoreGroupTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL storeGroupTypeBL) : base(serviceProvider)
        {
            _storeGroupTypeBL = storeGroupTypeBL;
        }
        [HttpPost]
        [Route("SelectAllStoreGroupType")]
        public async Task<ActionResult> SelectAllStoreGroupType(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType> pagedResponseStoreGroupTypeList = null;
                pagedResponseStoreGroupTypeList = await _storeGroupTypeBL.SelectAllStoreGroupType(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreGroupTypeList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreGroupTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreGroupType  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreGroupTypeByUID")]
        public async Task<ActionResult> SelectStoreGroupTypeByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType = await _storeGroupTypeBL.SelectStoreGroupTypeByUID(UID);
                if (storeGroupType != null)
                {
                    return CreateOkApiResponse(storeGroupType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreGroupTypeList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreGroupType")]
        public async Task<ActionResult> CreateStoreGroupType([FromBody] Winit.Modules.Store.Model.Classes.StoreGroupType storeGroupType)
        {
            try
            {
                storeGroupType.ServerAddTime = DateTime.Now;
                storeGroupType.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeGroupTypeBL.CreateStoreGroupType(storeGroupType);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                  throw  new Exception("Insert Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreGroupType details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
       
        [HttpPut]
        [Route("UpdateStoreGroupType")]
        public async Task<ActionResult> UpdateStoreGroupType([FromBody] Winit.Modules.Store.Model.Classes.StoreGroupType StoreGroupType)
        {
            try
            {
                var existingStoregrouptypeList = await _storeGroupTypeBL.SelectStoreGroupTypeByUID(StoreGroupType.UID);
                if (existingStoregrouptypeList != null)
                {
                    StoreGroupType.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeGroupTypeBL.UpdateStoreGroupType(StoreGroupType);
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
                Log.Error(ex, "Error updating StoreGroupType Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreGroupType")]
        public async Task<ActionResult> DeleteStoreGroupType(string UID)
        {
            try
            {
                var retVal = await _storeGroupTypeBL.DeleteStoreGroupType(UID);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                  throw  new Exception("Delete Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
    }
}