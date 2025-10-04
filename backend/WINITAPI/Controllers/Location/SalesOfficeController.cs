using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;


namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesOfficeController : WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.ISalesOfficeBL _salesOfficeBL;

        public SalesOfficeController(IServiceProvider serviceProvider,
            Winit.Modules.Location.BL.Interfaces.ISalesOfficeBL salesOfficeBL)
            : base(serviceProvider)
        {
            _salesOfficeBL = salesOfficeBL;
        }
        [HttpPost]
        [Route("SelectAllSalesOfficeDetails")]
        public async Task<ActionResult> SelectAllSalesOfficeDetails(PagingRequest pagingRequest)
        {

            try
            {
                var hed = Request.Headers;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ISalesOffice> PagedResponseSalesOfficeList = null;
                PagedResponseSalesOfficeList = await _salesOfficeBL.SelectAllSalesOfficeDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseSalesOfficeList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseSalesOfficeList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Sales Office Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSalesOfficeByUID")]
        public async Task<IActionResult> GetSalesOfficeByUID([FromQuery] string UID)
        {
            try
            {
                List<Winit.Modules.Location.Model.Interfaces.ISalesOffice> salesOfficeDetails = await _salesOfficeBL.GetSalesOfficeByUID(UID);
                if (salesOfficeDetails != null)
                {
                    return CreateOkApiResponse(salesOfficeDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Sales Office Details with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateSalesOffice")]
        public async Task<ActionResult> CreateSalesOffice([FromBody] Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            try
            {
                var retVal = await _salesOfficeBL.CreateSalesOffice(salesOffice);
                if (retVal>0)
                {
                    PerformAuditTrial(salesOffice, AuditTrailCommandType.Create);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Sales Office details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrial(ISalesOffice salesOffice, string commondType)
        {
            try
            {
                var newData = new Dictionary<string, object>();
                newData["SalesOffice"] = DictionaryConverter.ToDictionary(salesOffice);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.SalesOffice,
                    linkedItemUID: salesOffice?.UID,
                    commandType: commondType,
                    docNo: salesOffice.Code,
                    jobPositionUID: null,
                    empUID: salesOffice?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(salesOffice)}");
            }
        }
        [HttpPut]
        [Route("UpdateSalesOffice")]
        public async Task<ActionResult> UpdateSalesOffice([FromBody] Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            try
            {
                var existingbranchDetails = await _salesOfficeBL.GetSalesOfficeByUID(salesOffice.UID);
                if (existingbranchDetails != null)
                {
                    var retVal = await _salesOfficeBL.UpdateSalesOffice(salesOffice);
                    if (retVal > 0)
                    {
                        PerformAuditTrial(salesOffice, AuditTrailCommandType.Update);
                    }
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Sales Office Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpDelete]
        [Route("DeleteSalesOffice")]
        public async Task<ActionResult> DeleteSalesOffice([FromBody] ISalesOffice salesOffice)
        {
            try
            {
                var retVal = await _salesOfficeBL.DeleteSalesOffice(salesOffice.UID);
                if (retVal > 0)
                {
                    PerformAuditTrial(salesOffice, AuditTrailCommandType.Delete);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet("GetWareHouseUIDbySalesOfficeUID/{salesOfficeUID}")]
        public async Task<IActionResult> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID)
        {
            try
            {
                if (string.IsNullOrEmpty(salesOfficeUID)) return BadRequest("Sales Office UID is null or empty");
                var retVal = await _salesOfficeBL.GetWareHouseUIDbySalesOfficeUID(salesOfficeUID);
                return (!string.IsNullOrEmpty(retVal)) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Retrive Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

    }
}
