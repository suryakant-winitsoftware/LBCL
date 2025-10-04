using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.ListHeader.Model.Interfaces;
using iTextSharp.text;
using Newtonsoft.Json;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Nest;
using System.Security.Cryptography;
using Winit.Modules.Common.BL.Classes;
using Microsoft.Extensions.Configuration;
using Winit.Modules.ListHeader.Model.Classes;

namespace Winit.Modules.ServiceAndCallRegistration.BL.Classes
{
    public class ServiceAndCallRegistrationWebViewModel : ServiceAndCallRegistrationBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();

        private readonly IConfiguration _configuration;

        public ServiceAndCallRegistrationWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
           Winit.Modules.Base.BL.ApiService apiService,
           IConfiguration configuration
         ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _configuration = configuration;
            _appConfigs = appConfigs;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetProductCategorySelectionList(string skuGroupTypeUid)
        {
            return await GetProductCategorySelectionListFromApi(skuGroupTypeUid);
        }
        public override async Task<List<IListItem>> GetCustTypeFromListItem(string listHeaderCode)
        {
            return await GetCustTypeFromListItemFromAPI(listHeaderCode);
        }
        public override async Task<ICallRegistrationResponce> SaveCallRegistrationDetailsAPI(ICallRegistration callRegistrationDetails)
        {
            return await SaveCallRegistrationDetailsByAPI(callRegistrationDetails);
        }
        public override async Task<bool> SaveCallRegistrationDetailsToDB(ICallRegistration callRegistrationDetails)
        {
            return await SaveCallRegistrationDetailsToDBByAPI(callRegistrationDetails);
        }
        public override async Task<List<IListItem>> GetServiceAndCallRegistrationDropDownsItems(List<string> serviceCallRegistrationDropDownsItems)
        {
            return await GetServiceAndCallRegistrationDropDownsItemsFromApi(serviceCallRegistrationDropDownsItems);
        }


        public override async Task<IServiceRequestStatusResponce> GetServiceStatusBasedOnNumberAPI(IServiceRequestStatus serviceNumber)
        {
            return await GetServiceStatusBasedOnNumberFromAPI(serviceNumber);
        }
        public override async Task<List<ICallRegistration>> PopulateCallRegistrationsAsync()
        {
            return await PopulateCallRegistrationsAsyncFromAPI();
        }
        public override async Task<ICallRegistration> PopulateCallRegistrationItemDetailsByUIDAsync(string serviceCallNumber)
        {
            return await PopulateCallRegistrationItemDetailsByUIDAsyncFromAPI(serviceCallNumber);
        }
        private async Task<List<IListItem>> GetServiceAndCallRegistrationDropDownsItemsFromApi(List<string> serviceCallRegistrationDropDownsItems)
        {
            try
            {
                var listItemRequest = new ListItems
                {
                    ListItemRequest = new ListItemRequest
                    {
                        Codes = serviceCallRegistrationDropDownsItems, // Assuming 'uid' is a string representing a code
                        isCountRequired = true            // Set this to true as required
                    },
                    PagingRequest = new PagingRequest
                    {
                        PageNumber = PageNumber,     // Example: setting page number
                        PageSize = PageSize       // Example: setting page size
                    }
                };

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.SortCriterias = this.SortCriterias;
                //pagingRequest.PageNumber = PageNumber;
                //pagingRequest.PageSize = PageSize;
                if (listItemRequest != null)
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByListHeaderCodes",
                    HttpMethod.Post, listItemRequest);
                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                        if (pagedResponse != null)
                        {
                            TotalItemsCount = pagedResponse.Data.TotalCount;
                            return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public async Task<IServiceRequestStatusResponce> GetServiceStatusBasedOnNumberFromAPI(IServiceRequestStatus serviceNumber)
        {
            try
            {

                ApiResponse<Winit.Modules.ServiceAndCallRegistration.Model.Classes.ServiceRequestStatusResponce> apiResponse =
                      await _apiService.FetchDataAsync<Winit.Modules.ServiceAndCallRegistration.Model.Classes.ServiceRequestStatusResponce>(
                      $"{_appConfigs.ApiBaseUrl}ServiceAndCallRegistration/ServiceStatus?callId={serviceNumber.CallId}",
                      HttpMethod.Get, serviceNumber);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {

            }
            return default;
        }
        private async Task<ICallRegistrationResponce> SaveCallRegistrationDetailsByAPI(ICallRegistration callRegistrationDetails)
        {
            try
            {

                ApiResponse<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistrationResponce> apiResponse =
                      await _apiService.FetchDataAsync<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistrationResponce>(
                      $"{_appConfigs.ApiBaseUrl}ServiceAndCallRegistration/LogACall",
                      HttpMethod.Post, callRegistrationDetails);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<bool> SaveCallRegistrationDetailsToDBByAPI(ICallRegistration callRegistrationDetails)
        {
            try
            {
                ApiResponse<string> apiResponse;
                callRegistrationDetails.UID = Guid.NewGuid().ToString();
                callRegistrationDetails.CreatedBy = _appUser.Emp.Name;
                callRegistrationDetails.ModifiedBy = _appUser.Emp.Name;
                callRegistrationDetails.CreatedTime = DateTime.Now;
                callRegistrationDetails.ModifiedTime = DateTime.Now;
                callRegistrationDetails.ServerAddTime = DateTime.Now;
                callRegistrationDetails.ServerModifiedTime = DateTime.Now;
                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}CallRegistration/SaveCallRegistrationDetails",
                    HttpMethod.Post, callRegistrationDetails);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<List<ISKUGroup>> GetProductCategorySelectionListFromApi(string skuGroupTypeUid)
        {
            try
            {
                ApiResponse<IEnumerable<Winit.Modules.SKU.Model.Classes.SKUGroup>> apiResponse =
                      await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.SKU.Model.Classes.SKUGroup>>(
                      $"{_appConfigs.ApiBaseUrl}Org/GetSkuGroupBySkuGroupTypeUID?skuGroupTypeUid={skuGroupTypeUid}",
                      HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToHashSet<ISKUGroup>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<List<IListItem>> GetCustTypeFromListItemFromAPI(string listHeaderCode)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new Winit.Modules.ListHeader.Model.Classes.ListItemRequest();
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;

                listItemRequest.Codes = new List<string>() { listHeaderCode };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                    HttpMethod.Post, listItemRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {
                        return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<ICallRegistration>> PopulateCallRegistrationsAsyncFromAPI()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = CallRegistrationFilterCriteria;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}CallRegistration/GetCallRegistrations/{_appUser.SelectedJobPosition.UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return [];
        }

        private async Task<ICallRegistration> PopulateCallRegistrationItemDetailsByUIDAsyncFromAPI(string serviceCallNumber)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration>(
                $"{_appConfigs.ApiBaseUrl}CallRegistration/GetCallRegistrationItemDetailsByCallID/{serviceCallNumber}",
                HttpMethod.Get, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }
    }
}
