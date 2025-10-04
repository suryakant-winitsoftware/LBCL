using Microsoft.AspNetCore.Components;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.Maintain_Error;

public partial class AddEditMaintainError : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsEditError { get; set; }
    public bool IsLoaded { get; set; } 
    public string? ErrorUID { get; set; }
    private string? validationMessage;
    IDataService dataService = new DataServiceModel()
    {
        BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="Error Details",URL="ViewErrorDetails",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text="Maintain Error Details"},
      }
    };
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        ErrorUID = _commonFunctions.GetParameterValueFromURL("ErrorUID");
        await _addEditMaintainErrorViewModel.PopulateViewModel(ErrorUID);
        if (ErrorUID != null)
        {
            IsEditError = true;
        }
        else
        {
        }
        dataService.HeaderText = $"{(IsEditError ?  "Edit Error Details" : "Add Error Details")}";
        IsLoaded = true;
    }
    
    

    public void OnSeveritySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selecetedValue != null)  
                _addEditMaintainErrorViewModel.ErrorDetail.Severity =  int.Parse(selecetedValue.Code);
        }
    }
    public void OnCategorySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selecetedValue != null)
                _addEditMaintainErrorViewModel.ErrorDetail.Category = selecetedValue.Code;
        }
    }
    public void OnPlatformSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedValue = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selectedValue != null)
                _addEditMaintainErrorViewModel.ErrorDetail.Platform = selectedValue.Code;
        }
    }
    public void OnModuleSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedValue = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selectedValue != null)
                _addEditMaintainErrorViewModel.ErrorDetail.Module = selectedValue.Code;
        }
    }
    public void OnSubModuleSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selecetedValue = dropDownEvent.SelectionItems.First();
            if(selecetedValue != null)
            _addEditMaintainErrorViewModel.ErrorDetail.SubModule = selecetedValue.Code;
        }
    }

    private async Task SaveOrUpdate()
    {
        if (CheckAddErrorDetails())
        {
            try
            {
                ShowLoader();
                if (IsEditError)
                {
                    if (await _addEditMaintainErrorViewModel.Update())
                    {
                        ShowSuccessSnackBar(@Localizer["success"], @Localizer["errordetails_updated_successfully"] );
                        ShowLoader();
                        _navigationManager.NavigateTo("ViewErrorDetails");

                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_update"] );
                        _navigationManager.NavigateTo("ViewErrorDetails");
                    }
                }
                else
                {
                    if (await _addEditMaintainErrorViewModel.Save())
                    {
                        ShowSuccessSnackBar(@Localizer["success"], @Localizer["errordetails_saved_successfully"]);
                        ShowLoader();
                        _navigationManager.NavigateTo("ViewErrorDetails");
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_save"] );
                        _navigationManager.NavigateTo("ViewErrorDetails");
                    }
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

    private bool CheckAddErrorDetails()
    {
        validationMessage = "";
        
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.ErrorCode))
        {
            validationMessage += @Localizer["error_code,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.Severity.ToString()))
        {
            validationMessage += @Localizer["severity,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.Category))
        {
            validationMessage += @Localizer["category,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.Platform))
        {
            validationMessage += @Localizer["platform,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.Module))
        {
            validationMessage += @Localizer["module,"];
        }
        if (string.IsNullOrWhiteSpace(_addEditMaintainErrorViewModel.ErrorDetail.SubModule))
        {
            validationMessage += @Localizer["submodule"];
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
}

