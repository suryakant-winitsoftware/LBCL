using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Winit.Shared.Models.Common;
using Winit.Modules.UOM.Model.Interfaces;

namespace WINITAPI.Controllers.UOM
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class UOMTypeController : WINITBaseController
    {
        private readonly Winit.Modules.UOM.BL.Interfaces.IUOMTypeBL _uomTypeBL;

        public UOMTypeController(IServiceProvider serviceProvider, 
            Winit.Modules.UOM.BL.Interfaces.IUOMTypeBL uomTypeBL) : base(serviceProvider)
        {
            _uomTypeBL = uomTypeBL;
        }
        [HttpPost]
        [Route("SelectAllUOMTypeDetails")]
        public async Task<ActionResult> SelectAllUOMTypeDetails(
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
                PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType> pagedResponseUomTypeList = null;


                pagedResponseUomTypeList = await _uomTypeBL.SelectAllUOMTypeDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseUomTypeList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseUomTypeList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve UOMType  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

      




    }
}
