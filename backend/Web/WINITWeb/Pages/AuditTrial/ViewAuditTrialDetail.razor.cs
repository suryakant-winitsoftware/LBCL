using System.Text.Json;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.AuditTrial
{
    public partial class ViewAuditTrialDetail
    {
        private string searchTerm = "";
        public bool IsDataViewRequested = false;
        public bool IsRequestForOldData = false;
        public bool IsPageLoading = true;
        private bool IsTableView = true;
        public string Id { get; set; } = default!;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Audit Trail Detail",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo = 1, Text = "View Trail Detail", IsClickable = true, URL = "viewaudittrial"},
                new BreadCrumModel() {SlNo = 1, Text = "Audit Trail Detail",IsClickable = false }
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                FetchFromQuerySting();
                await _viewModel.GetAuditTrailByIdAndPopulateViewModel(Id, true);

                HideLoader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                IsPageLoading=false;
            }

        }
        public void FetchFromQuerySting()
        {
            var uri = new Uri(NavManager.Uri);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                if (queryParams != null)
                {
                    Id = queryParams.Get("Id")!;
                }
            }
        }
        Dictionary<string, object>? OldData { get; set; }
        private async Task LoadOldRequestData(bool isRequestForOldData)
        {
            IsDataViewRequested=true;
            IsRequestForOldData =isRequestForOldData;
            if (_viewModel.OriginalAuditTrailEntry == null)
            {
                await _viewModel.LoadOldRequestData();
            }
            OldData = IsRequestForOldData
                            ? _viewModel?.OriginalAuditTrailEntry?.NewData as Dictionary<string, object>
                            : _viewModel?.CurrentAuditTrailEntry?.NewData as Dictionary<string, object>;
            var firstKey = OldData?.Keys.FirstOrDefault();
            foreach (var key in OldData?.Keys)
            {
                sectionVisibility[key] = key == firstKey; // First section expanded, others collapsed
            }
        }

        Dictionary<string, object>? ParseJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                return null;
            }
        }

        List<Dictionary<string, object>>? ParseJsonArray(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            }
            catch
            {
                return null;
            }
        }
        private void SearchAmongChangeData(string e)
        {
            searchTerm = e;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _viewModel.FilteredChangeData = new List<ChangeLog>(_viewModel?.ChangeData);
            }
            else
            {
                if (_viewModel?.ChangeData !=null)
                {
                    _viewModel.FilteredChangeData = _viewModel?.ChangeData
                                           .Where(item =>
                                               item.Field.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                           ).ToList();

                }
            }
        }
        //public void SearchStateItemInList(string searchString)
        //{
        //    ShowLoader();
        //    FilteredStateSelectionItems = maintainBranchMappingViewModel.StatesForSelection
        //       .Where(e => string.IsNullOrEmpty(searchString) ||
        //                   e.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
        //                   e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //                   .ToList().OrderByDescending(e => e.IsSelected)
        //                    .ThenBy(s => s.Name)
        //                    .ToList();
        //    IsRenderRequired = true;
        //    StateHasChanged();
        //    HideLoader();
        //}
        private void ToggleRawTableView()
        {
            IsTableView = !IsTableView;
        }
        private void OnClickCloseViewDataPopup()
        {
            IsDataViewRequested = false;
            IsTableView = true;
        }
        //private Dictionary<string, object> FixDictionary(Dictionary<string, object> data)
        //{
        //    var fixedData = new Dictionary<string, object>();

        //    foreach (var kvp in data)
        //    {
        //        if (kvp.Value is string jsonString)
        //        {
        //            // Try parsing JSON string into a proper object
        //            try
        //            {
        //                var parsed = JsonSerializer.Deserialize<object>(jsonString);
        //                fixedData[kvp.Key] = parsed ?? jsonString;
        //            }
        //            catch
        //            {
        //                fixedData[kvp.Key] = jsonString;
        //            }
        //        }
        //        else
        //        {
        //            fixedData[kvp.Key] = kvp.Value;
        //        }
        //    }

        //    return fixedData;
        //}
        private Dictionary<string, bool> sectionVisibility = new Dictionary<string, bool>();

        //protected override void OnInitialized()
        //{
        //    if (oldData != null && oldData.Any())
        //    {
        //        var firstKey = oldData.Keys.FirstOrDefault();
        //        foreach (var key in oldData.Keys)
        //        {
        //            sectionVisibility[key] = key == firstKey; // First section expanded, others collapsed
        //        }
        //    }
        //}

        private void ToggleSection(string key)
        {
            foreach (var k in sectionVisibility.Keys.ToList())
            {
                sectionVisibility[k] = false; // Collapse all sections
            }
            sectionVisibility[key] = true; // Expand only clicked section
        }
    }
}
