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
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class TaxSkuMapController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ITaxSkuMapBL _taxSkuMapBL;
        private readonly DataPreparationController _dataPreparationController;
        public TaxSkuMapController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ITaxSkuMapBL taxSkuMapBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _taxSkuMapBL = taxSkuMapBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllTaxSkuMapDetails")]
        public async Task<ActionResult> SelectAllTaxSkuMapDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap> pagedResponseTaxSKUMapList = null;
                pagedResponseTaxSKUMapList = await _taxSkuMapBL.SelectAllTaxSkuMapDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (pagedResponseTaxSKUMapList == null)
                {
                    return NotFound();
                }
               
                return CreateOkApiResponse(pagedResponseTaxSKUMapList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve TaxSKUMap  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectTaxSkuMapByUID")]
        public async Task<ActionResult> SelectTaxSkuMapByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMapList = await _taxSkuMapBL.SelectTaxSkuMapByUID(UID);
                if (taxSkuMapList != null)
                {
                    return CreateOkApiResponse(taxSkuMapList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TaxSkuMap with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTaxSkuMap")]
        public async Task<ActionResult> CreateTaxSkuMap([FromBody] Winit.Modules.SKU.Model.Classes.TaxSkuMap taxSkuMap)
        {
            try
            {
                taxSkuMap.ServerAddTime = DateTime.Now;
                taxSkuMap.ServerModifiedTime = DateTime.Now;
                var ratValue = await _taxSkuMapBL.CreateTaxSkuMap(taxSkuMap);
                if (ratValue > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new()
                    {
                        SKUUIDs = new List<string> { taxSkuMap.SKUUID }
                    };
                    _ = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create TaxSKUMap details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateTaxSkuMap")]
        public async Task<ActionResult> UpdateTaxSkuMap([FromBody] Winit.Modules.SKU.Model.Classes.TaxSkuMap taxSkuMap)
        {
            try
            {
                var existingDetails = await _taxSkuMapBL.SelectTaxSkuMapByUID(taxSkuMap.UID);
                if (existingDetails != null)
                {
                    taxSkuMap.ServerModifiedTime = DateTime.Now;
                    var ratValue= await _taxSkuMapBL.UpdateTaxSkuMap(taxSkuMap);
                    if (ratValue > 0)
                    {
                        PrepareSKURequestModel prepareSKURequestModel = new()
                        {
                            SKUUIDs = new List<string> { taxSkuMap.SKUUID }
                        };
                        _ = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                        return CreateOkApiResponse(ratValue);
                    }
                    else
                    {
                        throw new Exception("Create failed");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating TaxSKUMap Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteTaxSkuMapByUID")]
        public async Task<ActionResult> DeleteTaxSkuMapByUID([FromQuery] string UID)
        {
            try
            {
                var result = await _taxSkuMapBL.DeleteTaxSkuMapByUID(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
