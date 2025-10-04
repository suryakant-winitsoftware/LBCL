using DBServices.Classes;
using DBServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NRediSearch.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Web.Paging;
using WINITAPI.Controllers.CollectionModule;

namespace WINITAPI.Controllers.Tally
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class TallyController : WINITBaseController
    {
        private readonly Winit.Modules.Tally.BL.Interfaces.ITallyMappingBL _tallySKUMappingService;
        private readonly IDBService _dbService;
        private readonly ILogger<TallyController> _logger;
        private readonly string queueName;
        public TallyController(IServiceProvider serviceProvider, 
            Winit.Modules.Tally.BL.Interfaces.ITallyMappingBL tallySKUMappingService, 
            IDBService dbService,
        ILogger<TallyController> logger) : base(serviceProvider)
        {
            _tallySKUMappingService = tallySKUMappingService;
            _dbService = dbService;
            _logger = logger;
        }

        [HttpPost]
        [Route("GetAllTallySKU")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallySKU>>> GetAllTallySKU([FromBody] PagingRequest pagingRequest)
        {
            try
            {
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKU> TallySKUList = await _tallySKUMappingService.GetAllTallySKU(pagingRequest.SortCriterias, pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired);
                if (TallySKUList == null)
                {
                    return BadRequest();
                }
                else
                {
                }
                return CreateOkApiResponse(TallySKUList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("GetAllTallySKUMappingByDistCode")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>>> GetAllTallySKUMappingByDistCode([FromBody] PagingRequest pagingRequest, [FromQuery] string Code, [FromQuery] string Tab)
        {
            try
            {
                if (string.IsNullOrEmpty(Code))
                {
                    return BadRequest();
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> PagedResponseTallySKUList = null;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                PagedResponseTallySKUList = await _tallySKUMappingService.GetAllTallySKUMappingByDistCode(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired, Code, Tab);
                if (PagedResponseTallySKUList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(PagedResponseTallySKUList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("SelectSKUByOrgUID")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKU>>> SelectSKUByOrgUID([FromQuery] string OrgUID)
        {
            try
            {
                List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUList = await _tallySKUMappingService.SelectSKUByOrgUID(OrgUID);
                if (SKUList == null)
                {
                    return BadRequest();
                }
                else
                {
                }
                return CreateOkApiResponse(SKUList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("GetAllDistributors")]
        public async Task<ActionResult> GetAllDistributors()
        {
            try
            {
                List<IEmp> SKUList = await _tallySKUMappingService.GetAllDistributors();
                if (SKUList == null)
                {
                    return BadRequest();
                }
                else
                {
                }
                return CreateOkApiResponse(SKUList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("InsertTallySKUMapping")]
        public async Task<ActionResult> InsertTallySKUMapping([FromBody] List<ITallySKUMapping> tallySKUMappings)
        {
            try
            {
                bool retValue = false;
                foreach (var data in tallySKUMappings)
                {
                    var existingTallySku = await _tallySKUMappingService.SelectTallySKUMappingBySKUCode(data.PrincipalSKUCode, data.DistributorCode);
                    if (existingTallySku != null)
                    {
                        data.ModifiedTime = DateTime.Now;
                        data.ServerModifiedTime = DateTime.Now;
                        retValue = await _tallySKUMappingService.UpdateTallySKUMapping(data);
                    }
                    else
                    {
                        retValue = await _tallySKUMappingService.InsertTallySKUMapping(data);
                    }
                    if (!retValue)
                    {
                        return StatusCode(500, retValue);
                    }
                }
                return CreateOkApiResponse(retValue);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details " + ex.ToString());
            }
        }
        [HttpPut]
        [Route("UpdateTallySKUMapping")]
        public async Task<ActionResult> UpdateTallySKUMapping([FromBody] List<ITallySKUMapping> tallySKUMappings)
        {
            try
            {
                bool retValue = false;
                foreach (var data in tallySKUMappings)
                {
                    data.ModifiedTime = DateTime.Now;
                    data.ServerModifiedTime = DateTime.Now;
                    retValue = await _tallySKUMappingService.UpdateTallySKUMapping(data);
                    if (!retValue)
                    {
                        return StatusCode(500, retValue);
                    }
                }
                return CreateOkApiResponse(retValue);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create details " + ex.ToString());
            }
        }
    }
}
