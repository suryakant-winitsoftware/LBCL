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
using Winit.Modules.Scheme.DL.Classes;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ManageSchemeController : WINITBaseController
    {
        private readonly ISchemesBL _schemeBL;
        public ManageSchemeController(IServiceProvider serviceProvider, 
            ISchemesBL schemeBL) : base(serviceProvider)
        {
            _schemeBL = schemeBL;
        }

        [HttpPost]
        [Route("SelectAllSchemes/{jobPositionUid}/{isAdmin}")]
        public async Task<ActionResult> SelectAllSchemes([FromBody] PagingRequest pagingRequest, string jobPositionUid, bool isAdmin)
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

                var pagedResponse = await _schemeBL.SelectAllSchemes(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, jobPositionUid, isAdmin);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SalesPromotionSchemes");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetChannelPartner/{isAdmin}")]
        public async Task<ActionResult> GetChannelPartner([FromQuery] string jobPositionUID, bool isAdmin)
        {
            try
            {

                //if (string.IsNullOrEmpty(branchUID))
                //{
                //    return BadRequest("branchUID cannot be null or empty.");
                //}
                var pagedResponse = await _schemeBL.GetChannelPartner(jobPositionUID, isAdmin);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Channel Partner");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetschemeExtendHistoryBySchemeUID/{schemeUID}")]
        public async Task<ActionResult> GetschemeExtendHistoryBySchemeUID( string schemeUID)
        {
            try
            {

                //if (string.IsNullOrEmpty(branchUID))
                //{
                //    return BadRequest("branchUID cannot be null or empty.");
                //}
                var pagedResponse = await _schemeBL.GetschemeExtendHistoryBySchemeUID(schemeUID);
                if (pagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve History");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPost]
        [Route("InsertSchemeExtendHistory")]
        public async Task<ActionResult> InsertSchemeExtendHistory(ISchemeExtendHistory schemeExtendHistory)
        {
            try
            {
                if (schemeExtendHistory == null)
                {
                    return BadRequest();
                }

                return CreateOkApiResponse(await _schemeBL.InsertSchemeExtendHistory(schemeExtendHistory));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create History");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }


    }
}