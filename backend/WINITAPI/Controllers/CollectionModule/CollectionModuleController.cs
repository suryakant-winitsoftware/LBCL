using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Winit.Shared.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections;
using Winit.Shared.Models.Constants;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using static Winit.Modules.CollectionModule.Model.Classes.AccCollection;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Nest;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WINIT.Shared.Models.Models;
using DBServices.Interfaces;
using Microsoft.Extensions.Logging;
using WINITAPI.Controllers.ReturnOrder;
using static Winit.Modules.CollectionModule.Model.Classes.Collections;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Bank.Model.Interfaces;
using Dapper;
using Winit.Modules.Contact.Model.Interfaces;

namespace WINITAPI.Controllers.CollectionModule;

[Route("api/[controller]")]
[ApiController]
public class CollectionModuleController : WINITBaseController
{
    private readonly Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL _collectionModuleService;
    private readonly IDBService _dbService;
    private readonly ILogger<CollectionModuleController> _logger;
    private readonly string queueName;

    public CollectionModuleController(IServiceProvider serviceProvider, 
        Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL collectionModuleService,
        IDBService dbService,
        ILogger<CollectionModuleController> logger) : base(serviceProvider)
    {
        _collectionModuleService = collectionModuleService;
        _dbService = dbService;
        _logger = logger;
        queueName = "CollectionQueue";
    }



