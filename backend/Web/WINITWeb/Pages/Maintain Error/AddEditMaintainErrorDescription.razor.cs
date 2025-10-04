using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Resources;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Enums;

using Winit.UIComponents.Common.Language;
namespace WinIt.Pages.Maintain_Error;

public partial class AddEditMaintainErrorDescription
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsEditErrorDescription { get; set; }

    public bool IsLoaded { get; set; }
    public string? ErrorDescriptionUID { get; set; }
    public string? ErrorDescriptionCode { get; set; }
    private static string? _currentErrorDescriptionCode;
    private string? validationMessage ;
    protected override async Task OnInitializedAsync()
    {
        ErrorDescriptionUID = _commonFunctions.GetParameterValueFromURL("ErrorUID");
        ErrorDescriptionCode= _commonFunctions.GetParameterValueFromURL("ErrorCode");
        LoadResources(null, _languageService.SelectedCulture);
        if (ErrorDescriptionUID != null)
        {
            IsEditErrorDescription = true;
            _addEditMaintainErrorDescriptionViewModel.IsEditErrorDescription = true;
            await _addEditMaintainErrorDescriptionViewModel.PopulateErrorDescriptionViewModel(ErrorDescriptionUID);
        }
        else
        {
            _addEditMaintainErrorDescriptionViewModel.ErrorDescriptionCode= ErrorDescriptionCode;
        }
        IsLoaded = true;
        await SetHeaderName();
    }
    
    private async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new();
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Maintain Error Description", IsClickable = true, URL = "ViewErrorDetails" });
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{(IsEditErrorDescription ? "Edit " : "Add ")} Error Description", IsClickable = false });
        _IDataService.HeaderText = $"{(IsEditErrorDescription ? "Edit " : "Add ")} Error Description ";
        await CallbackService.InvokeAsync(_IDataService);
    }

    private async Task SaveOrUpdateAsync()
    {
        if (CheckLanguageDescriptionFields())
        {
            try
            {
                ShowLoader();

                if (await _addEditMaintainErrorDescriptionViewModel.SaveOrUpdate())
                {
                    _currentErrorDescriptionCode = _addEditMaintainErrorDescriptionViewModel?.ErrorDetailsLocalization?.ErrorCode;
                    ShowLoader();
                    ShowSuccessSnackBar(@Localizer["success"], @Localizer["errordetails_saved_or_updated_successfully"]);

                    //await _addEditMaintainErrorDescriptionViewModel.PopulateErrorDescriptionViewModel(ErrorDescriptionUID);
                    _navigationManager.NavigateTo($"ErrorDescription?ErrorUID2={_currentErrorDescriptionCode}");
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_save"]);
                    ShowLoader();
                    _navigationManager.NavigateTo("ErrorDescription");
                }
                HideLoader();
            }
            catch (Exception)
            {
                HideLoader();
                throw;
            }
        }
    }
    private bool CheckLanguageDescriptionFields()
    {
        validationMessage = ""; 
        if (CheckLanguageCode(_addEditMaintainErrorDescriptionViewModel.ErrorDetailsLocalization.LanguageCode))
        {
            validationMessage += "";
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorDescriptionViewModel.ErrorDetailsLocalization.Cause))
        {
            validationMessage += @Localizer["cause,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorDescriptionViewModel.ErrorDetailsLocalization.Resolution))
        {
            validationMessage += @Localizer["resolution,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorDescriptionViewModel.ErrorDetailsLocalization.ShortDescription))
        {
            validationMessage += @Localizer["short_description,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorDescriptionViewModel.ErrorDetailsLocalization.Description))
        {
            validationMessage += @Localizer["description,"];
        }
        validationMessage = validationMessage.TrimEnd(' ', ',');
        if (validationMessage.Length > 0)
        {
            _tost.Add(@Localizer["the_following_field(s)_are_mandatory_:"] + validationMessage, Winit.UIComponents.SnackBar.Enum.Severity.Error.ToString());
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool CheckLanguageCode(string languageCode)
    {
        if( !string.IsNullOrEmpty(languageCode))
        { 
            if(languageCode.Count() > 0  && languageCode.Count() <=2)
            {
                return true;
            }
            else
            {
                validationMessage += @Localizer["language_code_should_be_of_length_1_-_2"];
                return false;
            }
        }
        else
        {
            validationMessage += @Localizer["language_code,"];
            return false;
        }

    }
}


