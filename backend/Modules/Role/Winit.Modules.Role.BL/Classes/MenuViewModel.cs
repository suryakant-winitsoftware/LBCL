using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Role.BL.Classes
{
    public class MenuViewModel
    {
        public MenuViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService, IAppUser appUser)
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
        ILoadingService _loadingService;
        IAppUser _appUser;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }

        protected async Task<List<Permission>> GetPermissions(string RoleUID, string platForm, bool IsPrincipalRole)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/SelectAllPermissionsByRoleUID?roleUID={RoleUID}&platform={platForm}&isPrincipalTypePermission={IsPrincipalRole}", HttpMethod.Get);
                if (apiResponse != null)
                {
                    ApiResponse<List<Permission>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Permission>>>(apiResponse.Data);
                    if (apiResponse1 != null)
                    {
                        if (apiResponse1.IsSuccess && apiResponse1.Data != null)
                        {
                            return apiResponse1.Data;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;

        }
        protected async Task<ModulesMasterView> GetModulesMasterByPlatForm(string platForm)
        {
            ModulesMasterView? modulesMasterView = new();
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetModulesMasterByPlatForm?Platform={platForm}", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<ModulesMasterView>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                    if (apiResponse1?.StatusCode == 200 && apiResponse1?.Data != null)
                    {
                        modulesMasterView = apiResponse1.Data;
                    }
                }
            }
            return modulesMasterView;
        }
        protected async Task SavePermissions(object data)
        {

            int retVal = 0;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/CUDPermissionMaster", HttpMethod.Post, data);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<string>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    if (apiResponse1 != null)
                    {
                        retVal = CommonFunctions.GetIntValue(apiResponse1.Data);
                        if (retVal > 0)
                        {
                            _navigationManager.NavigateTo("maintainUserRole");
                            await _alertService.ShowSuccessAlert("Success", "Saved Successfully!");
                        }
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", apiResponse.Data);
                }
            }
        }



    }
}
