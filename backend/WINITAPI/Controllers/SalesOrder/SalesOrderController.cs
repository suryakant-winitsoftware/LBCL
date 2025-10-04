using DBServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQService.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINIT.Shared.Models.Models;

namespace WINITAPI.Controllers.SalesOrder;

[Route("api/[controller]")]
[ApiController]
public class SalesOrderController : WINITBaseController
{
    private readonly Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL _salesOrderBL;
    private readonly IDBService _dbService;
    private readonly ILogger<SalesOrderController> _logger;
    private readonly string queueName;
    private readonly IServiceProvider _serviceProvider;

    public SalesOrderController(IServiceProvider serviceProvider, 
        Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL, 
        IDBService dbService,
        ILogger<SalesOrderController> logger) : base(serviceProvider)
    {
        _salesOrderBL = salesOrderBL;
        _dbService = dbService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        queueName = "SalesOrderQueue";
    }
    [HttpGet]
    [Route("SelectSalesOrderDetailsAll")]
    public async Task<ActionResult<IEnumerable<ISalesOrderViewModel>>> SelectSalesOrderDetailsAll([FromQuery] List<Winit.Shared.Models.Enums.SortCriteria> sortCriterias, int pageNumber, int pageSize,
        [FromQuery] List<Winit.Shared.Models.Enums.FilterCriteria> filterCriterias)
    {
        try
        {
            
            IEnumerable<ISalesOrderViewModel> salesOrderDetailsModel = await _salesOrderBL.SelectSalesOrderDetailsAll(sortCriterias, pageNumber, pageSize, filterCriterias);
            if (salesOrderDetailsModel == null)
            {
                return NotFound();
            }
                return CreateOkApiResponse(salesOrderDetailsModel);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet]
    [Route("SelectSalesOrderByUID")]
    public async Task<IActionResult> SelectSalesOrderByUID([FromQuery] string SalesOrderUID)
    {
        try
        {
            ISalesOrder salesOrder = await _salesOrderBL.GetSalesOrderByUID(SalesOrderUID);
            return salesOrder != null ? CreateOkApiResponse(salesOrder) : NotFound(salesOrder);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve salesOrder with SalesOrderUID: {@SalesOrderUID}", SalesOrderUID);
            throw;
        }
    }

    [HttpPost]
    [Route("CreateSalesOrder")]
    public async Task<ActionResult<int>> CreateSalesOrder([FromBody] Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel)
    {
        try
        {
            int retVal = await _salesOrderBL.SaveSalesOrder(salesOrderViewModel);
            return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Sales Order  details");
            return StatusCode(500, new { success = false, message = "Error creating Sales Order Details", error = ex.Message });
        }

    }

    [HttpGet]
    [Route("GetSalesOrderPrintView")]
    public async Task<ActionResult> GetSalesOrderPrintView(string SalesOrderUID)
    {
        try
        {
            ISalesOrderPrintView salesOrderPrintViewDetails = await _salesOrderBL.GetSalesOrderPrintView(SalesOrderUID);
            return salesOrderPrintViewDetails != null ? CreateOkApiResponse(salesOrderPrintViewDetails) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SalesOrderPrintView with SalesOrderUID: {@SalesOrderUID}", SalesOrderUID);
            throw;
        }
    }

    [HttpGet]
    [Route("GetSalesOrderLinePrintView")]
    public async Task<ActionResult> GetSalesOrderLinePrintView(string SalesOrderUID)
    {
        try
        {
            IEnumerable<ISalesOrderLinePrintView> salesOrderPrintLineViewDetails = await _salesOrderBL.GetSalesOrderLinePrintView(SalesOrderUID);
            return salesOrderPrintLineViewDetails != null ? CreateOkApiResponse(salesOrderPrintLineViewDetails) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SalesOrderLinePrintView with SalesOrderUID: {@SalesOrderUID}", SalesOrderUID);
            throw;
        }
    }

    [HttpPost]
    [Route("SelectDeliveredPreSales")]
    public async Task<ActionResult> SelectDeliveredPreSales(PagingRequest pagingRequest, DateTime startDate, DateTime endDate, string Status)
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
            PagedResponse<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales> pagedResponse = null;
            pagedResponse = await _salesOrderBL.SelectDeliveredPreSales(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, startDate, endDate, Status);
            return pagedResponse == null ? NotFound() : CreateOkApiResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve DeliveredPreSales");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("SelectDeliveredPreSalesBySalesOrderUID")]
    public async Task<ActionResult> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID)
    {
        try
        {
            IViewPreSales viewPreSales = await _salesOrderBL.SelectDeliveredPreSalesBySalesOrderUID(SalesOrderUID);
            return viewPreSales != null ? CreateOkApiResponse(viewPreSales) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve viewPreSales with SalesOrderUID: {@SalesOrderUID}", SalesOrderUID);
            throw;
        }
    }
    [HttpPost]
    [Route("CUD_SalesOrder")]
    public async Task<ActionResult> CUD_SalesOrder([FromBody] Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel)
    {
        try
        {
            int retVal = await _salesOrderBL.CUD_SalesOrder(salesOrderViewModel);
            return (retVal == -1) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CUD Operations Failed");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSalesOrderMasterDataBySalesOrderUID")]
    public async Task<ActionResult> GetSalesOrderMasterDataBySalesOrderUID(string salesOrderUID)
    {
        try
        {
            SalesOrderViewModelDCO salesOrderMaster = await _salesOrderBL.GetSalesOrderMasterDataBySalesOrderUID(salesOrderUID);
            return salesOrderMaster != null ? CreateOkApiResponse(salesOrderMaster) : NotFound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SalesOrderMaster with SalesOrderUID: {@SalesOrderUID}", salesOrderUID);
            throw;
        }
    }

