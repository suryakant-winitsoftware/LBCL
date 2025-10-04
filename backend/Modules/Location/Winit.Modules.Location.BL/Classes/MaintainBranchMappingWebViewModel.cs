using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;

namespace Winit.Modules.Location.BL.Classes
{
    public class MaintainBranchMappingWebViewModel : MaintainBranchMappingBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly ILanguageService _languageService;
        public List<FilterCriteria> FilterCriterias { get; set; }
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public MaintainBranchMappingWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            ILanguageService languageService
         ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _languageService = languageService;
            //WareHouseItemViewList = new List<IOrgType>();
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchDetailsData()
        {
            return await GetBranchDetailsDataDataFromAPIAsync();
        }



        public override async Task<Winit.Modules.Location.Model.Interfaces.IBranch> GetBranchDatailsDataByUID(string UID)
        {
            return await GetBranchDetailsDataDataFromAPIAsync(UID);
        }
        public override async Task<bool> SaveOrUpdateBranchDetails(IBranch viewBranchDetails, bool @operator)
        {
            return await SaveOrUpdateBranchDetailsAsync(viewBranchDetails, @operator);

        }
        public override async Task<bool> SaveStoreDetailsDetails(ISalesOffice salesOffice)
        {
            return await SaveStoreDetailsAsync(salesOffice);
        }
        public override async Task<List<ISalesOffice>> GetSalesOfficeDataByBranchUID(string UID)
        {
            return await GetSalesOrderDataByBranchUidAsync(UID);
        }
        public override async Task<List<ISelectionItem>> GetSalesOfficeOrgTypesForSelection(string orgTypeUID)
        {
            return await GetSalesOfficeOrgTypesForSelectionFromApiAsync(orgTypeUID);
        }


