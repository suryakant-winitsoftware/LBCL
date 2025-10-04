using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using System;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.CollectionModule;

[Route("api/[controller]")]
[ApiController]
public class AccPayableCMIController : WINITBaseController
{
    private readonly Winit.Modules.CollectionModule.BL.Interfaces.IAccPayableCMIBL _accPayableCMIBL;

    public AccPayableCMIController(IServiceProvider serviceProvider,
        IAccPayableCMIBL accPayableCMIBL)
        : base(serviceProvider)
    {
        _accPayableCMIBL = accPayableCMIBL;
    }
    [HttpPost]
    [Route("GetAccPayableCMIDetails/{jobPositionUID}")]
    public async Task<ActionResult> GetAccPayableCMIDetails(PagingRequest pagingRequest, string jobPositionUID)
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
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI> pagedResponseAccPayableCMIList = null;
            pagedResponseAccPayableCMIList = await _accPayableCMIBL.GetAccPayableCMIDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, jobPositionUID);
            if (pagedResponseAccPayableCMIList == null)
            {
                return NotFound();
            }

            return CreateOkApiResponse(pagedResponseAccPayableCMIList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve AccPayableCMI");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("GetAccPayableMasterByUID/{UID}")]
    public async Task<ActionResult> GetAccPayableMasterByUID(string UID)
    {
        try
        {
            Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableMaster accPayableMaster = await _accPayableCMIBL.GetAccPayableMasterByUID(UID);
            if (accPayableMaster != null)
            {
                return CreateOkApiResponse(accPayableMaster);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AccPayableMaster with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("OutSTandingInvoicesByStoreCode/{storeCode}/{pageNumber}/{pageSize}")]
    public async Task<ActionResult> OutSTandingInvoicesByStoreCode(string storeCode, int pageNumber, int pageSize)
    {
        try
        {
            var outstandingInvoices = await _accPayableCMIBL.OutSTandingInvoicesByStoreCode(storeCode, pageNumber, pageSize);
            if (outstandingInvoices != null)
            {
                return CreateOkApiResponse(outstandingInvoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to retrieve out standing invoice details with Store Code: {storeCode}");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }



}
