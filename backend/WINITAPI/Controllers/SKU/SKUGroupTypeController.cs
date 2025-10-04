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

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SKUGroupTypeController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL _skuGroupTypeBL;

        public SKUGroupTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL skuGroupTypeBL) : base(serviceProvider)
        {
            _skuGroupTypeBL = skuGroupTypeBL;
        }
        [HttpPost]
        [Route("SelectAllSKUGroupTypeDetails")]
        public async Task<ActionResult> SelectAllSKUGroupTypeDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType> pagedResponseSKUGroupTypeList = null;
                pagedResponseSKUGroupTypeList = await _skuGroupTypeBL.SelectAllSKUGroupTypeDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (pagedResponseSKUGroupTypeList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSKUGroupTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SKU Group Type  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUGroupTypeByUID")]
        public async Task<ActionResult> SelectSKUGroupTypeByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUGroupType skuGroupTypeList = await _skuGroupTypeBL.SelectSKUGroupTypeByUID(UID);
                if (skuGroupTypeList != null)
                {
                    return CreateOkApiResponse(skuGroupTypeList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUGroupType with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateSKUGroupType")]
        public async Task<ActionResult> CreateSKUGroupType([FromBody] Winit.Modules.SKU.Model.Classes.SKUGroupType sKUGroupType)
        {
            try
            {
                sKUGroupType.ServerAddTime = DateTime.Now;
                sKUGroupType.ServerModifiedTime = DateTime.Now;
                var ratValue = await _skuGroupTypeBL.CreateSKUGroupType(sKUGroupType);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKUGroup Type details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateSKUGroupType")]
        public async Task<ActionResult> UpdateSKUGroupType([FromBody] Winit.Modules.SKU.Model.Classes.SKUGroupType sKUGroupType)
        {
            try
            {
                var existingDetails = await _skuGroupTypeBL.SelectSKUGroupTypeByUID(sKUGroupType.UID);
                if (existingDetails != null)
                {
                    sKUGroupType.ServerModifiedTime = DateTime.Now;
                    var ratValue= await _skuGroupTypeBL.UpdateSKUGroupType(sKUGroupType);
                    return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SKUGroup Type Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteSKUGroupTypeByUID")]
        public async Task<ActionResult> DeleteSKUGroupTypeByUID([FromQuery] string UID)
        {
            try
            {
                var result = await _skuGroupTypeBL.DeleteSKUGroupTypeByUID(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectSKUGroupTypeView")]
        public async Task<ActionResult> SelectSKUGroupTypeView()
        {
            try
            {
                IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView> skuGroupTypeList = await _skuGroupTypeBL.SelectSKUGroupTypeView();
                if (skuGroupTypeList != null)
                {
                    return CreateOkApiResponse(skuGroupTypeList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SKUGroupType");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectSKUAttributeDDL")]
        public async Task<ActionResult> SelectSKUAttributeDDL()
        {
            try
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUAttributeLevel skuAttributeLevelList = await _skuGroupTypeBL.SelectSKUAttributeDDL();
                if (skuAttributeLevelList != null)
                {
                    return CreateOkApiResponse(skuAttributeLevelList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SkuAttributeLevelList");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
