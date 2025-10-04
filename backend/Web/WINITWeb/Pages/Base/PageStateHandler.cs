using Microsoft.AspNetCore.Components;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common.Filter;

namespace WinIt.Pages.Base
{
    public class PageStateHandler
    {
        Winit.Modules.Common.UIState.Classes.BaseModuleState _stateService;
        Winit.Modules.Common.UIState.Classes.NavigationHistoryService _navigationhistory;
        public Dictionary<string, string> _currentFilters = new Dictionary<string, string>();
        NavigationManager _navigationManager;
        public PageStateHandler(BaseModuleState stateService, NavigationHistoryService navigationhistory, NavigationManager navigationManager)
        {
            _stateService = stateService;
            _navigationhistory = navigationhistory;
            _navigationManager = navigationManager;
        }

        public void SaveCurrentState(string routeName, string selectedTabUID = null)
        {
            // Check if we're navigating away from our page
            string currentUrl = _navigationManager.Uri;
            var uri = new Uri(currentUrl);

            // Get only the absolute path and split by slashes, trim leading/trailing
            var path = uri.AbsolutePath.Trim('/');
            var currentRoute = path.Split('/').FirstOrDefault(); // Get the first segment only
            if (!currentRoute!.Equals(routeName, StringComparison.OrdinalIgnoreCase))
            {
                var state = new Winit.Modules.Common.UIState.Classes.PageState
                {
                    Filters = _currentFilters,
                    SelectedTabUID = selectedTabUID
                };

                _stateService.SavePageState(routeName, state);
            }
        }
        public bool RestoreState(string currentRoute, ref List<FilterModel> FilterColumns, out PageState pageState)
        {
            pageState = _stateService.GetPageState(currentRoute);
            if (string.IsNullOrEmpty(_navigationhistory.PreviousUrl))
            {
                _stateService.ClearPageState("referrer");
                return false;
            }

            // Check if any of the acceptable previous pages is contained in the PreviousUrl
            bool isFromAcceptablePage = _stateService.GetAcceptablePreviousPages()
                .Any(page => _navigationhistory.PreviousUrl.Contains(page, StringComparison.OrdinalIgnoreCase));

            if (!isFromAcceptablePage)
            {
                _stateService.ClearPageState("referrer");
                return false;
            }
            if (pageState != null)
            {
                // Restore filters
                if (pageState.Filters != null && pageState.Filters.Count > 0)
                {
                    _currentFilters = pageState.Filters;

                    BindFiltersToUI(_currentFilters, ref FilterColumns);
                    // Apply the saved filters
                }

                // Restore selected tab
                //if (!string.IsNullOrEmpty(pageState.SelectedTabUID))
                //{
                //    var selectedTab = TabSelectionItems.FirstOrDefault(t => t.UID == savedState.SelectedTabUID);
                //    if (selectedTab != null)
                //    {
                //        _ = OnTabSelect(selectedTab);
                //    }
                //}

                // Clear state after restoring
                _stateService.ClearPageState(currentRoute);
                _stateService.ClearPageState("referrer");
                return true;
            }
            return false;
        }
        private void BindFiltersToUI(Dictionary<string, string> filters, ref List<FilterModel> FilterColumns)
        {
            foreach (var filter in FilterColumns)
            {
                if (filters.TryGetValue(filter.ColumnName, out var value))
                {
                    if (filter.FilterType == FilterConst.TextBox || filter.FilterType == FilterConst.Date )
                    {
                        filter.SelectedValue = value;
                    }
                    else if (filter.FilterType == FilterConst.CheckBox)
                    {
                        filter.SelectedBoolValue = Convert.ToBoolean(value);
                    }
                    else if (filter.FilterType == FilterConst.DropDown)
                    {
                        var selectedValues = value.Split(',').ToList();

                        // Match the selected values with the actual dropdown items
                        //var matchedItems = filter.DropDownValues
                        //    .Where(item => selectedValues.Contains(item.UID))
                        //    .Cast<ISelectionItem>()
                        //    .ToList();
                        filter.SelectedValues = [];
                        filter.DropDownValues.ForEach(p =>
                        {
                            if (selectedValues.Contains(p.UID) || selectedValues.Contains(p.Code))
                            {
                                p.IsSelected = true;
                                filter.SelectedValues.Add(p);
                            }
                        });

                        //_viewModel.ChannelPartnerSelectionItems.ForEach(p =>
                        //{
                        //    p.IsSelected = filter.SelectedValues.Equals(QtyCaptureMode=);
                        //});
                    }

                }
            }
        }
    }
}
