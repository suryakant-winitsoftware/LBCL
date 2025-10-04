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
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Winit.Modules.Store.Model.Classes;
using Microsoft.Extensions.Options;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.UIModels.Web.SKU;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUAttributesController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUAttributesBL _skuAttributesBL;
        private readonly DataPreparationController  _dataPreparationController;
        

        public SKUAttributesController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUAttributesBL skuAttributesBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _skuAttributesBL = skuAttributesBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllSKUAttributesDetails")]
        public async Task<ActionResult> SelectAllSKUAttributesDetails(PagingRequest pagingRequest)
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
                //var cachedData = _cache.HGetAll<List<Winit.Modules.SKU.Model.Classes.SKUAttributes>>("SKUAttributes");
                //if (cachedData != null)
                //{
                //    var skuattributesList = cachedData.Values.SelectMany(innerList => innerList);

                //    return CreateOkApiResponse(skuattributesList);
                //}
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> pagedResponseSKUAttributesList = null;
                pagedResponseSKUAttributesList = await _skuAttributesBL.SelectAllSKUAttributesDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (pagedResponseSKUAttributesList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUAttributesList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKU Attributes  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUAttributesByUID")]
        public async Task<ActionResult> SelectSKUAttributesByUID([FromQuery] string UID)
        {
            try
            {
                //var cachedData = _cache.HGet<List<Winit.Modules.SKU.Model.Classes.SKUAttributes>>("SKUAttributes", UID);
                //if (cachedData != null)
                //{
                //    return CreateOkApiResponse(cachedData);
                //}
                Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuAttributesList = await _skuAttributesBL.SelectSKUAttributesByUID(UID);
                if (skuAttributesList != null)
                {
                    return CreateOkApiResponse(skuAttributesList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUAttributes with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSKUGroupTypeForSKuAttribute")]
        public async Task<ActionResult> GetSKUGroupTypeForSKuAttribute()
        {
            try
            {
                var sKUAttributeDropdownModels = await _skuAttributesBL.GetSKUGroupTypeForSKuAttribute();
                if (sKUAttributeDropdownModels != null)
                {
                    return CreateOkApiResponse(sKUAttributeDropdownModels);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sKUAttributeDropdownModels");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUAttributes")]
        public async Task<ActionResult> CreateSKUAttributes([FromBody] Winit.Modules.SKU.Model.Classes.SKUAttributes skuAttributes)
        {
            try
            {
                skuAttributes.ServerAddTime = DateTime.Now;
                skuAttributes.ServerModifiedTime = DateTime.Now;
                var ratValue = await _skuAttributesBL.CreateSKUAttributes(skuAttributes);
                if (ratValue > 0)
                {
                    var prepareSKURequestModel = new PrepareSKURequestModel
                    {
                        SKUUIDs = { skuAttributes.SKUUID }
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
                Log.Error(ex, "Failed to create SKUAttributes details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateBulkSKUAttributes")]
        public async Task<ActionResult> CreateBulkSKUAttributes([FromBody] List<Winit.Modules.SKU.Model.Classes.SKUAttributes> skuAttributes)
        {
            try
            {
                var ratValue = await _skuAttributesBL.CreateBulkSKUAttributes(skuAttributes.ToList<ISKUAttributes>());
                if (ratValue > 0)
                {
                    var prepareSKURequestModel = new PrepareSKURequestModel
                    {
                        SKUUIDs = skuAttributes.Select(e => e.SKUUID).ToList()
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
                Log.Error(ex, "Failed to create SKUAttributes details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDBulkSKUAttributes")]
        public async Task<ActionResult> CUDBulkSKUAttributes([FromBody] List<Winit.Modules.SKU.Model.Classes.SKUAttributes> skuAttributes)
        {
            try
            {
                var ratValue = await _skuAttributesBL.CUDBulkSKUAttributes(skuAttributes.ToList<ISKUAttributes>());
                if (ratValue > 0)
                {
                    var prepareSKURequestModel = new PrepareSKURequestModel
                    {
                        SKUUIDs = skuAttributes.Select(e => e.SKUUID).ToList()
                    };
                    _ = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    throw new Exception("CUDBulkSKUAttributes failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to CUDBulkSKUAttributes details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUAttributes")]
        public async Task<ActionResult> UpdateSKUConfig([FromBody] Winit.Modules.SKU.Model.Classes.SKUAttributes sKUAttributes)
        {
            try
            {
                var existingDetails = await _skuAttributesBL.SelectSKUAttributesByUID(sKUAttributes.UID);
                if (existingDetails == null)
                    return NotFound();
                sKUAttributes.ServerModifiedTime = DateTime.Now;
                var ratValue = await _skuAttributesBL.UpdateSKUAttributes(sKUAttributes);
                PrepareSKURequestModel prepareSKURequestModel = new()
                {
                    SKUUIDs = new List<string> { sKUAttributes.SKUUID }
                };
                _ = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                return CreateOkApiResponse(ratValue);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKU Attributes Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUAttributesByUID")]
        public async Task<ActionResult> DeleteSKUAttributesByUID([FromQuery] string UID)
        {
            try
            {
                var result = await _skuAttributesBL.DeleteSKUAttributesByUID(UID);
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