    [HttpPost]
    [Route("InsertorUpdate_SalesOrders")]
    public async Task<ActionResult> InsertorUpdate_SalesOrders([FromBody] Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel)
    {
        try
        {
            int retVal = await _salesOrderBL.InsertorUpdate_SalesOrders(salesOrderViewModel);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CUD Operations Failed");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost("CreateSalesOrderFromQueue")]
    public async Task<IActionResult> CreateSalesOrderFromQueue([FromBody] SalesOrderViewModelDCO[] salesOrderModels)
    {
        try
        {
            string step = "";
            IRabbitMQService _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
            foreach (SalesOrderViewModelDCO salesOrderModel in salesOrderModels)
            {
                if (HttpContext.Request.Headers.TryGetValue("RequestUID", out Microsoft.Extensions.Primitives.StringValues messageid)) { }
                //step 1
                messageid = await _dbService.GenerateLogUID(salesOrderModel.SalesOrder.ReferenceUID, "SalesOrder", JsonConvert.SerializeObject(salesOrderModel), salesOrderModel.SalesOrder.EmpUID, salesOrderModel.SalesOrder.StoreUID, messageid);
                _logger.LogInformation("LogUID : {@messegeid}", messageid);
                step = "Step2";
                try
                {
                    MessageModel messageModel = new() { MessageUID = messageid, Message = salesOrderModel };
                    string messageBody = JsonConvert.SerializeObject(messageModel);
                    _logger.LogInformation("From App: {@SalesOrderModel}", salesOrderModel);
                    _rabbitMQService?.SendMessage(queueName, messageBody);
                    //step 2
                    _ = await _dbService.UpdateLogByStepAsync(messageid, step, true, false, null);
                    _logger.LogInformation(" API Publishing: {@objTrxHeaderDco}", salesOrderModel);
                }
                catch (Exception ex)
                {
                    _ = await _dbService.UpdateLogByStepAsync(messageid, step, false, true, ex.Message);
                    _logger.LogError(ex, "Error occurred while sending message to RabbitMQ.");
                    throw;
                }
            }
            return Ok(new { Message = "Request Submitted Successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while publishing the message." + ex.Message);
            return StatusCode(500, "An error occurred while publishing the message.");
        }
    }

    [HttpPut]
    [Route("UpdateSalesOrderStatus")]
    public async Task<ActionResult> UpdateSalesOrderStatus([FromBody] Winit.Modules.SalesOrder.Model.Classes.SalesOrderStatusModel salesOrderStatus)
    {
        try
        {
            int retVal = await _salesOrderBL.UpdateSalesOrderStatus(salesOrderStatus);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Update Failed");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("GetAllSalesOrderInvoices")]
    public async Task<IActionResult> GetAllSalesOrderInvoices(string? storeUID = null)
    {
        try
        {
            IEnumerable<ISalesOrderInvoice> salesOrderDetailsModel = null;

            salesOrderDetailsModel = await _salesOrderBL.GetAllSalesOrderInvoices(storeUID);
            return salesOrderDetailsModel == null ? NotFound() : (IActionResult)CreateOkApiResponse(salesOrderDetailsModel);
        }
        catch (Exception)
        {
            throw;
        }
    }
    [HttpGet]
    [Route("GetSalesOrderLineInvoiceItems")]
    public async Task<IActionResult> GetSalesOrderLineInvoiceItems(string salesOrderUID)
    {
        try
        {
            IEnumerable<ISalesOrderLineInvoice> salesOrderLineInvoices = null;

            salesOrderLineInvoices = await _salesOrderBL.GetSalesOrderLineInvoiceItems(salesOrderUID);
            return salesOrderLineInvoices == null ? NotFound() : (IActionResult)CreateOkApiResponse(salesOrderLineInvoices);
        }
        catch (Exception)
        {
            throw;
        }
    }
    [HttpPut]
    [Route("UpdateSalesOrderLinesReturnQty")]
    public async Task<IActionResult> GetSalesOrderLineInvoiceItems(List<SalesOrderLine> salesOrderLines)
    {
        try
        {
            int data = await _salesOrderBL.UpdateSalesOrderLinesReturnQty(salesOrderLines.ToList<ISalesOrderLine>());
            return data != salesOrderLines.Count ? CreateErrorResponse("update failed") : (IActionResult)CreateOkApiResponse(data);
        }
        catch (Exception)
        {
            throw;
        }
    }
    [HttpGet]
    [Route("GetSalesOrderLinesBySalesOrderUID")]
    public async Task<IActionResult> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        try
        {
            IEnumerable<ISalesOrderLine> salesOrderLines = null;

            salesOrderLines = await _salesOrderBL.GetSalesOrderLinesBySalesOrderUID(salesOrderUID);
            return salesOrderLines == null ? NotFound() : (IActionResult)CreateOkApiResponse(salesOrderLines);
        }
        catch (Exception)
        {
            throw;
        }
    }
}