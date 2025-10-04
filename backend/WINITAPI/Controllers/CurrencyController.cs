using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Winit.Modules.Currency.Model.Interfaces;


namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrencyController : WINITBaseController
    {
        private readonly Winit.Modules.Currency.BL.Interfaces.ICurrencyBL _currencyBLservice;
        public CurrencyController(IServiceProvider serviceProvider, 
            Winit.Modules.Currency.BL.Interfaces.ICurrencyBL currencyBLservice) : base(serviceProvider)
        {
            _currencyBLservice = currencyBLservice;
        }
        [HttpPost]
        [Route("GetCurrencyDetails")]
        public async Task<ActionResult<ApiResponse<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>>>> GetCurrencyDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency> PagedResponseCurrencyList = null;
                PagedResponseCurrencyList = await _currencyBLservice.GetCurrencyDetails(pagingRequest.SortCriterias,
                   pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                   pagingRequest.IsCountRequired);
                if (PagedResponseCurrencyList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseCurrencyList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve CurrencyDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetCurrencyById")]
        public async Task<ActionResult<ApiResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>>> GetCurrencyById(string UID)
        {
            try
            {
                Winit.Modules.Currency.Model.Interfaces.ICurrency currencyDetails = await _currencyBLservice.GetCurrencyById(UID);
                if (currencyDetails != null)
                {
                    return CreateOkApiResponse(currencyDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve currencyDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateCurrency")]
        public async Task<ActionResult<int>> CreateCurrency([FromBody] Winit.Modules.Currency.Model.Classes.Currency createCurrency)
        {
            try
            {

                createCurrency.ServerAddTime = DateTime.Now;
                createCurrency.ServerModifiedTime = DateTime.Now;
                var retVal = await _currencyBLservice.CreateCurrency(createCurrency);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Currency details");
                return StatusCode(500, new { success = false, message = "Error creating Currency Details", error = ex.Message });
            }
        }
        [HttpPut]
        [Route("UpdateCurrency")]
        public async Task<ActionResult<int>> UpdateCurrency([FromBody] Winit.Modules.Currency.Model.Classes.Currency updateCurrency)
        {
            try
            {
                var existingDetails = await _currencyBLservice.GetCurrencyById(updateCurrency.UID);
                if (existingDetails != null)
                {
                    //updateCurrency.ModifiedTime = DateTime.Now;
                    updateCurrency.ServerModifiedTime = DateTime.Now;
                    var retVal = await _currencyBLservice.UpdateCurrency(updateCurrency);
                    return (retVal > 0) ? Accepted("Updated", retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Currency Details");
                return StatusCode(500, new { success = false, message = "Error updating Currency Details", error = ex.Message });
            }
        }
        [HttpDelete]
        [Route("DeleteCurrency")]
        public async Task<ActionResult<int>> DeleteCurrency([FromQuery] string UID)
        {
            try
            {
                var retVal = await _currencyBLservice.DeleteCurrency(UID);
                return (retVal > 0) ? Accepted("Deleted", retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, new { success = false, message = "Deleting Failure", error = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetCurrencyListByOrgUID")]
        public async Task<ActionResult<ApiResponse<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency>>> GetCurrencyListByOrgUID(string orgUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency> currencyDetails = await _currencyBLservice.GetOrgCurrencyListBySelectedOrg(orgUID);
                if (currencyDetails != null)
                {
                    return CreateOkApiResponse(currencyDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve currency list with UID: {@UID}", orgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateOrgCurrency")]
        public async Task<ActionResult> CreateOrgCurrency([FromBody] List<Winit.Modules.Currency.Model.Classes.OrgCurrency> orgCurrencies)
        {
            int count = 0;
            try
            {
                foreach (IOrgCurrency orgCurrency in orgCurrencies)
                {
                    count += await _currencyBLservice.CreateOrgCurrency(orgCurrency);
                }
                return CreateOkApiResponse(count);
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Failed to retrieve currency list with UID: {@UID}", orgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteOrgCurrency")]
        public async Task<ActionResult<int>> DeleteOrgCurrency([FromQuery] string UID)
        {
            try
            {
                var retVal = await _currencyBLservice.DeleteOrgCurrency(UID);
                return (retVal > 0) ? Accepted("Deleted", retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return StatusCode(500, new { success = false, message = "Deleting Failure", error = ex.Message });
            }
        }
    }
}