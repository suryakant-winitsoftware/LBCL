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
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Nest;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Newtonsoft.Json;
using WINITSharedObjects.Models;
using DBServices.Interfaces;
using DBServices.Classes;
using Microsoft.Extensions.Logging;
using RabbitMQService.Interfaces;
using WINIT.Shared.Models.Models;
using Microsoft.Extensions.DependencyInjection;

namespace WINITAPI.Controllers.ReturnOrder;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReturnOrderController : WINITBaseController
{
    private readonly Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL _ReturnOrderBL;
    private readonly ILogger<ReturnOrderController> _logger;
    private readonly IDBService _dbService;
    private readonly string queueName;
    private readonly IServiceProvider _serviceProvider;

    public ReturnOrderController(IServiceProvider serviceProvider, 
        Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL ReturnOrderBL, 
        IDBService dbService, 
        ILogger<ReturnOrderController> logger/*, IRabbitMQService rabbitMQService*/) : base(serviceProvider)
    {
        _ReturnOrderBL = ReturnOrderBL;
        _dbService = dbService;
        _logger = logger;
        queueName = "ReturnOrderQueue";
        _serviceProvider = serviceProvider;
    }

    [HttpPost]
    [Route("SelectAllReturnOrderDetails")]
    public async Task<ActionResult> SelectAllReturnOrderDetails(PagingRequest pagingRequest)
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
            PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder> PagedResponseReturnOrderList = null;
            PagedResponseReturnOrderList = await _ReturnOrderBL.SelectAllReturnOrderDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
            if (PagedResponseReturnOrderList == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(PagedResponseReturnOrderList);
        }

