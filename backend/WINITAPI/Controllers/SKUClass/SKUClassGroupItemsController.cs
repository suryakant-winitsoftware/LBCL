using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.SKUClass;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class SKUClassGroupItemsController : WINITBaseController
{
    private readonly Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL _sKUClassGroupItemsBL;

    public SKUClassGroupItemsController(IServiceProvider serviceProvider, 
        Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL sKUClassGroupItemsBL) : base(serviceProvider)
    {
        _sKUClassGroupItemsBL = sKUClassGroupItemsBL;
    }
    [HttpPost]
    [Route("SelectAllSKUClassGroupItemsDetails")]
    public async Task<ActionResult> SelectAllSKUClassGroupItemsDetails(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> pagedResponseSKUClassGroupItemsList = null;
            pagedResponseSKUClassGroupItemsList = await _sKUClassGroupItemsBL.SelectAllSKUClassGroupItemsDetails(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
            if (pagedResponseSKUClassGroupItemsList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(pagedResponseSKUClassGroupItemsList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKUClassGroupItems  Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSKUClassGroupItemsByUID")]
    public async Task<ActionResult> GetSKUClassGroupItemsByUID([FromQuery] string UID)
    {
        try
        {
            Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems sKUClassGroupItems = await _sKUClassGroupItemsBL.GetSKUClassGroupItemsByUID(UID);
            if (sKUClassGroupItems != null)
            {
                return CreateOkApiResponse(sKUClassGroupItems);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SKUClassGroupItemsList with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateSKUClassGroupItems")]
    public async Task<ActionResult> CreateSKUClassGroupItems([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClassGroupItems createSKUClassGroupItems)
    {
        try
        {
            createSKUClassGroupItems.ServerAddTime = DateTime.Now;
            createSKUClassGroupItems.ServerModifiedTime = DateTime.Now;
            var retValue = await _sKUClassGroupItemsBL.CreateSKUClassGroupItems(createSKUClassGroupItems);
            return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Create failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SKUClassGroupItems details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPut]
    [Route("UpdateSKUClassGroupItems")]
    public async Task<ActionResult> UpdateSKUClassGroupItems([FromBody] Winit.Modules.SKUClass.Model.Classes.SKUClassGroupItems updateSKUClassGroupItems)
    {
        try
        {
            var existingDetails = await _sKUClassGroupItemsBL.GetSKUClassGroupItemsByUID(updateSKUClassGroupItems.UID);
            if (existingDetails != null)
            {
                updateSKUClassGroupItems.ServerModifiedTime = DateTime.Now;
                var retValue = await _sKUClassGroupItemsBL.UpdateSKUClassGroupItems(updateSKUClassGroupItems);
                return (retValue > 0) ? CreateOkApiResponse(retValue) : throw new Exception("Update Failed");
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating SKUClassGroupItems Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteSKUClassGroupItems")]
    public async Task<ActionResult> DeleteSKUClassGroupItems([FromQuery] string UID)
    {
        try
        {
            var result = await _sKUClassGroupItemsBL.DeleteSKUClassGroupItems(UID);
            return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("SelectAllSKUClassGroupItemView")]
    public async Task<ActionResult> SelectAllSKUClassGroupItemView(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView>  pagedResponseSKUClassGroupItemsList = null;
            pagedResponseSKUClassGroupItemsList = await _sKUClassGroupItemsBL.SelectAllSKUClassGroupItemView(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
            if (pagedResponseSKUClassGroupItemsList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(pagedResponseSKUClassGroupItemsList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve SKUClassGroupItems  Details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}

