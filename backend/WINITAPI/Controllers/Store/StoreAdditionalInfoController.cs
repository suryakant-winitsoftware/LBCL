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
using WINITServices.Interfaces.CacheHandler;
using Newtonsoft.Json;
using Winit.Modules.Store.Model.Classes;
using WINITAPI.Controllers.SKU;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreAdditionalInfoController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL _storeAdditionalInfoBL;
        private readonly DataPreparationController _dataPreparationController;
        public StoreAdditionalInfoController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL storeAdditionalInfoBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _storeAdditionalInfoBL = storeAdditionalInfoBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllStoreAdditionalInfo")]
        public async Task<ActionResult> SelectAllStoreAdditionalInfo(PagingRequest pagingRequest)
        {
            try
            {  PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> pagedResponseStoreAdditionalInfoList = null;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }

                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                pagedResponseStoreAdditionalInfoList = await _storeAdditionalInfoBL.SelectAllStoreAdditionalInfo(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreAdditionalInfoList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreAdditionalInfoList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreAdditionalInfo  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectStoreAdditionalInfoByUID")]
        public async Task<ActionResult> SelectStoreAdditionalInfoByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo = await _storeAdditionalInfoBL.SelectStoreAdditionalInfoByUID(UID);
                if (storeAdditionalInfo != null)
                {
                    return CreateOkApiResponse(storeAdditionalInfo);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreAdditionalInfoList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreAdditionalInfo")]
        public async Task<ActionResult> CreateStoreAdditionalInfo([FromBody] Winit.Modules.Store.Model.Classes.StoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {
                storeAdditionalInfo.ServerAddTime = DateTime.Now;
                storeAdditionalInfo.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeAdditionalInfoBL.CreateStoreAdditionalInfo(storeAdditionalInfo);
                
                if (retVal > 0)
                {
                    List<string> uids = new List<string>{ storeAdditionalInfo.StoreUID };
                    _ = await _dataPreparationController.PrepareStoreMaster(uids);
                    return CreateOkApiResponse(retVal);

                }
                else
                {
                   throw new Exception("Insert Failed");
                }
               
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreAdditionalInfo details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreAdditionalInfo")]
        public async Task<ActionResult> UpdateStoreAdditionalInfo([FromBody] Winit.Modules.Store.Model.Classes.StoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {
                var existingStoreAdditionalInfoList = await _storeAdditionalInfoBL.SelectStoreAdditionalInfoByUID(storeAdditionalInfo.UID);
                if (existingStoreAdditionalInfoList != null)
                {
                    storeAdditionalInfo.ModifiedTime = DateTime.Now;
                    storeAdditionalInfo.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeAdditionalInfoBL.UpdateStoreAdditionalInfo(storeAdditionalInfo);
                    if (retVal > 0)
                    {
                        List<string> uids = new List<string> { storeAdditionalInfo.StoreUID };
                        _ = await _dataPreparationController.PrepareStoreMaster(uids);
                        return CreateOkApiResponse(retVal);
                    }
                    else {

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
                Log.Error(ex, "Error updating Store Additional Info Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreAdditionalInfo")]
        public async Task<ActionResult> DeleteStoreAdditionalInfo(string UID)
        {
            try
            {
                var retVal = await _storeAdditionalInfoBL.DeleteStoreAdditionalInfo(UID);
                if (retVal > 0)
                {
                    //List<string> uids = new List<string> { UID };
                    //_ = await _dataPreparationController.PrepareStoreMaster(uids);
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

        [HttpPost]
        [Route("CreatePaymentForMobile")]
        public async Task<ActionResult> CreatePaymentForMobile([FromBody] Winit.Modules.Store.Model.Classes.Payment payment)
        {
            try
            {
                payment.ServerAddTime = DateTime.Now;
                payment.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeAdditionalInfoBL.CreatePaymentForMobile(payment);
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
                Log.Error(ex, "Failed to create Payment details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdatePaymentForMobile")]
        public async Task<ActionResult> UpdatePaymentForMobile([FromBody] Winit.Modules.Store.Model.Classes.Payment payment)
        {
            try
            {
                var existingPaymentList = await _storeAdditionalInfoBL.SelectPaymentByUID(payment.UID);
                if (existingPaymentList != null)
                {
                    payment.ModifiedTime = DateTime.Now;
                    payment.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeAdditionalInfoBL.UpdatePaymentForMobile(payment);
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
                Log.Error(ex, "Error updating payment Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectPaymentByUID")]
        public async Task<ActionResult> SelectPaymentByUID([FromQuery] string UID)
        {
            try
            {
                
                Winit.Modules.Store.Model.Interfaces.IPayment paymentDetails = await _storeAdditionalInfoBL.SelectPaymentByUID(UID);
                if (paymentDetails != null)
                {
                    return CreateOkApiResponse(paymentDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve payment with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}