using Microsoft.AspNetCore.Components;
using Nest;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.util;
using Winit.Modules.Common.BL;
using Winit.Modules.Emp.BL.Classes;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Common;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Maintain_User
{
    public partial class ApplicableOrganization
    {
        private bool IsLoaded { get; set; }
        private bool IsInitailized { get; set; }
        public string ValidationMessage;
        public List<DataGridColumn> DataGridColumns { get; set; }
        [Parameter]
        public string? LoginID { get; set; }
        [Parameter] public bool IsNew { get; set; }
        private Winit.UIComponents.Common.CustomControls.DropDown? OrgDDL;
        [Parameter] public List<ISelectionItem> ApplicableOrganizationSelectionItems { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnApplicableOrganizationSelection { get; set; }
        [Parameter] public List<IEmpOrgMappingDDL> EmpOrgMappingDD { get; set; }
        [Parameter] public EventCallback<string> GetEmpOrgMappingDD { get; set; }
        [Parameter] public EventCallback GetGridviewData { get; set; }
        [Parameter] public EventCallback<List<EmpOrgMapping>> SaveApplicableOrg { get; set; }
        [Parameter] public EventCallback<string> DeleteApplicableOrg { get; set; }
        [Parameter] public IJobPosition jobPosition { get; set; } = new JobPosition();
        [Parameter]
        public List<EmpOrgMapping> empOrgMappings { get; set; }
        [Parameter]
        public List<EmpOrgMappingDDL> empOrgMappingDDLs { get; set; }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            EmpOrgMappingDD = new List<IEmpOrgMappingDDL>();
            EmpOrgMappingDD.Clear();
            string? empUID = _iAppUser?.SelectedJobPosition?.OrgUID;
            if (empUID != null)
            {
                await GetEmpOrgMappingDD.InvokeAsync(empUID);
            }
            if (LoginID != null)
            {
                IsNew = false;
                //await _addEditEmployeeViewModel.PopulateViewModelForApplicableOrg(LoginID, IsNew);
                //_addEditEmployeeViewModel.EmpOrgMappingDD.Clear();
                //var data = await _addEditEmployeeViewModel.GetGridviewData(true);
                //_addEditEmployeeViewModel.EmpOrgMappingDD.AddRange(data.Item2);
                IsInitailized = true;
                StateHasChanged();
            }
            IsNew = true;
            IsLoaded = true;
            StateHasChanged();
        }
        private bool IsOrgSelectionValid()
        {
            return !string.IsNullOrEmpty(jobPosition.OrgUID);
        }
        public async Task AddSelectedItems()
        {
             await GetGridviewData.InvokeAsync();
            await SaveOrgs(empOrgMappings, empOrgMappingDDLs);
        }
        //public async Task GetGridviewData()
        //{
        //    await _addEditEmployeeViewModel.GetGridviewData(false);
        //}
        private async Task SaveOrgs(List<EmpOrgMapping> empOrgMappings, List<EmpOrgMappingDDL> empOrgMappingDDLs)
        {
            ValidationMessage = null;
            if (!IsOrgSelectionValid())
            {
                ValidationMessage = @Localizer["the_following_field(s)_have_invalid_value(s):"];
                if (!IsOrgSelectionValid())
                {
                    ValidationMessage += @Localizer["org,"];
                }
                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
            else
            {
                ShowLoader();
                await SaveApplicableOrg.InvokeAsync(empOrgMappings);
                //bool isCreated = await _addEditEmployeeViewModel.SaveUpdateEmpOrgMapping(empOrgMappings);
                //if (isCreated)
                //{
                //    _addEditEmployeeViewModel.EmpOrgMappingDD.AddRange(empOrgMappingDDLs);
                //    _addEditEmployeeViewModel.ApplicableOrganizationSelectionItems.ForEach(item => item.IsSelected = false);
                    IsInitailized = true;
                //    StateHasChanged();
                //    _tost.Add(@Localizer["applicable_organization"], @Localizer["applicable_organization_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                //}
                //else
                //{
                //    _tost.Add(@Localizer["applicable_organization"], @Localizer["applicable_organization_failed_to_save."] , Winit.UIComponents.SnackBar.Enum.Severity.Error);
                //}
                HideLoader();
            }
        }
        public async Task OnDeleteClick(IEmpOrgMappingDDL empOrgMapping)
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete?"]))
            {
                ShowLoader();
                await DeleteApplicableOrg.InvokeAsync(empOrgMapping.EmpOrgMappingUID);
                HideLoader();
            }
            StateHasChanged();
        }
        public async Task OnApplicableOrganizationSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            await OnApplicableOrganizationSelection.InvokeAsync(dropDownEvent);
            StateHasChanged();
        }
    }
}