    [HttpGet]
    [Route("GetAllOutstandingTransactionsByCustomerCode")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>>> GetAllOutstandingTransactionsByCustomerCode([FromQuery] string SessionUserCode, string CustomerCode, string SalesOrgCode)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule> CollectionModuleList = await _collectionModuleService.GetAllOutstandingTransactionsByCustomerCode(CustomerCode, SessionUserCode, SalesOrgCode);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllOutstandingTransactionsByCustomerCode ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllConfiguredCurrencyDetailsBySalesOrgCode")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IExchangeRate>>> GetAllConfiguredCurrencyDetailsBySalesOrgCode([FromQuery] string SessionUserCode, string SalesOrgCode)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IExchangeRate> CollectionModuleList = await _collectionModuleService.GetAllConfiguredCurrencyDetailsBySalesOrgCode(SessionUserCode, SalesOrgCode);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllConfiguredCurrencyDetailsBySalesOrgCode ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllConfiguredDocumentTypesBySalesOrgCode")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>>> GetAllConfiguredDocumentTypesBySalesOrgCode([FromQuery] string SessionUserCode, string SalesOrgCode)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await _collectionModuleService.GetAllConfiguredDocumentTypesBySalesOrgCode(SessionUserCode, SalesOrgCode);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllConfiguredDocumentTypesBySalesOrgCode ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllConfiguredPaymentModesBySalesOrgCode")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> GetAllConfiguredPaymentModesBySalesOrgCode([FromQuery] string SessionUserCode, string SalesOrgCode)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> CollectionModuleList = await _collectionModuleService.GetAllConfiguredPaymentModesBySalesOrgCode(SessionUserCode, SalesOrgCode);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return new AccCollection[0];
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllConfiguredPaymentModesBySalesOrgCode ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllCustomersBySalesOrgCode")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>>> GetAllCustomersBySalesOrgCode([FromQuery] string SessionUserCode, string SalesOrgCode)
    {
        try
        {
            IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> CollectionModuleList = await _collectionModuleService.GetAllCustomersBySalesOrgCode(SessionUserCode, SalesOrgCode);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return Array.Empty<IStore>();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllCustomersBySalesOrgCode ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllotmentAmount")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>>> GetAllotmentAmount([FromQuery] string TargetUID, string AccCollectionUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await _collectionModuleService.GetAllotmentAmount(TargetUID, AccCollectionUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllotmentAmount ", TargetUID);
            throw;
        }
    }
    [HttpGet]
    [Route("AllInvoices")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>>> AllInvoices([FromQuery] string AccCollectionUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await _collectionModuleService.AllInvoices(AccCollectionUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AllInvoices ", AccCollectionUID);
            throw;
        }
    }
    [HttpGet]
    [Route("GetBankNames")]
    public async Task<ActionResult<IEnumerable<IBank>>> GetBankNames()
    {
        try
        {
            IEnumerable<IBank> CollectionModuleList = await _collectionModuleService.GetBankNames();
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetBankNames ");
            throw;
        }
    }
    [HttpPost]
    [Route("PaymentDetails")]
    public async Task<ActionResult> PaymentDetails(PagingRequest pagingRequest, string AccCollectionUID)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.PaymentDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, AccCollectionUID);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in PaymentDetails", AccCollectionUID);
            throw;
        }
    }
    [HttpGet]
    [Route("SettledDetails")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> SettledDetails([FromQuery] string AccCollectionUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> CollectionModuleList = await _collectionModuleService.SettledDetails(AccCollectionUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve SettledDetails ", AccCollectionUID);
            throw;
        }
    }
    [HttpPost]
    [Route("CashierSettlement")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> CashierSettlement(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> collectionModuleList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponsecollectionModuleList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }

            PagedResponsecollectionModuleList = await _collectionModuleService.CashierSettlement(pagingRequest.SortCriterias,
               pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
               pagingRequest.IsCountRequired);

            if (PagedResponsecollectionModuleList == null)
            {
                return NotFound();
            }
            else
            {
            }
            return CreateOkApiResponse(PagedResponsecollectionModuleList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CashierSettlement");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("CashierSettlementVoid")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> CashierSettlementVoid(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> collectionModuleList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponsecollectionModuleList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }

            PagedResponsecollectionModuleList = await _collectionModuleService.CashierSettlementVoid(pagingRequest.SortCriterias,
               pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
               pagingRequest.IsCountRequired);

            if (PagedResponsecollectionModuleList == null)
            {
                return NotFound();
            }
            else
            {
            }
            return CreateOkApiResponse(PagedResponsecollectionModuleList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CashierSettlementVoid");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("CashierSettlementSettled")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> CashierSettlementSettled(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> collectionModuleList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponsecollectionModuleList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }

            PagedResponsecollectionModuleList = await _collectionModuleService.CashierSettlementSettled(pagingRequest.SortCriterias,
               pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
               pagingRequest.IsCountRequired);

            if (PagedResponsecollectionModuleList == null)
            {
                return NotFound();
            }
            else
            {
            }
            return CreateOkApiResponse(PagedResponsecollectionModuleList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CashierSettlementSettled");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }

    [HttpPost]
    [Route("CashCollectionSettlement")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashCollectionSettlement([FromBody] List<string> collection)
    {
        try
        {
            var createdResponses = new List<object>();
            var Failure = "";
            var retValue = "";
            foreach (var list in collection)
            {
                retValue = await _collectionModuleService.CashCollectionSettlement(list);
                if (retValue == "Successfully Inserted Data Into tables")
                {
                    var createdData = new CustomResponse
                    {
                        StatusCode = (int)HttpStatusCode.Created,
                        ResponseMessage = retValue,
                        Value = 1
                    };
                    createdResponses.Add(createdData);
                }
                else if (retValue == "Amount Variance")
                {
                    var createdData = new CustomResponse
                    {
                        ResponseMessage = retValue,
                    };
                    createdResponses.Add(createdData);
                    Failure = retValue;
                }
                else
                {
                    var createdData = new ExcepResponse
                    {
                        StatusCode = 400,
                        ServerAddTime = DateTime.Now,
                        ResponseMessage = retValue
                    };
                    createdResponses.Add(createdData);
                    Failure = retValue;
                }
            }
            if (Failure == "" && retValue != "")
                return CreatedAtAction("CreateReceipt", createdResponses);
            if (collection.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "parameter missing or Not found");
            else
                return BadRequest(createdResponses);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CollectionModuleList with SalesOrgCode");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowPaymentDetails")]
    public async Task<ActionResult> ShowPaymentDetails(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> collectionModuleList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> PagedResponsecollectionModuleList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }

            PagedResponsecollectionModuleList = await _collectionModuleService.ShowPaymentDetails(pagingRequest.SortCriterias,
               pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
               pagingRequest.IsCountRequired);

            if (PagedResponsecollectionModuleList == null)
            {
                return NotFound();
            }
            else
            {
            }
            return CreateOkApiResponse(PagedResponsecollectionModuleList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve ShowPaymentDetails");
            return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        }
    }
    [HttpPost]
    [Route("UpdateChequeDetails")]
    public async Task<ActionResult> UpdateChequeDetails([FromBody] Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode collection)
    {
        try
        {
            var CollectionModuleList = await _collectionModuleService.UpdateChequeDetails(collection);
            return (CollectionModuleList > 0) ? CreateOkApiResponse(CollectionModuleList) : throw new Exception("Update Failed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CollectionModuleList with SalesOrgCode");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowPending")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>>> ShowPending(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ShowPending(pagingRequest.SortCriterias,
            pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
            pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in ShowPending");
            throw;
        }
    }
    [HttpGet]
    [Route("IsReversal")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> IsReversal(string UID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> CollectionModuleList = await _collectionModuleService.IsReversal(UID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve IsReversal ");
            throw;
        }
    }
    [HttpGet]
    [Route("IsReversalCash")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>>> IsReversalCash(string UID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> CollectionModuleList = await _collectionModuleService.IsReversalCash(UID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve IsReversalCash ");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowSettled")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowSettled(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ShowSettled(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in ShowSettled");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowApproved")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>>> ShowApproved(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ShowApproved(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in ShowApproved");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowRejected")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>>> ShowRejected(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ShowRejected(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in ShowRejected");
            throw;
        }
    }
    [HttpPost]
    [Route("ShowBounced")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>>> ShowBounced(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ShowBounced(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve PagedResponsepaymentList in ShowBounced");
            throw;
        }
    }
    [HttpGet]
    [Route("GetAllOutstandingTransactionsByMultipleFilters")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>>> GetAllOutstandingTransactionsByMultipleFilters([FromQuery] string SessionUserCode, string SalesOrgCode, string CustomerCode, string StartDueDate, string EndDueDate, string StartInvoiceDate, string EndInvoiceDate)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule> CollectionModuleList = await _collectionModuleService.GetAllOutstandingTransactionsByMultipleFilters(SessionUserCode, SalesOrgCode, CustomerCode, StartDueDate, EndDueDate, StartInvoiceDate, EndInvoiceDate);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetAllOutstandingTransactionsByMultipleFilters ", SalesOrgCode);
            throw;
        }
    }
    [HttpGet]
    [Route("AllocateSelectedInvoiceswithCreditNotes")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>>> AllocateSelectedInvoiceswithCreditNotes([FromQuery] string SessionUserCode, string TrxCode, string TrxType, string PaidAmount)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule> CollectionModuleList = await _collectionModuleService.AllocateSelectedInvoiceswithCreditNotes(SessionUserCode, TrxCode, TrxType, PaidAmount);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve AllocateSelectedInvoiceswithCreditNotes ", TrxCode);
            throw;
        }
    }
    [HttpGet]
    [Route("GetOrgwiseConfigurationsData")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>>> GetOrgwiseConfigurationsData([FromQuery] string SessionUserCode, string OrgUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule> CollectionModuleList = await _collectionModuleService.GetOrgwiseConfigurationsData(SessionUserCode, OrgUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetOrgwiseConfigurationsData ", OrgUID);
            throw;
        }
    }
    [HttpGet]
    [Route("GetOrgwiseConfigValueByConfigName")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>>> GetOrgwiseConfigValueByConfigName([FromQuery] string SessionUserCode, string OrgUID, string Configname)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule> CollectionModuleList = await _collectionModuleService.GetOrgwiseConfigValueByConfigName(SessionUserCode, OrgUID, Configname);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve CollectionModuleList with OrgUID", OrgUID);
            throw;
        }
    }
    [HttpPost]
    [Route("CreateReceipt")]
    public async Task<ActionResult<object>> CreateReceipt([FromBody] Winit.Modules.CollectionModule.Model.Classes.CollectionDTO[] collectionDTOs)
    {
        if (collectionDTOs == null)
        {
            return CreateErrorResponse("Data Empty");
        }
        List<ICollections> collections = await ConvertCollectionDTOtoCollection(collectionDTOs);
        var createdResponses = new List<object>();
        var Failure = "";
        var retValue = "";
        try
        {
            retValue = await _collectionModuleService.CreateReceipt(collections.ToArray());
            if (retValue == "Successfully Inserted Data Into tables")
            {
                return CreatedAtAction("CreateReceipt", createdResponses);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Create Receipt");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create Receipt");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Create Receipt");
        }
    }

    [HttpPost]
    [Route("CreateReceiptFromQueue")]
    public async Task<ActionResult<object>> CreateReceiptFromQueue([FromBody] Winit.Modules.CollectionModule.Model.Classes.Collections[] collections)

    {
        var retValue = "";
        /*try
        {
            retValue = await _collectionModuleService.CreateReceipt(collection);
            if (retValue == "Successfully Inserted Data Into tables")
            {
                return CreatedAtAction("CreateReceipt", createdResponses);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Create Receipt");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Create Receipt");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Create Receipt");
        }*/
        try
        {
            string step = "";
            var _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
            foreach (var eachCollection in collections)
            {
                if (HttpContext.Request.Headers.TryGetValue("RequestUID", out var messageid)) { }
                //step 1
                messageid = await _dbService.GenerateLogUID(eachCollection.AccCollection.ReceiptNumber, "Collection", JsonConvert.SerializeObject(eachCollection), eachCollection.AccCollection.EmpUID, eachCollection.AccCollection.StoreUID, messageid);
                _logger.LogInformation("LogUID : {@messageid}", messageid);
                step = "Step2";
                try
                {
                    MessageModel messageModel = new MessageModel { MessageUID = messageid, Message = eachCollection };
                    string messageBody = JsonConvert.SerializeObject(messageModel);
                    _logger.LogInformation("From App: {@CollectionReceipt}", eachCollection);
                    if (_rabbitMQService != null)
                    {
                        _rabbitMQService.SendMessage(queueName, messageBody);
                    }
                    //step 2
                    _dbService.UpdateLogByStepAsync(messageid, step, true, false, null);
                    _logger.LogInformation(" API Publishing: {@objEachCollection}", eachCollection);
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
    [HttpPost]
    [Route("CreateReceiptWithZeroValue")]
    public async Task<ActionResult<int>> CreateReceiptWithZeroValue([FromBody] Winit.Modules.CollectionModule.Model.Classes.CollectionDTO collectionModule)

    {
        try
        {
            List<ICollections> collections = await ConvertCollectionDTOtoCollection(collectionModule);
            collectionModule.AccCollection.ServerAddTime = DateTime.Now;
            collectionModule.AccCollection.ServerModifiedTime = DateTime.Now;
            var retValue = await _collectionModuleService.CreateReceiptWithZeroValue(collections.FirstOrDefault());
            if (retValue == "Successfully Inserted Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = collectionModule.AccCollection.ReceiptNumber,
                    ServerAddTime = collectionModule.AccCollection.ServerAddTime,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(500, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to CreateZeroValue Receipt");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to CreateZeroValue Receipt");
        }
    }
    [HttpPost]
    [Route("CreateReceiptWithAutoAllocation")]
    public async Task<ActionResult<int>> CreateReceiptWithAutoAllocation([FromBody] Winit.Modules.CollectionModule.Model.Classes.CollectionDTO[] collectionModule)

    {
        try
        {
            List<ICollections> collections = await ConvertCollectionDTOtoCollection(collectionModule);
            var retValue = await _collectionModuleService.CreateReceiptWithAutoAllocation(collections.ToArray());
            if (retValue == 1)
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
                return Created("Created", createdData);
            }
            else if (retValue == 0)
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now
                };
                return StatusCode(500, createdData);
            }
            else
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    RemainingAmount = retValue
                };
                return StatusCode(400, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create Collection details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create Collection details");
        }
    }
    [HttpPost]
    [Route("CreateReceiptWithEarlyPaymentDiscount")]
    public async Task<ActionResult<int>> CreateReceiptWithEarlyPaymentDiscount([FromBody] Winit.Modules.CollectionModule.Model.Classes.Collections EarlyPaymentRecords)

    {
        try
        {
            EarlyPaymentRecords.AccCollection.ServerAddTime = DateTime.Now;
            EarlyPaymentRecords.AccCollection.ServerModifiedTime = DateTime.Now;
            var retValue = await _collectionModuleService.CreateReceiptWithEarlyPaymentDiscount(EarlyPaymentRecords);
            if (retValue == "Successfully Inserted Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ReceiptNumber = EarlyPaymentRecords.AccCollection.ReceiptNumber,
                    ServerAddTime = EarlyPaymentRecords.AccCollection.ServerAddTime
                };
                return Created("Created", createdData);
            }
            else if (retValue == "Amount Variance")
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now
                };
                return StatusCode(500, createdData);
            }
            else
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return StatusCode(400, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create Collection details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create Collection details");
        }
    }
    [HttpPost]
    [Route("CreateOnAccountReceipt")]
    public async Task<ActionResult<int>> CreateOnAccountReceipt([FromBody] Winit.Modules.CollectionModule.Model.Classes.CollectionDTO collectionModule)
    {
        try
        {
            List<ICollections> collections = await ConvertCollectionDTOtoCollection(collectionModule);
            var retValue = await _collectionModuleService.CreateOnAccountReceipt(collections.FirstOrDefault());
            if (retValue == "Successfully Inserted Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = collectionModule.AccCollection.ReceiptNumber,
                    ServerAddTime = collectionModule.AccCollection.ServerAddTime,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(500, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create OnAccount details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create OnAccount details");
        }
    }
    [HttpPost]
    [Route("CreateReversalReceiptByReceiptNumber")]
    public async Task<ActionResult<int>> CreateReversalReceiptByReceiptNumber([FromQuery] string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
    {
        try
        {
            var retValue = await _collectionModuleService.CreateReversalReceiptByReceiptNumber(ReceiptNumber, TargetUID, Amount, ChequeNo, SessionUserCode, ReasonforCancelation);
            if (retValue == "Successfully Inserted Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = ReceiptNumber,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(500, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create Reversal details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create Reversal details");
        }
    }
    [HttpPost]
    [Route("VOIDCollectionByReceiptNumber")]
    public async Task<ActionResult<int>> VOIDCollectionByReceiptNumber([FromQuery] string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode = null, string ReasonforCancelation = null)
    {
        try
        {
            var retValue = await _collectionModuleService.VOIDCollectionByReceiptNumber(ReceiptNumber, TargetUID, Amount, ChequeNo, SessionUserCode, ReasonforCancelation);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = ReceiptNumber,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(500, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to VOID Receipt");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to VOID Receipt");
        }
    }

    [HttpPost]
    [Route("CreateCollectionSettlementByCashier")]
    public async Task<ActionResult<int>> CreateCollectionSettlementByCashier([FromBody] Winit.Modules.CollectionModule.Model.Classes.AccCollectionSettlement collectionModule)
    {
        try
        {
            var retValue = await _collectionModuleService.CreateCollectionSettlementByCashier(collectionModule);
            if (retValue == "Successfully Inserted Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = collectionModule.ReceiptNumber,
                    ServerAddTime = collectionModule.ServerAddTime,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 500,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(500, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create CollectionSettlement details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create CollectionSettlement details");
        }
    }
    [HttpPost]
    [Route("UpdatePaymentModeDetails")]
    public async Task<ActionResult<int>> UpdatePaymentModeDetails([FromBody] Winit.Modules.CollectionModule.Model.Classes.Collections collectionModule)
    {
        try
        {
            if (collectionModule != null)
            {
                var retValue = await _collectionModuleService.UpdatePaymentModeDetails(collectionModule.AccCollectionPaymentMode);
                if (retValue == "Successfully Updated Data Into tables")
                {
                    var createdData = new CustomResponse
                    {
                        StatusCode = (int)HttpStatusCode.Created,
                        ResponseMessage = retValue,
                        ReceiptNumber = collectionModule.AccCollectionPaymentMode.AccCollectionUID,
                        ServerAddTime = collectionModule.AccCollectionPaymentMode.ServerAddTime,
                        Value = 1
                    };
                    return Created("Created", createdData);
                }
                else
                {
                    var createdData = new ExcepResponse
                    {
                        StatusCode = 404,
                        ServerAddTime = DateTime.Now,
                        ResponseMessage = retValue
                    };
                    return StatusCode(404, createdData);
                }
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update PaymentMode details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Update PaymentMode details");
        }
    }

    [HttpPost]
    [Route("ValidateChequeReceiptByPaymentMode")]
    public async Task<ActionResult<string>> ValidateChequeReceiptByPaymentMode([FromQuery] string UID, string Button, string Comments, string SessionUserCode, string CashNumber)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidateChequeReceiptByPaymentMode(UID, Button, Comments, SessionUserCode, CashNumber);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update Cheque_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Update Cheque_details");
        }
    }

    [HttpPost]
    [Route("ValidatePOSReceiptByPaymentMode")]
    public async Task<ActionResult<string>> ValidatePOSReceiptByPaymentMode([FromQuery] string UID, string Comments, string SessionUserCode)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidatePOSReceiptByPaymentMode(UID, Comments, SessionUserCode);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Vaidate POS_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Vaidate POS_details");
        }
    }

    [HttpPost]
    [Route("ValidateONLINEReceiptByPaymentMode")]
    public async Task<ActionResult<string>> ValidateONLINEReceiptByPaymentMode([FromQuery] string UID, string Comments, string Status, string SessionUserCode)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidateONLINEReceiptByPaymentMode(UID, Comments, SessionUserCode);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Vaidate ONLINE_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Vaidate ONLINE_details");
        }
    }

    [HttpPost]
    [Route("ValidateChequeSettlement")]
    public async Task<ActionResult<string>> ValidateChequeSettlement([FromQuery] string UID, string Comments, string Status, string SessionUserCode, string ReceiptUID, string ChequeNo)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidateChequeSettlement(UID, Comments, Status, SessionUserCode, ReceiptUID, ChequeNo);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update Cheque_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Update Cheque_details");
        }
    }
    [HttpPost]
    [Route("ValidatePOSSettlement")]
    public async Task<ActionResult<string>> ValidatePOSSettlement([FromQuery] string UID, string Comments, string Status, string SessionUserCode)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidatePOSSettlement(UID, Comments, Status, SessionUserCode);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Update POS_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Update POS_details");
        }
    }

    [HttpPost]
    [Route("ValidateONLINESettlement")]
    public async Task<ActionResult<string>> ValidateONLINESettlement([FromQuery] string UID, string Comments, string Status, string SessionUserCode)
    {
        try
        {
            var retValue = await _collectionModuleService.ValidateONLINESettlement(UID, Comments, Status, SessionUserCode);
            if (retValue == "Successfully Updated Data Into tables")
            {
                var createdData = new CustomResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ResponseMessage = retValue,
                    ReceiptNumber = SessionUserCode,
                    Value = 1
                };
                return Created("Created", createdData);
            }
            else
            {
                var createdData = new ExcepResponse
                {
                    StatusCode = 404,
                    ServerAddTime = DateTime.Now,
                    ResponseMessage = retValue
                };
                return StatusCode(404, createdData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to Vaidate ONLINE_details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Vaidate ONLINE_details");
        }
    }

    [HttpGet]
    [Route("GetUser")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccUser>>> GetUser()
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccUser> CollectionModuleList = await _collectionModuleService.GetUser();
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetUser ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetAccountStatement")]
    public async Task<ActionResult<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>>> GetAccountStatement(string StoreUID, string FromDate, string ToDate)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger> _statementList = await _collectionModuleService.GetAccountStatement(StoreUID, FromDate, ToDate);
            if (_statementList.Any() || _statementList.Count == 0)
            {
                return CreateOkApiResponse(_statementList);
            }
            else
            {
                throw new Exception();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetUser ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetAccountStatementPay")]
    public async Task<ActionResult<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>>> GetAccountStatementPay(string StoreUID)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _statementList = await _collectionModuleService.GetAccountStatementPay(StoreUID);
            if (_statementList.Any() || _statementList.Count == 0)
            {
                return CreateOkApiResponse(_statementList);
            }
            else
            {
                throw new Exception();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetUser ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetChequeDetails")]
    public async Task<ActionResult<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>>> GetChequeDetails([FromQuery] string UID, string TargetUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await _collectionModuleService.GetChequeDetails(UID, TargetUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetChequeDetails ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetSettingByType")]
    public async Task<ActionResult<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetSettingByType(string UID)
    {
        try
        {
            IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting> CollectionModuleList = await _collectionModuleService.GetSettingByType(UID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetUser ");
            throw;
        }
    }

    [HttpGet]
    [Route("GetUnSettleAmount")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> GetUnSettleAmount(string AccCollectionUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await _collectionModuleService.GetUnSettleAmount(AccCollectionUID);
            if (CollectionModuleList.Any() || CollectionModuleList.Count() == 0)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve GetUser ");
            throw;
        }
    }

    //DaysTable data bind
    [HttpGet]
    [Route("DaysTable")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> DaysTable(string StoreUID)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> CollectionModuleList = await _collectionModuleService.DaysTable(StoreUID);
            if (CollectionModuleList.Any())
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve DaysTable ");
            throw;
        }
    }
    [HttpGet]
    [Route("DaysTableParent")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> DaysTableParent(string StoreUID, int startDay, int endDay)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> CollectionModuleList = await _collectionModuleService.DaysTableParent(StoreUID, startDay, endDay);
            if (CollectionModuleList.Any() || CollectionModuleList.Count() == 0)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve DaysTable ");
            throw;
        }
    }
    //Receiving the excel 


    //getting data for excel

    [HttpGet]
    [Route("ExcelBalance")]
    public async Task<ActionResult<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> ExcelBalance(string ReceiptNumber, string StoreUID)
    {
        try
        {
            Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable CollectionModuleList = await _collectionModuleService.ExcelBalance(ReceiptNumber, StoreUID);
            if (CollectionModuleList != null)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve DaysTable ");
            throw;
        }
    }

    [HttpGet]
    [Route("PopulateCollectionPage")]
    public async Task<ActionResult<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> CollectionModuleList = await _collectionModuleService.PopulateCollectionPage(CustomerCode, Tabs);
            if (CollectionModuleList != null)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetInvoicesMobile")]
    public async Task<ActionResult<IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> CollectionModuleList = await _collectionModuleService.GetInvoicesMobile(AccCollectionUID);
            if (CollectionModuleList != null)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("ViewPayments")]
    public async Task<ActionResult<IAccCollection>> ViewPayments(PagingRequest pagingRequest)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> paymentList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponsepaymentList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.ViewPayments(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);

            if (PagedResponsepaymentList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("ViewPaymentsDetails")]
    public async Task<ActionResult<IAccCollectionAllotment>> ViewPaymentsDetails(string UID)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await _collectionModuleService.ViewPaymentsDetails(UID);
            if (CollectionModuleList != null)
            {
                return CreateOkApiResponse(CollectionModuleList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("CheckEligibleForDiscount")]
    public async Task<ActionResult<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
    {
        try
        {
            IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IEarlyPaymentDiscountConfiguration> eligibileRecordsList = await _collectionModuleService.CheckEligibleForDiscount(ApplicableCode);
            if (eligibileRecordsList != null)
            {
                return CreateOkApiResponse(eligibileRecordsList);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }

    [HttpGet]
    [Route("GetInvoices")]
    public async Task<ActionResult<IAccPayable>> GetInvoices(string StoreUID)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> invoices = await _collectionModuleService.GetInvoices(StoreUID);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetCustomerCode")]
    public async Task<ActionResult<IStore>> GetCustomerCode(string CustomerCode)
    {
        try
        {
            List<Winit.Modules.Store.Model.Interfaces.IStore> invoices = await _collectionModuleService.GetCustomerCode(CustomerCode);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetCurrencyRateRecords")]
    public async Task<ActionResult<IExchangeRate>> GetCurrencyRateRecords(string StoreUID)
    {
        try
        {
            List<IExchangeRate> invoices = await _collectionModuleService.GetCurrencyRateRecords(StoreUID);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetMultiCurrencyDetails")]
    public async Task<ActionResult<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID)
    {
        try
        {
            List<IAccCollectionCurrencyDetails> invoices = await _collectionModuleService.GetMultiCurrencyDetails(AccCollectionUID);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }

    [HttpGet]
    [Route("GetConfigurationDetails")]
    public async Task<ActionResult<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails()
    {
        try
        {
            List<IEarlyPaymentDiscountConfiguration> invoices = await _collectionModuleService.GetConfigurationDetails();
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetReceipts")]
    public async Task<ActionResult<IAccCollection>> GetReceipts()
    {
        try
        {
            List<IAccCollection> invoices = await _collectionModuleService.GetReceipts();
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetRequestReceipts")]
    public async Task<ActionResult<IAccCollectionDeposit>> GetRequestReceipts(string Status)
    {
        try
        {
            List<IAccCollectionDeposit> invoices = await _collectionModuleService.GetRequestReceipts(Status);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("ViewReceipts")]
    public async Task<ActionResult<CollectionDepositDTO>> ViewReceipts(string RequestNo)
    {
        try
        {
            IAccCollectionAndDeposit invoices = await _collectionModuleService.ViewReceipts(RequestNo);
            if (invoices != null)
            {
                List<AccCollectionAndDeposit> collectionDeposit = await ConvertCollectionDeposittoCollectionDepositDTO(invoices);
                return CreateOkApiResponse(collectionDeposit.First());
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }

    [HttpPost]
    [Route("AddEarlyPayment")]
    public async Task<ActionResult<string>> AddEarlyPayment(EarlyPaymentDiscountConfiguration EarlyPayment)
    {
        try
        {
            string _earlyPayment = await _collectionModuleService.AddEarlyPayment(EarlyPayment);
            if (_earlyPayment != null)
            {
                return CreateOkApiResponse(_earlyPayment);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("ApproveOrRejectDepositRequest")]
    public async Task<ActionResult<string>> ApproveOrRejectDepositRequest([FromBody] AccCollectionDeposit accCollectionDeposit, [FromQuery]string Status)
    {
        try
        {
            bool result = await _collectionModuleService.ApproveOrRejectDepositRequest(accCollectionDeposit, Status);
            if (result)
            {
                return CreateOkApiResponse(Status == "Approved" ? "1" : Status == "Rejected" ?  "2" : "3");
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("UpdateCollectionLimit")]
    public async Task<ActionResult<bool>> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
    {
        try
        {
            bool result = await _collectionModuleService.UpdateCollectionLimit(Limit, EmpUID, Action);
            if (result)
            {
                return CreateOkApiResponse(result);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("UpdateBankDetails")]
    public async Task<IActionResult> UpdateBankDetails(string UID, string BankName, string Branch, string ReferenceNumber)
    {
        try
        {
            bool result = await _collectionModuleService.UpdateBankDetails(UID, BankName, Branch, ReferenceNumber);
            if (result)
            {
                return CreateOkApiResponse(result);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("GetCollectionTabsDetails")]
    public async Task<ActionResult<IAccCollection>> GetCollectionTabsDetails([FromBody]PagingRequest pagingRequest, [FromQuery]string PageName)
    {
        try
        {
            List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> collectionTabList = null;
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> PagedResponseCollectionTabList = null;
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            PagedResponseCollectionTabList = await _collectionModuleService.GetCollectionTabsDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias, PageName);

            if (PagedResponseCollectionTabList == null)
            {
                return NotFound();
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponseCollectionTabList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    
    [HttpGet]
    [Route("ConvertCollectionDTOtoCollection")]
    public async Task<List<ICollections>> ConvertCollectionDTOtoCollection(object data)
    {
        try
        {
            ICollections result = null;

            if (data is CollectionDTO[] array)
            {
                List<ICollections> collections = new List<ICollections>();
                foreach (CollectionDTO collectionDTO in array)
                {
                    Collections collection = new Collections();
                    collection.AccCollection = collectionDTO.AccCollection;
                    collection.AccCollectionPaymentMode = collectionDTO.AccCollectionPaymentMode;
                    collection.AccStoreLedger = collectionDTO.AccStoreLedger;
                    collection.AccCollectionSettlement = collectionDTO.AccCollectionSettlement;
                    collection.AccCollectionAllotment = collectionDTO.AccCollectionAllotment?.ToList<IAccCollectionAllotment>();
                    collection.AccPayable = collectionDTO.AccPayable?.ToList<IAccPayable>();
                    collection.AccReceivable = collectionDTO.AccReceivable?.ToList<IAccReceivable>();
                    collection.AccCollectionSettlementReceipts = collectionDTO.AccCollectionSettlementReceipts?.ToList<IAccCollectionSettlementReceipts>();
                    collection.AccCollectionCurrencyDetails = collectionDTO.AccCollectionCurrencyDetails?.ToList<IAccCollectionCurrencyDetails>();
                    collections.Add(collection);
                }
                return collections;
            }
            else if (data is CollectionDTO singleDTO)
            {
                Collections collection = new Collections();
                collection.AccCollection = singleDTO.AccCollection;
                collection.AccCollectionPaymentMode = singleDTO.AccCollectionPaymentMode;
                collection.AccStoreLedger = singleDTO.AccStoreLedger;
                collection.AccCollectionSettlement = singleDTO.AccCollectionSettlement;
                collection.AccCollectionAllotment = singleDTO.AccCollectionAllotment?.ToList<IAccCollectionAllotment>();
                collection.AccPayable = singleDTO.AccPayable?.ToList<IAccPayable>();
                collection.AccReceivable = singleDTO.AccReceivable?.ToList<IAccReceivable>();
                collection.AccCollectionSettlementReceipts = singleDTO.AccCollectionSettlementReceipts?.ToList<IAccCollectionSettlementReceipts>();
                collection.AccCollectionCurrencyDetails = singleDTO.AccCollectionCurrencyDetails?.ToList<IAccCollectionCurrencyDetails>();
                return new List<ICollections>() { collection };
            }
            throw new Exception();

        }
        catch (Exception ex)
        {
            throw new Exception();
        }
    }
    [HttpGet]
    [Route("ConvertCollectionDeposittoCollectionDepositDTO")]
    public async Task<List<AccCollectionAndDeposit>> ConvertCollectionDeposittoCollectionDepositDTO(object data)
    {
        try
        {
            if (data is List<AccCollectionAndDeposit> array)
            {
                List<AccCollectionAndDeposit> accCollectionDeposits = new List<AccCollectionAndDeposit>();
                foreach (AccCollectionAndDeposit collectionDepositDTO in array)
                {
                    AccCollectionAndDeposit collectionAndDeposit = new AccCollectionAndDeposit();
                    collectionAndDeposit.accCollections = collectionDepositDTO.accCollections?.ToList<IAccCollection>();
                    collectionAndDeposit.accCollectionDeposits = collectionDepositDTO.accCollectionDeposits?.ToList<IAccCollectionDeposit>();
                    accCollectionDeposits.Add(collectionAndDeposit);
                }
                return accCollectionDeposits;
            }
            else if (data is AccCollectionAndDeposit singleDTO)
            {
                AccCollectionAndDeposit collectionAndDeposit = new AccCollectionAndDeposit();
                collectionAndDeposit.accCollections = singleDTO.accCollections?.ToList<IAccCollection>();
                collectionAndDeposit.accCollectionDeposits = singleDTO.accCollectionDeposits?.ToList<IAccCollectionDeposit>();
                return new List<AccCollectionAndDeposit>() { collectionAndDeposit };
            }
            throw new Exception();
        }
        catch (Exception ex)
        {
            throw new Exception();
        }
    }
    [HttpPost]
    [Route("StoreStatementRecords")]
    public async Task<ActionResult> StoreStatementRecords([FromBody]PagingRequest pagingRequest, [FromQuery] string StartDate, [FromQuery] string EndDate)
    {
        try
        {
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IStoreStatement> PagedResponsepaymentList = new PagedResponse<IStoreStatement>();
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            PagedResponsepaymentList = await _collectionModuleService.StoreStatementRecords(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, StartDate, EndDate);

            if (PagedResponsepaymentList == null)
            {
                return BadRequest("No Records");
            }
            else
            {
                //AddressListResponse = AddressList.OfType<object>().ToList();
                //_cacheService.Set(cacheKey, AddressListResponse, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
            }
            return CreateOkApiResponse(PagedResponsepaymentList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPut]
    [Route("UpdateBalanceConfirmation")]
    public async Task<ActionResult<bool>> UpdateBalanceConfirmation([FromBody]IBalanceConfirmation balanceConfirmation)
    {
        try
        {
            bool result = await _collectionModuleService.UpdateBalanceConfirmation(balanceConfirmation);
            if (result)
            {
                return CreateOkApiResponse(result);
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPut]
    [Route("UpdateBalanceConfirmationForResolvingDispute")]
    public async Task<ActionResult<bool>> UpdateBalanceConfirmationForResolvingDispute([FromBody]IBalanceConfirmation balanceConfirmation)
    {
        try
        {
            bool result = await _collectionModuleService.UpdateBalanceConfirmationForResolvingDispute(balanceConfirmation);
            if (result)
            {
                return CreateOkApiResponse(result);
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpPost]
    [Route("InsertDisputeRecords")]
    public async Task<ActionResult> InsertDisputeRecords([FromBody] List<IBalanceConfirmationLine> balanceConfirmationLine)
    {
        try
        {
            bool retValue = await _collectionModuleService.InsertDisputeRecords(balanceConfirmationLine);
            if (retValue )
            {
                return Created("Created", retValue);
            }
            else
            {
                return StatusCode(500, retValue);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create CollectionSettlement details");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create CollectionSettlement details");
        }
    }
    [HttpGet]
    [Route("GetBalanceConfirmationDetails")]
    public async Task<ActionResult<IBalanceConfirmation>> GetBalanceConfirmationDetails(string StoreUID)
    {
        try
        {
            IBalanceConfirmation invoices = await _collectionModuleService.GetBalanceConfirmationDetails(StoreUID);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetBalanceConfirmationLineDetails")]
    public async Task<ActionResult<List<IBalanceConfirmationLine>>> GetBalanceConfirmationLineDetails(string UID)
    {
        try
        {
            List<IBalanceConfirmationLine> invoices = await _collectionModuleService.GetBalanceConfirmationLineDetails(UID);
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetBalanceConfirmationListDetails")]
    public async Task<ActionResult<List<IBalanceConfirmation>>> GetBalanceConfirmationListDetails()
    {
        try
        {
            List<IBalanceConfirmation> invoices = await _collectionModuleService.GetBalanceConfirmationListDetails();
            if (invoices != null)
            {
                return CreateOkApiResponse(invoices);
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
    [HttpGet]
    [Route("GetContactDetails")]
    public async Task<ActionResult<IContact>> GetContactDetails([FromQuery] string EmpCode)
    {
        try
        {
            IContact contact = await _collectionModuleService.GetContactDetails(EmpCode);
            if (contact != null)
            {
                return CreateOkApiResponse(contact);
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve Data ");
            throw;
        }
    }
}
