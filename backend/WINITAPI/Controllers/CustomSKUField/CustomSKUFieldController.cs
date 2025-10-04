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
using WINITServices.Classes.CacheHandler;
using WINITServices.Interfaces.CacheHandler;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Interfaces;
using Npgsql;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class CustomSKUFieldController : WINITBaseController
    {
        private readonly Winit.Modules.CustomSKUField.BL.Interfaces.ICustomSKUFieldsBL _customSKUFieldsBL;
        private readonly DataPreparationController _dataPreparationController;
        public CustomSKUFieldController(IServiceProvider serviceProvider, 
            Winit.Modules.CustomSKUField.BL.Interfaces.ICustomSKUFieldsBL customSKUFieldsBL,
            DataPreparationController dataPreparationController) : base(serviceProvider)
        {
            _customSKUFieldsBL = customSKUFieldsBL;
            _dataPreparationController = dataPreparationController;
        }
        [HttpPost]
        [Route("CreateCustomSKUField")]
        public async Task<ActionResult> CreateCustomSKUField([FromBody] Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUFields)
        {
            try
            {
                var ratValue = await _customSKUFieldsBL.CreateCustomSKUField(customSKUFields);
                if (ratValue > 0) {
                    PrepareSKURequestModel uids = new PrepareSKURequestModel()
                    {
                        SKUUIDs = new List<string> { customSKUFields.SKUUID }
                    };
                    _=await _dataPreparationController.PrepareSKUMaster(uids);
                    return CreateOkApiResponse(ratValue); 

                } else { throw new Exception("Create failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create CustomSKUField details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateCustomSKUField")]
        public async Task<ActionResult> UpdateCustomSKUField([FromBody] Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUFields)
        {
            try
            {
                customSKUFields.ServerModifiedTime = DateTime.Now;
                var ratValue = await _customSKUFieldsBL.UpdateCustomSKUField(customSKUFields);
                if (ratValue > 0) {
                    PrepareSKURequestModel uids = new PrepareSKURequestModel()
                    {
                        SKUUIDs = new List<string> { customSKUFields.SKUUID }
                    };
                    _ = await _dataPreparationController.PrepareSKUMaster(uids);
                   return CreateOkApiResponse(ratValue); } else { throw new Exception("Update failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Update CustomSKUField details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectAllCustomFieldsDetails")]
        public async Task<ActionResult> SelectAllCustomFieldsDetails(string SKUUID)
        {
            try
            {
                IEnumerable <Winit.Modules.CustomSKUField.Model.Classes.CustomField> customFields  = await _customSKUFieldsBL.SelectAllCustomFieldsDetails(SKUUID);
                if (customFields != null)
                {
                    return CreateOkApiResponse(customFields);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve CustomFields with UID: {@UID}", SKUUID);
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDCustomSKUFields")]
        public async Task<ActionResult> CUDCustomSKUFields([FromBody] Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUFields)
        {
            try
            {
                var ratValue = await _customSKUFieldsBL.CUDCustomSKUFields(customSKUFields);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create CustomSKUField details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }

}

