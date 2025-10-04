using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using Winit.Modules.SKU.Model.Classes;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITServices.Classes.CacheHandler;
using WINITServices.Interfaces.CacheHandler;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Interfaces;

namespace WINITAPI.Controllers.SKU
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class SkuSequenceController : WINITBaseController
    {
        private readonly Winit.Modules.SKU.BL.Interfaces.ISkuSequenceBL _skuSequenceBL;
        private readonly ICacheService _cache;
        public SkuSequenceController(IServiceProvider serviceProvider, 
            Winit.Modules.SKU.BL.Interfaces.ISkuSequenceBL skuSequenceBL) : base(serviceProvider)
        {
            _skuSequenceBL = skuSequenceBL;
        }

        [HttpPost]
        [Route("SelectAllSkuSequenceDetails")]
        public async Task<ActionResult> SelectAllSkuSequenceDetails(PagingRequest pagingRequest, [FromQuery]string SeqType)
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
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence> PagedResponseList = null;
               
                PagedResponseList = await _skuSequenceBL.SelectAllSkuSequenceDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, SeqType);
                if (PagedResponseList == null)
                {
                    return NotFound();
                }
             
                return CreateOkApiResponse(PagedResponseList);

            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SkuSequence Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDSkuSequence")]
        public async Task<ActionResult> CUDSkuSequence(List<Winit.Modules.SKU.Model.Classes.SkuSequence> skuSequencesList)
        {
            try
            {
                int returnValue = await _skuSequenceBL.CUDSkuSequence(skuSequencesList);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Operation Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKU Sequence details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateGeneralSKUSequenceForSKU")]
        public async Task<ActionResult> CreateGeneralSKUSequenceForSKU(string BUOrgUID, string SKUUID)
        {
            try
            {
                int returnValue = await _skuSequenceBL.CreateGeneralSKUSequenceForSKU(BUOrgUID, SKUUID);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Operation Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SKU Sequence details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



    }

}

