using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.SKU;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class SKUGroupController : WINITBaseController
{
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUGroupBL _skuGroupBL;

    public SKUGroupController(IServiceProvider serviceProvider, 
        Winit.Modules.SKU.BL.Interfaces.ISKUGroupBL skuGroupBL) : base(serviceProvider)
    {
        _skuGroupBL = skuGroupBL;
    }
    [HttpPost]
    [Route("SelectAllSKUGroupDetails")]
    public async Task<ActionResult> SelectAllSKUGroupDetails(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> pagedResponseSKUGroupList = null;
            if (pagedResponseSKUGroupList != null)
            {
                return CreateOkApiResponse(pagedResponseSKUGroupList);
            }
            pagedResponseSKUGroupList = await _skuGroupBL.SelectAllSKUGroupDetails(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
            if (pagedResponseSKUGroupList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(pagedResponseSKUGroupList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKU Group Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectSKUGroupByUID")]
    public async Task<ActionResult> SelectSKUGroupByUID([FromQuery] string UID)
    {
        try
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUGroup skuGroupList = await _skuGroupBL.SelectSKUGroupByUID(UID);
            return skuGroupList != null ? CreateOkApiResponse(skuGroupList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUGroup with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateSKUGroup")]
    public async Task<ActionResult> CreateSKUGroup([FromBody] Winit.Modules.SKU.Model.Classes.SKUGroup sKUGroup)
    {
        try
        {
            sKUGroup.ServerAddTime = DateTime.Now;
            sKUGroup.ServerModifiedTime = DateTime.Now;
            int ratValue = await _skuGroupBL.CreateSKUGroup(sKUGroup);
            return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Create failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKUGroup details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("InsertSKUGroupHierarchy")]
    public async Task<ActionResult> InsertSKUGroupHierarchy(string type, string uid)
    {
        try
        {
            int retValue = await _skuGroupBL.InsertSKUGroupHierarchy(type, uid);
            return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Create failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKUGroup hierarchy details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateSKUGroup")]
    public async Task<ActionResult> UpdateSKUGroup([FromBody] Winit.Modules.SKU.Model.Classes.SKUGroup sKUGroup)
    {
        try
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUGroup existingDetails = await _skuGroupBL.SelectSKUGroupByUID(sKUGroup.UID);
            if (existingDetails != null)
            {
                sKUGroup.ServerModifiedTime = DateTime.Now;
                int ratValue = await _skuGroupBL.UpdateSKUGroup(sKUGroup);
                return (ratValue > 0) ? CreateOkApiResponse(ratValue) : throw new Exception("Update Failed");
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating SKUGroup Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteSKUGroup")]
    public async Task<ActionResult> DeleteSKUGroup([FromQuery] string UID)
    {
        try
        {
            int result = await _skuGroupBL.DeleteSKUGroup(UID);
            return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectSKUGroupView")]
    public async Task<ActionResult> SelectSKUGroupView()
    {
        try
        {
            IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView> skuGroupList = await _skuGroupBL.SelectSKUGroupView();
            return skuGroupList != null ? CreateOkApiResponse(skuGroupList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUGroup");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("GetSKUGroupSelectionItemBySKUGroupTypeUID")]
    public async Task<ActionResult> GetSKUGroupSelectionItemBySKUGroupTypeUID(string skuGroupTypeUID, string parentUID)
    {
        try
        {
            List<SKUGroupSelectionItem> skuGroupList = await _skuGroupBL.GetSKUGroupSelectionItemBySKUGroupTypeUID(skuGroupTypeUID, parentUID);
            return skuGroupList != null ? CreateOkApiResponse(skuGroupList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUGroup");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("GetAllSKUGroupBySKUGroupTypeUID")]
    public async Task<ActionResult> GetAllSKUGroupBySKUGroupTypeUID(string skuGroupTypeUID)
    {
        try
        {
            List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> skuGroupList = await _skuGroupBL.GetSKUGroupBySKUGroupTypeUID(skuGroupTypeUID);
            return skuGroupList != null ? CreateOkApiResponse(skuGroupList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUGroup");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("SelectAllSKUGroupItemViews")]
    public async Task<ActionResult> SelectAllSKUGroupItemViews(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView> skuGroupList = await _skuGroupBL.SelectAllSKUGroupItemViews
                (pagingRequest.SortCriterias, pagingRequest.FilterCriterias);
            return skuGroupList != null ? CreateOkApiResponse(skuGroupList) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUGroup");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
