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


namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CityBranchController : WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ICityBranchMappingBL _cityBranchMappingBL;
        public CityBranchController(IServiceProvider serviceProvider, 
            Winit.Modules.Location.BL.Interfaces.ICityBranchMappingBL cityBranchMappingBL) 
            : base(serviceProvider)
        {
            _cityBranchMappingBL = cityBranchMappingBL;
        }
        [HttpPost]
        [Route("SelectCityBranchDetails")]
        public async Task<ActionResult> SelectCityBranchDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch> PagedResponseCityBranchList = null;
                PagedResponseCityBranchList = await _cityBranchMappingBL.SelectCityBranchDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseCityBranchList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseCityBranchList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve City Branch Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("SelectBranchDetails")]
        public async Task<ActionResult> SelectBranchDetails()
        {
            try
            {
                List<ISelectionItem> selectionItems = await _cityBranchMappingBL.SelectBranchDetails();
                if (selectionItems == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(selectionItems);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve  Branch Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateCityBranchMapping")]
        public async Task<ActionResult> CreateCityBranchMapping([FromBody] List<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping> cityBranchMappings)
        {
            try
            {
                var retVal = await _cityBranchMappingBL.CreateCityBranchMapping(cityBranchMappings);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create City Branch details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}
