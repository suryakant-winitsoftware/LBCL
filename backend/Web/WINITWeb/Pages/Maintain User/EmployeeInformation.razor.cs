using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Resources;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.UIComponents.Common.Language;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
namespace WinIt.Pages.Maintain_User
{
    public partial class EmployeeInformation 
    {
        [Parameter] public string UID { get; set; }
        [Parameter] public bool IsNew { get; set; } = true;
        private bool IsLoading = false;

        public bool isEdited { get; set; }
        private bool IsLoaded { get; set; }

        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;

        public string LoginID { get; set; }
        private bool isSaveButtonDisabled = false;

        //ramana
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
        private string? FilePath { get; set; }
        private List<IFileSys> fileSysList { get; set; } = new List<IFileSys>();
        //ramana
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            LoginID = _commonFunctions.GetParameterValueFromURL("LoginID");
            if (LoginID != null)
            {
                IsNew = false;
            }
            await _addEditEmployeeViewModel.PopulateViewModel(UID, IsNew);
            FilePath = FileSysTemplateControles.GetEmpFolderPath(Guid.NewGuid().ToString());
            if (_addEditEmployeeViewModel.EmpDTOmaintainUser.FileSys != null)
            {
                fileSysList.Add(_addEditEmployeeViewModel.EmpDTOmaintainUser.FileSys);
            }
            IsLoaded = true;

            //await _addEditEmployeeViewModel.PopulateUsersDetailsforEdit(UID);
        }

       
        private bool IsAuthtypeSelectionValid()
        {
            return !string.IsNullOrEmpty(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.AuthType);
        }

        private async Task SaveUpdateEmployee()
        {
            IsSaveAttempted = true;

            try
            {
                ValidationMessage = null;

                // Validate required fields
                if (!IsAuthtypeSelectionValid() ||
                    string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Code) ||
                    string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.LoginId) ||
                    string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.EmpNo) ||
                    string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Name) ||
                    (_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.AuthType == "Active Directory" &&
                     (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.EmpInfo.ADGroup) ||
                      string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.EmpInfo.ADUsername))))
                {
                    // Validation error messages
                    ValidationMessage = @Localizer["the_following_field(s)_have_invalid_value(s)"] + ": ";
                    if (!IsAuthtypeSelectionValid()) ValidationMessage += @Localizer["auth_type,"];
                    if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Code)) ValidationMessage += @Localizer["code,"];
                    if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.LoginId)) ValidationMessage += @Localizer["loginid,"];
                    if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.EmpNo)) ValidationMessage += @Localizer["employee_no,"];
                    if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Name)) ValidationMessage += @Localizer["name,"];
                    if (_addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.AuthType == "Active Directory")
                    {
                        if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.EmpInfo.ADGroup)) ValidationMessage += @Localizer["ad_group,"];
                        if (string.IsNullOrWhiteSpace(_addEditEmployeeViewModel.EmpDTOmaintainUser.EmpInfo.ADUsername)) ValidationMessage += @Localizer["ad_username,"];
                    }
                    ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                    return; // Exit early if validation fails
                }

                // Check for existing user
                bool userExists = await UserExistsAsync(
                    _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.Code,
                    _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.LoginId,
                    _addEditEmployeeViewModel.EmpDTOmaintainUser.Emp.EmpNo);

                if (userExists)
                {
                    ValidationMessage = "An employee with the same Code, LoginId, or EmpNo already exists.";
                    return; // Exit early if user already exists
                }

                IsLoading = true;
                ShowLoader();

                if (IsNew)
                {
                    IEmpDTO empDTO = new EmpDTO();
                    await _addEditEmployeeViewModel.SaveUpdateEmployee(_addEditEmployeeViewModel.EmpDTOmaintainUser, true);

                    // Handle file system if necessary
                    if (fileSysList != null && fileSysList.Count > 0)
                    {
                        await SaveFileSys();
                    }
                    await Task.Delay(1000);
                    _tost.Add(@Localizer["employee"], @Localizer["employee_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    await _addEditEmployeeViewModel.SaveUpdateEmployee(_addEditEmployeeViewModel.EmpDTOmaintainUser, false);
                    if (fileSysList != null && fileSysList.Count > 0)
                    {
                        await SaveFileSys();
                    }
                    await Task.Delay(1000);
                    _tost.Add(@Localizer["employee"], @Localizer["employee_details_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
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

        // This method checks if a user with the same Code, LoginId, or EmpNo already exists
        private async Task<bool> UserExistsAsync(string code, string loginId, string empNo)
        {
            // Implement the logic to check if the user exists in the database
            // This could involve a call to a service or repository that queries the user data
            return await _maintainusersViewModel.CheckUserExistsAsync(code, loginId, empNo);
        }
       

        //ramana
        private void UploadImage(List<IFileSys> ImagePath)
        {
            fileSysList = ImagePath;
        }
        protected async Task SaveFileSys()
        {
            if (fileSysList != null)
            {
                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await fileUploader.MoveFiles();
                if (apiResponse.IsSuccess)
                {
                    var tasks = fileSysList.Select(async fileSys =>
                    {
                        fileSys.LinkedItemUID = _addEditEmployeeViewModel.Uid;
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
                _ = await _addEditEmployeeViewModel.CreateUpdateFileSysData(fileSys);
                StateHasChanged();
                _ = _tost.Add(@Localizer["user_profile"], @Localizer["profile_saved_successfully,"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            HideLoader();
        }
        private void AfterDeleteImage()
        {

        }
    }
}
