using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.TemporaryCredit
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TemporaryCreditController : WINITBaseController
    {
        private readonly Winit.Modules.CreditLimit.BL.Interfaces.ITemporaryCreditBL _temporaryCreditBL;
        public TemporaryCreditController(IServiceProvider serviceProvider,
            Winit.Modules.CreditLimit.BL.Interfaces.ITemporaryCreditBL temporaryCreditBL)
            : base(serviceProvider)
        {
            _temporaryCreditBL = temporaryCreditBL;

        }

        [HttpPost]
        [Route("SelectTemporaryCreditDetails/{jobPositionUID}")]
        public async Task<ActionResult> SelectTemporaryCreditDetails(PagingRequest pagingRequest, string jobPositionUID)
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
                PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> pagedResponseList = null;
                pagedResponseList = await _temporaryCreditBL.SelectTemporaryCreditDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, jobPositionUID);
                if (pagedResponseList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve TemporaryCredit");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTemporaryCreditByUID/{UID}")]
        public async Task<ActionResult> GetTemporaryCreditByUID(string UID)
        {
            try
            {
                Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit = await _temporaryCreditBL.GetTemporaryCreditByUID(UID);
                if (temporaryCredit != null)
                {
                    return CreateOkApiResponse(temporaryCredit);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve TemporaryCredit with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateTemporaryCreditDetails")]
        public async Task<ActionResult> CreateTemporaryCreditDetails([FromBody] Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
        {
            try
            {
                var retVal = await _temporaryCreditBL.CreateTemporaryCreditDetails(temporaryCredit);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else { throw new Exception("Insert Failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Temporary Credit details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateTemporaryCreditDetails")]
        public async Task<ActionResult> UpdateTemporaryCreditDetails([FromBody] Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
        {
            try
            {
                var existingDetails = await _temporaryCreditBL.GetTemporaryCreditByUID(temporaryCredit.UID);
                if (existingDetails != null)
                {
                    var retVal = await _temporaryCreditBL.UpdateTemporaryCreditDetails(temporaryCredit);
                    if (retVal > 0)
                    {
                        return CreateOkApiResponse(retVal);
                    }
                    else
                    {
                        throw new Exception("Update Failed");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating TemporaryCredit");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteTemporaryCreditDetails/{UID}")]
        public async Task<ActionResult> DeleteTemporaryCreditDetails(string UID)
        {
            try
            {
                var retVal = await _temporaryCreditBL.DeleteTemporaryCreditDetails(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
