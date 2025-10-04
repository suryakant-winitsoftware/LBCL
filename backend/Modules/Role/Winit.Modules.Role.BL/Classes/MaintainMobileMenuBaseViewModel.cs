using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
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
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Role.BL.Classes
{
    public class MaintainMobileMenuBaseViewModel : MenuViewModel, IMaintainMobileMenuBaseViewModel
    {
        public MaintainMobileMenuBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService, IAppUser appUser) : base(apiService, appConfigs, commonFunctions, navigationManager, dataManager, alertService, loadingService, appUser)
        {
            _apiService = apiService;
            _appConfigs = appConfigs;
            _commonFunctions = commonFunctions;
            _navigationManager = navigationManager;
            _dataManager = dataManager;
            _alertService = alertService;
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

        public List<ModuleMasterHierarchy> ModuleMasters { get; set; } = new();
        public ModulesMasterView ModulesMasterView { get; set; }

        List<Permission> Permissions { get; set; }
        public bool IsLoad { get; set; }
        string RoleUID { get; set; }
        bool IsPrincipalRole { get; set; }
        public string RoleName { get; set; }
        PermissionMaster PermissionsMaster { get; set; }
        const string Mobile = "Mobile";
        public async Task PopulateViewModel()
        {
            _loadingService.ShowLoading();
            RoleUID = _commonFunctions.GetParameterValueFromURL("RoleUID");
            RoleName = _commonFunctions.GetParameterValueFromURL("RoleName");
            IsPrincipalRole = CommonFunctions.GetBooleanValue(_commonFunctions.GetParameterValueFromURL("RoleType"));
            Permissions = await GetPermissions(RoleUID, Mobile, IsPrincipalRole);
            ModulesMasterView=await GetModulesMasterByPlatForm(Mobile);
            PrePareModuleMasterViewForMenu();


            IsLoad = true;
            _loadingService.HideLoading();
        }
        protected void PrePareModuleMasterViewForMenu()
        {
            //ModuleMasters = new();
            //try
            //{
            //    ModulesMasterView = (ModulesMasterView)_dataManager.GetData("MobilesModules");
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
                           
                                Permission? permission = Permissions.Find(p => p.SubSubModuleUid == subModule.UID);
                            if (permission == null)
                            {
                                permission=new Permission()
                                {
                                    UID=Guid.NewGuid().ToString(),
                                    CreatedBy=_appUser.Emp.UID,
                                    CreatedTime=DateTime.Now,
                                    ModifiedBy=_appUser.Emp.UID,
                                    Platform= "Mobile",
                                    SubSubModuleUid = subModule.UID,
                                    RoleUid= RoleUID,
                                };
                            }
                                subModuleHierarchy.SubModulePermissions = permission ;
                            if (string.IsNullOrEmpty(subModule.RelativePath))
                            {
                                foreach (SubSubModules SubSubModules in ModulesMasterView.SubSubModules)
                                {
                                    if (subModule.UID.Equals(SubSubModules.SubModuleUid))
                                    {
                                        SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new()
                                        {
                                            SubSubModule = SubSubModules,
                                        };
                                        Permission? permission1 = Permissions.Find(p => p.SubSubModuleUid == SubSubModules.UID);

                                        if (permission1 == null)
                                        {
                                            permission1 = new Permission()
                                            {
                                                UID = Guid.NewGuid().ToString(),
                                                CreatedBy= _appUser.Emp.UID,
                                                CreatedTime = DateTime.Now,
                                                ModifiedBy = _appUser.Emp.UID,
                                                Platform = "Mobile",
                                                SubSubModuleUid = SubSubModules.UID,
                                                RoleUid = RoleUID,
                                            };
                                        }
                                        subSubModuleMasterHierarchy.Permissions = permission1;
                                        if(!string.IsNullOrEmpty(SubSubModules.RelativePath))
                                        {
                                            subModuleHierarchy.SubSubModules.Add(subSubModuleMasterHierarchy);
                                        }
                                    }
                                }
                            }
                            moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                        }
                    }
                    ModuleMasters.Add(moduleMasterHierarchy);
                }
            }
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
                Platform="Mobile"
            };
            foreach (ModuleMasterHierarchy moduleMaster in ModuleMasters)
            {
                foreach (SubModuleHierarchy subModuleHierarchy in moduleMaster.SubModuleHierarchies)
                {
                    //if (subModuleHierarchy.SubModulePermissions != null)
                    //{
                    //    subModuleHierarchy.SubModulePermissions.modified_by=_appUser.Emp.UID;
                    //    subModuleHierarchy.SubModulePermissions.modified_time = DateTime.Now;
                    //    PermissionsMaster.Permissions.Add(subModuleHierarchy.SubModulePermissions);
                    //}
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


        public void SelectOrDeselectSubModules(SubModuleHierarchy subModuleHierarchy,bool isYes)
        {
            if (subModuleHierarchy.SubModulePermissions != null)
            {
                subModuleHierarchy.SubModulePermissions.FullAccess = subModuleHierarchy.SubModulePermissions.FullAccess= isYes;
                subModuleHierarchy.SubModulePermissions.IsModified = true;

                if (subModuleHierarchy.SubSubModules!=null && (subModuleHierarchy.SubModule.SS != 0 || string.IsNullOrEmpty(subModuleHierarchy.SubModule.RelativePath)))
                {
                    foreach (var item in subModuleHierarchy.SubSubModules)
                    {
                        if (item.Permissions != null)
                        {
                            item.Permissions.FullAccess = subModuleHierarchy.SubModulePermissions.FullAccess;
                            item.Permissions.IsModified = true;
                        }
                    }
                }

            }
        }
        public void SelectOrDeselectSubSubModules(SubSubModuleMasterHierarchy subSubModuleMasterHierarchy, SubModuleHierarchy subModuleHierarchy, bool isYes)
        {
            if (subSubModuleMasterHierarchy != null)
            {
                if (subSubModuleMasterHierarchy.Permissions != null)
                {
                    subSubModuleMasterHierarchy.Permissions.FullAccess = isYes;
                    subSubModuleMasterHierarchy.Permissions.IsModified = true;
                    if(subModuleHierarchy!=null & subModuleHierarchy.SubSubModules != null)
                    {
                        bool? prevFullAccess=null;
                        int count = 0;
                        foreach (SubSubModuleMasterHierarchy subSubModuleMasterHierarchy1 in subModuleHierarchy.SubSubModules)
                        {
                            if(subSubModuleMasterHierarchy1.Permissions != null)
                            {
                                if (prevFullAccess != null)
                                {
                                    if(prevFullAccess!= subSubModuleMasterHierarchy1.Permissions.FullAccess)
                                    {
                                        count++;
                                    }
                                }
                                prevFullAccess = subSubModuleMasterHierarchy1.Permissions.FullAccess;
                            }
                        }
                        if (count == 0)
                        {
                            subModuleHierarchy.SubModulePermissions.FullAccess= isYes;
                        }
                        else
                        {
                            subModuleHierarchy.SubModulePermissions.FullAccess = null;
                        }
                    }
                  
                }
            }
        }

        public void SelectOrDeselectAllSubModules()
        {

        }


    }
}
