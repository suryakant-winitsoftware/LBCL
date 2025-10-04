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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WINIT.Shared.Models.Models;
using DBServices.Classes;
using DBServices.Interfaces;
using Microsoft.Extensions.Logging;
using WINITAPI.Controllers.CollectionModule;
using Serilog.Core;

namespace WINITAPI.Controllers.WHStockRequest
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WHStockController : WINITBaseController
    {
        private readonly Winit.Modules.WHStock.BL.Interfaces.IWHStockBL _whStockBL;
        private readonly IDBService _dbService;
        private readonly ILogger<WHStockController> _logger;
        private readonly string queueName;


        public WHStockController(IServiceProvider serviceProvider, 
            Winit.Modules.WHStock.BL.Interfaces.IWHStockBL whStockBL, 
            IDBService dbService,
            ILogger<WHStockController> logger) : base(serviceProvider)
        {
            _whStockBL = whStockBL;
            _dbService = dbService;
            _logger = logger;
            queueName = "WHStockQueue";
        }

        [HttpPost]
        [Route("CUDWHStock")]
        public async Task<ActionResult> CUDWHStock([FromBody] Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel)
        {
            try
            {
                var retVal = await _whStockBL.CUDWHStock(wHRequestTempleteModel);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Operation failed");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateWHStockFromQueue")]
        public async Task<ActionResult> CreateWHStockFromQueue([FromBody] Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel[] wHRequestTemplateModels)
        {
            try
            {
                string step = "";
                var _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
                foreach (var wHRequestTemplateModel in wHRequestTemplateModels)
                {
                    if (HttpContext.Request.Headers.TryGetValue("RequestUID", out var messageid)) { }
                    //step 1
                    messageid = await _dbService.GenerateLogUID(wHRequestTemplateModel.WHStockRequest.UID, "WHStock", JsonConvert.SerializeObject(wHRequestTemplateModel), wHRequestTemplateModel.WHStockRequest.RequestByEmpUID, wHRequestTemplateModel.WHStockRequest.SourceOrgUID, messageid);
                    _logger.LogInformation("LogUID : {@messageid}", messageid);
                    step = "Step2";
                    try
                    {
                        MessageModel messageModel = new MessageModel { MessageUID = messageid, Message = wHRequestTemplateModel };
                        string messageBody = JsonConvert.SerializeObject(messageModel);
                        _logger.LogInformation("From App: {@WHStock}", wHRequestTemplateModel);
                        if (_rabbitMQService != null)
                        {
                            _rabbitMQService.SendMessage(queueName, messageBody);
                        }
                        //step 2
                        await _dbService.UpdateLogByStepAsync(messageid, step, true, false, null);
                        _logger.LogInformation(" API Publishing: {@objwHRequestTemplateModel}", wHRequestTemplateModel);
                    }
                    catch (Exception ex)
                    {
                        await _dbService.UpdateLogByStepAsync(messageid, step, false, true, ex.Message);
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

        [HttpPost]
        [Route("SelectLoadRequestData")]
        public async Task<ActionResult> SelectLoadRequestData(PagingRequest pagingRequest, string StockType)
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
                PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView> PagedResponse = null;
                PagedResponse = await _whStockBL.SelectLoadRequestData(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired, StockType);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Load Request Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("SelectLoadRequestDataByUID")]
        public async Task<ActionResult> SelectLoadRequestDataByUID([FromQuery] string UID)
        {
            try
            {
                Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView loadRequestItemView = await _whStockBL.SelectLoadRequestDataByUID(UID);
                if (loadRequestItemView != null)
                {
                    return CreateOkApiResponse(loadRequestItemView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve LoadRequest View with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDWHStockRequestLine")]
        public async Task<ActionResult> CUDWHStockRequestLine([FromBody]List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine>  wHStockRequestLines)
        {
            try
            {
                var retVal = await _whStockBL.CUDWHStockRequestLine(wHStockRequestLines);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Operation failed");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


    }
}
