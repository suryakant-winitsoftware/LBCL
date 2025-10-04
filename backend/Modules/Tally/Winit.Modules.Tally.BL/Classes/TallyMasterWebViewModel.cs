using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Classes
{
    public class TallyMasterWebViewModel : TallyMasterBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public TallyMasterWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task<List<IStore>> GetChannelPartnersListForMaster()
        {
            return await GetChannelPartnersListForMasterFromAPIAsync();
        }
        public override async Task<List<ITallyDealerMaster>> GetDealerMasterGridDataByDistUID(string UID)
        {
            return await GetDealerMasterGridDataByDistFromApiAsync(UID);
        }
        public override async Task<List<ITallyInventoryMaster>> GetInventoryMasterGridDataByDistUID(string UID)
        {
            return await GetInventoryMasterGridDataByDistFromApiAsync(UID);
        }
        public override async Task<List<ITallySalesInvoiceMaster>> GetSalesInvoiceMasterGridDataByDistUID(string UID)
        {
            return await GetSalesInvoiceMasterGridDataByDistFromApiAsync(UID);
        }
        public override async Task<List<ITallySalesInvoiceLineMaster>> GetSalesInvoiceLineMasterGridData(string UID)
        {
            return await GetSalesInvoiceLineMasterGridDataFromApiAsync(UID);
        }
        public override async Task <ITallyDealerMaster> GetDealerMasterItemDetailsByUID(string uid)
        {
            return await GetDealerMasterItemDetailsByUIDFromApiAsync(uid);
        }
        public override async Task<ITallyInventoryMaster> GetInventoryMasterItemDetailsByUID(string uid)
        {
            return await GetInventoryMasterItemDetailsByUIDFromApiAsync(uid);
        }
        public override async Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItemDetailsUID(string uid)
        {
            return await GetSalesInvoiceMasterItemDetailsUIDFromApiAsync(uid);
        }
        public override async Task<List<ITallySalesInvoiceResult>> GetTallySalesInvoiceDataByUID(string uid)
        {
            return await GetTallySalesInvoiceDataByUIDFromApiAsync(uid);
        }
        private async Task<List<IStore>> GetChannelPartnersListForMasterFromAPIAsync()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>(
                    $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IStore>();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<List<ITallyDealerMaster>> GetDealerMasterGridDataByDistFromApiAsync(string UID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = DealerMasterFilterCriteria;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetDealerMasterByDistByUID/{UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallyDealerMaster>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallyDealerMaster>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<List<ITallyInventoryMaster>> GetInventoryMasterGridDataByDistFromApiAsync(string UID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = InventoryMasterFilterCriteria;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetInventoryMasterByDistByUID/{UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallyInventoryMaster>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallyInventoryMaster>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<List<ITallySalesInvoiceMaster>> GetSalesInvoiceMasterGridDataByDistFromApiAsync(string UID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = SalesInvoiceMasterFilterCriteria;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;
                //List<SortCriteria> sorts = new List<SortCriteria>();
                //sorts.Add(new SortCriteria("date", SortDirection.Desc));
                //pagingRequest.SortCriterias.AddRange(sorts);
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetSalesInvoiceMasterByDistByUID/{UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<List<ITallySalesInvoiceLineMaster>> GetSalesInvoiceLineMasterGridDataFromApiAsync(string UID)
        {
            try
            {
                string encodedUid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(UID));
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;   
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetSalesInvoiceLineMasterByDistByUID/{encodedUid}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceLineMaster>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceLineMaster>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<ITallyDealerMaster> GetDealerMasterItemDetailsByUIDFromApiAsync(string uid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Tally.Model.Classes.TallyDealerMaster> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Tally.Model.Classes.TallyDealerMaster>(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetDealerMasterItemDetails/{uid}",
                    HttpMethod.Get, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<ITallyInventoryMaster> GetInventoryMasterItemDetailsByUIDFromApiAsync(string uid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = InventoryMasterFilterCriteria;
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Tally.Model.Classes.TallyInventoryMaster> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Tally.Model.Classes.TallyInventoryMaster>(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetInventoryMasterItemDetails/{uid}",
                    HttpMethod.Get,pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItemDetailsUIDFromApiAsync(string uid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetSalesInvoiceMasterItemDetails/{uid}",
                    HttpMethod.Get,pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<ITallySalesInvoiceResult>> GetTallySalesInvoiceDataByUIDFromApiAsync(string UID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}TallyMaster/GetTallySalesInvoiceData/{UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceResult>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceResult>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public override async Task<bool> InsertTallyMaster(List<ITallyDealerMaster> tallyDBDetails)
        {
            try
            {
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>
                ($"{_appConfigs.ApiBaseUrl}TallyMaster/UpdateTallyMasterData", HttpMethod.Put, tallyDBDetails);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
