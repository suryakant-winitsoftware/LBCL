
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationMasterBaseViewModel : ILocationMasterBaseViewModel
    {
        IAppConfig _appConfig;
        IAppUser _appUser;
        public LocationMasterBaseViewModel(IAppConfig appConfig, IAppUser appUser, ApiService apiService, CommonFunctions commonFunctions, IDataManager dataManager)
        {
            _appConfig = appConfig;
            _appUser = appUser;
            _apiService = apiService;
            _commonFunctions = commonFunctions;
            _dataManager = dataManager;
        }
        IDataManager _dataManager;
        public List<LocationData> LocationMasterForUIs { get; set; } = new();
        public List<LocationData> DispayLocationMasterForUIs { get; set; } = new();
        private ApiService _apiService { get; set; }
        private CommonFunctions _commonFunctions { get; set; }
        public LocationData SelectedLocationMasterForUI { get; set; }
        public StoreGroupData SelectedStoreGroupData { get; set; }
        public string SearchField { get; set; } = string.Empty;
        public List<ISelectionItem> LocationData { get; set; }
        public List<Winit.Modules.Store.Model.Classes.StoreGroupData> StoreGroupData { get; set; } = new();
        public List<Winit.Modules.Store.Model.Classes.StoreGroupData> DisplayStoreGroupData { get; set; } = new();

        public bool IsLocationHierarchyData { get; set; }
        public string SelectedLocationOrStoreGroupUID { get; set; }
        public dynamic objectData { get; set; }
        public async Task PopulateViewModel()
        {
            //object locationMasterData = _dataManager.GetData(Winit.Modules.Common.Model.Constants.CommonConstants.LocationMasterData);
            //object storeGroupData = _dataManager.GetData(Winit.Modules.Common.Model.Constants.CommonConstants.ChannelMasterData);
            if (IsLocationHierarchyData)
            {
                if (DatamanagerGeneric<List<LocationData>>.
                    TryGet(Winit.Modules.Common.Model.Constants.CommonConstants.LocationMasterData, out List<LocationData> data))
                {
                    LocationMasterForUIs.Clear();
                    LocationMasterForUIs.AddRange(data);
                }
                //await GetLocationMaster();
                DispayLocationMasterForUIs.Clear();
                foreach (var item in LocationMasterForUIs)
                {
                    if (item.PrimaryUid == SelectedLocationOrStoreGroupUID)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                    DispayLocationMasterForUIs.Add(item);
                }

                objectData = LocationMasterForUIs;
            }
            if (!IsLocationHierarchyData)
            {
                await GetChannelMasterData();
                //StoreGroupData = (List<Winit.Modules.Store.Model.Classes.StoreGroupData>)storeGroupData;
                // DisplayStoreGroupData = StoreGroupData;
                foreach (var item in StoreGroupData)
                {
                    if (item.PrimaryUID == SelectedLocationOrStoreGroupUID)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                    DisplayStoreGroupData.Add(item);
                }
            }

            //await GetDataFromAPI();
        }
        public void OnSelected(LocationData locationMasterForUI)
        {
            if (!locationMasterForUI.IsSelected)
            {

                foreach (LocationData locationMasterFor in DispayLocationMasterForUIs)
                {
                    locationMasterFor.IsSelected = false;
                }

                SelectedLocationMasterForUI = locationMasterForUI;
            }
            else
            {
                SelectedLocationMasterForUI = null;
            }
            locationMasterForUI.IsSelected = !locationMasterForUI.IsSelected;
        }
        public void OnSelected(StoreGroupData StoreGroupData)
        {
            if (!StoreGroupData.IsSelected)
            {

                foreach (StoreGroupData locationMasterFor in DisplayStoreGroupData)
                {
                    locationMasterFor.IsSelected = false;
                }

                SelectedStoreGroupData = StoreGroupData;
            }
            else
            {
                SelectedStoreGroupData = null;
            }
            StoreGroupData.IsSelected = !StoreGroupData.IsSelected;
        }
        public async Task OnSeach(string searchField)
        {
            if (!string.IsNullOrEmpty(searchField))
            {
                if (IsLocationHierarchyData)
                {
                    DispayLocationMasterForUIs = LocationMasterForUIs.Where(x => x.Label.ToLower().Contains(searchField.ToLower())).ToList();
                }
                else
                {
                    DisplayStoreGroupData = StoreGroupData.Where(x => x.Label.ToLower().Contains(searchField.ToLower())).ToList();
                }
            }
            else
            {
                if (IsLocationHierarchyData)
                {
                    DispayLocationMasterForUIs = LocationMasterForUIs;
                }
                else
                {
                    DisplayStoreGroupData = StoreGroupData;
                }
            }

            //await GetDataFromAPI();
        }
        private async Task GetLocationMaster()
        {

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfig.ApiBaseUrl}LocationMapping/GetLocationMasterByLocationName", HttpMethod.Get);
            if (apiResponse.Data != null)
            {
                // string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                ApiResponse<List<Location.Model.Classes.LocationData>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Location.Model.Classes.LocationData>>>(apiResponse.Data);
                if (apiResponse1 != null && apiResponse1.Data != null)
                {
                    LocationMasterForUIs = apiResponse1.Data;
                    //_dataManager.DeleteData(Winit.Modules.Common.Model.Constants.CommonConstants.LocationMasterData);
                    //_dataManager.SetData(Model.Constants.CommonConstants.LocationMasterData, LocationMasterForUIs);
                }
            }

        }
        private async Task GetChannelMasterData()
        {

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfig.ApiBaseUrl}StoreToGroupMapping/SelectAllChannelMasterData", HttpMethod.Get);
            if (apiResponse.Data != null)
            {
                // string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreGroupData>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Store.Model.Classes.StoreGroupData>>>(apiResponse.Data);
                if (apiResponse1 != null && apiResponse1.Data != null)
                {
                    StoreGroupData = apiResponse1.Data;
                    //_dataManager.DeleteData(Winit.Modules.Common.Model.Constants.CommonConstants.ChannelMasterData);
                    //_dataManager.SetData(Model.Constants.CommonConstants.ChannelMasterData, ChannelMasterData);
                }
            }

        }
        private async Task GetDataFromAPI()
        {
            LocationMasterForUIs = new List<LocationData>();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfig.ApiBaseUrl}LocationMapping/GetLocationMasterByLocationName", HttpMethod.Get);
            if (apiResponse.Data != null)
            {
                // string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                ApiResponse<List<LocationData>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<LocationData>>>(apiResponse.Data);
                if (apiResponse1 != null)
                {
                    LocationMasterForUIs = apiResponse1.Data;
                    DispayLocationMasterForUIs = apiResponse1.Data;
                }
            }

        }
    }
}
