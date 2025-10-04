using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Winit.Shared.Models.Enums;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Holiday
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HolidayListController:WINITBaseController
    {
        private readonly Winit.Modules.Holiday.BL.Interfaces.IHolidayListBL _holidayListBL;

        public HolidayListController(IServiceProvider serviceProvider, 
            Winit.Modules.Holiday.BL.Interfaces.IHolidayListBL holidayListBL) 
            : base(serviceProvider)
        {
            _holidayListBL = holidayListBL;
        }
        [HttpPost]
        [Route("SelectAllHolidayListDetails")]
        public async Task<ActionResult> SelectAllHolidayListDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> pagedResponseHolidayList = null;

                pagedResponseHolidayList = await _holidayListBL.SelectAllHolidayListDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseHolidayList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseHolidayList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve HolidayListDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetHolidayListByHolidayListUID")]
        public async Task<ActionResult> GetHolidayListByHolidayListUID(string UID)
        {
            try
            {
                Winit.Modules.Holiday.Model.Interfaces.IHolidayList HolidayDetails = await _holidayListBL.GetHolidayListByHolidayListUID(UID);
                if (HolidayDetails != null)
                {
                    return CreateOkApiResponse(HolidayDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve HolidayListDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateHolidayList")]
        public async Task<ActionResult> CreateHolidayList([FromBody] Winit.Modules.Holiday.Model.Classes.HolidayList createHolidayList)
        {
            try
            {
                createHolidayList.ServerAddTime = DateTime.Now;
                createHolidayList.ServerModifiedTime = DateTime.Now;
                var returnValue = await _holidayListBL.CreateHolidayList(createHolidayList);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create HolidayList details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateHolidayList")]
        public async Task<ActionResult> UpdateHolidayList([FromBody] Winit.Modules.Holiday.Model.Classes.HolidayList updateHolidayList)
        {
            try
            {
                var existingDetails = await _holidayListBL.GetHolidayListByHolidayListUID(updateHolidayList.UID);
                if (existingDetails != null)
                {
                    updateHolidayList.ServerModifiedTime = DateTime.Now;
                    var returnValue = await _holidayListBL.UpdateHolidayList(updateHolidayList);
                    return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating HolidayList Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteHolidayList")]
        public async Task<ActionResult> DeleteHolidayList([FromQuery] string UID)
        {
            try
            {
                var result = await _holidayListBL.DeleteHolidayList(UID);
                return (result> 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        
    }
}
