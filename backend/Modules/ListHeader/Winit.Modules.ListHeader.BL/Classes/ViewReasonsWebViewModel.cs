using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.BL.Classes
{
    public class ViewReasonsWebViewModel:ViewReasonsBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public ViewReasonsWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, Common.BL.Interfaces.IAppUser appUser)
        : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService, appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            ListHeaders = new List<IListHeader>();
            _appUser = appUser;
        }
        public override async Task PopulateViewModel()
        {
           await base.PopulateViewModel();
        }
        public override async Task<List<IListHeader>> GetReasonsListHeaderData()
        {
            return await GetReasonsListHeaderDataFromAPIAsync();
        }
        public override async Task<List<IListItem>> GetReasonsListItemData(string uid)
        {
            return await GetReasonsListItemDataFromAPIAsync(uid);
        }
        public override async Task<IListItem> GetViewReasonsforEditDetailsData(string orguid)
        {
            return await GetViewReasonsforEditDetailsDataFromAPIAsync(orguid);
        }
        //public override async Task<bool> UpdateViewReasons_data(IListItem listUID)
        //{
        //    return await UpdateViewReasonsDataFromAPIAsync(listUID);
        //}
        //public override async Task<bool> SaveReasonsData_data()
        //{
        //    return await SaveReasonsData_dataFromAPIAsync();
        //}
        public override async Task<bool> CreateUpdateReasonsData(Winit.Modules.ListHeader.Model.Interfaces.IListItem ListItem, bool IsCreate)
        {
            return await CreateUpdateReasonsDataFromAPIAsync(ListItem, IsCreate);
        }
        public override async Task<string> DeleteReason(string uid)
        {
            return await DeleteReasonDataFromAPIAsync(uid);
        }
        private async Task<bool> CreateUpdateReasonsDataFromAPIAsync(Winit.Modules.ListHeader.Model.Interfaces.IListItem ListItem, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(ListItem, true);
                    string jsonBody = JsonConvert.SerializeObject(ListItem);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}ListItemHeader/CreateListItem", HttpMethod.Post, ListItem);
                }
                else
                {
                    AddUpdateFields(ListItem);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}ListItemHeader/UpdateListItem", HttpMethod.Put, ListItem);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<IListHeader>> GetReasonsListHeaderDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
               // pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListHeaders",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListHeader>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListHeader>>>(apiResponse.Data);

                    if (pagedResponse != null)
                    {
                        // Ensure that the returned list contains IListHeader objects
                        return pagedResponse.Data.PagedData.OfType<IListHeader>().ToList();

                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return null;
        }
        List<string> Codes = new List<string>(); // Convert array to List<string>
        
        private async Task<List<IListItem>> GetReasonsListItemDataFromAPIAsync(string uid)
        {
            try
            {

                //var listItemRequest = new ListItemRequest
                //{
                //    Codes = new List<string> {uid},
                //    isCountRequired = true
                //};
                // Create a new instance of ListItems and assign values to ListItemRequest and PagingRequest
                var listItemRequest = new ListItems
                {
                    ListItemRequest = new ListItemRequest
                    {
                        Codes = new List<string> { uid }, // Assuming 'uid' is a string representing a code
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
                        ApiResponse< PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                        if (pagedResponse != null)
                        {
                            TotalItemsCount = pagedResponse.Data.TotalCount;
                            return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                        }
                    }
                }
               
            }
            catch (Exception)
            {
               
            }
            return null;
        }
        private async Task<IListItem> GetViewReasonsforEditDetailsDataFromAPIAsync(string orguid)
        {
            try
            {
                ApiResponse<Winit.Modules.ListHeader.Model.Classes.ListItem> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.ListHeader.Model.Classes.ListItem>(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByUID?UID={orguid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> SaveReasonsData_dataFromAPIAsync()
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                //string jsonBody = JsonConvert.SerializeObject();
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}ListItemHeader/CreateListItem", HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> UpdateViewReasonsDataFromAPIAsync(IListItem listUID)
        {
            try
            {
                ApiResponse<string> apiResponse = null;

                string jsonBody = JsonConvert.SerializeObject(listUID);
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}ListItemHeader/UpdateListItem", HttpMethod.Put, listUID);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<string> DeleteReasonDataFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/DeleteListItemByUID?UID={uid}",
                    HttpMethod.Delete, uid);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var name = ListHeaders?.FirstOrDefault()?.Name;

                    if (name != null)
                    {
                        return $"{name} successfully deleted.";
                    }
                    else
                    {
                        return "Item successfully deleted."; // Fallback message if no name is found
                    }
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error Failed to delete customers. Error: {data.ErrorMessage}";
                }
                else
                {
                    return "An unexpected error occurred.";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
