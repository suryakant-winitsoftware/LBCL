using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Invoice;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvoiceController : WINITBaseController
{
    private readonly Winit.Modules.Invoice.BL.Interfaces.IInvoiceBL _invoiceBL;
    public InvoiceController(IServiceProvider serviceProvider,
        Winit.Modules.Invoice.BL.Interfaces.IInvoiceBL invoiceBL) : base(serviceProvider)
    {
        _invoiceBL = invoiceBL;
    }


    [HttpPost]
    [Route("GetAllInvoices/{jobPositionUID}")]
    public async Task<ActionResult> GetAllInvoices([FromBody] PagingRequest pagingRequest,string jobPositionUID)
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
            PagedResponse<Winit.Modules.Invoice.Model.Interfaces.IInvoiceHeaderView> PagedResponseInvoiceHeaderViewHeaderlist = null;
            PagedResponseInvoiceHeaderViewHeaderlist = await _invoiceBL.GetAllInvoices(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired,jobPositionUID);
            return PagedResponseInvoiceHeaderViewHeaderlist == null ? NotFound() : CreateOkApiResponse(PagedResponseInvoiceHeaderViewHeaderlist);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve InvoiceHeaderView");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("GetInvoiceApproveSatsusDetails")]
    public async Task<ActionResult> GetInvoiceApproveSatsusDetails([FromBody] PagingRequest pagingRequest, [FromQuery] bool Status)
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
            PagedResponse<Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView> pagedResponse = null;
            pagedResponse = await _invoiceBL.GetInvoiceApproveSatsusDetails(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired, Status);
            return pagedResponse == null ? NotFound() : CreateOkApiResponse(pagedResponse);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve InvoiceApprove");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetInvoiceMasterByInvoiceUID/{invoiceUid}")]
    public async Task<ActionResult> GetInvoiceMasterByInvoiceUID(string invoiceUid)
    {

        try
        {
            if (string.IsNullOrEmpty(invoiceUid))
            {
                return BadRequest("Invalid request data");
            }

            IInvoiceMaster invoiceMaster = await _invoiceBL.GetInvoiceMasterByInvoiceUID(invoiceUid);
            return invoiceMaster == null ? NotFound() : CreateOkApiResponse(invoiceMaster);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve InvoiceMaster");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateInvoiceProvisioning")]
    public async Task<IActionResult> UpdateInvoiceProvisioning([FromBody] List<IProvisioningCreditNoteView> provisioningCreditNoteViews)
    {
        try
        {
            if (provisioningCreditNoteViews == null || !provisioningCreditNoteViews.Any())
            {
                return BadRequest();
            }
            int count = await _invoiceBL.UpdateApprovedStatus(provisioningCreditNoteViews);
            if (count == 0)
            {
                _ = CreateErrorResponse("Error occured while updating the InvoiceProvisioningr");
            }
            return CreateOkApiResponse(count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to update InvoiceProvisioning");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }

    }


    [HttpPost]
    [Route("GetOutstandingInvoiceReportData")]
    public async Task<ActionResult> GetOutstandingInvoiceReportData([FromBody] PagingRequest pagingRequest, [FromQuery] bool Status)
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
            PagedResponse<Winit.Modules.Invoice.Model.Interfaces.IOutstandingInvoiceReport> pagedResponse = null;
            pagedResponse = await _invoiceBL.GetOutstandingInvoiceReportData(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired, Status);
            return pagedResponse == null ? NotFound() : CreateOkApiResponse(pagedResponse);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve InvoiceApprove");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("GetInvoicesForReturnOrder")]
    public async Task<ActionResult> GetInvoicesForReturnOrder([FromBody] InvoiceListRequest invoiceListRequest)
    {
        try
        {
            if (invoiceListRequest == null)
            {
                return BadRequest("Invalid request data");
            }

            var response = await _invoiceBL.GetInvoicesForReturnOrder(invoiceListRequest);
            return response == null ? NotFound() : CreateOkApiResponse(response);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve InvoiceForReturnOrder");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost("CreateProvision")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateProvision([FromBody] InvoiceView invoiceView)
    {
        try
        {
            if (invoiceView == null)
            {
                return BadRequest("Invalid request data");
            }

            var response = await _invoiceBL.CreateProvision(invoiceView.UID);
            return response == null ? NotFound() : CreateOkApiResponse(response);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to Create Provision");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("GetProvisionComparisonReport")]
    [AllowAnonymous]
    public async Task<ActionResult> GetProvisionComparisonReport([FromBody] PagingRequest pagingRequest)
    {
        try
        {

            var pagedResponse = await _invoiceBL.GetProvisionComparisonReport(pagingRequest.SortCriterias,
                 pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                 pagingRequest.IsCountRequired);
            return pagedResponse == null ? NotFound() : CreateOkApiResponse(pagedResponse);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve GetProvisionComparisonReport");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
}
