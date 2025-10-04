using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.Shared.Models.Enums;
using Winit.Modules.Common.BL.Interfaces;
using iTextSharp.text;


namespace Winit.Modules.Role.BL.Classes
{
    public class MaintainUserRoleBaseViewMode : IMaintainUserRoleBaseViewMode
    {

        public MaintainUserRoleBaseViewMode(IAppUser appUser, ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            _loadingService = loadingService;
            _appUser = appUser;
        }
        IAppUser _appUser;
        ILoadingService _loadingService;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }

        PagingRequest PagingRequest { get; set; } = new()
        {
            FilterCriterias = new(),
            SortCriterias = new(),
        };
        List<FilterCriteria> FilterCriterias = [];
        public bool IsLoad { get; set; }
        public List<Winit.Modules.Role.Model.Interfaces.IRole> RoleList { get; set; } = [];

        public async Task PopulateViewModel()
        {
            PagingRequest.PageNumber = PageNumber = 1;
            PagingRequest.PageSize = PageSize = 10;
            PagingRequest.IsCountRequired = true;
            FilterCriterias.Add(new FilterCriteria("OrgUID", _appUser?.SelectedJobPosition?.OrgUID, FilterType.Equal));
            FilterCriterias.Add(new FilterCriteria("RoleNameEn", "admin", FilterType.NotEqual));
            PagingRequest.FilterCriterias = [];
            PagingRequest.IsCountRequired = true;
            PagingRequest.FilterCriterias.AddRange(FilterCriterias);
            _loadingService.ShowLoading();
            await GetDataFromAPIAsync();
            IsLoad = true;
            _loadingService.HideLoading();

        }

        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias.Clear();
            PagingRequest.FilterCriterias.AddRange(FilterCriterias);
            foreach (var item in filterCriteria)
            {
                PagingRequest.FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Like));
            }
            PagingRequest.PageNumber = PageNumber = 1;
            await GetDataFromAPIAsync();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = PageNumber = pageNumber;
            await GetDataFromAPIAsync();
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias = [sortCriteria];
            await GetDataFromAPIAsync();
        }
        public async Task GetDataFromAPIAsync()
        {
            Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.Role.Model.Classes.Role>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Role.Model.Classes.Role>>($"{_appConfigs.ApiBaseUrl}Role/SelectAllRoles", HttpMethod.Post, PagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.StatusCode == 200)
            {
                RoleList.Clear();
                if (apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    RoleList.AddRange(apiResponse!.Data.PagedData);
                    TotalItemsCount = apiResponse.Data.TotalCount;
                }
            }
        }

        public int TotalItemsCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

    }
}
