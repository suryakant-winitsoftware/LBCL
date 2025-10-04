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
using Winit.Modules.Distributor.BL.Interfaces;
using Winit.Modules.Distributor.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIModels.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Winit.Modules.Distributor.BL.Classes
{
    public class DistributorAdminBaseViewModel : IDistributorAdminBaseViewModel
    {
        public DistributorAdminBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
            Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService,
            ILoadingService loadingService, IAppUser appUser, IDistributorAdminDTO DistributorAdminDTO)
        {
            _alertService = alertService;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _commonFunctions = commonFunctions;
            _navigationManager = navigationManager;
            _dataManager = dataManager;
            _loadingService = loadingService;
            _iAppUser = appUser;
            _DistributorAdminDTO = DistributorAdminDTO;
        }
        IAppUser _iAppUser;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }
        IDistributorAdminDTO _DistributorAdminDTO { get; set; }
        ILoadingService _loadingService { get; set; }
        public List<IEmp> EmpList { get; set; } = new();
        public bool IsEditLoginID { get; set; }
        public bool IsNewEmp { get; set; }
        public bool IsPassword { get; set; }
        public bool IsLoad { get; set; }
        public bool IsShowPopUp { get; set; }
        public string Name { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        string OrgUID = string.Empty;
        string Status = string.Empty;
        IRole? Role { get; set; }
        SHACommonFunctions _sHACommonFunctions = new SHACommonFunctions();
        public async Task PopulateViewModel()
        {
            _loadingService.ShowLoading();
            OrgUID = _commonFunctions.GetParameterValueFromURL("OrgUID");
            Status = _commonFunctions.GetParameterValueFromURL("Status");
            SetColumnHeaders();
            await GetDataFromAPIAsync();
            IsLoad = true;
            _loadingService.HideLoading();
            await GetRoleByCode();
        }

        protected async Task GetRoleByCode()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest()
                {
                    FilterCriterias = new()
                    {
                        new Shared.Models.Enums.FilterCriteria("code","Distributor",Shared.Models.Enums.FilterType.Equal)
                    }
                    ,
                    IsCountRequired = true,
                    PageNumber = 1,
                    PageSize = 10,
                };

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Role/SelectAllRoles",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {

                    PagedResponse<Winit.Modules.Role.Model.Classes.Role>? response = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Role.Model.Classes.Role>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                    if (response != null && response.PagedData != null & response.TotalCount > 0)
                    {
                        Role = response?.PagedData?.FirstOrDefault<IRole>();
                    }
                }
                else
                {
                    //  _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                // _totalItems = 0;
            }
        }
        private async Task GetDataFromAPIAsync()
        {
            try
            {

                ApiResponse<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Interfaces.IEmp>>(
                    $"{_appConfigs.ApiBaseUrl}Distributor/SelectAllDistributorAdminDetailsByOrgUID?OrgUID={OrgUID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    EmpList = apiResponse.Data;
                }
                else
                {
                    //  _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                // _totalItems = 0;
            }
        }
        public List<DataGridColumn> Columns { get; set; }
        protected void SetColumnHeaders()
        {
            Columns = new List<DataGridColumn>
            {
                 new DataGridColumn { Header = "Login ID", GetValue = s => ((Winit.Modules.Emp.Model.Classes.Emp)s).Name, IsSortable = false, SortField = "Name" },
                 new DataGridColumn { Header = "Name", GetValue = s => ((Winit.Modules.Emp.Model.Classes.Emp)s).Name, IsSortable = false, SortField = "Name" },

                 new DataGridColumn
                 {
                     Header = "Actions",
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Image,
                             URL="Images/edit.png",
                             Action = item => EditLogInIdName((Winit.Modules.Emp.Model.Interfaces.IEmp)item)
                         },
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Text,
                             Text = "Reset",
                             Action = item => ResetPassword((Winit.Modules.Emp.Model.Interfaces.IEmp)item)

                         },

                     }
                 }
           };
        }

        protected Winit.UIModels.Common.Validation IsValidated()
        {
            string message = string.Empty;
            bool isVal = true;
            if (IsNewEmp || IsEditLoginID)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    message += "Name ,";
                    isVal = false;
                }
                if (string.IsNullOrEmpty(LoginId))
                {
                    message += "Login ID,";
                    isVal = false;
                }
            }
            if (IsNewEmp || IsPassword)
            {
                if (string.IsNullOrEmpty(Password))
                {
                    message += "Password ,";
                    isVal = false;
                }
                if (string.IsNullOrEmpty(ConfirmPassword))
                {
                    message += "Confirm Password ,";
                    isVal = false;
                }
            }
            if (isVal)
            {
                if ((IsNewEmp || IsPassword) && !Password.Equals(ConfirmPassword))
                {
                    message = "Password and Confirm Password must be same";
                    isVal = false;
                }
            }
            else
            {
                message = $"The following field(s) have invalid value(s) : {message.Substring(0, message.Length - 2)}";
            }

            return new UIModels.Common.Validation(isVal, message);
        }

        public void EditLogInIdName(Winit.Modules.Emp.Model.Interfaces.IEmp emp)
        {
            IsNewEmp = false;
            IsEditLoginID = true;
            IsShowPopUp = true;
            _DistributorAdminDTO.Emp = emp;

            Name = emp.Name;
            LoginId = emp.LoginId;
        }
        public void ResetPassword(Winit.Modules.Emp.Model.Interfaces.IEmp emp)
        {
            IsNewEmp = false;
            IsPassword = true;
            IsShowPopUp = true;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            _DistributorAdminDTO.Emp = emp;


        }
        public void CloseTab()
        {
            IsShowPopUp = false;
        }
        public void AddNewAdmin()
        {
            IsShowPopUp = true;
            IsNewEmp = true;
            Name = string.Empty;
            LoginId = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
        public async Task SaveOrUpdate()
        {
            _loadingService.ShowLoading();
            Validation validation = IsValidated();
            if (validation.IsValidated)
            {
                int retVal = 0;
                if (IsNewEmp)
                {
                    Save();
                    IsNewEmp = false;
                }
                else if (IsPassword)
                {
                    _DistributorAdminDTO.Emp.ModifiedBy = _iAppUser.Emp.UID;
                    _DistributorAdminDTO.Emp.ModifiedTime = DateTime.Now;
                    _DistributorAdminDTO.Emp.EncryptedPassword = Password;
                    _DistributorAdminDTO.ActionType = DistributorAdminActionType.UpdatePW;
                    IsPassword = false;
                }
                else if (IsEditLoginID)
                {
                    _DistributorAdminDTO.Emp.Name = this.Name;
                    _DistributorAdminDTO.Emp.AliasName = this.Name;
                    _DistributorAdminDTO.Emp.LoginId = this.LoginId;
                    _DistributorAdminDTO.Emp.EmpNo = LoginId;
                    _DistributorAdminDTO.Emp.ModifiedBy = _iAppUser.Emp.UID;
                    _DistributorAdminDTO.Emp.ModifiedTime = DateTime.Now;
                    _DistributorAdminDTO.ActionType = DistributorAdminActionType.UpdateUserName;
                    IsEditLoginID = false;
                }
                IsShowPopUp = false;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                     $"{_appConfigs.ApiBaseUrl}Distributor/CUDDistributorAdmin",
                     HttpMethod.Post, _DistributorAdminDTO);

                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    retVal = CommonFunctions.GetIntValue(apiResponse.Data);
                    if (retVal > 1)
                    {
                        EmpList.Add(_DistributorAdminDTO.Emp);
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", apiResponse.ErrorMessage);
                }
            }
            else
            {
                await _alertService.ShowErrorAlert("Error", validation.ErrorMessage);
            }
            _loadingService.HideLoading();
        }


        protected void Save()
        {
            string empUID = Guid.NewGuid().ToString();

            //insert EMP
            _DistributorAdminDTO.Emp = new Winit.Modules.Emp.Model.Classes.Emp()
            {
                UID = empUID,
                Name = this.Name,
                AliasName = this.Name,
                LoginId = this.LoginId,
                EmpNo = LoginId,
                Code = LoginId,
                AuthType = "Local",
                Status = Status,
                CreatedBy = _iAppUser.Emp.UID,
                ModifiedBy = _iAppUser.Emp.UID,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                EncryptedPassword = Password,
                CompanyUID = _iAppUser.Emp.CompanyUID,
            };
         
            _DistributorAdminDTO.JobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition()
            {
                UID = Guid.NewGuid().ToString(),
                EmpUID = empUID,
                OrgUID = OrgUID,
                UserRoleUID = Role?.UID,
                CreatedBy = _iAppUser.Emp.UID,
                ModifiedBy = _iAppUser.Emp.UID,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                Department = "Sales"
            };
            _DistributorAdminDTO.ActionType = DistributorAdminActionType.Add;
        }
        protected async Task<int> CUDDistributorAdmin(string api, HttpMethod httpMethod, object data)
        {
            int retVal = 0;
            try
            {

            }
            catch (Exception ex)
            {

            }

            return retVal;
        }

    }
}
