using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using System.Net.Http.Headers;
using Winit.Shared.Models.Common;
using WINITServices.Interfaces.CacheHandler;
using Newtonsoft.Json;
using WINITServices.Classes.CacheHandler;
using WINITAPI.Controllers.SKU;
using Winit.Modules.Store.Model.Classes;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreAttributesController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreAttributesBL _storeAttributesBL;
        private readonly DataPreparationController _dataPreparationController;
        public StoreAttributesController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreAttributesBL storeAttributesService,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _storeAttributesBL = storeAttributesService;
        }
        [HttpGet]
        [Route("SelectStoreAttributesByName")]
        public async Task<ActionResult<IEnumerable<string>>> SelectStoreAttributesByName(string attributeName)
        {
            try
            {
                List<string> storeList = null;

                List<string> keys = _cacheService.GetKeyByPattern($"{attributeName}:*");

                if (keys != null && keys.Count > 0)
                {
                    storeList = new List<string>();
                    foreach (string key in keys)
                    {
                        storeList.AddRange(_cacheService.SMembers<string>(key));
                    }

                    if (storeList != null && storeList.Count > 0)
                    {
                        return Ok(storeList);
                    }
                }

                // If the data is not in the cache, fetch it from the service
                IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList = await _storeAttributesBL.SelectStoreAttributesByName(attributeName);
                if (storeAttributesList == null)
                {
                    return NotFound();
                }
                else
                {
                    // Cache the data
                    foreach (Winit.Modules.Store.Model.Interfaces.IStoreAttributes storeAttributes in storeAttributesList)
                    {
                        var cacheKey = $"{attributeName}:{storeAttributes.Code}";
                        storeList.Add(storeAttributes.StoreUID);
                        _cacheService.SAdd<string>(cacheKey, storeAttributes.StoreUID, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                    }
                }
                return Ok(storeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve storeList data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve storeList data");
            }
        }


        [HttpPost]
        [Route("SelectAllStoreAttributes")]
        public async Task<ActionResult> SelectAllStoreAttributes(PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> pagedResponseStoreAttributesList = null;
                List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList = null;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }

                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                var cachedData = _cacheService.HGetAll<List<Winit.Modules.Store.Model.Classes.StoreAttributes>>("STOREATTRIBUTES");
               
                if (cachedData != null && cachedData.Count>0)
                {
                    storeAttributesList = new();
                    foreach (string key in cachedData.Keys)
                    {
                        storeAttributesList.AddRange(cachedData[key]);
                    }
                    pagedResponseStoreAttributesList = new();
                    pagedResponseStoreAttributesList.PagedData=storeAttributesList;
                    pagedResponseStoreAttributesList.TotalCount = storeAttributesList.Count;
                    return CreateOkApiResponse(pagedResponseStoreAttributesList);
                }
                pagedResponseStoreAttributesList = await _storeAttributesBL.SelectAllStoreAttributes(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreAttributesList == null)
                {
                    return NotFound();
                }
                else
                {
                    //StoreAttributesListResponse = pagedResponseStoreAttributesList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, StoreAttributesListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return CreateOkApiResponse(pagedResponseStoreAttributesList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve StoreAttributes  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectStoreAttributesByUID")]
        public async Task<ActionResult> SelectStoreAttributesByUID([FromQuery] string UID)
        {
            try
            {
                var CachedData = _cacheService.HGet<object>("STOREATTRIBUTES", UID);
                Winit.Modules.Store.Model.Classes.StoreAttributes storeAttributesList = JsonConvert.DeserializeObject<Winit.Modules.Store.Model.Classes.StoreAttributes>(CachedData.ToString());
                if (storeAttributesList != null)
                {
                    return CreateOkApiResponse(storeAttributesList);
                }
                Winit.Modules.Store.Model.Interfaces.IStoreAttributes storeAttributes = await _storeAttributesBL.SelectStoreAttributesByStoreUID(UID);
                if (storeAttributes != null)
                {
                    return CreateOkApiResponse(storeAttributes);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreAttributesList with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreAttributes")]
        public async Task<ActionResult> CreateStoreAttributes([FromBody] Winit.Modules.Store.Model.Classes.StoreAttributes storeAttributes)
        {
            try
            {
                //storeAttributes.UID = Guid.NewGuid().ToString();
                storeAttributes.ServerAddTime = DateTime.Now;
                storeAttributes.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeAttributesBL.CreateStoreAttributes(storeAttributes);
                if (retVal > 0)
                {
                    List<string> storeUID = new List<string> { storeAttributes.StoreUID };
                    _ = await _dataPreparationController.PrepareStoreMaster(storeUID);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                   throw new Exception("Insert Failed");
                    
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreAttributes details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreAttributes")]
        public async Task<ActionResult> UpdateStoreAttributes(
            [FromBody] Winit.Modules.Store.Model.Classes.StoreAttributes storeAttributes)
        {
            try
            {
                var existingStoreAttributesList = await _storeAttributesBL.SelectStoreAttributesByStoreUID(storeAttributes.UID);
                if (existingStoreAttributesList != null)
                {
                    storeAttributes.ModifiedTime = DateTime.Now;
                    storeAttributes.ServerModifiedTime = DateTime.Now;
                    var retVal = await _storeAttributesBL.UpdateStoreAttributes(storeAttributes);
                    if (retVal > 0)
                    {
                        List<string>storeUID=new List<string> { storeAttributes.StoreUID };
                        _ = await _dataPreparationController.PrepareStoreMaster(storeUID);
                        return CreateOkApiResponse(retVal);
                    }
                    else
                    {
                     throw   new Exception("Update Failed");
                       
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating StoreAttributes Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteStoreAttributes")]
        public async Task<ActionResult> DeleteStoreAttributes(string UID)
        {
            try
            {
               var retVal = await _storeAttributesBL.DeleteStoreAttributes(UID);
                if (retVal > 0)
                {
                    List<string> storeUID = new List<string> { UID };
                    _ = await _dataPreparationController.PrepareStoreMaster(storeUID);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                 throw   new Exception("Delete Failed");
                   
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