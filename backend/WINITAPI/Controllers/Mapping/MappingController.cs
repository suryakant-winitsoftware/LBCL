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
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Org.BouncyCastle.Bcpg.OpenPgp;


namespace WINITAPI.Controllers.Mapping;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MappingController : WINITBaseController
{
    private readonly Winit.Modules.Mapping.BL.Interfaces.ISelectionMapCriteriaBL _selectionMapCriteriaBL;

    public MappingController(IServiceProvider serviceProvider, 
        Winit.Modules.Mapping.BL.Interfaces.ISelectionMapCriteriaBL selectionMapCriteriaBL) 
        : base(serviceProvider)
    {
        _selectionMapCriteriaBL = selectionMapCriteriaBL;
    }
    [HttpPost]
    [Route("CUDSelectionMapMaster")]
    public async Task<ActionResult> CUDSelectionMapMaster([FromBody] Winit.Modules.Mapping.Model.Classes.SelectionMapMasterDTO selectionMapMasterDTO)
    {
        try
        {
            if (selectionMapMasterDTO != null)
            {
                DateTime currentDate = DateTime.Now;
                ISelectionMapMaster selectionMapMaster = new SelectionMapMaster
                {
                    SelectionMapDetails = selectionMapMasterDTO.SelectionMapDetails?.ToList<ISelectionMapDetails>() ?? new List<ISelectionMapDetails>(),
                    SelectionMapCriteria = selectionMapMasterDTO.SelectionMapCriteria ?? new SelectionMapCriteria()
                };
                selectionMapMaster.SelectionMapCriteria.ServerAddTime = currentDate;
                selectionMapMaster.SelectionMapCriteria.ServerModifiedTime = currentDate;
                selectionMapMaster.SelectionMapDetails.ForEach(e =>
                {
                    e.ServerAddTime = currentDate;
                    e.ServerModifiedTime = currentDate;
                });
                var retVal = await _selectionMapCriteriaBL.CUDSelectionMapMaster(selectionMapMaster);
                return (retVal) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            else{
                return CreateErrorResponse("Deserialization error", 500);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create SelectionMapMaster details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("SelectAllSelectionMapCriteria")]
    public async Task<ActionResult> SelectAllSelectionMapCriteria([FromBody] PagingRequest pagingRequest)
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

            var pagedResponse = await _selectionMapCriteriaBL.SelectAllSelectionMapCriteria(
                pagingRequest.SortCriterias,
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (pagedResponse == null)
            {
                return NotFound();
            }

            return CreateOkApiResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SelectionMapCriteria list");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("GetSelectionMapMasterByLinkedItemUID")]
    public async Task<ActionResult> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
    {
        try
        {
            if (string.IsNullOrEmpty(linkedItemUID))
            {
                return BadRequest("Linked Item UID is required");
            }

            var retVal = await _selectionMapCriteriaBL.GetSelectionMapMasterByLinkedItemUID(linkedItemUID);

            if (retVal == null)
            {
                // Return 404 Not Found when no mapping exists
                return NotFound($"No selection map found for linked item UID: {linkedItemUID}");
            }

            return CreateOkApiResponse(retVal);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SelectionMapMaster details for UID: {LinkedItemUID}", linkedItemUID);
            return CreateErrorResponse("An error occurred while processing the request: " + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSelectionMapMasterByCriteriaUID")]
    public async Task<ActionResult> GetSelectionMapMasterByCriteriaUID(string criteriaUID)
    {
        try
        {
            if (string.IsNullOrEmpty(criteriaUID))
            {
                return BadRequest("Criteria UID is required");
            }

            var retVal = await _selectionMapCriteriaBL.GetSelectionMapMasterByCriteriaUID(criteriaUID);

            if (retVal == null)
            {
                // Return 404 Not Found when no mapping exists
                return NotFound($"No selection map found for criteria UID: {criteriaUID}");
            }

            return CreateOkApiResponse(retVal);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SelectionMapMaster details for criteria UID: {CriteriaUID}", criteriaUID);
            return CreateErrorResponse("An error occurred while processing the request: " + ex.Message);
        }
    }
}

