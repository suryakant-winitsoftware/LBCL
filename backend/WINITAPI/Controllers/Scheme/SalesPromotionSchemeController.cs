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
using Newtonsoft.Json;

namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SalesPromotionSchemeController : WINITBaseController
    {

      

        private readonly ISalesPromotionSchemeBL _salesPromotionSchemeBL;
 

        public SalesPromotionSchemeController(IServiceProvider serviceProvider, 
            ISalesPromotionSchemeBL salesPromotionSchemeBL) : base(serviceProvider)
        {
            _salesPromotionSchemeBL = salesPromotionSchemeBL;
        }

        [HttpPost]
        [Route("SelectAllSalesPromotionScheme")]
        public async Task<ActionResult> SelectAllSalesPromotionScheme(PagingRequest pagingRequest)
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

                var pagedResponse = await _salesPromotionSchemeBL.SelectAllSalesPromotionScheme(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Failed to retrieve SalesPromotionSchemes");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSalesPromotionSchemeByUID")]
        public async Task<ActionResult> GetSalesPromotionSchemeByUID([FromQuery]string UID)
        {
            try
            {
                if (string.IsNullOrEmpty(UID))
                {
                    return BadRequest();
                }
                ISalesPromotionScheme salesPromotionScheme = await _salesPromotionSchemeBL.GetSalesPromotionSchemeByUID(UID);
                if (salesPromotionScheme != null)
                {
                    return CreateOkApiResponse(salesPromotionScheme);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SalesPromotionScheme with UID: {@UID}", UID);
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }

        }

        [HttpPost]
        [Route("CreateSalesPromotionScheme")]
        public async Task<ActionResult> CreateSalesPromotionScheme([FromBody] SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            try
            {
                salesPromotionSchemeApprovalDTO.SalesPromotion.ServerAddTime = DateTime.Now;
                salesPromotionSchemeApprovalDTO.SalesPromotion.ServerModifiedTime = DateTime.Now;
                var retVal = await _salesPromotionSchemeBL.CreateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : CreateErrorResponse("Insert Failed");
            }
            catch (JsonSerializationException jex)
            {
                Log.Error(jex, "JSON serialization failed.");
                return BadRequest("Invalid JSON format.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SalesPromotionScheme ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateSalesPromotionScheme")]
        public async Task<ActionResult> UpdateSalesPromotionScheme([FromBody] SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            try
            {
                var existing = await _salesPromotionSchemeBL.GetSalesPromotionSchemeByUID(salesPromotionSchemeApprovalDTO.SalesPromotion.UID);
                if (existing != null)
                {
                    //salesPromotionScheme.ServerModifiedTime = DateTime.Now;
                    var retVal = await _salesPromotionSchemeBL.UpdateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : CreateErrorResponse("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating SalesPromotionScheme ");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteSalesPromotionScheme")]
        public async Task<ActionResult> DeleteSalesPromotionScheme([FromQuery] string UID)
        {
            try
            {
                var retVal = await _salesPromotionSchemeBL.DeleteSalesPromotionScheme(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
    }
}