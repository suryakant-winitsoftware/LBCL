using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Provisioning.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Modules.Common.BL.Classes;
using Winit.Shared.Models.Enums;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.BroadClassification.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.Modules.Provisioning.Model.Classes;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public class ProvisioningHeaderViewWebViewModel : ProvisioningHeaderViewBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ProvisioningHeaderViewWebViewModel(IServiceProvider serviceProvider,
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

        public override async Task<List<IStore>> GetChannelPartnersListForProvisioning()
        {
            return await GetChannelPartnersListForProvisioningFromAPIAsync();
        }
        public override async Task<IProvisionHeaderView> GetprovisionHeaderViewDetailsByUID(string UID)
        {
            return await GetprovisionHeaderViewDetailsByUIDFromApiAsync(UID);
        }

       
        private async Task<List<IStore>> GetChannelPartnersListForProvisioningFromAPIAsync()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>(
                    $"{_appConfigs.ApiBaseUrl}store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
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
        private async Task<IProvisionHeaderView> GetprovisionHeaderViewDetailsByUIDFromApiAsync(string UID)
        {
            try
            {
                ApiResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionHeaderView> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Provisioning.Model.Classes.ProvisionHeaderView>(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/GetProvisioningHeaderViewByUID/{UID}",
                    HttpMethod.Get);
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
        // Provision Approval
        protected override async Task<bool> InsertProvisionRequestDetails(List<IProvisionApproval> provisionApproval)
        {
            return await InsertProvisionRequestHistoryDetailsFromApiAsync(provisionApproval);
        }
        protected override async Task<List<IBranch>> GetBranch()
        {
            return await GetBranchFromApiAsync();
        }
        protected override async Task<List<ISalesOffice>> GetSalesOffice()
        {
            return await GetSalesOfficeFromApiAsync();
        }
        protected override async Task<List<IBroadClassificationHeader>> GetBroadClassification()
        {
            return await GetBroadClassificationFromApiAsync();
        }
        protected override async Task<List<IStore>> GetChannelPartnerDetails()
        {
            return await GetChannelPartnerDetailsFromApiAsync();
        }
        protected override async Task<List<IListItem>> GetListItemsByCodes(List<string> codes)
        {
            return await GetListItemsByCodesFromApiAsync(codes);
        }
        public override async Task<List<IProvisionApproval>> GetSummaryDetails()
        {
            return await GetSummaryDetailsFromApiAsync();
        }
        public override async Task<List<IProvisionApproval>> GetSummaryProvisionRequestHistoryDetails()
        {
            return await GetSummaryProvisionRequestHistoryDetailsFromApiAsync();
        }
        public override async Task<List<IProvisionApproval>> GetProvisionRequestHistoryDetailsByProvisionIds(List<string> ProvisionIds)
        {
            return await GetProvisionRequestHistoryDetailsByProvisionIdsFromApiAsync(ProvisionIds);
        }
        public override async Task<List<IProvisionApproval>> GetDetailViewDetails()
        {
            return await GetDetailViewDetailsFromApiAsync();
        }
        protected override async Task<bool> UpdateProvisionStatus(List<IProvisionApproval> provisionApprovals, string View)
        {
            return await UpdateProvisionStatusFromApiAsync(provisionApprovals, View);
        }
        private async Task<List<IProvisionApproval>> GetSummaryDetailsFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = 50;
                pagingRequest.FilterCriterias = FilterCriterias;
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
                pagingRequest.IsCountRequired = true;
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/GetProvisionApprovalSummary",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<IProvisionApproval>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IProvisionApproval>> GetSummaryProvisionRequestHistoryDetailsFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = 500;
                pagingRequest.FilterCriterias = FilterCriterias;
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
                pagingRequest.IsCountRequired = true;
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/GetProvisionRequestHistoryDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<IProvisionApproval>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IProvisionApproval>> GetDetailViewDetailsFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = 500;
                pagingRequest.FilterCriterias = FilterCriterias;
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
                pagingRequest.IsCountRequired = true;
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/GetProvisionApprovalDetail",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<IProvisionApproval>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IProvisionApproval>> GetProvisionRequestHistoryDetailsByProvisionIdsFromApiAsync(List<string> ProvisionIds)
        {
            try
            {
                var provisionIdsQuery = string.Join(",", ProvisionIds);
                ApiResponse<List<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>>(
                $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/SelectProvisionRequestHistoryByProvisionIds",
                HttpMethod.Post, ProvisionIds);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IProvisionApproval>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> UpdateProvisionStatusFromApiAsync(List<IProvisionApproval> provisionData, string View)
        {
            try
            {
                List<Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest> pendingRequestData = provisionData.Select(data => 
                new PendingDataRequest
                {
                    LinkedItemUid = View == ProvisionConstants.SummaryView ? data.ProvisionIDs : data.Id.ToString(),
                    LinkedItemType = "Provision",
                    Status = "Pending"
                }
                ).Cast<IPendingDataRequest>().ToList();

                ApiResponse<string>? apiResponse = null;
                apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/UpdateProvisionData", HttpMethod.Put, provisionData
                .Select(p => View == ProvisionConstants.SummaryView ? p.ProvisionIDs : p.Id.ToString()).ToList());
                        // }
                ApiResponse<string>? apiResponseIntegration = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}IntPendingDataInsertion/InsertPendingDataList", HttpMethod.Post, pendingRequestData);

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
        private async Task<bool> InsertProvisionRequestHistoryDetailsFromApiAsync(List<IProvisionApproval> provisionApproval)
        {
            try
            {

                provisionApproval.ForEach(p =>
                {
                    p.UID = Guid.NewGuid().ToString();
                    p.CreatedBy = _appUser.Emp.UID;
                    p.CreatedTime = DateTime.Now;
                    p.ModifiedBy = _appUser.Emp.UID;
                    p.ModifiedTime = DateTime.Now;
                    p.ServerAddTime = DateTime.Now;
                    p.ServerModifiedTime = DateTime.Now;
                    p.RequestedDate = DateTime.Now;
                    p.SS = 0;
                    p.RequestedByEmpUID = _appUser.Emp.UID;
                });
                ApiResponse<string>? apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}ProvisioningHeaderView/InsertProvisionRequestHistory", HttpMethod.Post, provisionApproval);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return false;
        }
        private async Task<List<IBranch>> GetBranchFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Branch/SelectAllBranchDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Branch>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Branch>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.ToList<IBranch>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<ISalesOffice>> GetSalesOfficeFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SalesOffice/SelectAllSalesOfficeDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<SalesOffice>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<SalesOffice>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.ToList<ISalesOffice>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IBroadClassificationHeader>> GetBroadClassificationFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<BroadClassificationHeader>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<BroadClassificationHeader>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.ToList<IBroadClassificationHeader>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IStore>> GetChannelPartnerDetailsFromApiAsync()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>(
                $"{_appConfigs.ApiBaseUrl}store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
                HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IStore>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        
        private async Task<List<IListItem>> GetListItemsByCodesFromApiAsync(List<string> codes)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new();
                listItemRequest.Codes = codes;
                ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<ListHeader.Model.Classes.ListItem>>(
                        $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                        HttpMethod.Post, listItemRequest);
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    return apiResponse.Data.PagedData.ToList<IListItem>();
                }
                return new();
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
