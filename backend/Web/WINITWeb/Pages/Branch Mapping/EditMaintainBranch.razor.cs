using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.Modules.Location.Model.Classes;
using WinIt.Pages.Base;
using Winit.Modules.Location.Model.Interfaces;

namespace WinIt.Pages.Branch_Mapping
{
    public partial class EditMaintainBranch : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsErrorPopUp { get; set; } = false;
        public bool IsEdit { get; set; } = false;
        public string? BranchMappingUID { get; set; }
        public List<ILocation> FilteredStateSelectionItems = new List<ILocation>();
        public List<ILocation> FilteredCitySelectionItems = new List<ILocation>();
        public List<ILocation> FilteredLocalitySelectionItems = new List<ILocation>();
        List<string> stateSelected = new List<string>();
        List<string> citySelected = new List<string>();
        List<string> localitySelected = new List<string>();
        List<string> citySelectedToBeRemoved = new List<string>();
        List<string> localitySelectedToBeRemoved = new List<string>();
        private BranchRenderModel render = new BranchRenderModel();
        private bool IsRenderRequired = false;
        public string? ValidationError;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Branch",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel() { SlNo = 1, Text = "Maintain Branch", URL = "MaintainBranch", IsClickable = true },
                new BreadCrumModel() { SlNo = 2, Text = "Edit Branch" },
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            BranchMappingUID = _commonFunctions.GetParameterValueFromURL("BranchMappingUID");
            await maintainBranchMappingViewModel.GetStatesForSelection();
            if (BranchMappingUID != null)
            {
                IsEdit = true;
                await maintainBranchMappingViewModel.PopulateBranchDetailsByUID(BranchMappingUID);
                if (maintainBranchMappingViewModel?.ViewBranchDetails?.Level1Count > 0)
                {
                    stateSelected = maintainBranchMappingViewModel.ViewBranchDetails.Level1Data
                                    .Split(',')
                                    .Select(s => s.Trim())
                                    .ToList();
                }
                else
                {
                    stateSelected = new List<string>(); 
                }
                if (maintainBranchMappingViewModel?.ViewBranchDetails?.Level2Count > 0)
                {
                    citySelected = maintainBranchMappingViewModel.ViewBranchDetails.Level2Data
                                    .Split(',')
                                    .Select(s => s.Trim())
                                    .ToList();
                }
                else
                {
                    citySelected = new List<string>();
                }
                if (maintainBranchMappingViewModel?.ViewBranchDetails?.Level3Count > 0)
                {
                    localitySelected = maintainBranchMappingViewModel.ViewBranchDetails.Level3Data
                                    .Split(',')
                                    .Select(s => s.Trim())
                                    .ToList();
                }
                else
                {
                    localitySelected = new List<string>();
                }
                await PopulateLocationData();
            }
            else
            {
                maintainBranchMappingViewModel.ViewBranchDetails = new Branch();
            }
            await OnStatesInfoClick();
            IsInitialised = true;
            HideLoader();
        }

        private async Task PopulateLocationData()
        {
            if (stateSelected != null)
            {
                foreach (var stateItem in maintainBranchMappingViewModel.StatesForSelection)
                {
                    stateItem.IsSelected = stateSelected.Contains(stateItem.Code);
                }
                FilteredStateSelectionItems = maintainBranchMappingViewModel.StatesForSelection
                    .OrderByDescending(state => state.IsSelected)
                    .ToList();
            }
            if (citySelected != null)
            {
                await maintainBranchMappingViewModel.GetCitiesForSelection(maintainBranchMappingViewModel.StatesForSelection.Where(item => item.IsSelected).ToList());
                foreach (var cityItem in maintainBranchMappingViewModel.CitiesForSelection)
                {
                    cityItem.IsSelected = citySelected.Contains(cityItem.Code);
                }
                FilteredCitySelectionItems = maintainBranchMappingViewModel.CitiesForSelection
                    .OrderByDescending(state => state.IsSelected)
                    .ToList();
            }
            if (localitySelected != null)
            {
                await maintainBranchMappingViewModel.GetLocalitiesForSelection(maintainBranchMappingViewModel.CitiesForSelection.Where(item => item.IsSelected).ToList());
                foreach (var localityItem in maintainBranchMappingViewModel.LocalitiesForSelection)
                {
                    localityItem.IsSelected = localitySelected.Contains(localityItem.Code);
                }
                FilteredLocalitySelectionItems = maintainBranchMappingViewModel.LocalitiesForSelection
                    .OrderByDescending(locality => locality.IsSelected)
                    .ToList();
            }
        }

        public async Task OnStatesInfoClick()
        {
            ShowLoader();
            await maintainBranchMappingViewModel.GetStatesForSelection();
            if (IsAnyItemSelected(maintainBranchMappingViewModel.LocalitiesForSelection))
            {
                localitySelected = maintainBranchMappingViewModel.LocalitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (IsAnyItemSelected(maintainBranchMappingViewModel.CitiesForSelection))
            {
                citySelected = maintainBranchMappingViewModel.CitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (stateSelected != null)
            {
                foreach (var stateItem in maintainBranchMappingViewModel.StatesForSelection)
                {
                    stateItem.IsSelected = stateSelected.Contains(stateItem.Code);
                }
                FilteredStateSelectionItems = maintainBranchMappingViewModel.StatesForSelection
                    .OrderByDescending(state => state.IsSelected)
                    .ToList();
            }
            render.IsStatesInformationRendered = true;
            render.IsCitiesInformationRendered = false;
            render.IsLocalitiesInformationRendered = false;
            HideLoader();
        }
        public async Task OnCitiesInfoClick()
        {
            ShowLoader();
            if (IsAnyItemSelected(maintainBranchMappingViewModel.LocalitiesForSelection))
            {
                localitySelected = maintainBranchMappingViewModel.LocalitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (IsAnyItemSelected(maintainBranchMappingViewModel.StatesForSelection))
            {
                stateSelected = maintainBranchMappingViewModel.StatesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
                await maintainBranchMappingViewModel.GetCitiesForSelection(maintainBranchMappingViewModel.StatesForSelection.Where(item => item.IsSelected).ToList());
                if (citySelected != null)
                {
                    foreach (var cityItem in maintainBranchMappingViewModel.CitiesForSelection)
                    {
                        cityItem.IsSelected = citySelected.Contains(cityItem.Code);
                    }
                    FilteredCitySelectionItems = maintainBranchMappingViewModel.CitiesForSelection
                        .OrderByDescending(state => state.IsSelected)
                        .ToList();
                }
                render.IsStatesInformationRendered = false;
                render.IsCitiesInformationRendered = true;
                render.IsLocalitiesInformationRendered = false;
            }
            else
            {
                ShowErrorSnackBar("Alert", "Select at least one State to select Cities");
            }
            HideLoader();
        }
        public async Task OnLocalitiesInfoClick()
        {
            ShowLoader();
            if (IsAnyItemSelected(maintainBranchMappingViewModel.StatesForSelection))
            {
                stateSelected = maintainBranchMappingViewModel.StatesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (IsAnyItemSelected(maintainBranchMappingViewModel.CitiesForSelection))
            {
                citySelected = maintainBranchMappingViewModel.CitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
                await maintainBranchMappingViewModel.GetLocalitiesForSelection(maintainBranchMappingViewModel.CitiesForSelection.Where(item => item.IsSelected).ToList());
                if (localitySelected != null)
                {
                    foreach (var localityItem in maintainBranchMappingViewModel.LocalitiesForSelection)
                    {
                        localityItem.IsSelected = localitySelected.Contains(localityItem.Code);
                    }
                    FilteredLocalitySelectionItems = maintainBranchMappingViewModel.LocalitiesForSelection
                        .OrderByDescending(locality => locality.IsSelected)
                        .ToList();
                }
                render.IsStatesInformationRendered = false;
                render.IsCitiesInformationRendered = false;
                render.IsLocalitiesInformationRendered = true;
            }
            else
            {
                ShowErrorSnackBar("Alert", "Select at least one City to select Locaties");
            }
            HideLoader();
        }
        private bool IsAnyItemSelected(List<ILocation> items)
        {
            return items.Any(item => item.IsSelected);
        }
        public void SearchStateItemInList(string searchString)
        {
            ShowLoader();
            FilteredStateSelectionItems = maintainBranchMappingViewModel.StatesForSelection
               .Where(e => string.IsNullOrEmpty(searchString) ||
                           e.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                           e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                           .ToList().OrderByDescending(e => e.IsSelected)
                            .ThenBy(s => s.Name)
                            .ToList();
            IsRenderRequired = true;
            StateHasChanged();
            HideLoader();
        }
        public void SearchCityItemInList(string searchString)
        {
            ShowLoader();
            FilteredCitySelectionItems = maintainBranchMappingViewModel.CitiesForSelection
                .Where(e => string.IsNullOrEmpty(searchString) ||
                            e.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                            e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                            .ToList().OrderByDescending(e => e.IsSelected)
                            .ThenBy(s => s.Name)
                            .ToList();
            IsRenderRequired = true;
            StateHasChanged();
            HideLoader();
        }
        public void SearchLocalityItemInList(string searchString)
        {
            ShowLoader();
            FilteredLocalitySelectionItems = maintainBranchMappingViewModel.LocalitiesForSelection
                .Where(e => string.IsNullOrEmpty(searchString) ||
                            e.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                            e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                            .ToList().OrderByDescending(e => e.IsSelected)  
                            .ThenBy(s => s.Name)                  
                            .ToList();
            IsRenderRequired = true;
            StateHasChanged();
            HideLoader();
        }
        private void OnItemCheckedChanged(ILocation state)
        {
            state.IsSelected = !state.IsSelected;
            IsRenderRequired = true;
        }
        protected override void OnAfterRender(bool firstRender)
        {
            if (IsRenderRequired)
            {
                SortFilteredStateSelectionItems();
                SortFilteredCitySelectionItems();
                SortFilteredLocalitySelectionItems();
                IsRenderRequired = false;
            }
        }
        private void SortFilteredStateSelectionItems()
        {
            if (FilteredStateSelectionItems == null || !FilteredStateSelectionItems.Any()) return; 
            FilteredStateSelectionItems = FilteredStateSelectionItems
                .OrderByDescending(e => e.IsSelected)
                .ThenBy(s => s.Name)
                .ToList();
            StateHasChanged();
        }
        private void SortFilteredCitySelectionItems()
        {
            if (FilteredCitySelectionItems == null || !FilteredCitySelectionItems.Any()) return;
            FilteredCitySelectionItems = FilteredCitySelectionItems
                .OrderByDescending(e => e.IsSelected)
                .ThenBy(s => s.Name)
                .ToList();
            StateHasChanged();
        }
        private void SortFilteredLocalitySelectionItems()
        {
            if (FilteredLocalitySelectionItems == null || !FilteredLocalitySelectionItems.Any()) return;
            FilteredLocalitySelectionItems = FilteredLocalitySelectionItems
                .OrderByDescending(e => e.IsSelected)
                .ThenBy(s => s.Name)
                .ToList();
            StateHasChanged();
        }       
        public void BackBtnClicked()
        {
            ShowLoader();
            _navigationManager.NavigateTo($"MaintainBranch");
            HideLoader();
        }
        public async Task SaveBranchDetails()
        {
            if (IsAnyItemSelected(maintainBranchMappingViewModel.StatesForSelection))
            {
                stateSelected = maintainBranchMappingViewModel.StatesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (IsAnyItemSelected(maintainBranchMappingViewModel.CitiesForSelection))
            {
                citySelected = maintainBranchMappingViewModel.CitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            if (IsAnyItemSelected(maintainBranchMappingViewModel.LocalitiesForSelection))
            {
                localitySelected = maintainBranchMappingViewModel.LocalitiesForSelection.Where(item => item.IsSelected).Select(item => item.Code).ToList();
            }
            await maintainBranchMappingViewModel.PopulateViewModel();
            ValidationError = "";
            ShowLoader();
            if (string.IsNullOrWhiteSpace(maintainBranchMappingViewModel.ViewBranchDetails.Code))
            {
                ValidationError += "Code Should not be Null or Empty,Null or WhiteSpaces.";
            }
            if (string.IsNullOrWhiteSpace(maintainBranchMappingViewModel.ViewBranchDetails.Name))
            {
                ValidationError += "Name Should not be Null or Empty,Null or WhiteSpaces.";
            }
            //if (stateSelected.Count < 1)
            //{
            //    ValidationError += "You must select at least one State.";
            //}
            //if (citySelected.Count < 1)
            //{
            //    ValidationError += "You must select at least one City.";
            //}
            //if (localitySelected.Count < 1)
            //{
            //    ValidationError += "You must select at least one Locality.";
            //}
            if (ValidationError.Length > 0)
            {
                ShowErrorSnackBar(" Error ", ValidationError);
            }
            else
            {
                if (CheckCode(maintainBranchMappingViewModel.ViewBranchDetails.Code))
                {
                    if (DataValidated())
                    {
                        maintainBranchMappingViewModel.ViewBranchDetails.Level1Data = string.Join(",", stateSelected);
                        maintainBranchMappingViewModel.ViewBranchDetails.Level1Count = stateSelected.Count;
                        maintainBranchMappingViewModel.ViewBranchDetails.Level2Data = string.Join(",", citySelected);
                        maintainBranchMappingViewModel.ViewBranchDetails.Level2Count = citySelected.Count;
                        maintainBranchMappingViewModel.ViewBranchDetails.Level3Data = string.Join(",", localitySelected);
                        maintainBranchMappingViewModel.ViewBranchDetails.Level3Count = localitySelected.Count;
                        if (await maintainBranchMappingViewModel.SaveOrUpdateBranchDetails(maintainBranchMappingViewModel.ViewBranchDetails, IsEdit))
                        {
                            ShowSuccessSnackBar("Success", "Data saved successfully.");
                            _navigationManager.NavigateTo($"MaintainBranch");
                        }
                        else
                        {
                            ShowErrorSnackBar("Failed", "Error In Saving");
                        }
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Code should be Unique");
                }
            }
            HideLoader();
        }
        private bool CheckCode(string code)
        {
            if (IsEdit)
            {
                int occurrenceCount = maintainBranchMappingViewModel.BranchDetailsList
                 .Count(branch => string.Equals(branch.Code, code, StringComparison.OrdinalIgnoreCase));
                return (occurrenceCount == 1);
            }
            else
            {
                int occurrenceCount = maintainBranchMappingViewModel.BranchDetailsList
                 .Count(branch => string.Equals(branch.Code, code, StringComparison.OrdinalIgnoreCase));
                return (occurrenceCount == 0);
            }

        }
        private bool DataValidated()
        {
            ValidationError = "";
            var selectedStateUIDs = maintainBranchMappingViewModel.StatesForSelection.Where(state => state.IsSelected).Select(state => state.UID).ToHashSet();
            var selectedCityUIDs = maintainBranchMappingViewModel.CitiesForSelection.Where(city => city.IsSelected).Select(city => city.UID).ToHashSet();
            var selectedLocalities = maintainBranchMappingViewModel.LocalitiesForSelection.Where(locality => locality.IsSelected).Select(locality => locality.UID).ToHashSet();

            var SelectedCityParentUIDs = maintainBranchMappingViewModel.StatesForSelection.Where(city => city.IsSelected).Select(city => city.ParentUID).ToHashSet();
            var selectedLocalityParentUIDs = maintainBranchMappingViewModel.LocalitiesForSelection.Where(locality => locality.IsSelected).Select(locality => locality.ParentUID).ToHashSet();
            
            foreach (var locality in maintainBranchMappingViewModel.LocalitiesForSelection.Where(locality => locality.IsSelected))
            {
                var parentLocalityUID = locality.ParentUID;
                if (!selectedCityUIDs.Contains(parentLocalityUID))
                {
                    ValidationError += locality.Name + " ";
                    localitySelectedToBeRemoved.Add(locality.Code);
                }
                else
                {
                    var parentCity = maintainBranchMappingViewModel.CitiesForSelection.First(city => city.UID == parentLocalityUID);
                    if (!selectedStateUIDs.Contains(parentCity.ParentUID))
                    {
                        ValidationError += parentCity.Name + " ";
                        citySelectedToBeRemoved.Add(parentCity.Code);
                    }
                }
            }
            foreach (var city in maintainBranchMappingViewModel.CitiesForSelection.Where(city => city.IsSelected))
            {
                var parentStateUID = city.ParentUID;
                if (!selectedStateUIDs.Contains(parentStateUID))
                {
                    ValidationError += city.Name + " ";
                    citySelectedToBeRemoved.Add(city.Code);
                }
            }

            if (ValidationError.Length > 0)
            {
                string Message = "The Following Parent's Items are Not Selected :";
                ValidationError = Message + ValidationError + ". Do you want to remove them from Saving ? ";
                IsErrorPopUp = true;
                StateHasChanged();
                return false;
            }
            return true;
        }
        public void OnOkBTnPopUpClick()
        {
            ShowLoader();
            if (localitySelectedToBeRemoved.Count > 0)
            {
                localitySelected.RemoveAll(locality => localitySelectedToBeRemoved.Contains(locality));

                foreach (var item in maintainBranchMappingViewModel.LocalitiesForSelection)
                {
                    if (localitySelectedToBeRemoved.Contains(item.Code))
                    {
                        item.IsSelected = false;
                    }
                }
            }
            if (citySelectedToBeRemoved.Count > 0)
            {
                citySelected.RemoveAll(city => citySelectedToBeRemoved.Contains(city));

                foreach (var item in maintainBranchMappingViewModel.CitiesForSelection)
                {
                    if (citySelectedToBeRemoved.Contains(item.Code))
                    {
                        item.IsSelected = false;
                    }
                }
            }
            IsErrorPopUp = false;
            StateHasChanged();
            SaveBranchDetails();
            HideLoader();
        }
    }
}