        catch (Exception ex)
        {
            Log.Error(ex, "Fail to retrieve ReturnOrderDetails");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpGet]
    [Route("GetReturnOrderByUID")]
    public async Task<ActionResult> GetReturnOrderByUID(string UID)
    {
        try
        {
            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder ReturnOrderDetails = await _ReturnOrderBL.SelectReturnOrderByUID(UID);
            if (ReturnOrderDetails != null)
            {
                return CreateOkApiResponse(ReturnOrderDetails);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ReturnOrderDetails with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

        }
    }

    [HttpPost]
    [Route("CreateReturnOrder")]
    public async Task<ActionResult> CreateReturnOrder([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder returnOrder)
    {
        try
        {
            returnOrder.ServerAddTime = DateTime.Now;
            returnOrder.ServerModifiedTime = DateTime.Now;
            var retVal = await _ReturnOrderBL.CreateReturnOrder(returnOrder);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create ReturnOrder details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPut]
    [Route("UpdateReturnOrderDetails")]
    public async Task<ActionResult> UpdateReturnOrderDetails([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder updateReturnOrder)
    {
        try
        {
            var existingReturnOrderDetails = await _ReturnOrderBL.SelectReturnOrderByUID(updateReturnOrder.UID);
            if (existingReturnOrderDetails != null)
            {
                updateReturnOrder.ServerModifiedTime = DateTime.Now;
                var retVal = await _ReturnOrderBL.UpdateReturnOrder(updateReturnOrder);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating ReturnOrderDetails");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpDelete]
    [Route("DeleteReturnOrderDetails")]
    public async Task<ActionResult> DeleteReturnOrderDetails([FromQuery] string UID)
    {
        try
        {
            var retVal = await _ReturnOrderBL.DeleteReturnOrder(UID);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Failure");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CreateReturnOrderMaster")]
    public async Task<ActionResult> CreateReturnOrderMaster([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMaster)
    {
        try
        {
            var retVal = await _ReturnOrderBL.CreateReturnOrderMaster(returnOrderMaster);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create ReturnOrder Master details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("UpdateReturnOrderMaster")]
    public async Task<ActionResult> UpdateReturnOrderMaster([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO updateReturnOrderMaster)
    {
        try
        {

            var retVal = await _ReturnOrderBL.UpdateReturnOrderMaster(updateReturnOrderMaster);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update ReturnOrder Master details");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpGet]
    [Route("SelectReturnOrderMasterByUIDTest")]
    public async Task<ActionResult<IReturnOrderMaster>> SelectReturnOrderMasterByUIDTest(string UID)
    {
        try
        {
            IReturnOrderMaster ReturnOrderMasterDetails = null;
            ReturnOrderMasterDetails = await _ReturnOrderBL.SelectReturnOrderMasterByUID(UID);
            if (ReturnOrderMasterDetails != null)
            {
                return CreateOkApiResponse(ReturnOrderMasterDetails);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ReturnOrderMasterDetails with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("GetReturnSummaryItemView")]
    public async Task<ActionResult> GetReturnSummaryItemView([FromBody] ReturnSummaryItemApiRequest returnSummaryItemApiRequest)
    {
        try
        {
            List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView> returnSummaryItemViews =
            await _ReturnOrderBL.GetReturnSummaryItemView(returnSummaryItemApiRequest.StartDate, returnSummaryItemApiRequest.EndDate,
            returnSummaryItemApiRequest.StoreUID,returnSummaryItemApiRequest.FilterCriterias);
            if (returnSummaryItemViews == null)
            {
                return NotFound();
            }
            return CreateOkApiResponse(returnSummaryItemViews);
        }
        catch (Exception)
        {
            return CreateErrorResponse("Failed to retrive the data");
        }
    }

    [HttpGet]
    [Route("SelectReturnOrderMasterByUID")]
    public async Task<ActionResult> SelectReturnOrderMasterByUID(string UID)
    {
        try
        {
            IReturnOrderMaster ReturnOrderMasterDetails = null;
            ReturnOrderMasterDetails = await _ReturnOrderBL.SelectReturnOrderMasterByUID(UID);
            if (ReturnOrderMasterDetails != null)
            {
                return CreateOkApiResponse(ReturnOrderMasterDetails);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ReturnOrderMasterDetails with UID: {@UID}", UID);
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

        }
    }

    [HttpPut]
    [Route("UpdateReturnOrderStatus")]
    public async Task<ActionResult> UpdateReturnOrderStatus(ReturnOrderUpdateRequest returnOrderUpdateRequest)
    {
        try
        {
            var retVal = await _ReturnOrderBL.UpdateReturnOrderStatus(returnOrderUpdateRequest.ReturnOrderUIDs, returnOrderUpdateRequest.Status);
            return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating ReturnOrderStatus");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost("CreateReturnOrderFromQueue")]
    public async Task<IActionResult> CreateReturnOrderFromQueue([FromBody] Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder[] returnOrders)
    {
        try
        {
            string step = "";
            var _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
            foreach (var returnOrder in returnOrders)
            {
                if (HttpContext.Request.Headers.TryGetValue("RequestUID", out var messageid)) { }
                //step 1
                messageid = await _dbService.GenerateLogUID(returnOrder.ReturnOrderNumber, "ReturnOrder", JsonConvert.SerializeObject(returnOrder), returnOrder.EmpUID, returnOrder.StoreUID, messageid);
                _logger.LogInformation("LogUID : {@messegeid}", messageid);
                step = "Step2";
                try
                {
                    MessageModel messageModel = new MessageModel { MessageUID = messageid, Message = returnOrder };
                    string messageBody = JsonConvert.SerializeObject(messageModel);
                    _logger.LogInformation("From App: {@SalesOrderModel}", returnOrder);
                    if (_rabbitMQService != null)
                    {
                        _rabbitMQService.SendMessage(queueName, messageBody);
                    }
                    //step 2
                    _dbService.UpdateLogByStepAsync(messageid, step, true, false, null);
                    _logger.LogInformation(" API Publishing: {@objTrxHeaderDco}", returnOrder);
                }
                catch (Exception ex)
                {
                    _dbService.UpdateLogByStepAsync(messageid, step, false, true, ex.Message);
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
}

