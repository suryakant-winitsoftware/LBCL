using Microsoft.AspNetCore.Components;
using Nest;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace WinIt.Pages.Maintain_User
{
    public partial class EmployeeDetails
    {
        [Parameter] public List<ISelectionItem> AuthTypeSelectionItems { get; set; }
        [Parameter]public IEmpDTO? EmpDTOmaintainUser { get; set; } = new EmpDTO();
        public string ButtonName { get; set; } = "Save";
        [Parameter] public bool IsEditDetails { get; set; }
        [Parameter] public EventCallback<IEmpDTO> SaveOrUpdateEmployeeInformation { get; set; }
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
        private string? FilePath { get; set; }
        [Parameter] public string TabName { get; set; }
        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;
        [Parameter] public EventCallback<IEmpDTO> SaveOrUpdateFileSys { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        //[Parameter] public EventCallback<DropDownEvent> AuthTypeSelection { get; set; }
        [Parameter] public EventCallback<IFileSys> CreateUpdateFileSysData { get; set; }
        [Parameter] public EventCallback CheckDuplication { get; set; }

        public bool Isinitialized { get; set; } = true;
        [Parameter] public bool IsNew { get; set; } = true;
        private bool IsLoading = false;
        public string LoginID { get; set; }
        [Parameter] public List<IFileSys> FileSysList { get; set; } = new List<IFileSys>();

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            LoginID = _commonFunctions.GetParameterValueFromURL("LoginID");
            if (LoginID != null)
            {
                IsNew = false;
            }
            //FirmTypeselectionItems = new();
            AuthTypeSelectionItems = new List<ISelectionItem>();
            EmpDTOmaintainUser = _serviceProvider.CreateInstance<IEmpDTO>();
            EmpDTOmaintainUser.EmpInfo = _serviceProvider.CreateInstance<IEmpInfo>();
            EmpDTOmaintainUser.Emp = _serviceProvider.CreateInstance<IEmp>();
            EmpDTOmaintainUser.JobPosition = _serviceProvider.CreateInstance<IJobPosition>();
            EmpDTOmaintainUser.FileSys = _serviceProvider.CreateInstance<IFileSys>();
            //EmpDTOmaintainUser.FileSysList = new List<IFileSys>();
           // EmpDTOmaintainUser.FileSysList = _serviceProvider.CreateInstance<List<IFileSys>>();
            ButtonName = IsEditDetails ? "Update" : "Save";
            FilePath = FileSysTemplateControles.GetEmpFolderPath(Guid.NewGuid().ToString());
            // MapGSTonAdd();
            //Isinitialized = true;
            if (IsEditDetails)
            {
                //await MapGSTonEdit();
            }
            _loadingService.HideLoading();
            StateHasChanged();
        }
        private void GetsavedImagePath(List<IFileSys> ImagePath)
        {
           FileSysList = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }
        private bool IsAuthtypeSelectionValid()
        {
            return !string.IsNullOrEmpty(EmpDTOmaintainUser.Emp.AuthType);
        }
        protected async Task SaveOrUpdate()
        {
            IsSaveAttempted = true;
            try

            {
                ValidationMessage = null;
                if (!IsAuthtypeSelectionValid() ||
                   string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.Code) ||
                   string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.LoginId) ||
                   string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.EmpNo) ||
                   string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.Name) ||
                   (EmpDTOmaintainUser.Emp.AuthType == "Active Directory" &&
                    (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.EmpInfo.ADGroup) ||
                     string.IsNullOrWhiteSpace(EmpDTOmaintainUser.EmpInfo.ADUsername))))
                {
                    // Validation error messages
                    ValidationMessage = @Localizer["the_following_field(s)_have_invalid_value(s)"] + ": ";
                    if (!IsAuthtypeSelectionValid()) ValidationMessage += @Localizer["auth_type,"];
                    if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.Code)) ValidationMessage += @Localizer["code,"];
                    if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.LoginId)) ValidationMessage += @Localizer["loginid,"];
                    if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.EmpNo)) ValidationMessage += @Localizer["employee_no,"];
                    if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.Emp.Name)) ValidationMessage += @Localizer["name,"];
                    if (EmpDTOmaintainUser.Emp.AuthType == "Active Directory")
                    {
                        if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.EmpInfo.ADGroup)) ValidationMessage += @Localizer["ad_group,"];
                        if (string.IsNullOrWhiteSpace(EmpDTOmaintainUser.EmpInfo.ADUsername)) ValidationMessage += @Localizer["ad_username,"];
                    }
                    ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                    return; // Exit early if validation fails
                }

             

                IsLoading = true;
                ShowLoader();
                if (IsNew)
                {
                    // Check for existing user
                    await CheckDuplication.InvokeAsync();
                    IEmpDTO empDTO = new EmpDTO();
                    await SaveOrUpdateEmployeeInformation.InvokeAsync(EmpDTOmaintainUser);

                    // Handle file system if necessary
                    if (FileSysList != null && FileSysList.Count > 0)
                    {
                        await SaveFileSys();
                    }
                    await Task.Delay(1000);
                   // _tost.Add(@Localizer["employee"], @Localizer["employee_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    await SaveOrUpdateEmployeeInformation.InvokeAsync(EmpDTOmaintainUser);
                    if (FileSysList != null && FileSysList.Count > 0)
                    {
                        await SaveFileSys();
                    }
                    await Task.Delay(1000);
                   // _tost.Add(@Localizer["employee"], @Localizer["employee_details_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
                HideLoader();
            }
        }
        
        protected async Task SaveFileSys()
        {
            if (FileSysList != null)
            {
                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await fileUploader.MoveFiles();
                if (apiResponse.IsSuccess)
                {
                    var tasks = FileSysList.Select(async fileSys =>
                    {
                        //fileSys.LinkedItemUID = _addEditEmployeeViewModel.Uid;
                        await CreateUpdateEmpImage(fileSys);

                    });
                    await Task.WhenAll(tasks);
                }
            }
        }

        public async Task CreateUpdateEmpImage(IFileSys fileSys)
        {
            if (fileSys != null)
            {
                ShowLoader();
                await CreateUpdateFileSysData.InvokeAsync(fileSys);
                StateHasChanged();
                _ = _tost.Add(@Localizer["user_profile"], @Localizer["profile_saved_successfully,"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            HideLoader();
        }
      
        public async Task OnAuthTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            EmpDTOmaintainUser.Emp.AuthType = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                //sku.Code = selecetedValue?.Code;
                //await AuthTypeSelection.InvokeAsync();
                EmpDTOmaintainUser.Emp.AuthType = selecetedValue?.Code;
            }
        }
    }
}
