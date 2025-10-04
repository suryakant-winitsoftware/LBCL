using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.UIInterfaces;


namespace WINITAPI.Controllers.SKUClass;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class SKUClassGroupController : WINITBaseController
{
    private readonly Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupBL _sKUClassGroupBL;

    public SKUClassGroupController(IServiceProvider serviceProvider, 
        Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupBL sKUClassGroupBL) : base(serviceProvider)
    {
        _sKUClassGroupBL = sKUClassGroupBL;
    }
    [HttpPost]
    [Route("SelectAllSKUClassGroupDetails")]
    public async Task<ActionResult> SelectAllSKUClassGroupDetails(
        PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup> pagedResponseSKUClassGroupList = null;
            pagedResponseSKUClassGroupList = await _sKUClassGroupBL.SelectAllSKUClassGroupDetails(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
            if (pagedResponseSKUClassGroupList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(pagedResponseSKUClassGroupList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKUClassGroup  Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSKUClassGroupByUID")]
    public async Task<ActionResult> GetSKUClassGroupByUID([FromQuery] string UID)
    {
        try
        {
            Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup sKUClassGroup = await _sKUClassGroupBL.GetSKUClassGroupByUID(UID);
            if (sKUClassGroup != null)
            {
                return CreateOkApiResponse(sKUClassGroup);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUClassGroupList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateSKUClassGroup")]
    public async Task<ActionResult> CreateSKUClassGroup([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClassGroup createSKUClassGroup)
    {
        try
        {
            createSKUClassGroup.ServerAddTime = DateTime.Now;
            createSKUClassGroup.ServerModifiedTime = DateTime.Now;
            var retValue = await _sKUClassGroupBL.CreateSKUClassGroup(createSKUClassGroup);
            return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Create failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKUClassGroup details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPut]
    [Route("UpdateSKUClassGroup")]
    public async Task<ActionResult> UpdateSKUClassGroup([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClassGroup updateSKUClassGroup)
    {
        try
        {
            var existingDetails = await _sKUClassGroupBL.GetSKUClassGroupByUID(updateSKUClassGroup.UID);
            if (existingDetails != null)
            {
                updateSKUClassGroup.ServerModifiedTime = DateTime.Now;
                var retValue = await _sKUClassGroupBL.UpdateSKUClassGroup(updateSKUClassGroup);
                return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Update Failed");
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating SKUClassGroup Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteSKUClassGroup")]
    public async Task<ActionResult> DeleteSKUClassGroup([FromQuery] string UID)
    {
        try
        {
            var result = await _sKUClassGroupBL.DeleteSKUClassGroup(UID);
            return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }[HttpDelete]
    [Route("DeleteSKUClassGroupMaster")]
    public async Task<ActionResult> DeleteSKUClassGroupMaster([FromQuery] string sKUClassGroupUID)
    {
        try
        {
            var result = await _sKUClassGroupBL.DeleteSKUClassGroupMaster(sKUClassGroupUID);
            return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("CUD_SKUClassGroupMaster")]
    public async Task<ActionResult> CUD_SKUClassGroupMaster([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClassGroupDTO sKUClassGroupDTO)
    {
        try
        {
            if (sKUClassGroupDTO != null)
            {
                DateTime currentDate = DateTime.Now;
                ISKUClassGroupMaster sKUClassGroupMaster = new SKUClassGroupMaster
                {
                    SKUClassGroupItems = sKUClassGroupDTO.SKUClassGroupItems?.ToList<ISKUClassGroupItemView>() ?? new List<ISKUClassGroupItemView>(),
                    SKUClassGroup = sKUClassGroupDTO.SKUClassGroup ?? new SKUClassGroup()
                };
                sKUClassGroupMaster.SKUClassGroup.ServerAddTime = currentDate;
                sKUClassGroupMaster.SKUClassGroup.ServerModifiedTime = currentDate;
                sKUClassGroupMaster.SKUClassGroupItems.ForEach(e =>
                {
                    e.ServerAddTime = currentDate;
                    e.ServerModifiedTime = currentDate;
                });
                var retVal = await _sKUClassGroupBL.CUD_SKUClassGroupMaster(sKUClassGroupMaster);
                return (retVal) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            else
            {
                return CreateErrorResponse("Deserialization error", 500);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKUClassGroup details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }

    }
    [HttpPost]
    [Route("GetSKUClassGroupMaster")]
    public async Task<ActionResult> GetSKUClassGroupMaster([FromQuery] string sKUClassGroupUID)
    {
        try
        {
            Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster sKUClassGroupMaster = await _sKUClassGroupBL.GetSKUClassGroupMaster(sKUClassGroupUID);
            if (sKUClassGroupMaster != null)
            {
                return CreateOkApiResponse(sKUClassGroupMaster);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUClassGroupMaster with UID: {@UID}", sKUClassGroupUID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

}

