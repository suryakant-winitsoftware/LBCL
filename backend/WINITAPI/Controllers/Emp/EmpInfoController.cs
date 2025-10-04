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


namespace WINITAPI.Controllers.Emp
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpInfoController:WINITBaseController
    {
        private readonly Winit.Modules.Emp.BL.Interfaces.IEmpInfoBL _EmpInfoBL;

        public EmpInfoController(IServiceProvider serviceProvider, 
            Winit.Modules.Emp.BL.Interfaces.IEmpInfoBL EmpInfoBL) 
            : base(serviceProvider)
        {
            _EmpInfoBL = EmpInfoBL;
        }
        [HttpPost]
        [Route("GetEmpInfoDetails")]
        public async Task<ActionResult> GetEmpInfoDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> pagedResponseEmpInfoList = null;
                pagedResponseEmpInfoList = await _EmpInfoBL.GetEmpInfoDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseEmpInfoList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseEmpInfoList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve EmpInfoDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetEmpInfoByUID")]
        public async Task<ActionResult> GetEmpInfoByUID(string UID)
        {
            try
            {
                Winit.Modules.Emp.Model.Interfaces.IEmpInfo EmpInfoDetails = await _EmpInfoBL.GetEmpInfoByUID(UID);
                if (EmpInfoDetails != null)
                {
                    return CreateOkApiResponse(EmpInfoDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpInfoDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateEmpInfo")]
        public async Task<ActionResult> CreateEmpInfo([FromBody] Winit.Modules.Emp.Model.Classes.EmpInfo EmpInfo)
        {
            try
            {
                EmpInfo.ServerModifiedTime = DateTime.Now;
                var retVal = await _EmpInfoBL.CreateEmpInfo(EmpInfo);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Emp details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateEmpInfoDetails")]
        public async Task<ActionResult> UpdateEmpInfoDetails([FromBody] Winit.Modules.Emp.Model.Classes.EmpInfo updateEmpInfo)
        {
            try
            {
                var existingEmpInfoDetails = await _EmpInfoBL.GetEmpInfoByUID(updateEmpInfo.UID);
                if (existingEmpInfoDetails != null)
                {
                    updateEmpInfo.ServerModifiedTime = DateTime.Now;
                    var retVal = await _EmpInfoBL.UpdateEmpInfoDetails(updateEmpInfo);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating EmpInfoDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteEmpInfoDetails")]
        public async Task<ActionResult> DeleteEmpInfoDetails([FromQuery] string UID)
        {
            try
            {
                var retVal = await _EmpInfoBL.DeleteEmpInfoDetails(UID);
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
