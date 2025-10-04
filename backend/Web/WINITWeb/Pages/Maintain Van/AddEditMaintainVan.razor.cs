using Microsoft.AspNetCore.Components;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;
using System.Globalization;
using System.Resources;

namespace WinIt.Pages.Maintain_Van;

public partial class AddEditMaintainVan: BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsEditPage { get; set; }
    public bool IsLoaded { get; set; }
    [Parameter]
    public string VehicleUID { get; set; }
    private string validationMessage;
    public bool IsBackBtnPopUp { get; set; }
    IDataService dataService = new DataServiceModel()
    {
        BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="Maintain Van",URL="MaintainVan",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text="Maintain Van"},
      }
    };
    protected override async Task OnInitializedAsync()
    {
        _addEditMaintainVanViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
        LoadResources(null, _languageService.SelectedCulture);
        var ViewQueryParamEdit = _commonFunctions.GetParameterValueFromURL("IsEditPage");
        if (ViewQueryParamEdit != null)
        {
            IsEditPage = bool.Parse(ViewQueryParamEdit);
        }
        VehicleUID = _commonFunctions.GetParameterValueFromURL("VehicleUID");
        if (VehicleUID != null)
        {
            await _addEditMaintainVanViewModel.PopulateViewModel(VehicleUID);
            //_addEditMaintainVanViewModel.InstilizeFieldsForEditPage(_addEditMaintainVanViewModel.VEHICLE);
        }
        else
        {
            IsEditPage = false;
            _addEditMaintainVanViewModel.VEHICLE.TruckSIDate = DateTime.Today;
            _addEditMaintainVanViewModel.VEHICLE.RoadTaxExpiryDate = new DateTime(2099, 12, 31);
            _addEditMaintainVanViewModel.VEHICLE.RoadTaxExpiryDate = DateTime.Parse(_addEditMaintainVanViewModel.VEHICLE.RoadTaxExpiryDate.ToString("dd/MMM/yyyy"));
            _addEditMaintainVanViewModel.VEHICLE.InspectionDate = DateTime.Today;
        }
        IsLoaded = true;
        //await SetHeaderName();
        dataService.HeaderText = $"{(IsEditPage ? "Edit Van" : "Add Van")}";

        StateHasChanged();
    }
   
    //public async Task SetHeaderName()
    //{
    //    _IDataService.BreadcrumList = new();
    //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_van"], IsClickable = true, URL = "MaintainVan" });
    //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{(IsEditPage ? @Localizer["edit"] : @Localizer["add"])}{@Localizer["van"]}  ", IsClickable = false });
    //    _IDataService.HeaderText = $"{(IsEditPage ? @Localizer["edit"] :@Localizer["add"] )}{@Localizer["van"]}  ";
    //    await CallbackService.InvokeAsync(_IDataService);
    //}
    public void OnVehicleTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selecetedValue = dropDownEvent.SelectionItems.First();
            // sku.Code = selecetedValue?.Code;
            _addEditMaintainVanViewModel.VEHICLE.Type = selecetedValue.Label!;
        }
    }
    private async Task SaveVehicle()
    {
        validationMessage = null;
        if (string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.VehicleNo) ||
            string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.RegistrationNo) ||
            string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.Model))
        {
            validationMessage = @Localizer["the_following_field(s)_have_invalid_value(s)"];
            if (string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.VehicleNo))
            {
                validationMessage += @Localizer["vehicle,"];
            }
            if (string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.RegistrationNo))
            {
                validationMessage += @Localizer["registration_no,"];
            }
            if (string.IsNullOrWhiteSpace(_addEditMaintainVanViewModel.VEHICLE.Model))
            {
                validationMessage += @Localizer["model,"];
            }
            validationMessage = validationMessage.TrimEnd(' ', ',');
        }

        else
        {
            if (!IsEditPage)
            {
                await _addEditMaintainVanViewModel.SaveUpdateVanItem(_addEditMaintainVanViewModel.VEHICLE, true);
                await Task.Delay(1000);
                _navigationManager.NavigateTo("MaintainVan");
                StateHasChanged();
                _tost.Add(@Localizer["vehicle"], @Localizer["vehicle_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                await _addEditMaintainVanViewModel.SaveUpdateVanItem(_addEditMaintainVanViewModel.VEHICLE, false);
                await Task.Delay(1000);
                _navigationManager.NavigateTo("MaintainVan");
                _tost.Add(@Localizer["vehicle"], @Localizer["vehicle_details_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }

        }

    }
    public async Task BackBtnClicked()
    {
        if (await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_go_back?"], @Localizer["yes"], @Localizer["no"]))
        {
            _navigationManager.NavigateTo($"MaintainVan");
        }
        else
        {
            return;
        }
    }
    
}
