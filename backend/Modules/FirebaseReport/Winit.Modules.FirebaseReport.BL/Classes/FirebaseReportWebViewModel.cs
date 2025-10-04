using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.FirebaseReport.Models.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models;
using DBServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Winit.Modules.FirebaseReport.BL.Classes;

public class FirebaseReportWebViewModel : FirebaseReportBaseViewModel
{
    protected readonly IAppConfig _appConfigs;
    protected readonly ApiService _apiService;

    public FirebaseReportWebViewModel(IAppUser appUser, IAppConfig appConfig, ApiService apiService) :base(appUser)
    {
        _appConfigs = appConfig;    
        _apiService = apiService;
    }
    public async override Task<List<IFirebaseReport>?> GetFirebaseData()
    {

        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = this.FilterCriterias;
            ApiResponse<PagedResponse<Winit.Modules.FirebaseReport.Models.Classes.FirebaseReport>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.FirebaseReport.Models.Classes.FirebaseReport>>($"{_appConfigs.ApiBaseUrl}FirebaseReport/SelectAllFirebaseReportDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList<IFirebaseReport>();
            }
            return default;
        }
        catch (Exception)
        {
            throw;

        }
    }

    public async override Task<IFirebaseReport?> GetFirebaseDetailsData(string UID)
    {

        try
        {
            //var requestData = new { UID };
            ApiResponse<Winit.Modules.FirebaseReport.Models.Classes.FirebaseReport> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.FirebaseReport.Models.Classes.FirebaseReport>($"{_appConfigs.ApiBaseUrl}FirebaseReport/SelectFirebaseDetailsData",
                HttpMethod.Post, UID);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
            return default;
        }
        catch (Exception)
        {
            throw;

        }
    }

    public async override Task<int> RepostFirebaseLog(IFirebaseReport FirebaseReport)
    {

        try
        {
            object requestData = null;
            string endpoint = string.Empty;
            switch (FirebaseReport.LinkedItemType)
            {
                case "ReturnOrder":
                    var returnOrder = JsonConvert.DeserializeObject<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>(FirebaseReport.RequestBody);
                    List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder> objReturnOrderList = new List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>();
                    objReturnOrderList.Add(returnOrder);
                    requestData = objReturnOrderList;
                    //requestData = JsonConvert.DeserializeObject<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder[]>(FirebaseReport.RequestBody);
                    //objReturnOrderList.Add(requestData.GetType().);
                    endpoint = "ReturnOrder/CreateReturnOrderFromQueue";
                    break;
                case "SalesOrder":
                    var salesOrder = JsonConvert.DeserializeObject<Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO>(FirebaseReport.RequestBody);
                    List<Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO> objSalesOrderList = new List<Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO>();
                    objSalesOrderList.Add(salesOrder);
                    requestData = objSalesOrderList;
                    //requestData = JsonConvert.DeserializeObject<Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO[]>(FirebaseReport.RequestBody);
                    endpoint = "SalesOrder/CreateSalesOrderFromQueue";
                    break;
                case "WHStock":
                    var whStock = JsonConvert.DeserializeObject<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>(FirebaseReport.RequestBody);
                    List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel> objWhStock = new List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>();
                    objWhStock.Add(whStock);
                    requestData = objWhStock;
                    //requestData = JsonConvert.DeserializeObject<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel[]>(FirebaseReport.RequestBody);
                    endpoint = "WHStock/CreateWHStockFromQueue";
                    break;
                case "Collection":
                    var collection = JsonConvert.DeserializeObject<Winit.Modules.CollectionModule.Model.Classes.Collections>(FirebaseReport.RequestBody);
                    List<Winit.Modules.CollectionModule.Model.Classes.Collections> objCollection = new List<Winit.Modules.CollectionModule.Model.Classes.Collections>();
                    objCollection.Add(collection);
                    requestData = objCollection;
                    //requestData = JsonConvert.DeserializeObject<Winit.Modules.CollectionModule.Model.Classes.AccCollectionNew[]>(FirebaseReport.RequestBody);
                    endpoint = "CollectionModule/CreateReceiptFromQueue";
                    break;
                default:
                    throw new ArgumentException($"Unsupported LinkedItemType: {FirebaseReport.LinkedItemType}");
            }
            var headers = new Dictionary<string, string>
            {
                { "RequestUID", FirebaseReport.UID },
            };
            ApiResponse<string> apiResponseMove =
            await _apiService.FetchDataAsync<string>($"{_appConfigs.ApiBaseUrl}Repost/MoveLogToRepost",
            HttpMethod.Post, FirebaseReport.UID);
            ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync<string>($"{_appConfigs.ApiBaseUrl}{endpoint}",
            HttpMethod.Post, requestData!, headers);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.StatusCode;
            }
            return default;
        }
        catch (Exception ex)
        {
            throw;

        }
    }

    /*public async override Task<int> RepostFirebaseLog_Data(LogEntry logEntry)
    {
        try
        {
            if (logEntry.NextUID != null)
            {
                List<LogEntry> logEntries = new List<LogEntry> { logEntry };
                ApiResponse<int> apiResponse =
                    await _apiService.FetchDataAsync<int>($"{_appConfigs.ApiBaseUrl}UpsertLogs/UPSertLogs",
                    HttpMethod.Post, logEntries);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != 0)
                {
                    return apiResponse.Data;
                }
                return default;
            }
            else
            {
                return 2;
            }
        }
        catch(Exception ex) {
            throw;
        }
    }

    public async override Task<List<string>?> GetTypeFilterData()
    {

        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = this.FilterCriterias;
            ApiResponse<IEnumerable<string>> apiResponse =
                await _apiService.FetchDataAsync<IEnumerable<string>>($"{_appConfigs.ApiBaseUrl}FirebaseReport/SelectAllFirebaseReportTypes",
                HttpMethod.Get, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.ToList<string>();
            }
            return default;
        }
        catch (Exception)
        {
            throw;

        }
    }*/
}