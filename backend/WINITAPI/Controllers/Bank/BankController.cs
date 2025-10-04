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
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace WINITAPI.Controllers.Bank
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankController : WINITBaseController
    {
        private readonly Winit.Modules.Bank.BL.Interfaces.IBankBL _bankBL;

        public BankController(IServiceProvider serviceProvider, 
            Winit.Modules.Bank.BL.Interfaces.IBankBL bankBL) 
            : base(serviceProvider)
        {
            _bankBL = bankBL;
        }
        [HttpPost]
        [Route("GetBankDetails")]
        public async Task<ActionResult> GetBankDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank> pagedResponseBankList = null;
                pagedResponseBankList = await _bankBL.GetBankDetails(pagingRequest.SortCriterias,
                   pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                   pagingRequest.IsCountRequired);
                if (pagedResponseBankList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseBankList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve BankDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetBankDetailsByUID")]
        public async Task<ActionResult> GetBankDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.Bank.Model.Interfaces.IBank BankDetails = await _bankBL.GetBankDetailsByUID(UID);
                if (BankDetails != null)
                {
                    return CreateOkApiResponse(BankDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve BankDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateBankDetails")]
        public async Task<ActionResult> CreateBankDetails([FromBody] Winit.Modules.Bank.Model.Classes.Bank bank)
        {
            try
            {               
                bank.ServerAddTime = DateTime.Now;
                bank.ServerModifiedTime = DateTime.Now;
                var retVal = await _bankBL.CreateBankDetails(bank);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Bank details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateBankDetails")]
        public async Task<ActionResult> UpdateBankDetails([FromBody] Winit.Modules.Bank.Model.Classes.Bank bankDetails)
        {
            try
            {
                var existingBankDetails = await _bankBL.GetBankDetailsByUID(bankDetails.UID);
                if (existingBankDetails != null)
                {
                    bankDetails.ModifiedTime = DateTime.Now;
                    bankDetails.ServerModifiedTime = DateTime.Now;
                    var retVal = await _bankBL.UpdateBankDetails(bankDetails);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating BankDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteBankDetail")]
        public async Task<ActionResult> DeleteBankDetail([FromQuery] string UID)
        {
            try
            {
                var retVal = await _bankBL.DeleteBankDetail(UID);
                return (retVal > 0) ? Accepted("Deleted", retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }
        }
    }
}