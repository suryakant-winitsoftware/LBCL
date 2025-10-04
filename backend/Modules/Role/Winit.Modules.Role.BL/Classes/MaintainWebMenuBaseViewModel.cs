using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Role.BL.Classes
{
    public class MaintainWebMenuBaseViewModel : MenuViewModel, IMaintainWebMenuBaseViewModel
    {
        public MaintainWebMenuBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService, IAppUser appUser) : base(apiService, appConfigs, commonFunctions, navigationManager, dataManager, alertService, loadingService, appUser)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            _loadingService = loadingService;
            _appUser = appUser;
            IsAdmin = appUser.Role?.Code?.ToLower() == "admin";
        }
        bool IsAdmin { get; set; }
        ILoadingService _loadingService;
        IAppUser _appUser;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }

        public List<ModuleMasterHierarchy> ModuleMasters { get; set; } = new();
        public ModulesMasterView ModulesMasterView { get; set; }
        public string RoleName { get; set; }
        List<Permission> Permissions { get; set; }
        List<Permission> ParentPermissions { get; set; }
        public bool IsLoad { get; set; }
        string RoleUID { get; set; }
        bool IsPrincipalRole { get; set; }
        PermissionMaster PermissionsMaster { get; set; }
        const string Web = "Web";
        public async Task PopulateViewModel()
        {
            _loadingService.ShowLoading();
            
            RoleUID = _commonFunctions.GetParameterValueFromURL("RoleUID");
            RoleName = _commonFunctions.GetParameterValueFromURL("RoleName");
            IsPrincipalRole = CommonFunctions.GetBooleanValue(_commonFunctions.GetParameterValueFromURL("RoleType"));
            Permissions = await GetPermissions(RoleUID, Web, IsPrincipalRole);
            if (!IsAdmin)
            {
                ParentPermissions = await GetPermissions(_appUser.SelectedJobPosition.UserRoleUID, "Web", IsPrincipalRole);
            }
            ModulesMasterView = await GetModulesMasterByPlatForm(Web);
            PrePareModuleMasterViewForMenu();
            IsLoad = true;
            _loadingService.HideLoading();
        }
        protected async Task GetAllWebModules()
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetAllModulesMaster?Platform=Web", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<ModulesMasterView>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                    if (apiResponse1.StatusCode == 200 && apiResponse1.Data != null)
                    {
                        _dataManager.SetData("WebModules", apiResponse1.Data);
                    }
                }
            }
        }
        //public async Task<List<ModuleMasterHierarchy>> GetPermissions(string RoleUID, bool IsPrincipalRole)
        //{
        //    try
        //    {
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/SelectAllPermissionsByRoleUID?roleUID={RoleUID}&isPrincipalTypePermission={IsPrincipalRole}", HttpMethod.Get);
        //        if (apiResponse != null)
        //        {
        //            ApiResponse<List<Permission>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Permission>>>(apiResponse.Data);
        //            if (apiResponse1 != null)
        //            {
        //                if (apiResponse1.IsSuccess && apiResponse1.Data != null)
        //                {
        //                    Permissions = apiResponse1.Data;
        //                    return PrePareModuleMasterViewForMenu();
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    return ModuleMasters;
        //}

        protected List<ModuleMasterHierarchy> PrePareModuleMasterViewForMenu()
        {
            //ModuleMasters = new();
            //try
            //{
            //    ModulesMasterView = (ModulesMasterView)_dataManager.GetData("WebModules");
            //}
            //catch (Exception ex) { }
            if (ModulesMasterView != null)
            {
                foreach (Module module in ModulesMasterView.Modules)
                {
                    ModuleMasterHierarchy moduleMasterHierarchy = new()
                    {
                        Module = module,
                        SubModuleHierarchies = new(),
                    };
                    foreach (SubModule subModule in ModulesMasterView.SubModules)
                    {
                        if (module.UID.Equals(subModule.ModuleUid))
                        {
                            SubModuleHierarchy subModuleHierarchy = new();
                            subModuleHierarchy.SubModule = subModule;
                            subModuleHierarchy.SubSubModules = new();
                            foreach (SubSubModules sub_Sub_Modules in ModulesMasterView.SubSubModules)
                            {
                                IPermission? permission1 = ParentPermissions?.Find(p => p.SubSubModuleUid.Equals(sub_Sub_Modules?.UID));
                                if ((permission1 != null && CommonFunctions.GetBooleanValue(permission1?.FullAccess) || IsAdmin) && subModule.UID.Equals(sub_Sub_Modules.SubModuleUid))
                                {
                                    Permission? permission = Permissions.Find(p => p.SubSubModuleUid.Equals(sub_Sub_Modules.UID));
                                    if (permission == null)
                                    {
                                        permission = new Permission()
                                        {
                                            UID = Guid.NewGuid().ToString(),
                                            CreatedBy = _appUser.Emp.UID,
                                            CreatedTime = DateTime.Now,
                                            RoleUid = RoleUID,
                                            Platform = "Web",
                                            SubSubModuleUid = sub_Sub_Modules.UID,
                                        };
                                    }
                                    permission.SubModuleUid = subModule.UID;

                                    SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new()
                                    {
                                        SubSubModule = sub_Sub_Modules,
                                        Permissions = permission,
                                    };
                                    subModuleHierarchy.SubSubModules.Add(subSubModuleMasterHierarchy);
                                }
                            }
                            if (subModuleHierarchy.SubSubModules != null && subModuleHierarchy.SubSubModules.Count > 0)
                                moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                        }
                    }
                    if (moduleMasterHierarchy.SubModuleHierarchies != null && moduleMasterHierarchy.SubModuleHierarchies.Count > 0)
                        ModuleMasters.Add(moduleMasterHierarchy);
                }
            }
            return ModuleMasters;
        }


        public async Task SavePermissions()
        {
            _loadingService.ShowLoading();
            PopulatePermissionMaster();
            await SavePermissions(PermissionsMaster);
            _loadingService.HideLoading();
        }

        protected void PopulatePermissionMaster()
        {
            PermissionsMaster = new()
            {
                RoleUID = this.RoleUID,
                IsPrincipalPermission = this.IsPrincipalRole,
                Permissions = new(),
                Platform = "Web"
            };
            foreach (ModuleMasterHierarchy moduleMaster in ModuleMasters)
            {
                foreach (SubModuleHierarchy subModuleHierarchy in moduleMaster.SubModuleHierarchies)
                {
                    foreach (SubSubModuleMasterHierarchy subSubModuleMasterHierarchy in subModuleHierarchy.SubSubModules)
                    {
                        subSubModuleMasterHierarchy.Permissions.ModifiedBy = _appUser.Emp.UID;
                        subSubModuleMasterHierarchy.Permissions.ModifiedTime = DateTime.Now;
                        if (subSubModuleMasterHierarchy.Permissions.IsModified)
                        {
                            PermissionsMaster.Permissions.Add(subSubModuleMasterHierarchy.Permissions);
                        }
                    }
                }
            }
        }

    }
}
