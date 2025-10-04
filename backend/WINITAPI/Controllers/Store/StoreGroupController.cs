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

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreGroupController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreGroupBL _storeGroupBL;
        public StoreGroupController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreGroupBL storeGroupBL) : base(serviceProvider)
        {
            _storeGroupBL = storeGroupBL;
        }
        [HttpPost]
        [Route("SelectAllStoreGroup")]
        public async Task<ActionResult> SelectAllStoreGroup(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroup> pagedResponseStoreGroupList = null;
                pagedResponseStoreGroupList = await _storeGroupBL.SelectAllStoreGroup(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreGroupList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreGroupList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreGroup  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreGroupByUID")]
        public async Task<ActionResult> SelectStoreGroupByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreGroup storeGroup = await _storeGroupBL.SelectStoreGroupByUID(UID);
                if (storeGroup != null)
                {
                    return CreateOkApiResponse(storeGroup);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreGroupList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreGroup")]
        public async Task<ActionResult> CreateStoreGroup([FromBody] Winit.Modules.Store.Model.Classes.StoreGroup storeGroup)
        {
            try
            {
                storeGroup.ServerAddTime = DateTime.Now;
                storeGroup.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeGroupBL.CreateStoreGroup(storeGroup);
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
                Log.Error(ex, "Failed to create StoreGroup details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("InsertStoreGroupHierarchy")]
        public async Task<ActionResult> InsertStoreGroupHierarchy(string type, string uid)
        {
            try
            {
                var retValue = await _storeGroupBL.InsertStoreGroupHierarchy(type, uid);
                return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreGroup hierarchy details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateStoreGroup")]
        public async Task<ActionResult> UpdateStoreGroup([FromBody] Winit.Modules.Store.Model.Classes.StoreGroup storeGroup)
        {
            try
            {
                var existingstoreGroupList = await _storeGroupBL.SelectStoreGroupByUID(storeGroup.UID);
                if (existingstoreGroupList != null)
                {
                    storeGroup.ModifiedTime = DateTime.Now;
                    storeGroup.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeGroupBL.UpdateStoreGroup(storeGroup);
                    if (retVal > 0)
                    {
                        return CreateOkApiResponse(retVal);
                    }
                    else
                    {
                      throw  new Exception("Update Failed");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Store Group Info Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreGroup")]
        public async Task<ActionResult> DeleteStoreGroup(string UID)
        {
            try
            {
                var retVal = await _storeGroupBL.DeleteStoreGroup(UID);
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
    }
}