using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Classes
{
    public class AddEditMaintainVanWebViewModel:AddEditMaintainVanBaseViewModel
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
        public AddEditMaintainVanWebViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper,
             IAppUser appUser,
             IAppSetting appSetting,
             IDataManager dataManager,
             Winit.Shared.Models.Common.IAppConfig appConfigs,
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
            VEHICLE = new Winit.Modules.Vehicle.Model.Classes.Vehicle();
            VanTypeSelectionItems = new List<SelectionItem>();

            // Property set for Search

            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public override async Task PopulateViewModel(string uid)
        {
            await base.PopulateViewModel(uid);
        }
        private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {

            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        public override async Task<bool> CreateUpdateVanData(Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle, bool IsCreate)
        {
            return await CreateUpdateVanDataFromAPIAsync(vehicle, IsCreate);
        }
        public override async Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetMaintainVanEditDetails(string vehicleuid)
        {
            return await GetVanDetailsDataForEditFromAPIAsync(vehicleuid);
        }
        private async Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetVanDetailsDataForEditFromAPIAsync(string vehicleuid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;

                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Vehicle.Model.Classes.Vehicle> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Vehicle.Model.Classes.Vehicle>(
                    $"{_appConfigs.ApiBaseUrl}Vehicle/GetVehicleByUID?UID={vehicleuid}",
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
        private async Task<bool> CreateUpdateVanDataFromAPIAsync(Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(vehicle);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(vehicle, false);
                    vehicle.UID = $"{vehicle.OrgUID}_{vehicle.VehicleNo}";
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Vehicle/CreateVehicle", HttpMethod.Post, vehicle);
                }
                else
                {
                    AddUpdateFields(vehicle);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Vehicle/UpdateVehicleDetails", HttpMethod.Put, vehicle);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
    }
}
