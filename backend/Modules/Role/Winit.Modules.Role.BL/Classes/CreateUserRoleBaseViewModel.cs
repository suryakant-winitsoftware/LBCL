using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;
using Winit.UIComponents.SnackBar.Enum;
using Winit.UIModels.Common;
using static iTextSharp.text.pdf.AcroFields;

namespace Winit.Modules.Role.BL.Classes
{
    public class CreateUserRoleBaseViewModel : ICreateUserRoleBaseViewModel
    {

        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        public CreateUserRoleBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService, IAppUser appUser, IToast toast, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            _loadingService = loadingService;
            _iAppUser = appUser;
            _toast = toast;
            _localizer = Localizer;
            _languageService = languageService;
        }
        IToast _toast;
        ILoadingService _loadingService;
        IAppUser _iAppUser;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }

        public bool IsAddOrEdit { get; set; }
        public Model.Interfaces.IRole UserRole { get; set; }
        public List<ISelectionItem> SelectionItems { get; set; } = [];
        List<Model.Interfaces.IRole> Roles { get; set; }
        ISelectionItem? SelectedItem { get; set; }
        public bool IsNew { get; set; }
        private ISelectionItem LogedInRole { get; set; }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public void PopulateViewModel(List<Model.Interfaces.IRole> roles)
        {
            LoadResources(null, _languageService.SelectedCulture);
            IsAddOrEdit = false;
            LogedInRole = new SelectionItem()
            {
                UID = _iAppUser.Role.UID,
                Code = _iAppUser.Role.Code,
                Label = _iAppUser.Role.RoleNameEn,
            };
            Roles = roles;
        }

        public void IsPrincipalOrDistributor(bool isPrincOrDist)
        {
            if (isPrincOrDist)
            {
                if (!_iAppUser.Role.IsDistributorRole)
                {
                    UserRole.IsPrincipalRole = true;
                    UserRole.IsDistributorRole = false;
                }
            }
            else
            {
                UserRole.IsPrincipalRole = false;
                UserRole.IsDistributorRole = true;
            }
        }
        public void IsAdminIsPrincipalToDistributor(bool idAdmin)
        {
            if (idAdmin)
            {
                UserRole.IsAdmin = true;
                UserRole.BuToDistAccess = false;
            }
            else
            {
                UserRole.IsAdmin = false;
                UserRole.BuToDistAccess = true;
            }
        }
        public void ONParentRoleChanged(DropDownEvent dropDownEvent)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    SelectedItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    if (SelectedItem != null)
                    {
                        UserRole.ParentRoleName = SelectedItem.Label;
                        UserRole.ParentRoleUid = SelectedItem.UID;
                    }
                }
                else
                {
                    UserRole.ParentRoleUid = string.Empty;
                }
            }
        }
        public void AddOrEditRole(bool isAdd, List<IRole> roles, Model.Interfaces.IRole userRole = null)
        {
            SelectionItems.Clear();
            LogedInRole.IsSelected = false;
            SelectionItems.Add(LogedInRole);
            foreach (Model.Classes.Role role in roles)
            {
                //if (role.IsPrincipalRole || CommonFunctions.GetBooleanValue(_iAppUser.Role.IsDistributorRole))
                if (role.IsForReportsTo && LogedInRole.UID != role.UID)
                {
                    ISelectionItem selectionItem = new SelectionItem()
                    {
                        UID = role.UID,
                        Code = role.Code,
                        Label = role.RoleNameEn,
                    };
                    if (!isAdd && userRole != null)
                    {
                        selectionItem.IsSelected = selectionItem.UID == userRole.ParentRoleUid;
                    }
                    SelectionItems.Add(selectionItem);
                }
            }
            if (isAdd)
            {
                IsNew = true;
                UserRole = new Model.Classes.Role()
                {
                    UID = Guid.NewGuid().ToString(),
                    CreatedBy = _iAppUser.Emp.UID,
                    OrgUid = _iAppUser.SelectedJobPosition.OrgUID,
                    CreatedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                };
                if (_iAppUser.Role.IsDistributorRole)
                {
                    IsPrincipalOrDistributor(false);
                }
            }
            else
            {
                IsNew = false;
                UserRole = userRole;
                //ParentLabel = string.IsNullOrEmpty(userRole.ParentRoleName) ? @_localizer["select_parent_role"] : userRole.ParentRoleName;
                //foreach (var item in SelectionItems)
                //{

                //}
            }
            //if (Isdisabled)
            //{
            //    UserRole.IsPrincipalRole = false;
            //    UserRole.IsDistributorRole = true;
            //}
            //else
            //{
            //    UserRole.IsPrincipalRole = true;
            //    UserRole.IsDistributorRole = false;
            //}
            //UserRole.ParentRoleName = ParentLabel;
            IsAddOrEdit = true;
        }

        protected bool IsValidated()
        {
            bool isVal = true; string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(UserRole.Code))
            {
                errorMessage += @_localizer["code"] + ", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(UserRole.RoleNameEn))
            {
                errorMessage += @_localizer["role_name"] + ", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(UserRole.RoleNameOther))
            {
                errorMessage += @_localizer["alias_name"] + ", ";
                isVal = false;
            }
            if (string.IsNullOrEmpty(UserRole.ParentRoleUid))
            {
                //errorMessage += "Parent Role,";
                //isVal = false;
            }
            if (UserRole.IsPrincipalRole || UserRole.IsDistributorRole)
            {
                if (UserRole.IsPrincipalRole)
                {
                    if (!UserRole.IsAdmin && !UserRole.BuToDistAccess)
                    {
                        errorMessage = @_localizer["is_admin/principal_to_distributor_access"] + ", ";
                    }
                }
            }
            else
            {
                errorMessage += @_localizer["is_principal_role/_in_distributor_role"] + ", ";
                isVal = false;
            }
            if (!UserRole.IsAppUser && !UserRole.IsWebUser)
            {
                errorMessage += @_localizer["is_app_user/_in_web_user"] + ", ";
                isVal = false;
            }

            if (isVal)
            {
                bool isExist = Roles.Any(p => p.RoleNameEn == UserRole.RoleNameEn);
                if (isExist && IsNew)
                {
                    isVal = false;
                    _toast.Add(@_localizer["error"], @_localizer["role_already_exist"]);
                }
            }
            else
            {
                _toast.Add(@_localizer["error"], $"{errorMessage.Substring(0, errorMessage.Length - 2)} {@_localizer["should_not_be_empty"]}", Severity.Error);
            }

            return isVal;
        }
        public async Task<int> SaveOrUpdate()
        {
            if (!IsValidated())
            {
                return 0;
            }
            int retVal = 0;
            UserRole.ModifiedBy = _iAppUser.Emp.UID;
            UserRole.ModifiedTime = DateTime.Now;
            string endPointMethod = string.Empty;
            HttpMethod httpMethod;
            if (IsNew)
            {
                httpMethod = HttpMethod.Post;
                endPointMethod = @_localizer["createrole"];
            }
            else
            {
                httpMethod = HttpMethod.Put;
                endPointMethod = @_localizer["updateroles"];
            }


            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/{endPointMethod}", httpMethod, UserRole);
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
                            IsAddOrEdit = false;
                            await _alertService.ShowSuccessAlert(@_localizer["success"], IsNew ? @_localizer["saved_successfully"] : @_localizer["updated_successfully"]);
                        }
                    }

                }
                else
                {
                    await _alertService.ShowErrorAlert(@_localizer["error"], apiResponse.Data);
                }
            }
            return retVal;
        }



    }
}
