using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;

namespace WinIt.Pages.CityBranchMapping
{
    public partial class CityBranchMapping
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService { get; set; }
        public required List<DataGridColumn> DataGridColumns { get; set; }
        public  ICityBranchMapping cityBranchMapping { get; set; }
        public bool IsLoaded { get; set; }
        public string UID { get; set; }
        public bool IsEditPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        private Winit.UIComponents.Web.Filter.Filter? FilterRef;
        public List<FilterModel> FilterColumns = new();
        public required List<FilterCriteria> CityBranchFilterCriterials { get; set; }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            try
            {
                _cityBranchMappingViewModel.PageSize = 50;
                await _cityBranchMappingViewModel.PopulateViewModel();
                LoadResources(null, _languageService.SelectedCulture);
                PrepareFilter();
                GenerateGridColumns();
                await SetHeaderName();
                IsLoaded = true;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                HideLoader();
            }
            HideLoader();

        }
        public async Task SetHeaderName()
        {
            dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text= @Localizer["city_branch_mapping"], IsClickable = false, URL= "ViewCityBranchMapping" }
                },
                HeaderText = @Localizer["city_branch_mapping"]
            };
        }
        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
        {
        new() { Header = @Localizer["state"], GetValue = s => ((ICityBranch)s).StateCodeName },
        new() { Header = @Localizer["city"], GetValue = s => ((ICityBranch)s).CityCodeName },
        new() { Header = @Localizer["branch"], GetValue = s => ((ICityBranch)s).BranchCodeName },
        new() {
                Header = @Localizer["action"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new() {
                        Text = @Localizer["map"],
                        ButtonType=ButtonTypes.Text,
                        Action =async item =>await OnEditClick((ICityBranch)item)
                    }
                }
            }
        };
        }
        public async Task OnEditClick(ICityBranch cityBranch)
        {
            UID = cityBranch.UID;
            if (UID != null)
            {
                IsEditPopUp = true;
                await _cityBranchMappingViewModel.PopulatetBranchDetails(UID);
            }
            StateHasChanged();
        }
        private async Task DropDownSelection(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                await _cityBranchMappingViewModel.SelectedBranchInDDL(dropDownEvent);
            }
          
            StateHasChanged();
        }
        private async Task InsertCityBranchMapping()
        {
            await _cityBranchMappingViewModel.InsertCityBranchMapping();
            await _cityBranchMappingViewModel.PopulateViewModel();
            await Task.Delay(500);
            IsEditPopUp = false;
            _tost.Add(@Localizer["branch"], @Localizer["mapping_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        private async Task OnCancelFromBackBTnPopUpClick()
        {
            IsEditPopUp = false;
            StateHasChanged();
        }
        private async Task OnCloseFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = true;
        }
        private async Task OnOkFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = false;
        }
        private void PrepareFilter()
        {
            FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["statecodename"],
                    ColumnName = "statecodename"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["citycodename"],
                    ColumnName = "citycodename"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["branchcodename"],
                    ColumnName = "branchcodename"}
                ,
            });
        }
        private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            ShowLoader("Applying Filter");
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            if (keyValuePairs != null)
            {
                foreach (var item in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                            filterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Like));
                        
                    }
                }
            }
            await _cityBranchMappingViewModel.ApplyFilter(filterCriterias);
            HideLoader();
        }
       
    }
}
