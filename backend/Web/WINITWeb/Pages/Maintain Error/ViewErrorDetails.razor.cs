using Microsoft.AspNetCore.Components;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Web.Filter;
using Winit.UIModels.Common.Filter;
using Winit.Shared.Models.Enums;
using Winit.Modules.ErrorHandling.BL.Interfaces;
 using Winit.Modules.ErrorHandling.BL.Classes;
using Winit.Shared.Models.Events;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.Modules.Common.UIState.Classes;
namespace WinIt.Pages.Maintain_Error
{
    public partial class ViewErrorDetails
    {
        private bool IsInitialized { get; set; }
        private Filter? FilterRef;
     //   public List<FilterModel> ColumnsForFilter;
        public List<FilterModel> FilterColumns = new List<FilterModel>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Error Details",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Error Details"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                await SetHeaderName();
                await _ViewErrorDetailsViewModel.PopulateViewModel();
                await _addEditMaintainErrorViewModel.PopulateViewModel(null);
                await GenerateGridColumns();
                IsInitialized = true;
                Filterinitialized();
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
            bool stateRestored = _pageStateHandler.RestoreState("ViewErrorDetails", ref FilterColumns, out PageState pageState);
            if (stateRestored && pageState != null && pageState.SelectedTabUID != null)
            {
                ///only work with filters
                await OnFilterApply(_pageStateHandler._currentFilters);
            }
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ViewErrorDetails");
        }
        public void Filterinitialized()
        {
            FilterColumns = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["error_code"] ,
                    ColumnName = "error_code" },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_addEditMaintainErrorViewModel as AddEditMaintainErrorWebViewModel)!.ServeritySelectionItems ,
                    ColumnName = "Severity", Label =@Localizer["severity"]  },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_addEditMaintainErrorViewModel as AddEditMaintainErrorWebViewModel)!.CategorySelectionItems  ,
                    ColumnName = "Category", Label = @Localizer["category"] },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_addEditMaintainErrorViewModel as AddEditMaintainErrorWebViewModel)!.PlatformSelectionItems ,
                    ColumnName = "Platform", Label = @Localizer["platform"] },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_addEditMaintainErrorViewModel as AddEditMaintainErrorWebViewModel)!.ModuleSelectionItems ,
                    ColumnName = "Module", Label = @Localizer["module"] },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_addEditMaintainErrorViewModel as AddEditMaintainErrorWebViewModel)!.SubModuleSelectionItems ,
                    ColumnName = "sub_module", Label = @Localizer["sub_module"] }
            };

        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_error_details"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["view_error_details"];
            await CallbackService.InvokeAsync(_IDataService);
        }
       
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["error_code"], GetValue = s => ((IErrorDetail)s)?.ErrorCode ?? "N/A" ,IsSortable = true,SortField = "errorcode"},
                new DataGridColumn { Header = @Localizer["severity"], GetValue = s => ((IErrorDetail)s)?.Severity.ToString()?? "N/A" ,IsSortable = true ,SortField = "severity"},
                new DataGridColumn { Header = @Localizer["category"], GetValue = s => ((IErrorDetail)s)?.Category ?? "N/A" ,IsSortable = true ,SortField = "category"},
                new DataGridColumn { Header = @Localizer["platform"], GetValue = s => ((IErrorDetail)s)?.Platform?? "N/A",IsSortable = true ,SortField = "platform"},
                new DataGridColumn { Header = @Localizer["module"], GetValue = s => ((IErrorDetail)s)?.Module ?? "N/A",IsSortable = true ,SortField = "module"},
                new DataGridColumn { Header = @Localizer["sub_module"], GetValue = s => ((IErrorDetail)s)?.SubModule ?? "N/A" ,IsSortable = true ,SortField = "SubModule"},             
                new DataGridColumn
                {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Text,
                        Text = @Localizer["edit"],
                        Action = item => OnEditClick((IErrorDetail)item)
                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Text,
                        Text = @Localizer["description"],
                        Action = item => OnDescriptionClick((IErrorDetail)item)
                    }
                }
                }
             };
        }
        private async Task AddNewError()
        {
            _navigationManager.NavigateTo($"AddEditMaintainError", forceLoad: false);
        }
        private void OnEditClick(IErrorDetail item)
        {
            _navigationManager.NavigateTo($"AddEditMaintainError?ErrorUID={item.UID}");
        }
        private void OnDescriptionClick(IErrorDetail item)
        {
            _navigationManager.NavigateTo($"ErrorDescription?ErrorUID={item.ErrorCode}");
        }

        private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if(keyValue.Key == "Category")
                    {
                        ISelectionItem? selectionItem = _addEditMaintainErrorViewModel.CategorySelectionItems.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                    }
                    else if (keyValue.Key == "Module")
                    {
                        ISelectionItem? selectionItem = _addEditMaintainErrorViewModel.ModuleSelectionItems.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                    }
                    else if (keyValue.Key == "sub_module")
                    {
                        ISelectionItem? selectionItem = _addEditMaintainErrorViewModel.SubModuleSelectionItems.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                    }
                }
            }
            await _ViewErrorDetailsViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _ViewErrorDetailsViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }

    }
}
