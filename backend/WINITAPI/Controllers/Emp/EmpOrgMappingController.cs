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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;




namespace WINITAPI.Controllers.Emp
{
    [Route("api/[controller]")]
    [ApiController]
     [Authorize]
    public class EmpOrgMappingController : WINITBaseController
    {
        private readonly Winit.Modules.Emp.BL.Interfaces.IEmpOrgMappingBL _empOrgMappingBL;

        public EmpOrgMappingController(IServiceProvider serviceProvider, 
            Winit.Modules.Emp.BL.Interfaces.IEmpOrgMappingBL empOrgMappingBL) 
            : base(serviceProvider)
        {
            _empOrgMappingBL = empOrgMappingBL;
        }

        [HttpPost]
        [Route("GetEmpOrgMappingDetails")]
        public async Task<ActionResult> GetEmpOrgMappingDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> PagedResponse = null;
                PagedResponse = await _empOrgMappingBL.GetEmpOrgMappingDetails(pagingRequest.SortCriterias,
                   pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                   pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(PagedResponse);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve EmpOrgMapping Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateEmpOrgMapping")]
        public async Task<ActionResult> CreateEmpOrgMapping([FromBody] List<Winit.Modules.Emp.Model.Classes.EmpOrgMapping> empOrgMappings)
        {
            try
            {
                var retVal = await _empOrgMappingBL.CreateEmpOrgMapping(empOrgMappings);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create EmpOrgMapping details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        [HttpDelete]
        [Route("DeleteEmpOrgMapping")]
        public async Task<ActionResult> DeleteEmpOrgMapping(string uid)
        {
            try
            {
                var retVal = await _empOrgMappingBL.DeleteEmpOrgMapping(uid);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed EmpOrgMapping ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpGet]
        [Route("GetEmpOrgMappingDetailsByEmpUID")]
        public async Task<ActionResult> GetEmpOrgMappingDetailsByEmpUID(string empUID)
        {
            try
            {
                var retVal = await _empOrgMappingBL.GetEmpOrgMappingDetailsByEmpUID(empUID);
                if(retVal is not null && retVal.Any())
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    return CreateErrorResponse("No data Found", 404);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed EmpOrgMapping ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
    }
}
