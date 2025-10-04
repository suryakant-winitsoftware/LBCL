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
    public class StoreWeekOffController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreWeekOffBL _storeWeekOffBL;
        public StoreWeekOffController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreWeekOffBL storeWeekOffBL) : base(serviceProvider)
        {
            _storeWeekOffBL = storeWeekOffBL;
        }
        [HttpGet]
        [Route("SelectAllStoreWeekOff")]
        public async Task<ActionResult> SelectAllStoreWeekOff(
            PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreWeekOff> pagedResponseStoreWeekOffList = null;
                pagedResponseStoreWeekOffList = await _storeWeekOffBL.SelectAllStoreWeekOff(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreWeekOffList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreWeekOffList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreWeekOff  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreWeekOffByUID")]
        public async Task<ActionResult> SelectStoreWeekOffByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreWeekOff storeWeekOff = await _storeWeekOffBL.SelectStoreWeekOffByUID(UID);
                if (storeWeekOff != null)
                {
                    return CreateOkApiResponse(storeWeekOff);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreWeekOffList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreWeekOff")]
        public async Task<ActionResult> CreateStoreWeekOff([FromBody] Winit.Modules.Store.Model.Classes.StoreWeekOff storeWeekOff)
        {
            try
            {
                storeWeekOff.ServerAddTime = DateTime.Now;
                storeWeekOff.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeWeekOffBL.CreateStoreWeekOff(storeWeekOff);
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
                Log.Error(ex, "Failed to create StoreWeekOff details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreWeekOff")]
        public async Task<ActionResult> UpdateStoreWeekOff([FromBody] Winit.Modules.Store.Model.Classes.StoreWeekOff storeWeekOff)
        {
            try
            {
                var existingstoreWeekOffList = await _storeWeekOffBL.SelectStoreWeekOffByUID(storeWeekOff.UID);
                if (existingstoreWeekOffList != null)
                {
                    storeWeekOff.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeWeekOffBL.UpdateStoreWeekOff(storeWeekOff);
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
        [Route("DeleteStoreWeekOff")]
        public async Task<ActionResult> DeleteStoreWeekOff(string UID)
        {
            try
            {
                var retVal = await _storeWeekOffBL.DeleteStoreWeekOff(UID);
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