using Microsoft.AspNetCore.Components;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common.Filter;
using Winit.UIComponents.Web.Filter;
using Winit.Shared.Models.Enums;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Resources;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.Common.UIState.Classes;


namespace WinIt.Pages.Maintain_Currency
{
    public partial class MaintainCurrency: WinIt.Pages.Base.BaseComponentBase
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        public string Operation;
        private Filter? FilterRef;
        public List<FilterModel> FilterColumns = new List<FilterModel>();
        public ICurrency? SelectedCurrencyDetail { get; set; }
        private bool IsDeleteBtnPopUp { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Currency",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Currency"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                await _imaintainCurrencyViewModel.PopulateViewModel();
                await GenerateGridColumns();
                IsLoaded = true;
                //await SetHeaderName();
                FilterColumns.AddRange(new List<FilterModel> {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code"],
                    ColumnName = "Code" },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["name"],
                    ColumnName = "Name" }
                 //new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                 //   DropDownValues=(_imaintainCurrencyViewModel as ViewBankDetailsWebViewModel).CountrySelectionItems ,
                 //   ColumnName = "CountryName", Label = "Country Name" },
                 //new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                 //   DropDownValues = OrderTypeSelectionItems, ColumnName = "CountryUID", Label = "Country"}
                });
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
            bool stateRestored = _pageStateHandler.RestoreState("MaintainCurrency", ref FilterColumns, out PageState pageState);
            if (stateRestored && pageState != null && pageState.SelectedTabUID != null)
            {
                ///only work with filters
                await OnFilterApply(_pageStateHandler._currentFilters);
            }
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainCurrency");
        }

        public void FilterInitialized()
        {
            FilterColumns = new List<FilterModel>
            {
             new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code"],
                    ColumnName = "Code" },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["name"],
                    ColumnName = "Name" }
        };
        }
        private async Task AddCurrencyDetails()
        {
            _navigationManager.NavigateTo($"AddEditCurrency");
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((ICurrency)s)?.Code?? "N/A" },
                new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((ICurrency)s)?.Name?? "N/A" },
                new DataGridColumn { Header = @Localizer["symbol"], GetValue = s => ((ICurrency)s)?.Symbol ?? "N/A" },
                new DataGridColumn { Header = @Localizer["no_of_decimals"],  GetValue = s => ((ICurrency)s)?.Digits ?? 0},
                new DataGridColumn { Header = @Localizer["fraction_name"],  GetValue = s => ((ICurrency)s)?.FractionName ?? "N/A" },
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
                    //    Action = item => OnViewClick((ICurrency)item, "View")
                    //},
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = item => OnViewClick((ICurrency)item, "Edit")
                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                        Action = item => OnDeleteClick((ICurrency)item, "Edit")
                    }
                }
            }
             };
        }
        private void OnDeleteClick(ICurrency item, string v)
        {
            SelectedCurrencyDetail = item;
            IsDeleteBtnPopUp = true;
            StateHasChanged();
        }
        public async Task OnOkFromDeleteBTnPopUpClick()
        {
            IsDeleteBtnPopUp = false;
            string s = await _imaintainCurrencyViewModel.DeleteCurrency(SelectedCurrencyDetail?.UID);
            if (s.Contains("Failed"))
            {
                //  await _AlertMessgae.ShowErrorAlert("Failed", s);
            }
            else
            {
                //await _AlertMessgae.ShowSuccessAlert("Success", s);
                await _imaintainCurrencyViewModel.PopulateViewModel();
            }
        }
        public void OnViewClick(ICurrency sku, string Operation)
        {
            _navigationManager.NavigateTo($"AddEditCurrency?SKUUID={sku.UID}&Operation={Operation}");
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_currency_details"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["view_currency_details"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "Name")
                    {
                       // ISelectionItem? selectionItem = _imaintainCurrencyViewModel.DistributorSelectionList.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));

                    }
                }
            }
            //(_imaintainCurrencyViewModel as ViewBankDetailsWebViewModel)!.FilterCriterias.Clear();
            //(_imaintainCurrencyViewModel as ViewBankDetailsWebViewModel)!.FilterCriterias.AddRange(filterCriterias);
            await _imaintainCurrencyViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
    }
}
