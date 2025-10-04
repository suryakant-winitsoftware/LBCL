using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreSpecialDayController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreSpecialDayBL _storespecialdayBl;

        public StoreSpecialDayController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreSpecialDayBL storespecialdayBl) : base(serviceProvider)
        {
            _storespecialdayBl = storespecialdayBl;
        }
        [HttpPost]
        [Route("SelectAllStoreSpecialDay")]
        public async Task<ActionResult> SelectAllStoreSpecialDay(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay> pagedResponseStoreSpecialDayList = null;
                pagedResponseStoreSpecialDayList = await _storespecialdayBl.SelectAllStoreSpecialDay(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreSpecialDayList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreSpecialDayList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreSpecialDay  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreSpecialDayByUID")]
        public async Task<ActionResult> SelectStoreSpecialDayByUID( string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay storeSpecialDay = await _storespecialdayBl.SelectAllStoreSpecialDayByUID(UID);
                if (storeSpecialDay != null)
                {
                    return CreateOkApiResponse(storeSpecialDay);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreSpecialDayList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreSpecialDay")]
        public async Task<ActionResult> CreateStoreSpecialDay([FromBody] Winit.Modules.Store.Model.Classes.StoreSpecialDay storespecialday)
        {
            try
            {
                storespecialday.ServerAddTime = DateTime.Now;
                storespecialday.ServerModifiedTime = DateTime.Now;
                var retVal = await _storespecialdayBl.CreateStoreSpecialDay(storespecialday);
                return (retVal > 0)? CreateOkApiResponse(retVal) :throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Store Special Day details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreSpecialDay")]
        public async Task<ActionResult> UpdateStoreSpecialDay([FromBody] Winit.Modules.Store.Model.Classes.StoreSpecialDay Storespecialday)
        {
            try
            {
                var existingStorespecialdayList = await _storespecialdayBl.SelectAllStoreSpecialDayByUID(Storespecialday.UID);
                if (existingStorespecialdayList != null)
                {
                    Storespecialday.ModifiedTime = DateTime.Now;
                    Storespecialday.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storespecialdayBl.UpdateStoreSpecialDay(Storespecialday);
                    return (retVal > 0)? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Store Special Day Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreSpecialDay")]
        public async Task<ActionResult> DeleteStoreSpecialDay(string UID)
        {
            try
            {
                var retVal = await _storespecialdayBl.DeleteStoreSpecialDay(UID);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                   throw new Exception("Delete failed");
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