using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Common.Filter;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Azure;
using Winit.Modules.BroadClassification.BL.Interfaces;
using WinIt.Pages.Base;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.Common.UIState.Classes;

namespace WinIt.Pages.Maintain_BroadClassification
{
    public partial class MaintainBroadClassification : BaseComponentBase
    {
        private bool IsInitialized { get; set; }
        public List<FilterModel> FilterColumns = new List<FilterModel>();
        public List<IBroadClassificationHeader> broadClassificationHeaders { get; set; }
        public List<IBroadClassificationLine> broadClassificationLines { get; set; }
        public List<IListItem> CustomerClassifications { get; set; }
        public List<ISelectionItem> CustomerHeaderClassificationNameForDropDown { get; set; } = new List<ISelectionItem>();
        public bool IsAddOrEdit { get; set; } = false;
        public int Total_Customer_Classification = 0;
        public int Assigned_Customer_Classification = 0;
        public int pending_To_Be_Assigned_Classification = 0;
        private List<ISelectionItem> StatusSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem{UID="1",Code="1",Label="Active"},
            new SelectionItem{UID="0",Code="0",Label="Inactive"},
        };
        public List<DataGridColumn> DataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Broad Classification",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Broad Classification"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _broadClassificationHeaderBL.PopulateViewModel();
            await _broadClassificationLineBL.PopulateViewModel();
            broadClassificationHeaders = _broadClassificationHeaderBL.broadClassificationHeaderslist;
            broadClassificationLines = _broadClassificationLineBL.broadClassificationLinelist;
            CustomerClassifications = _broadClassificationHeaderBL.ClassificationTypes;
            Total_Customer_Classification = CustomerClassifications.Count();
            Assigned_Customer_Classification = broadClassificationLines.Count();
            pending_To_Be_Assigned_Classification = Total_Customer_Classification - Assigned_Customer_Classification;
            await GenerateGridColumns();
            FilterInitialised();
            IsInitialized = true;
            await StateChageHandler();
            HideLoader();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("MaintainBroadClassification", ref FilterColumns, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainBroadClassification");
        }

        private void FilterInitialised()
        {
            CustomerHeaderClassificationNameForDropDown.Clear();

            foreach (var item in broadClassificationHeaders)
            {
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    var selectionItem = new SelectionItem
                    {
                        UID = item.UID,
                        Label = item.Name,
                        Code = item.Name,
                    };

                    CustomerHeaderClassificationNameForDropDown.Add(selectionItem);
                }
            }
            FilterColumns = new List<FilterModel>
            {

                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues = CustomerHeaderClassificationNameForDropDown,
                     ColumnName = nameof(IBroadClassificationHeader.Name),
                     Label = "Classification Name",
                     SelectionMode = SelectionMode.Multiple

                 },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     Label = "status",DropDownValues=StatusSelectionItems,
                     ColumnName=nameof(IBroadClassificationHeader.IsActive),SelectionMode=SelectionMode.Single},

            };
        }
        public async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    string[] vals = keyValue.Value.Split(',');
                    if (keyValue.Key == nameof(IBroadClassificationHeader.Name))
                    {
                        var header = _broadClassificationHeaderBL.broadClassificationHeaderslist
                                    .FindAll(e => vals.Contains(e.UID));
                        if (header != null)
                        {
                            filterCriterias.Add(new FilterCriteria(keyValue.Key, header.Select(p => p.Name), FilterType.In));
                        }
                    }
                    else if (keyValue.Key == nameof(IBroadClassificationHeader.IsActive))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", vals, FilterType.In));
                        }
                        else
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                        }
                    }
                }
            }
            await _broadClassificationHeaderBL.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        private ISelectionItem ConvertToSelectionItem(IBroadClassificationHeader header)
        {
            return new SelectionItem
            {
                UID = header.UID,
                Label = header.Name
            };
        }
        async Task OnSort(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _broadClassificationHeaderBL.OnSort(sortCriteria);
            HideLoader();
            StateHasChanged();
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn
                {
                    Header = "Broad Classification Name",
                    GetValue = item => ((IBroadClassificationHeader)item).Name ?? "N/A",IsSortable=true,SortField=nameof(IBroadClassificationHeader.Name),
                },
                new DataGridColumn
                {
                    Header = "Classification Count",
                    GetValue = item => ((IBroadClassificationHeader)item).ClassificationCount.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "Active",
                    GetValue = item => ((IBroadClassificationHeader)item).IsActive.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "Customer Classification",
                    GetValue = item =>
                    {
                        var header = (IBroadClassificationHeader)item;
                        var classifications = broadClassificationLines
                            .Where(line => line.BroadClassificationHeaderUID == header.UID)
                            .Select(line => line.ClassificationCode)
                            .Where(code => !string.IsNullOrWhiteSpace(code)); // Filter out null, empty, and whitespace codes

                        // If there are valid classifications, return them as a string; otherwise, return null
                        var result = string.Join(", ", classifications);
                        return string.IsNullOrEmpty(result) ? null : result;
                    }
                },
                new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/edit.png",
                            Action = item => OnEditClick((IBroadClassificationHeader)item )
                        }
                    }
                }
            };
        }

        private void OnEditClick(IBroadClassificationHeader item)
        {
            _navigationManager.NavigateTo($"EditMaintainBroadClassification?BCUID={item.UID}");
        }
    }
}
