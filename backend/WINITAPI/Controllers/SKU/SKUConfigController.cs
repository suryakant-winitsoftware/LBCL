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
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Winit.UIModels.Web.SKU;
using Winit.Modules.Store.Model.Classes;
using Microsoft.Extensions.Options;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.BL.Classes;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUConfigController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUConfigBL _skuConfigBL;
        private readonly ApiSettings _apiSettings;
        private readonly DataPreparationController _dataPreparationController;
        public SKUConfigController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUConfigBL skuConfigBL,
            IOptions<ApiSettings> apiSettings, DataPreparationController dataPreparationController) 
            : base(serviceProvider)
        {
            _skuConfigBL = skuConfigBL;
            _apiSettings = apiSettings.Value;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("SelectAllSKUConfigDetails")]
        public async Task<ActionResult> SelectAllSKUConfigDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> pagedResponseSKUConfigList = null;
                pagedResponseSKUConfigList = await _skuConfigBL.SelectAllSKUConfigDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (pagedResponseSKUConfigList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUConfigList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKUConfig  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUConfigByUID")]
        public async Task<ActionResult> SelectSKUConfigByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfigList = await _skuConfigBL.SelectSKUConfigByUID(UID);
                if (skuConfigList != null)
                {
                    return CreateOkApiResponse(skuConfigList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUConfig with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUConfig")]
        public async Task<ActionResult> CreateSKUConfig([FromBody] Winit.Modules.SKU.Model.Classes.SKUConfig skuConfig)
        {
            try
            {
                skuConfig.ServerAddTime = DateTime.Now;
                skuConfig.ServerModifiedTime = DateTime.Now;
                var ratValue = await _skuConfigBL.CreateSKUConfig(skuConfig);
                if (ratValue > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new()
                    {
                        SKUUIDs = new List<string> { skuConfig.SKUUID }
                    };
                    _ = _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUConfig details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUConfig")]
        public async Task<ActionResult> UpdateSKUConfig([FromBody] Winit.Modules.SKU.Model.Classes.SKUConfig skuConfig)
        {
            try
            {
                var existingDetails = await _skuConfigBL.SelectSKUConfigByUID(skuConfig.UID);
                if (existingDetails == null)
                    return NotFound();

                skuConfig.ServerModifiedTime = DateTime.Now;
                var ratValue = await _skuConfigBL.UpdateSKUConfig(skuConfig);
                if (ratValue > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new()
                    {
                        SKUUIDs = new List<string> { skuConfig.SKUUID }
                    };
                    _ = _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(ratValue);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUConfig Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSKUConfig")]
        public async Task<ActionResult> DeleteSKUConfig([FromQuery] string UID)
        {
            try
            {
                var result = await _skuConfigBL.DeleteSKUConfig(UID);
                if (result > 0)
                {
                    //var prepareSKURequestModel = new PrepareSKURequestModel
                    //{
                    //    SKUUIDs = { UID }
                    //};
                    //_ = _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(result);
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
    }
}
