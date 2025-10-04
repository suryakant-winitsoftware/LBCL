using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Classes
{
    public class MaintainVanWebViewModel:MaintainVanBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public MaintainVanWebViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager,
              IAppConfig appConfigs,
              Base.BL.ApiService apiService
          ) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            // Initialize common properties or perform other common setup
            MaintainVanList = new List<IVehicle>();
           
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        #region Business Logics  
        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> GetMaintainVanData(string orgUID)
        {
            return await GetMaintainVanDataFromAPIAsync(orgUID);
        }
        public override async Task<string> DeleteVanFromGrid(string uid)
        {
            return await DeleteVanDataFromAPIAsync(uid);
        }
        #endregion
        #region Api Calling Methods
        private async Task<List<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> GetMaintainVanDataFromAPIAsync(string orgUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("VehicleModifiedTime",SortDirection.Desc)
                };
                pagingRequest.FilterCriterias = VanFilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Vehicle/SelectAllVehicleDetails?OrgUID={orgUID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Vehicle.Model.Classes.Vehicle>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Vehicle.Model.Classes.Vehicle>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<string> DeleteVanDataFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Vehicle/DeleteVehicleDetails?UID={uid}",
                    HttpMethod.Delete, uid);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "Van successfully deleted.";
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
        #endregion
    }
}
