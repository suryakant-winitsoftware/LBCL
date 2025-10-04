
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Components;
using System.Dynamic;
using Winit.Shared.Models.Enums;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;
using WinIt.Pages.Base;
using Winit.UIModels.Common.Filter;
using DocumentFormat.OpenXml.Office2010.Excel;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.UIComponents.Web.Store.AddNewStore;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Winit.Shared.Models.Constants;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.Modules.Store.Model.Interfaces;
using System.Data;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Common.UIState.Classes;
using System.Threading.Tasks;


namespace WinIt.Pages.Store
{
    public partial class ManageCustomers : BaseComponentBase
    {

        protected override void OnInitialized()
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            SetColumnHeaders();
            BreadCrumDTO.BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>();
            BreadCrumDTO.HeaderText = @Localizer["maintain_customer"];
            BreadCrumDTO.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_customer"], IsClickable = false, URL = "" });
            //  await CallbackService.InvokeAsync(_IDataService);
            HideLoader();
        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await StateChageHandler();
            HideLoader();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("ManageCustomers", ref ColumnsForFilter, out PageState pageState);
            await OnFilterApply(_pageStateHandler._currentFilters);
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ManageCustomers");
        }
        private async Task OnFilterApply(IDictionary<string, string> filters)
        {
            ShowLoader();
            await _viewModel.OnFilterApply(filters);
            HideLoader();
        }




        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,ColumnName = nameof(IStore.Number), Label =@Localizer["customer_number"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = nameof(IStore.Contact), Label = @Localizer["contact"]},
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = nameof(IStore.Name), Label =@Localizer["customer_name"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,ColumnName= nameof(IStore.SchoolWarehouse),Label = @Localizer["is_school"],
                                   DropDownValues= new() {
                                                            new SelectionItem() { UID="School",Label= @Localizer["yes"],Code="School"},
                                                            new SelectionItem() { UID = "WareHouse", Label = @Localizer["no"], Code = "WareHouse" },
                                                         }
                 },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,ColumnName= nameof(IStore.PriceType),Label = @Localizer["price_type"],DropDownValues=_viewModel.PriceTypeSelectionItems},
             };
        }

        protected void SetColumnHeaders()
        {
            DataGridColumns.AddRange(new List<DataGridColumn>
        {
            new DataGridColumn { Header = @Localizer["customer_code"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = @Localizer["customer_name"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Name, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = @Localizer["customer_arabic_name"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).ArabicName, IsSortable = false, SortField = "Name" },
            new DataGridColumn { Header = @Localizer["customer_number"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Number, IsSortable = false, SortField = "ArabicName" },
            new DataGridColumn { Header = @Localizer["mobile"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s). StoreNumber},
            new DataGridColumn { Header = @Localizer["email"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s). Email??"NA" },
            new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s). IsActive},
            new DataGridColumn { Header = @Localizer["total_outstanding"], GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).TotalOutStandings},
            new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Text,
                        Text =@Localizer["edit_customer"] ,
                        Action = item =>{
                            if(item is Winit.Modules.Store.Model.Classes.Store s)
                             NavigationManager.NavigateTo($"Store?UID={s.UID}&{PageType.Page}={PageType.Edit}");
                            }
                    },
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Text,
                        Text =@Localizer["price"] ,
                        Action = item =>
                        {
                            if(item is Winit.Modules.Store.Model.Classes.Store s)
                                NavigationManager.NavigateTo($"IndividualPrice?UID={s.UID}");
                        }
                    },
                }
            }
        });
        }




        private async Task OnSort(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _viewModel.OnSort(sortCriteria);
            HideLoader();
        }

        private async void OnPageChange(int pageNo)
        {
            _loadingService.ShowLoading();
            await _viewModel.OnPageChange(pageNo);
            _loadingService.HideLoading();
        }

        void OnImport(DataSet dataSet)
        {

        }
    }
}
