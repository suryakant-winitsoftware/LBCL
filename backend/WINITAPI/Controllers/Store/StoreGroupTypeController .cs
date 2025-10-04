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

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreGroupTypeController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL _storeGroupTypeBL;
        public StoreGroupTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL storeGroupTypeBL) : base(serviceProvider)
        {
            _storeGroupTypeBL = storeGroupTypeBL;
        }
        [HttpGet]
        [Route("SelectAllStoreGroupType")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>>> SelectAllStoreGroupType([FromQuery] List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, [FromQuery] List<FilterCriteria> filterCriterias)
        {
            try
            {
                if (pageNumber < 0 || pageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                var cacheKey =CacheConstants.ALL_StoreGroupType;
                IEnumerable<Winit.Modules.Base.Model.IBaseModel> StoreGroupTypeList = null;
                IEnumerable<object> StoreGroupTypeResponse = null;
                StoreGroupTypeResponse = _cacheService.Get<IEnumerable<object>>(cacheKey);
                if (StoreGroupTypeResponse != null)
                {
                    return Ok(StoreGroupTypeResponse);
                }
                StoreGroupTypeList = await _storeGroupTypeBL.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias);
                if (StoreGroupTypeList == null)
                {
                    return NotFound();
                }
                else
                {
                    StoreGroupTypeResponse = StoreGroupTypeList.OfType<object>().ToList();
                    //_cacheService.Set(cacheKey, StoreGroupTypeResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                return Ok(StoreGroupTypeResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Store GroupType Details");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve Store GroupType Details");
            }
        }
        [HttpGet]
        [Route("SelectStoreGroupTypeByUID")]
        public async Task<ActionResult> SelectStoreGroupTypeByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreGroupType StoreAdditionalInfoList = await _storeGroupTypeBL.SelectStoreGroupTypeByUID(UID);
                if (StoreAdditionalInfoList != null)
                {
                    return Ok((object)StoreAdditionalInfoList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StoreGroupTypeList with StoreGroupTypeUID: {@StoreGroupTypeUID}", UID);
                throw;
            }
        }
        [HttpPost]
        [Route("CreateStoreGroupType")]
        public async Task<ActionResult<int>> CreateStoreGroupType([FromBody] Winit.Modules.Store.Model.Classes.StoreGroupType storeGroupType)
        {
            try
            {
                //storeGroupType.UID = Guid.NewGuid().ToString();
                storeGroupType.ServerAddTime = DateTime.Now;
                storeGroupType.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeGroupTypeBL.CreateStoreGroupType(storeGroupType);
                if (retVal > 0)
                {
                    return Created("Created", retVal);
                }
                else
                {
                  throw  new Exception("Insert Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreGroupType details");
                return StatusCode(500, new { success = false, message = "Error creating StoreGroupType Details", error = ex.Message });
            }
        }
       
        [HttpPut]
        [Route("UpdateStoreGroupType")]
        public async Task<ActionResult<int>> UpdateStoreGroupType([FromBody] Winit.Modules.Store.Model.Classes.StoreGroupType StoreGroupType)
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
                        return Accepted("Updated", retVal);
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
                return StatusCode(500, new { success = false, message = "Error updating StoreGroupType Details", error = ex.Message });
            }
        }
        [HttpDelete]
        [Route("DeleteStoreGroupType")]
        public async Task<ActionResult<int>> DeleteStoreGroupType(string UID)
        {
            try
            {
                var retVal = await _storeGroupTypeBL.DeleteStoreGroupType(UID);
                if (retVal > 0)
                {
                    return Accepted("Deleted",retVal);
                }
                else
                {
                  throw  new Exception("Delete Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, new { success = false, message = "Deleting Failure", error = ex.Message });
            }

        }
    }
}