        public override async Task<bool> DeleteSalesOfficeDetails(ISalesOffice salesOffice)
        {
            return await DeleteSalesOfficeDetailsAsync(salesOffice);
        }
        public override async Task<List<ISalesOffice>> GetCompleteSalesOfficeDetailsList()
        {
            return await GetCompleteSalesOfficeDetailsListFromApiAsync();
        }
        private async Task<List<ISalesOffice>> GetCompleteSalesOfficeDetailsListFromApiAsync()
        {
            try
            {

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SalesOffice/SelectAllSalesOfficeDetails",
                HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Location.Model.Classes.SalesOffice>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Location.Model.Classes.SalesOffice>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Location.Model.Interfaces.ISalesOffice>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<bool> DeleteSalesOfficeDetailsAsync(ISalesOffice salesOffice)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SalesOffice/DeleteSalesOffice",
                    HttpMethod.Delete, salesOffice);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ISalesOffice>> GetSalesOrderDataByBranchUidAsync(string uID)
        {
            var encodedUID = Uri.EscapeDataString(uID);

            try
            {
                ApiResponse<List<ISalesOffice>> apiResponse =
                   await _apiService.FetchDataAsync<List<ISalesOffice>>(
                   $"{_appConfigs.ApiBaseUrl}SalesOffice/GetSalesOfficeByUID?UID={encodedUID}",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        private async Task<bool> SaveStoreDetailsAsync(ISalesOffice salesOffice)
        {
            try
            {
                ApiResponse<string> apiResponse;
                salesOffice.UID = salesOffice.Code;
                salesOffice.CreatedBy = _appUser.Emp.CreatedBy;
                salesOffice.ModifiedBy = _appUser.Emp.ModifiedBy;
                salesOffice.CreatedTime = DateTime.Now;
                salesOffice.ModifiedTime = DateTime.Now;
                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SalesOffice/CreateSalesOffice",
                    HttpMethod.Post, salesOffice);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                // Handle exceptions
                // Handle exceptions
            }
        }

        private async Task<bool> SaveOrUpdateBranchDetailsAsync(IBranch viewBranchDetails, bool @operator)
        {
            try
            {
                ApiResponse<string> apiResponse;
                if (!@operator)
                {
                    viewBranchDetails.UID = viewBranchDetails.Code;
                    viewBranchDetails.CreatedBy = _appUser.Emp.CreatedBy;
                    viewBranchDetails.ModifiedBy = _appUser.Emp.ModifiedBy;
                    viewBranchDetails.CreatedTime = DateTime.Now;
                    viewBranchDetails.ModifiedTime = DateTime.Now;
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}Branch/CreateBranch",
                        HttpMethod.Post, viewBranchDetails);
                }
                else
                {
                    apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}Branch/UpdateBranch",
                  HttpMethod.Put, viewBranchDetails);
                }
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                // Handle exceptions
                // Handle exceptions
            }
        }

        private async Task<List<IBranch>> GetBranchDetailsDataDataFromAPIAsync()
        {
            try
            {

                ApiResponse<PagedResponse<IBranch>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<IBranch>>(
                    $"{_appConfigs.ApiBaseUrl}Branch/SelectAllBranchDetails",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    TotalItems = apiResponse.Data.TotalCount;
                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        private async Task<Winit.Modules.Location.Model.Interfaces.IBranch> GetBranchDetailsDataDataFromAPIAsync(string UID)
        {
            try
            {
                var encodedUID = Uri.EscapeDataString(UID);
                ApiResponse<Winit.Modules.Location.Model.Classes.Branch> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Location.Model.Classes.Branch>(
                    $"{_appConfigs.ApiBaseUrl}Branch/GetBranchByUID?UID={encodedUID}",
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

        protected override async Task GetStatesData()
        {
            LocationsByType = new();
            StatesForSelection = new List<ILocation>();
            List<string> locations = new List<string>()
            {
                "state",
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes", HttpMethod.Post, locations);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {

                        foreach (var location in pagedResponse.Data)
                        {
                            LocationsByType.Add(location);
                            StatesForSelection.Add(location);
                        }
                    }
                }
            }
        }
        protected override async Task GetCitiesDataViaSelectedStates(List<ILocation> selectedStates)
        {
            LocationsByType = new();
            CitiesForSelection = new List<ILocation>();
            List<string> stateCodes = selectedStates.Select(state => state.UID).ToList();
            List<string> locations = new List<string>(stateCodes);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Location/GetCityandLoaclityByUIDs", HttpMethod.Post, locations);

            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>>(apiResponse.Data);

                    if (pagedResponse != null)
                    {
                        foreach (var location in pagedResponse.Data)
                        {
                            LocationsByType.Add(location);
                            CitiesForSelection.Add(location);
                        }
                    }
                }
            }
        }
        protected override async Task GetLocalitiesDataViaSelectedStates(List<ILocation> selectedCities)
        {
            LocationsByType = new();
            LocalitiesForSelection = new List<ILocation>();
            List<string> stateCodes = selectedCities.Select(state => state.UID).ToList();
            List<string> locations = new List<string>(stateCodes);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Location/GetCityandLoaclityByUIDs", HttpMethod.Post, locations);

            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>>(apiResponse.Data);

                    if (pagedResponse != null)
                    {
                        foreach (var location in pagedResponse.Data)
                        {
                            LocationsByType.Add(location);
                            LocalitiesForSelection.Add(location);
                        }
                    }
                }
            }
        }

        private async Task<List<ISelectionItem>> GetSalesOfficeOrgTypesForSelectionFromApiAsync(string orgTypeUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<IOrg>> apiResponse = await
                    _apiService.FetchDataAsync<List<IOrg>>
                    ($"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
                {
                    return apiResponse.Data
                                     .Select(org => new SelectionItem
                                     {
                                         UID = org.UID,
                                         Label = "[" + org.Code + "] " + org.Name,
                                         Code = org.Code,
                                         IsSelected = false,
                                     })
                                     .Cast<ISelectionItem>()
                                     .ToList();
                }
                return default;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
