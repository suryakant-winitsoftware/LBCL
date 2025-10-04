using Microsoft.AspNetCore.Components;
using System.util;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Web.Filter;
using Winit.UIModels.Common.Filter;
using Winit.Shared.Models.Enums;
using Winit.Modules.Bank.BL.Classes;
using System.Collections.Generic;
using Winit.Modules.ErrorHandling.BL.Classes;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;
using Winit.Modules.Common.UIState.Classes;


namespace WinIt.Pages.Maintain_Bank
{
    public partial class ViewBankDetails : WinIt.Pages.Base.BaseComponentBase
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        public string Operation;
        private Filter? FilterRef;
        public List<FilterModel> FilterColumns = new List<FilterModel>();
        public IBank? SelectedBankDetail { get; set; }
        private bool IsDeleteBtnPopUp { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                Filterinitialized();
                await _ViewBankDetailsViewModel.PopulateViewModel();
                await GenerateGridColumns();
                IsLoaded = true;
                await SetHeaderName();

                await StateChageHandler();
                HideLoader();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                HideLoader();
            }
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("ViewBankDetails", ref FilterColumns, out PageState pageState);
            
                ///only work with filters
                await OnFilterApply(_pageStateHandler._currentFilters);
            
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ViewBankDetails");
        }

        public void Filterinitialized()
        {
            FilterColumns=new List<FilterModel>
            {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = @Localizer["code"],
                    ColumnName = "BankCode"
                },
                //new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["country"],
                //    ColumnName = "CountryName" }
                 //new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                 //   DropDownValues=(_viewBankDetailsViewModel as ViewBankDetailsWebViewModel).CountrySelectionItems ,
                 //   ColumnName = "CountryName", Label = "Country Name" },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues = _ViewBankDetailsViewModel.CountrySelectionItems,
                     ColumnName = "CountryName",
                     Label = @Localizer["country"]
                 },
            };

        }
        private async Task AddNewBankDetails()
        {
            _navigationManager.NavigateTo($"ViewBankMaster");
        }

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((IBank)s)?.BankCode?? "N/A" },
                new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((IBank)s)?.BankName?? "N/A" },
                new DataGridColumn { Header = @Localizer["country_name"], GetValue = s => ((IBank)s)?.CountryName ?? "N/A" },
                new DataGridColumn { Header = @Localizer["cheque_fee"],  GetValue = s => ((IBank)s)?.ChequeFee != null ? $"{((IBank)s).ChequeFee:0.00}" : "N/A" },
                new DataGridColumn
                {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    //new ButtonAction
                    //{
                    //    ButtonType = ButtonTypes.Image,
                    //    URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                    //    Action = item => OnViewClick((IBank)item, "View")
                    //},
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = item => OnViewClick((IBank)item, "Edit")
                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                        Action = item => OnDeleteClick((IBank)item, "Edit")
                    }
                }
            }
             };
        }

        private void OnDeleteClick(IBank item, string v)
        {
            SelectedBankDetail = item;
            IsDeleteBtnPopUp = true;
            StateHasChanged();
        }
        public async Task OnOkFromDeleteBTnPopUpClick()
        {
            IsDeleteBtnPopUp = false;
            string s = await _ViewBankDetailsViewModel.DeleteVehicle(SelectedBankDetail?.UID);
            if (s.Contains("Failed"))
            {
              //  await _AlertMessgae.ShowErrorAlert("Failed", s);
            }
            else
            {
                //await _AlertMessgae.ShowSuccessAlert("Success", s);
                await _ViewBankDetailsViewModel.PopulateViewModel();
            }
        }

        public void OnViewClick(IBank sku, string Operation )
        {               
            _navigationManager.NavigateTo($"ViewBankMaster?SKUUID={sku.UID}&Operation={Operation}");
        }


        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_bank_details"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["view_bank_details"];
            await CallbackService.InvokeAsync(_IDataService);
        }

        private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if(keyValue.Key== "CountryName")
                    {
                        ISelectionItem ? selectionItem = _ViewBankDetailsViewModel.CountrySelectionItems.Find(x => x.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label , FilterType.Equal));
                    }
                    else if(keyValue.Key == "BankCode")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    
                }
            }
            //(_viewBankDetailsViewModel as ViewBankDetailsWebViewModel)!.FilterCriterias.Clear();
            //(_viewBankDetailsViewModel as ViewBankDetailsWebViewModel)!.FilterCriterias.AddRange(filterCriterias);
            await _ViewBankDetailsViewModel.ApplyFilter(filterCriterias);           
            StateHasChanged();
        }
    }
}
