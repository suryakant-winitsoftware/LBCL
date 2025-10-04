using Microsoft.AspNetCore.Components;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.User.BL.Classes;
using Winit.Modules.User.Model.Classes;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.Maintain_User
{
    public partial class AddEditEmployee
    {
        private EmployeeRenderModel render = new EmployeeRenderModel();
        private bool IsOnLocationSKUMApping { get; set; }
        public string? LoginID { get; set; }
        public bool IsNew { get; set; } = true;
        private string? EmpUID { get; set; }
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="Maintain User",URL="MaintainUsers",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text="Maintain User Details"},
      }
        };
        //new
        public bool IsEditDetails { get; set; }
        public bool IsSuccessUserInformation { get; set; }
        public List<IFileSys> FileSysList { get; set; } = new List<IFileSys>();
        public string UserCreationUID { get; set; }
        public string ValidationMessage { get; set; }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            LoginID = _commonFunctions.GetParameterValueFromURL("LoginID");
            await _addEditEmployeeViewModel.EmployeeViewModel();
            LoadResources(null, _languageService.SelectedCulture);
            await _addEditEmployeeViewModel.PopulateViewModel(LoginID, IsNew);

            if (LoginID != null)
            {
                IsNew = false;
                await _addEditEmployeeViewModel.OnRoleDD(_iAppUser.SelectedJobPosition.OrgUID);
                await _addEditEmployeeViewModel.OnDepartmentDD();
                await _addEditEmployeeViewModel.GetTheBranchesList();
                await _addEditEmployeeViewModel.PopulateViewModelForOrg_RoleMapping(LoginID, IsNew);
                await _addEditEmployeeViewModel.PopulateUsersDetailsforEdit(LoginID);
                await _addEditEmployeeViewModel.SetEditForAuthTypeDD(_addEditEmployeeViewModel.EmpDTOmaintainUser);
                await _addEditEmployeeViewModel.PopulateUserLocationTypeAndValue(LoginID);
                await _addEditEmployeeViewModel.PopulateViewModel(LoginID, IsNew);
            }
            else
            {
                if (!string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
                {
                    _addEditEmployeeViewModel.Uid = null;
                }
                //Org Mapping
                await _addEditEmployeeViewModel.OnDepartmentDD();
                await _addEditEmployeeViewModel.GetUserLocationMappingType();
                _addEditEmployeeViewModel.LocationTypeAndValueForLocationMapping = new LocationTypeAndValue();
                await PopulatelocationAndSKUMapping();
                _addEditEmployeeViewModel.RoleSelectionItems.Clear();
                _addEditEmployeeViewModel.ReportsToSelectionItems.Clear();
                _addEditEmployeeViewModel.BranchSelectionItems.Clear();
                _addEditEmployeeViewModel.SalesOfficeSelectionItems.Clear();
                await _addEditEmployeeViewModel.OnRoleDD(_iAppUser.SelectedJobPosition.OrgUID);
                await _addEditEmployeeViewModel.GetTheBranchesList();
            }
            if (_addEditEmployeeViewModel.EmpDTOmaintainUser.FileSys != null)
            {
                FileSysList.Add(_addEditEmployeeViewModel.EmpDTOmaintainUser.FileSys);
            }
            dataService.HeaderText = $"{(IsNew ? "Add User" : "Edit User")}";

            StateHasChanged();
            HideLoader();
        }
        public async Task OnEmployeeInfoClick()
        {
            render.IsOrg_RoleMapinngInformationRendered = false;
            render.IsApplicableOrganizationInformationRendered = false;
            render.IsEmployeeInformationRendered = true;
            render.IsLocationMappingInformationRendered = false;
            IsOnLocationSKUMApping = false;
            await _addEditEmployeeViewModel.EmployeeViewModel();
            if (LoginID != null)
            {
                await _addEditEmployeeViewModel.SetEditForAuthTypeDD(_addEditEmployeeViewModel.EmpDTOmaintainUser);
                await _addEditEmployeeViewModel.PopulateUsersDetailsforEdit(LoginID);
            }
        }
        public async Task OnOrg_RoleMappingInfoClick()
        {
            render.IsOrg_RoleMapinngInformationRendered = true;
            render.IsEmployeeInformationRendered = false;
            render.IsApplicableOrganizationInformationRendered = false;
            render.IsLocationMappingInformationRendered = false;
            IsOnLocationSKUMApping = false;
            if (LoginID != null)
            {
                await _addEditEmployeeViewModel.SetEditForAuthTypeDD(_addEditEmployeeViewModel.EmpDTOmaintainUser);
                await _addEditEmployeeViewModel.PopulateUsersDetailsforEdit(LoginID);
            }
        }
        public async Task OnApplicableOrganizationInfoClick()
        {
            render.IsApplicableOrganizationInformationRendered = true;
            render.IsEmployeeInformationRendered = false;
            render.IsOrg_RoleMapinngInformationRendered = false;
            render.IsLocationMappingInformationRendered = false;
            IsOnLocationSKUMApping = false;
            if (LoginID!=null)
            {
                await _addEditEmployeeViewModel.PopulateUsersDetailsforEdit(LoginID);
                await _addEditEmployeeViewModel.PopulateViewModelForApplicableOrg(LoginID, IsNew);
                _addEditEmployeeViewModel.EmpOrgMappingDD.Clear();
                var data = await _addEditEmployeeViewModel.GetGridviewData(false);
                _addEditEmployeeViewModel.EmpOrgMappingDD.AddRange(data.Item2);
            }
        }

        public async Task OnLocationMappingSelectionClick()
        {
            render.IsApplicableOrganizationInformationRendered = false;
            render.IsEmployeeInformationRendered = false;
            render.IsOrg_RoleMapinngInformationRendered = false;
            render.IsLocationMappingInformationRendered = true;
            IsOnLocationSKUMApping = false;
        }
        public async Task OnLocationSKUMAppingClick()
        {
            _loadingService.ShowLoading();
            IJobPosition JobPosition = await ((AddEditEmployeeWebViewModel)_addEditEmployeeViewModel).GetJobPositionByEmpUID();
            IsOnLocationSKUMApping = true;
            render.IsApplicableOrganizationInformationRendered = false;
            render.IsEmployeeInformationRendered = false;
            render.IsLocationMappingInformationRendered = false;
            render.IsOrg_RoleMapinngInformationRendered = false;

            _addEditEmployeeViewModel.LocationMappingLable = string.IsNullOrEmpty(JobPosition?.LocationMappingTemplateUID) ?
              "Select Location Template" : JobPosition?.LocationMappingTemplateName;
            _addEditEmployeeViewModel.SKUMappingLable = string.IsNullOrEmpty(JobPosition?.SKUMappingTemplateUID) ?
              "Select SKU Template" : JobPosition.SKUMappingTemplateName;
            _loadingService.HideLoading();
        }
        protected async Task SaveLocationAndSKUMapping()
        {
            ShowLoader();
            if (string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
            {
                ShowErrorSnackBar("Error", "Employee Details is mandatory to fill");
            }
            else
            {
                await ((AddEditEmployeeWebViewModel)_addEditEmployeeViewModel).SaveLocationAndSKUMapping();
            }
            HideLoader();
        }
        private async Task BackBtnClicked()
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["back"], @Localizer["are_you_sure_you_want_to_go_back?"]))
            {
                _navigationManager.NavigateTo("MaintainUsers");
            }

        }

        #region Location and SKU Mapping
        protected async Task PopulatelocationAndSKUMapping()
        {
            await ((AddEditEmployeeWebViewModel)_addEditEmployeeViewModel).GetLocationAndSKUMappingDetailFromAPI();
        }
        #endregion


        #region LocationTypeAndValue

        private async Task HandleLocationTypeSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                _addEditEmployeeViewModel.LocationTypeAndValueForLocationMapping.LocationType = selectedItem.Code;
                ShowLoader();
                await _addEditEmployeeViewModel.GetUserLocationMappingValue(selectedItem.Code);
                HideLoader();
            }
            else
            {

            }
        }
        private async Task HandleLocationValueSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                _addEditEmployeeViewModel.LocationTypeAndValueForLocationMapping.LocationValue = selectedItem.Code;
            }
            else
            {

            }
        }
        private async Task SaveLocationMappingAsync(ILocationTypeAndValue locationTypeAndValue)
        {
            if (string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
            {
                ShowErrorSnackBar("Error", "Employee Details are mandatory to fill");
            }
            else
            {
                if (await _addEditEmployeeViewModel.CreateUpdateUserLocationTypeAndValueDetails(_addEditEmployeeViewModel.LocationTypeAndValueForLocationMapping))
                {
                    ShowSuccessSnackBar("Success", "Location mapping data Saved Successfully");
                }
                else
                {
                    ShowErrorSnackBar("Error", "Location mapping data Failed");
                }
            }
        }


        #endregion
        private async Task<bool> UserExistsAsync(string code, string loginId, string empNo)
        {
            // Implement the logic to check if the user exists in the database
            // This could involve a call to a service or repository that queries the user data
            return await _maintainusersViewModel.CheckUserExistsAsync(code, loginId, empNo);
        }
        public async Task CheckDuplication()
        {
            bool userExists = await UserExistsAsync(
                  _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Code,
                   _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.LoginId,
                   _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.EmpNo);
            if (userExists)
            {
                ShowErrorSnackBar("Error", "An employee with the same Code, LoginId, or EmpNo already exists.");

                return; // Exit early if user already exists
            }
        }
        protected async Task SaveOrUpdateEmployeeInformation()
        {
            _loadingService.ShowLoading();


            if (string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
            {
                // await CheckDuplication();
                bool userExists = await UserExistsAsync(
               _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Code,
               _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.LoginId,
               _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.EmpNo);

                if (userExists)
                {
                    // Exit early if the duplication check found a duplicate
                    _loadingService.HideLoading();
                    ShowErrorSnackBar("Error", "An employee with the same Code, LoginId, or EmpNo already exists.");
                    return;
                }
                if (await _addEditEmployeeViewModel.SaveUpdateEmployee(_addEditEmployeeViewModel.EmpDTOmaintainUser, true))
                {
                    _tost.Add("User", "User Details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    IsSuccessUserInformation = true;
                    _addEditEmployeeViewModel.UserCreationStoreUID = _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.UID;
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to save...");
                }
            }
            else
            {
                if (await _addEditEmployeeViewModel.SaveUpdateEmployee(_addEditEmployeeViewModel.EmpDTOmaintainUser, false))
                {
                    _tost.Add("User", "User Details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }
            }
            _loadingService.HideLoading();
        }
        public string Msg = "";
        public async Task SaveUpdateOrgRoleMapping()
        {
            ShowLoader();
            if (string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
            {
                ShowErrorSnackBar("Error", "Employee Details is mandatory to fill");
            }
            else
            {
                (string, bool) createresponse = await _addEditEmployeeViewModel.SaveUpdateOrg_RoleMapping(_addEditEmployeeViewModel.jobPosition, false);
                Msg = createresponse.Item1;
                if (string.IsNullOrEmpty(Msg))
                {
                    _ = _tost.Add(@Localizer["org&rolemapping"], @Localizer["org&rolemapping_details_update_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _ = _tost.Add(@Localizer["org&rolemapping"], @Localizer["org&rolemapping_failed_to_update"], Winit.UIComponents.SnackBar.Enum.Severity.Error);

                }
            }
            HideLoader();
        }


        public async Task GetApplicableGridview()
        {
            var data = await _addEditEmployeeViewModel.GetGridviewData();
            empOrgMappings = data.Item1;
            empOrgMappingDDLs = data.Item2;
        }

        public List<EmpOrgMapping> empOrgMappings { get; set; }
        public List<EmpOrgMappingDDL> empOrgMappingDDLs { get; set; }
        public async Task SaveApplicableOrg()
        {
            ShowLoader();
            if (string.IsNullOrEmpty(_addEditEmployeeViewModel.Uid))
            {
                ShowErrorSnackBar("Error", "Employee Details is mandatory to fill");
            }
            else
            {
                bool isCreated = await _addEditEmployeeViewModel.SaveUpdateEmpOrgMapping(empOrgMappings);
                if (isCreated)
                {
                    _addEditEmployeeViewModel.EmpOrgMappingDD.AddRange(empOrgMappingDDLs);
                    _addEditEmployeeViewModel.ApplicableOrganizationSelectionItems.ForEach(item => item.IsSelected = false);
                    StateHasChanged();
                    _tost.Add(@Localizer["applicable_organization"], @Localizer["applicable_organization_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _tost.Add(@Localizer["applicable_organization"], @Localizer["applicable_organization_failed_to_save."], Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            HideLoader();
        }
        public async Task DeleteApplicableOrg(string empOrgMapping)
        {
            string s = await _addEditEmployeeViewModel.DeleteEmpOrgMapping(empOrgMapping);
            if (s.Contains("Failed"))
            {
                await _alertService.ShowErrorAlert(@Localizer["failed"], s);
            }
            else
            {
                _addEditEmployeeViewModel.EmpOrgMappingDD.RemoveAll(item => item.EmpOrgMappingUID == empOrgMapping);
                await _alertService.ShowSuccessAlert(@Localizer["success"], s);
            }
        }
        #region Approval logic
        private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
        {

            ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse();
            try
            {
                if (!approvalStatusUpdate.IsFinalApproval && approvalStatusUpdate.Status != ApprovalConst.Rejected)
                {
                    approvalActionResponse.IsApprovalActionRequired = false;
                    return approvalActionResponse;
                }
                approvalActionResponse.IsApprovalActionRequired = true;
                _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.ApprovalStatusUpdate=approvalStatusUpdate;
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.ApprovalStatus = ApprovalConst.Rejected;
                }
                else
                {
                    _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.ApprovalStatus = ApprovalConst.Approved;
                }
                if (await _addEditEmployeeViewModel.SaveUpdateEmployee(_addEditEmployeeViewModel.EmpDTOmaintainUser, false))
                {
                    approvalActionResponse.IsSuccess = true;
                    _tost.Add("User", "User Details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    approvalActionResponse.IsSuccess = false;
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }

                return approvalActionResponse;

            }
            catch (Exception ex)
            {

                approvalActionResponse.IsSuccess = false;
                return approvalActionResponse;
            }
        }
        #endregion
    }
}

