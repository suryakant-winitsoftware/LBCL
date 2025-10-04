using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WINITServices.Interfaces.CacheHandler;
using Serilog;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Winit.Modules.Tally.Model.Interfaces;

namespace WINITAPI.Controllers.ServiceAndCallRegistration
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CallRegistrationController : WINITBaseController
    {
        private readonly Winit.Modules.ServiceAndCallRegistration.BL.Interfaces.IServiceAndCallRegistrationBL _serviceAndCallRegistrationBL;
        public CallRegistrationController(IServiceProvider serviceProvider,
            Winit.Modules.ServiceAndCallRegistration.BL.Interfaces.IServiceAndCallRegistrationBL serviceAndCallRegistrationBL)
            : base(serviceProvider)
        {
            _serviceAndCallRegistrationBL = serviceAndCallRegistrationBL;
        }
        [HttpPost]
        [Route("GetCallRegistrations/{jobPositionUID}")]
        public async Task<ActionResult> GetCallRegistrations(PagingRequest pagingRequest, string jobPositionUID)
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
                PagedResponse<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration> pagedResponseList = null;
                pagedResponseList = await _serviceAndCallRegistrationBL.GetCallRegistrations(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Fail to retrieve Provisioning Items. ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetCallRegistrationItemDetailsByCallID/{serviceCallNumber}")]
        public async Task<ActionResult> GetCallRegistrationItemDetailsByCallID(string serviceCallNumber)
        {
            try
            {
                Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration CallRegistrationDetails = await _serviceAndCallRegistrationBL.GetCallRegistrationItemDetailsByCallID(serviceCallNumber);
                if (CallRegistrationDetails != null)
                {
                    return CreateOkApiResponse(CallRegistrationDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve BankDetails with CallID: {@CallID}", serviceCallNumber);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("SaveCallRegistrationDetails")]
        public async Task<ActionResult> SaveCallRegistrationDetails([FromBody] Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration callRegistrationDetails)
        {
            try
            {
                var retVal = await _serviceAndCallRegistrationBL.SaveCallRegistrationDetails(callRegistrationDetails);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Bank details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
