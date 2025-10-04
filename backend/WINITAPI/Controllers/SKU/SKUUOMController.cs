using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.SKU;
using WINITServices.Interfaces.CacheHandler;


namespace WINITAPI.Controllers.SKU;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class SKUUOMController : WINITBaseController
{
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUUOMBL _skuUOMBL;

    private readonly DataPreparationController _dataPreparationController;
    public SKUUOMController(IServiceProvider serviceProvider, 
        Winit.Modules.SKU.BL.Interfaces.ISKUUOMBL skuUOMBL,
        DataPreparationController dataPreparationController) :
        base(serviceProvider)
    {
        _skuUOMBL = skuUOMBL;
        _dataPreparationController = dataPreparationController;
    }
    [HttpPost]
    [Route("SelectAllSKUUOMDetails")]
    public async Task<ActionResult> SelectAllSKUUOMDetails(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> pagedResponseSKUUOMList = null;
            pagedResponseSKUUOMList = await _skuUOMBL.SelectAllSKUUOMDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
            if (pagedResponseSKUUOMList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(pagedResponseSKUUOMList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKU UOM  Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectSKUUOMByUID")]
    public async Task<ActionResult> SelectSKUUOMByUID([FromQuery] string UID)
    {
        try
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuUOMList = await _skuUOMBL.SelectSKUUOMByUID(UID);
            return skuUOMList != null ? CreateOkApiResponse(skuUOMList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUUOM with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateSKUUOM")]
    public async Task<ActionResult> CreateSKUUOM([FromBody] Winit.Modules.SKU.Model.Classes.SKUUOM skuUOM)
    {
        try
        {
            List<string> sKUUIDs = [];
            if (skuUOM != null)
            {
                sKUUIDs.Add(skuUOM.SKUUID);
            }
            skuUOM.ServerAddTime = DateTime.Now;
            skuUOM.ServerModifiedTime = DateTime.Now;
            int ratValue = await _skuUOMBL.CreateSKUUOM(skuUOM);
            if (ratValue > 0)
            {
                PrepareSKURequestModel prepareSKURequestModel = new()
                {
                    SKUUIDs = new List<string> { skuUOM.UID }
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
            Log.Error(ex, "Failed to create SKU UOM details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPut]
    [Route("UpdateSKUUOM")]
    public async Task<ActionResult> UpdateSKUUOM([FromBody] Winit.Modules.SKU.Model.Classes.SKUUOM skuUOM)
    {
        try
        {
            List<string> SKUUIDs = [];
            if (skuUOM != null)
            {
                SKUUIDs.Add(skuUOM.SKUUID);
            }

            Winit.Modules.SKU.Model.Interfaces.ISKUUOM existingDetails = await _skuUOMBL.SelectSKUUOMByUID(skuUOM.UID);
            if (existingDetails == null)
            {
                return NotFound();
            }
            skuUOM.ServerModifiedTime = DateTime.Now;
            int ratValue = await _skuUOMBL.UpdateSKUUOM(skuUOM);
            if (ratValue > 0)
            {
                PrepareSKURequestModel prepareSKURequestModel = new()
                {
                    SKUUIDs = new List<string> { skuUOM.UID }
                };
                _ = await _dataPreparationController.PrepareSKUMaster(prepareSKURequestModel);
                return CreateOkApiResponse(ratValue);
            }
            else
            {

                throw new Exception("Update Failed");
            }

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating SKU UOM Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteSKUUOMByUID")]
    public async Task<ActionResult> DeleteSKUUOMByUID([FromQuery] string UID)
    {
        try
        {
            int result = await _skuUOMBL.DeleteSKUUOMByUID(UID);
            if (result > 0)
            {
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
