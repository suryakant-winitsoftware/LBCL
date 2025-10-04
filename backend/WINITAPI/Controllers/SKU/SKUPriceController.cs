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
using System.Transactions;
using Winit.Modules.Setting.BL.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUPriceController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUPriceBL _sKUPriceBL;

        public SKUPriceController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUPriceBL sKUPriceyBL) : base(serviceProvider)
        {
            _sKUPriceBL = sKUPriceyBL;
        }
        [HttpPost]
        [Route("SelectAllSKUPriceDetails")]
        public async Task<ActionResult> SelectAllSKUPriceDetails(
            [FromBody] PagingRequest pagingRequest, [FromQuery] string type = null)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponseSKUPriceList = null;
                pagedResponseSKUPriceList = await _sKUPriceBL.SelectAllSKUPriceDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, type);
                if (pagedResponseSKUPriceList == null || pagedResponseSKUPriceList.PagedData == null || !pagedResponseSKUPriceList.PagedData.Any())
                {
                    return NotFound();
                }
                var settings = _cacheService.HGet<Winit.Modules.Setting.Model.Classes.Setting>($"{Winit.Shared.Models.Constants.CacheConstants.Setting}{AppSettingNames.MinPriceIncrementPercentage}", AppSettingNames.MinPriceIncrementPercentage);
                if (settings != null)
                {
                    decimal percentage = CommonFunctions.GetDecimalValue(settings.Value);
                    if (percentage > 0)
                    {
                        foreach (ISKUPrice skuPrice in pagedResponseSKUPriceList.PagedData)
                        {
                            skuPrice.PriceLowerLimit += skuPrice.PriceLowerLimit * percentage * 0.01m;
                        }
                    }
                }
                return CreateOkApiResponse(pagedResponseSKUPriceList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUPrice  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectAllSKUPriceDetailsByBroadClassification")]
        public async Task<ActionResult> SelectAllSKUPriceDetailsByBroadClassification(
            [FromBody] PagingRequest pagingRequest, [FromQuery] string broadClassification, [FromQuery] string branchUID, [FromQuery] string type = null)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponseSKUPriceList = null;
                pagedResponseSKUPriceList = await _sKUPriceBL.SelectAllSKUPriceDetailsByBroadClassification(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, broadClassification, branchUID, type);
                if (pagedResponseSKUPriceList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUPriceList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUPrice  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectAllSKUPriceDetailsV1")]
        public async Task<ActionResult> SelectAllSKUPriceDetailsV1(
            [FromBody] PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponseSKUPriceList = null;
                pagedResponseSKUPriceList = await _sKUPriceBL.SelectAllSKUPriceDetailsV1(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (pagedResponseSKUPriceList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUPriceList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUPrice  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



        [HttpPost]
        [Route("SelectAllSKUPriceDetails_BySKUUIDs")]
        public async Task<ActionResult> SelectAllSKUPriceDetails_BySKUUIDs(
            [FromBody] PagingRequest pagingRequest, [FromQuery] List<string> uidList)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponseSKUPriceList = null;
                pagedResponseSKUPriceList = await _sKUPriceBL.SelectAllSKUPriceDetails_BySKUUIDs(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, uidList);
                if (pagedResponseSKUPriceList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUPriceList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUPrice  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetApplicablePriceListByStoreUID")]
        public async Task<ActionResult> GetApplicablePriceListByStoreUID([FromQuery] string storeUID, [FromQuery] string storeType)
        {
            try
            {
                List<string> uIDList = await _sKUPriceBL.GetApplicablePriceListByStoreUID(storeUID, storeType);
                if (uIDList != null)
                {
                    return CreateOkApiResponse(uIDList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve  UIDList");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUPriceByUID")]
        public async Task<ActionResult> SelectSKUPriceByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice = await _sKUPriceBL.SelectSKUPriceByUID(UID);
                if (sKUPrice != null)
                {
                    return CreateOkApiResponse(sKUPrice);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUPriceList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUPrice")]
        public async Task<ActionResult> CreateSKUPrice([FromBody] Winit.Modules.SKU.Model.Classes.SKUPrice sKUPrice)
        {
            try
            {
                sKUPrice.ServerAddTime = DateTime.Now;
                sKUPrice.ServerModifiedTime = DateTime.Now;
                var ratValue = await _sKUPriceBL.CreateSKUPrice(sKUPrice);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUPrice details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUPrice")]
        public async Task<ActionResult> UpdateSKUPrice([FromBody] Winit.Modules.SKU.Model.Classes.SKUPrice sKUPrice)
        {
            try
            {
                var existingDetails = await _sKUPriceBL.SelectSKUPriceByUID(sKUPrice.UID);
                if (existingDetails != null)
                {
                    sKUPrice.ServerModifiedTime = DateTime.Now;
                    var ratValue = await _sKUPriceBL.UpdateSKUPrice(sKUPrice);
                    return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUPrice Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUPriceList")]
        public async Task<ActionResult> UpdateSKUPriceList([FromBody] List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPriceList)
        {

            try
            {

                var ratValue = await _sKUPriceBL.UpdateSKUPriceList(sKUPriceList);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUPrice Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUPrice")]
        public async Task<ActionResult> DeleteSKUPrice([FromQuery] string UID)
        {
            try
            {
                var result = await _sKUPriceBL.DeleteSKUPrice(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectSKUPriceViewByUID")]
        public async Task<ActionResult> SelectSKUPriceViewByUID(PagingRequest pagingRequest, string UID)
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

                var sKUPriceViewList = await _sKUPriceBL.SelectSKUPriceViewByUID(
                pagingRequest.SortCriterias,
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired,
                UID);

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView> pagedResponseSKUPriceList = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>();

                pagedResponseSKUPriceList.PagedData = sKUPriceViewList.Select(tuple => tuple.Item1);
                pagedResponseSKUPriceList.TotalCount = sKUPriceViewList.Any() ? sKUPriceViewList.FirstOrDefault().Item2 : 0;

                if (pagedResponseSKUPriceList.PagedData.Any())
                {
                    return CreateOkApiResponse(pagedResponseSKUPriceList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUPriceViewList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUPriceView")]
        public async Task<ActionResult> CreateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            try
            {
                int retVal = await _sKUPriceBL.CreateSKUPriceView(sKUPriceViewDTO);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUPriceView")]
        public async Task<ActionResult> UpdateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            try
            {
                int retVal = await _sKUPriceBL.UpdateSKUPriceView(sKUPriceViewDTO);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateStandardPriceForSKU")]
        public async Task<ActionResult> CreateStandardPriceForSKU([FromBody] string skuUID)
        {
            try
            {
                var ratValue = await _sKUPriceBL.CreateStandardPriceForSKU(skuUID);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKU Standard Price details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
