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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITAPI.Controllers.SKU;

namespace WINITAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityModuleController : WINITBaseController
    {
        private readonly Winit.Modules.Survey.BL.Interfaces.IActivityModuleBL _activityModuleBL;

        public ActivityModuleController(IServiceProvider serviceProvider,
           Winit.Modules.Survey.BL.Interfaces.IActivityModuleBL activityModuleBL) : base(serviceProvider)
        {
            _activityModuleBL = activityModuleBL;
        }
        [HttpPost]
        [Route("GetAllActivityModuleDeatils")]
        public async Task<ActionResult> GetAllActivityModuleDeatils(
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule> pagedResponseActivityModule = null;
                pagedResponseActivityModule = await _activityModuleBL.GetAllActivityModuleDeatils(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseActivityModule == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseActivityModule);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Activity Module Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
