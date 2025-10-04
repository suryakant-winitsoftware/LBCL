using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Collections;
using Winit.Shared.Models.Common;
using WINITServices.Interfaces.CacheHandler;
using Newtonsoft.Json;
using WINITAPI.Controllers.SKU;
using Winit.Modules.Store.Model.Classes;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreCreditController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreCreditBL _storeCreditBL;
        private readonly DataPreparationController _dataPreparationController;
        public StoreCreditController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreCreditBL storeCreditBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _storeCreditBL = storeCreditBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllStoreCredit")]
        public async Task<ActionResult> SelectAllStoreCredit(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCredit> pagedResponseStoreCreditList = null;
                pagedResponseStoreCreditList = await _storeCreditBL.SelectAllStoreCredit(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (pagedResponseStoreCreditList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreCreditList);
            }
            catch (JsonReaderException ex)
            {
                Log.Error(ex, "Fail to retrieve StoreCredit  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreCreditByUID")]
        public async Task<ActionResult> SelectStoreCreditByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreCredit storeCredit = await _storeCreditBL.SelectStoreCreditByUID(UID);
                if (storeCredit != null)
                {
                    return CreateOkApiResponse(storeCredit);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreCreditList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreCredit")]
        public async Task<ActionResult> CreateStoreCredit([FromBody] Winit.Modules.Store.Model.Classes.StoreCredit storeCredit)
        {
            try
            {
                storeCredit.ServerAddTime = DateTime.Now;
                storeCredit.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeCreditBL.CreateStoreCredit(storeCredit);
                if (retVal > 0)
                {
                    List<string> uid = new List<string>
                    {
                        storeCredit.StoreUID
                    };
                    _ = await _dataPreparationController.PrepareStoreMaster(uid);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Insert Failed");

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create storeCredit details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreCredit")]
        public async Task<ActionResult> UpdateStoreCredit([FromBody] Winit.Modules.Store.Model.Classes.StoreCredit storeCredit)
        {
            try
            {
                var existingStoreCreditList = await _storeCreditBL.SelectStoreCreditByUID(storeCredit.UID);
                if (existingStoreCreditList != null)
                {
                    storeCredit.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeCreditBL.UpdateStoreCredit(storeCredit);
                    if (retVal > 0)
                    {
                        List<string> uid = new List<string>
                        {
                            storeCredit.StoreUID
                        };
                        _ = await _dataPreparationController.PrepareStoreMaster(uid);
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
                Log.Error(ex, "Error updating Store Credit Info Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreCredit")]
        public async Task<ActionResult> DeleteStoreCredit(string UID)
        {
            try
            {
                var retVal = await _storeCreditBL.DeleteStoreCredit(UID);
                if (retVal > 0)
                {
                    //List<string> uid = new List<string> { UID };
                    //_ = await _dataPreparationController.PrepareStoreMaster(uid);
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

        [HttpPost("GetCurrentLimitByStoreAndDivision")]
        public async Task<IActionResult> GetCurrentLimitByStoreAndDivision(StoreCreditLimitRequest storeCreditLimitRequest)
        {
            if (storeCreditLimitRequest == null || storeCreditLimitRequest.StoreUids == null || !storeCreditLimitRequest.StoreUids.Any())
            {
                return BadRequest("Invalid request Body");
            }
            try
            {
                var response = await _storeCreditBL.GetCurrentLimitByStoreAndDivision(storeCreditLimitRequest.StoreUids, storeCreditLimitRequest.DivisionUID);
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    throw new Exception("Retrive failed");

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet("GetPurchaseOrderCreditLimitBufferRanges")]
        public async Task<IActionResult> GetPurchaseOrderCreditLimitBufferRanges()
        {
            try
            {
                var response = await _storeCreditBL.GetPurchaseOrderCreditLimitBufferRanges();
                if (response != null)
                {
                    return CreateOkApiResponse(response);
                }
                else
                {
                    throw new Exception("Retrive failed");

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Retrive Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
