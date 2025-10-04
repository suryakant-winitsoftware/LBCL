using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Scheme.Model.Interfaces;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class StandingProvisionSchemeBranchController : WINITBaseController
    {
        private readonly IStandingProvisionSchemeBranchBL _standingProvisionSchemeBranchBL;

        public StandingProvisionSchemeBranchController(IServiceProvider serviceProvider, 
            IStandingProvisionSchemeBranchBL standingProvisionSchemeBranchBL) : base(serviceProvider)
        {
            _standingProvisionSchemeBranchBL = standingProvisionSchemeBranchBL;
        }

        [HttpPost]
        [Route("SelectAllStandingProvisionBranches")]
        public async Task<ActionResult> SelectAllStandingProvisionBranches(PagingRequest pagingRequest)
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

                var pagedResponse = await _standingProvisionSchemeBranchBL.SelectAllStandingProvisionBranches(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StandingProvisionSchemeBranchDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStandingProvisionSchemeBranchDetailsByUID")]
        public async Task<ActionResult> GetStandingProvisionSchemeBranchDetailsByUID(string UID)
        {
            try
            {
                var details = await _standingProvisionSchemeBranchBL.GetStandingProvisionBranchByUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StandingProvisionSchemeBranchDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateStandingProvisionSchemeBranchDetails")]
        public async Task<ActionResult> CreateStandingProvisionSchemeBranchDetails([FromBody] StandingProvisionSchemeBranch standingProvisionSchemeBranch)
        {
            try
            {
                standingProvisionSchemeBranch.ServerAddTime = DateTime.Now;
                standingProvisionSchemeBranch.ServerModifiedTime = DateTime.Now;
                var retVal = await _standingProvisionSchemeBranchBL.CreateStandingProvisionBranch(standingProvisionSchemeBranch);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StandingProvisionSchemeBranch details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateStandingProvisionSchemeBranchDetails")]
        public async Task<ActionResult> UpdateStandingProvisionSchemeBranchDetails([FromBody] StandingProvisionSchemeBranch standingProvisionSchemeBranch)
        {
            try
            {
                var existingDetails = await _standingProvisionSchemeBranchBL.GetStandingProvisionBranchByUID(standingProvisionSchemeBranch.UID);
                if (existingDetails != null)
                {
                    standingProvisionSchemeBranch.ServerModifiedTime = DateTime.Now;
                    var retVal = await _standingProvisionSchemeBranchBL.UpdateStandingProvisionBranch(standingProvisionSchemeBranch);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating StandingProvisionSchemeBranch details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteStandingProvisionSchemeBranchDetail")]
        public async Task<ActionResult> DeleteStandingProvisionSchemeBranchDetail([FromQuery] string UID)
        {
            try
            {
                var retVal = await _standingProvisionSchemeBranchBL.DeleteStandingProvisionBranch(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}