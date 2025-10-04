using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINITAPI.Controllers.SKU;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers.Tax
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class TaxController : WINITBaseController
    {
        private readonly Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL _taxMasterBL;
        private DataPreparationController _preparationController;
        public TaxController(IServiceProvider serviceProvider, 
            Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL taxMasterBL,
            DataPreparationController preparationController) : base(serviceProvider)
        {
            _taxMasterBL = taxMasterBL;
            _preparationController = preparationController;
        }
        [HttpPost]
        [Route("GetTaxDetails")]
        public async Task<ActionResult> GetTaxDetails(PagingRequest pagingRequest)
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
                PagedResponse<ITax> PagedResponseTaxList = null;

                PagedResponseTaxList = await _taxMasterBL.GetTaxDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                return PagedResponseTaxList == null ? NotFound() : CreateOkApiResponse(PagedResponseTaxList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Tax Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetTaxByUID")]
        public async Task<ActionResult<ApiResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>>> GetTaxByUID(string UID)
        {
            try
            {
                ITax TaxDetails = await _taxMasterBL.GetTaxByUID(UID);
                return TaxDetails != null ? (ActionResult<ApiResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>>)CreateOkApiResponse(TaxDetails) : (ActionResult<ApiResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>>)NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TaxeDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateTax")]
        public async Task<ActionResult> CreateTax([FromBody] Winit.Modules.Tax.Model.Classes.Tax createTax)
        {
            try
            {
                createTax.ServerAddTime = DateTime.Now;
                createTax.ServerModifiedTime = DateTime.Now;
                int retVal = await _taxMasterBL.CreateTax(createTax);
                return retVal > 0 ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Tax details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPut]
        [Route("UpdateTax")]
        public async Task<ActionResult> UpdateTax([FromBody] Winit.Modules.Tax.Model.Classes.Tax updateTax)
        {
            try
            {
                ITax existingVehicleDetails = await _taxMasterBL.GetTaxByUID(updateTax.UID);
                if (existingVehicleDetails != null)
                {
                    updateTax.ServerModifiedTime = DateTime.Now;
                    int retVal = await _taxMasterBL.UpdateTax(updateTax);
                    return retVal > 0 ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating TaxDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpDelete]
        [Route("DeleteTax")]
        public async Task<ActionResult> DeleteTax([FromQuery] string UID)
        {
            try
            {
                int retVal = await _taxMasterBL.DeleteTax(UID);
                return retVal > 0 ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetTaxMaster")]
        public async Task<IActionResult> GetTaxMaster([FromBody] List<string> OrgUIDs)
        {
            try
            {

                List<ITaxMaster> taxMasterList = await _taxMasterBL.GetTaxMaster(OrgUIDs);
                Dictionary<string, ITaxMaster> keyvalueTaxMaster = new();
                if (taxMasterList == null)
                {
                    return NotFound();
                }


                //foreach (var taxMaster in taxMasterList)
                //{
                //    if (taxMaster.Tax.UID != null)
                //    {
                //        keyvalueTaxMaster.Add(taxMaster.Tax.UID, taxMaster);
                //    }
                //}
                var taxMasterDictionary = taxMasterList.ToDictionary(e => e.Tax.UID, e => e.Tax);


                return CreateOkApiResponse(taxMasterDictionary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to  Tax Master Details in Cache");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTaxMaster")]
        public async Task<ActionResult> CreateTaxMaster(TaxMasterDTO taxMasterDTO)
        {
            try
            {
                int retVal = await _taxMasterBL.CreateTaxMaster(taxMasterDTO);
                if (retVal > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new()
                    {
                        SKUUIDs = taxMasterDTO.TaxSKUMapList.Select(tsm => tsm.SKUUID).ToList()
                    };
                    _ = await _preparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateTaxMaster")]
        public async Task<ActionResult> UpdateTaxMaster(TaxMasterDTO taxMasterDTO)
        {
            try
            {
                int retVal = await _taxMasterBL.UpdateTaxMaster(taxMasterDTO);
                if (retVal > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new()
                    {
                        SKUUIDs = taxMasterDTO.TaxSKUMapList.Select(tsm => tsm.SKUUID).ToList()
                    };
                    _ = await _preparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("update failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectTaxMasterViewByUID")]
        public async Task<ActionResult> SelectTaxMasterViewByUID(string UID)
        {
            try
            {
                ITaxMaster taxMaster = null;
                taxMaster = await _taxMasterBL.SelectTaxMasterViewByUID(UID);
                return taxMaster != null ? CreateOkApiResponse(taxMaster) : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Tax Master with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("GetTaxGroupDetails")]
        public async Task<ActionResult> GetTaxGroupDetails(PagingRequest pagingRequest)
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
                PagedResponse<ITaxGroup> PagedResponseTaxGroupList = null;

                PagedResponseTaxGroupList = await _taxMasterBL.GetTaxGroupDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                return PagedResponseTaxGroupList == null ? NotFound() : CreateOkApiResponse(PagedResponseTaxGroupList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve TaxGroup Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateTaxGroupMaster")]
        public async Task<ActionResult> CreateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            try
            {
                int retVal = await _taxMasterBL.CreateTaxGroupMaster(taxGroupMasterDTO);
                if (retVal > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new();
                    
                    _ = await _preparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateTaxGroupMaster")]
        public async Task<ActionResult> UpdateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            try
            {
                int retVal = await _taxMasterBL.UpdateTaxGroupMaster(taxGroupMasterDTO);
                if (retVal > 0)
                {
                    PrepareSKURequestModel prepareSKURequestModel = new();

                    _ = await _preparationController.PrepareSKUMaster(prepareSKURequestModel);
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    throw new Exception("Create failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetTaxGroupByUID")]
        public async Task<ActionResult> GetTaxGroupByUID(string UID)
        {
            try
            {
                ITaxGroup TaxGroupDetails = await _taxMasterBL.GetTaxGroupByUID(UID);
                return TaxGroupDetails != null ? CreateOkApiResponse(TaxGroupDetails) : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TaxGroupDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetTaxSelectionItems")]
        public async Task<ActionResult> GetTaxSelectionItems(string UID)
        {
            try
            {
                IEnumerable<ITaxSelectionItem> TaxSelectionItems = await _taxMasterBL.GetTaxSelectionItems(UID);
                return TaxSelectionItems != null ? CreateOkApiResponse(TaxSelectionItems) : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TaxSelectionItems with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
