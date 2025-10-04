using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Maintain_Warehouse
{
    public partial class AddEditMaintainWareHouse
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        [Parameter]
        public bool IsViewPage { get; set; } = false;
        public bool IsEditPage { get; set; } = false;
        public bool IsBackBtnPopUp { get; set; }
        private string validationMessage;
        [Parameter]
        public string WareHouseUID { get; set; }
        private bool IsIntialized { get; set; }
        private bool IsDisabled { get; set; } = false;
        IDataService dataService = new DataServiceModel()
        {
            BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="Maintain Warehouse",URL="MaintainWarehouse",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text="Maintain Warehouse"},
      }
        };
        protected override async Task OnInitializedAsync()
        {
          try
          {
            ShowLoader();
            await _addEditMaintainWarehouseViewModel.PopulateViewModel();
            // _addEditMaintainWarehouseViewModel.ParentUID = "FR001";
            WareHouseUID = _commonFunctions.GetParameterValueFromURL("WareHouseUID");
            var ViewQueryParam = _commonFunctions.GetParameterValueFromURL("IsViewPage");
            var ViewQueryParamEdit = _commonFunctions.GetParameterValueFromURL("IsEditPage");
            LoadResources(null, _languageService.SelectedCulture);
            if (ViewQueryParam != null)
            {
                IsViewPage = bool.Parse(ViewQueryParam);
                IsDisabled = IsViewPage;
            }
            if (ViewQueryParamEdit != null)
            {
                IsEditPage = bool.Parse(ViewQueryParamEdit);
                IsDisabled = IsEditPage;
            }
            if (WareHouseUID != null)
            {
                //IsEditPage = true;
                await _addEditMaintainWarehouseViewModel.PopulateMaintainWarehouseEditDetails(WareHouseUID);
                await _addEditMaintainWarehouseViewModel.SetEditForOrgTypeDD(_addEditMaintainWarehouseViewModel.WareHouseEditItemView);
            }
            IsIntialized = true;
            // await SetHeaderName();
            dataService.HeaderText = $"{(IsEditPage ? "Edit Warehouse" : "Add Warehouse")}";

            //StateHasChanged();
          }
          catch (Exception ex)
          {
              await _AlertMessgae.ShowErrorAlert("Error", "Failed to initialize warehouse page");
          }
          finally
          {
            HideLoader();
            StateHasChanged();
          }
        }
      
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_warehouse"], IsClickable = true, URL = "MaintainWarehouse" });
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{(IsDisabled ? (IsViewPage ? @Localizer["view"] : @Localizer["edit"]) : @Localizer["add"])}{@Localizer["warehouse"]} ", IsClickable = false });
        //    _IDataService.HeaderText = IsDisabled
        //        ? (IsViewPage ? @Localizer["view"] : @Localizer["edit"]) +@Localizer["warehouse"] 
        //        : @Localizer["add_warehouse"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task SaveWareHouse()
        {
            validationMessage = null;
            if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.WarehouseCode) ||
                string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.WarehouseName) ||
                    !IsWarehouseSelectionValid() ||
                    string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.AddressLine1))
            {
                validationMessage = @Localizer["the_following_field(s)_have_invalid_value(s)"] +": ";
                if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.WarehouseCode))
                {
                    validationMessage += @Localizer["code,"];
                }
                if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.WarehouseName))
                {
                    validationMessage += @Localizer["name_,"];
                }
                if (!IsWarehouseSelectionValid())
                {
                    validationMessage += @Localizer["warehouse_type,"];
                }
                if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.AddressLine1))
                {
                    validationMessage += @Localizer["address_line_1"];
                }
                //if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.City))
                //{
                //    validationMessage += "City , ";
                //}
                //if (string.IsNullOrWhiteSpace(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.RegionCode))
                //{
                //    validationMessage += "Region , ";
                //}
                validationMessage = validationMessage.TrimEnd(' ', ',');
            }

            else
            {

                if (!IsEditPage)
                {
                    if (await _addEditMaintainWarehouseViewModel.SaveUpdateWareHouse(_addEditMaintainWarehouseViewModel.WareHouseEditItemView, true))
                    {
                        _tost.Add(@Localizer["warehouse"], "Warehouse details saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        _navigationManager.NavigateTo("MaintainWareHouse");
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], "Failed to save");
                    }
                }
                else
                {
                    if (await _addEditMaintainWarehouseViewModel.SaveUpdateWareHouse(_addEditMaintainWarehouseViewModel.WareHouseEditItemView, false))
                    {
                        _tost.Add(@Localizer["warehouse"], "Warehouse details updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        _navigationManager.NavigateTo("MaintainWareHouse");
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], "Failed to update");
                    }
                }
            }
        }
        private bool IsWarehouseSelectionValid()
        {
            // Check if SupplierOrgUID is not null or empty
            return !string.IsNullOrEmpty(_addEditMaintainWarehouseViewModel.WareHouseEditItemView.OrgTypeUID);
        }
        private async Task BackBtnClicked()
        {
            if (await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_go_back_?"], @Localizer["yes"], @Localizer["no"]))
            {
                _navigationManager.NavigateTo($"MaintainWareHouse");
            }
            else
            {
                return;
            }
        }
        private async Task OnOkFromBackBTnPopUpClick()
        {
            _navigationManager.NavigateTo($"MaintainWareHouse");
        }
        public void OnWareHouseSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                _addEditMaintainWarehouseViewModel.WareHouseEditItemView.OrgTypeUID = selecetedValue?.UID;

            }
        }
    }
}
