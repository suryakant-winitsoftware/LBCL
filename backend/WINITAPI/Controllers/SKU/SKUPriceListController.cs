using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Authorization;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUPriceListController:WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL _SKUPriceListBL;

        public SKUPriceListController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL SKUPriceListyBL) : base(serviceProvider)
        {
            _SKUPriceListBL = SKUPriceListyBL;
        }
        [HttpPost]
        [Route("SelectAllSKUPriceListDetails")]
        public async Task<ActionResult> SelectAllSKUPriceListDetails(
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList> pagedResponseSKUPriceListList = null;
                pagedResponseSKUPriceListList = await _SKUPriceListBL.SelectAllSKUPriceListDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseSKUPriceListList == null)
                {
                    return CreateErrorResponse("Not Found", StatusCodes.Status404NotFound);
                }
                return CreateOkApiResponse(pagedResponseSKUPriceListList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUPriceList  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUPriceListByUID")]
        public async Task<ActionResult> SelectSKUPriceListByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList = await _SKUPriceListBL.SelectSKUPriceListByUID(UID);
                if (sKUPriceList != null)
                {
                    return CreateOkApiResponse(sKUPriceList);
                }
                else
                {
                    return CreateErrorResponse("Not Found", StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUPriceListList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUPriceList")]
        public async Task<ActionResult> CreateSKUPriceList([FromBody] Winit.Modules.SKU.Model.Classes.SKUPriceList SKUPriceList)
        {
            try
            {
                SKUPriceList.ServerAddTime = DateTime.Now;
                SKUPriceList.ServerModifiedTime = DateTime.Now;
                var ratValue = await _SKUPriceListBL.CreateSKUPriceList(SKUPriceList);
                return  (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUPriceList details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUPriceList")]
        public async Task<ActionResult> UpdateSKUPriceList([FromBody] Winit.Modules.SKU.Model.Classes.SKUPriceList SKUPriceList)
        {
            try
            {
                var existingDetails = await _SKUPriceListBL.SelectSKUPriceListByUID(SKUPriceList.UID);
                if (existingDetails != null)
                {
                    SKUPriceList.ServerModifiedTime = DateTime.Now;
                    var ratValue = await _SKUPriceListBL.UpdateSKUPriceList(SKUPriceList);
                    return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Upadate Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUPriceList Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSKUPriceList")]
        public async Task<ActionResult> DeleteSKUPriceList([FromQuery] string UID)
        {
            try
            {
                var result = await _SKUPriceListBL.DeleteSKUPriceList(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
