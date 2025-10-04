using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System.Dynamic;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
using WINITSharedObjects.Models;

namespace WinIt.Pages.Maintain_User
{
    public partial class OrgRoleMapping
    {
        private bool IsLoaded { get; set; }
        [Parameter] public string UID { get; set; }
        [Parameter] public bool IsNew { get; set; }
        public string ValidationMessage;
        public string LoginID { get; set; }
        public string Msg = "";
        private bool IsLoading = false;

        [Parameter] public List<ISelectionItem> RoleSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnRoleSelection { get; set; }
        [Parameter] public List<ISelectionItem> BranchSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnBranchSelection { get; set; }
        [Parameter] public bool IsBranchApplicable { get; set; }
        [Parameter] public List<ISelectionItem> SalesOfficeSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnSalesOfficeSelection { get; set; }
        [Parameter] public List<ISelectionItem> ReportsToSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnReportsToSelection { get; set; }
        [Parameter] public List<ISelectionItem> DepartmentSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnDepartmentSelection { get; set; }

        [Parameter] public List<IRole> roleDD { get; set; }
        [Parameter] public IJobPosition jobPosition { get; set; }
        [Parameter] public Winit.UIComponents.Common.CustomControls.DropDown Roleddl { get; set; }
        [Parameter] public Winit.UIComponents.Common.CustomControls.DropDown Reportsddl { get; set; }
        public IOrg org { get; set; }
        public IEmp reportsTo { get; set; }
        public IRole role { get; set; }
        [Parameter] public EventCallback<(string message, bool success)> SaveUpdateOrgMapping { get; set; }
        [Parameter] public string? OrgName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // _addEditEmployeeViewModel.EmpDTOmaintainUser = _serviceProvider.CreateInstance<IEmpDTO>();
            // await _addEditEmployeeViewModel.OnOrgDD();
            //RoleSelectionItems = new List<ISelectionItem>();
            //BranchSelectionItems = new List<ISelectionItem>();
            //SalesOfficeSelectionItems = new List<ISelectionItem>();
            //ReportsToSelectionItems = new List<ISelectionItem>();
            //DepartmentSelectionItems = new List<ISelectionItem>();
           // jobPosition = _serviceProvider.CreateInstance<IJobPosition>();                       
            LoadResources(null, _languageService.SelectedCulture);
            LoginID = _commonFunctions.GetParameterValueFromURL("LoginID");
            if (LoginID != null)
            {
                IsNew = false;
            }
            else
            {
                IsNew = true;
            }
            IsLoaded = true;
        }
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }
        public List<ISelectionItem> SelectedItem = new List<ISelectionItem>();
        public bool CheckCollectionLimit()
        {
            try
            {
                if (RoleSelectionItems.Any())
                {
                    bool result = RoleSelectionItems.Any(p => p.IsSelected && (roleDD.Any(q => q.UID == p.UID && q.IsAppUser)));
                    SelectedItem = RoleSelectionItems.Where(p => p.IsSelected).ToList();
                    if (SelectedItem.Count > 0)
                    {
                        foreach (var item in roleDD.Where(p => p.UID == SelectedItem.First().UID))
                        {
                            if (item.IsAppUser)
                            {
                                return true;
                            }
                        }
                    }
                }
                    return false;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        protected override void OnInitialized()
        {
            IsLoaded = false;
            //_addEditEmployeeViewModel.jobPosition.OrgUID = null;
            jobPosition.UserRoleUID = null;
            jobPosition.ReportsToUID = null;
            jobPosition.Department = null;
            jobPosition.CollectionLimit = 0;
            //_addEditEmployeeViewModel.OrgCode = _iAppUser.CurrentOrg?.Code;
            //_addEditEmployeeViewModel.OrgName = _iAppUser.CurrentOrg?.Name;
            base.OnInitialized();
        }

        private async Task SaveUpdateOrgRoleMapping()
        {
            ValidationMessage = null;

            if (/*!IsOrgSelectionValid() ||*/
                !IsRoleSelectionValid() ||
                !IsReportsToSelectionValid() ||
                !IsDepartmentSelectionValid() ||
                !IsCollectionLimitValid()||
                !IsBranchSelectionValid()
                )
            {
                ValidationMessage = @Localizer["the_following_field(s)_have_invalid_value(s):"];
                //if (!IsOrgSelectionValid())
                //{
                //    ValidationMessage += "Org, ";
                //}
                if (!IsRoleSelectionValid())
                {
                    ValidationMessage += @Localizer["role_,"];
                }
                if (!IsReportsToSelectionValid())
                {
                    ValidationMessage += @Localizer["reports_,"];
                }
                if (!IsDepartmentSelectionValid())
                {
                    ValidationMessage += @Localizer["department_,"];
                }
                if (!IsCollectionLimitValid())
                {
                    ValidationMessage += @Localizer["collection_limit"];
                }
                if (!IsBranchSelectionValid())
                {
                    ValidationMessage += "Branch";
                }
                // if (!IsSalesOfficeSelectionValid())
                // {
                //     ValidationMessage += "SalesOffice";
                // }
                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
            else
            {
                IsLoading = true;
                ShowLoader();
                org = new Winit.Modules.Org.Model.Classes.Org();
                reportsTo = new Emp();
                role = new Role();
                //(string, bool) createresponse = await _addEditEmployeeViewModel.SaveUpdateOrg_RoleMapping(_addEditEmployeeViewModel.jobPosition, false);
                //Msg = createresponse.Item1;
                //if (string.IsNullOrEmpty(Msg))
                //{
                //    _ = _tost.Add(@Localizer["org&rolemapping"], @Localizer["org&rolemapping_details_update_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                //}
                //else
                //{
                //    _ = _tost.Add(@Localizer["org&rolemapping"], @Localizer["org&rolemapping_failed_to_update"], Winit.UIComponents.SnackBar.Enum.Severity.Error);

                //}
                await SaveUpdateOrgMapping.InvokeAsync();
                HideLoader();

                //}
                IsLoading = false;
                IsNew = false;
            }
        }
        //private bool IsOrgSelectionValid()
        //{
        //    return !string.IsNullOrEmpty(_addEditEmployeeViewModel.jobPosition.OrgUID);
        //}
        private bool IsRoleSelectionValid()
        {
            return !string.IsNullOrEmpty(jobPosition.UserRoleUID);
        }
        private bool IsReportsToSelectionValid()
        {
            return !string.IsNullOrEmpty(jobPosition.ReportsToUID);
        }
        private bool IsDepartmentSelectionValid()
        {
            return !string.IsNullOrEmpty(jobPosition.Department);
        }
        private bool IsBranchSelectionValid()
        {
            if(IsBranchApplicable)
            {
                return !string.IsNullOrEmpty(jobPosition.BranchUID);
            }
            else
            {
                return true;
            }
        }
        private bool IsSalesOfficeSelectionValid()
        {
            return !string.IsNullOrEmpty(jobPosition.SalesOfficeUID);
        }
        private bool IsCollectionLimitValid()
        {
            if (CheckCollectionLimit())
            {
                return jobPosition.CollectionLimit != 0;
            }
            return true;
        }
       public async Task OnRoleSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnRoleSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
        public async Task OnBranchSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnBranchSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
        public async Task OnSalesOfficeSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnSalesOfficeSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
        public async Task OnReportsToSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnReportsToSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
        public async Task OnDepartmentSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnDepartmentSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
    }

}
