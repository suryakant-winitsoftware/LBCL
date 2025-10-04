using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Common.UIState.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.AuditTrial
{
    public partial class ViewAuditTrial
    {
        #region Parameter Section
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Audit Trail",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel() {SlNo = 1, Text = "View Audit Trail",IsClickable = false }
            }
        };
        public bool IsPageLoading = true;
        public List<DataGridColumn>? DataGridColumns { get; set; }
        #endregion

        #region InitialRender
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                await _viewModel.GetAuditTrailAsync();
                GenerateGridColumns();
                await FilterInitialization();
                await StateChageHandler();
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsPageLoading = false;
            }

        }
        #endregion
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("viewaudittrial", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("viewaudittrial");
        }
        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn {Header = "Module", GetValue = s => ((IAuditTrailEntry)s)?.LinkedItemType ?? "N/A",IsSortable=true,SortField="LinkedItemType"},
                new DataGridColumn {Header = "Doc No", GetValue = s => ((IAuditTrailEntry)s)?.DocNo ?? "N/A",IsSortable=true,SortField="DocNo"},
                //new DataGridColumn {Header = "Command", GetValue = s => ((IAuditTrailEntry)s)?.CommandType ?? "N/A"},
                new DataGridColumn {Header = "Created On", GetValue = s => CommonFunctions.GetDateTimeInFormat(((IAuditTrailEntry)s)?.ServerCommandDate,"dd MMM, yyyy HH:mm:ss"),IsSortable=true,SortField="ServerCommandDate"},
                new DataGridColumn {Header = "Emp Name", GetValue = s => ((IAuditTrailEntry)s)?.EmpName?? "N/A",IsSortable=true,SortField="EmpName"},
                new DataGridColumn {Header = "Has Changes", GetValue = s => ((IAuditTrailEntry)s)?.HasChanges,IsSortable=true,SortField="HasChanges"},

                new DataGridColumn
                {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/view.png",
                        Action = item => OnViewClick((IAuditTrailEntry)item)

                    }
                }
            }
             };
        }
        public void OnViewClick(IAuditTrailEntry auditTrailEntry)
        {
            var id = auditTrailEntry.Id;
            NavigationManager.NavigateTo($"viewaudittraildetail?Id={id}");
        }
        public List<FilterModel>? ColumnsForFilter;
        public async Task FilterInitialization()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=await _viewModel.GetModuleDropdownValuesAsync(),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IAuditTrailEntry.LinkedItemType),
                     Label = "Module"
                 },
                  new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName = nameof(IAuditTrailEntry.DocNo),
                     Label = "Doc No"
                 },
                   new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=new List<ISelectionItem>()
                     {
                       new SelectionItem { UID = "create", Code = "create", Label = "create"},
                       new SelectionItem { UID = "update", Code = "update", Label = "update"}
                     },
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IAuditTrailEntry.CommandType),
                     Label = "Command Type"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = AuditTrailConst.StartDate,
                     Label = "Start Date",
                     IsDefaultValueNeeded=true,
                     SelectedValue = DateTime.Now.ToString("dd/MM/yyyy"),
                     DefaultValue=DateTime.Now.ToString("dd/MM/yyyy")
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = AuditTrailConst.EndDate,
                     Label = "End Date",
                     IsDefaultValueNeeded=true,
                     SelectedValue = DateTime.Now.ToString("dd/MM/yyyy"),
                     DefaultValue=DateTime.Now.ToString("dd/MM/yyyy")
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName = nameof(IAuditTrailEntry.EmpName),
                     Label = "Emp Name"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=new List<ISelectionItem>()
                     {
                       new SelectionItem { UID = "true", Code = "true", Label = "Yes"},
                       new SelectionItem { UID = "false", Code = "false", Label = "No"}
                     },
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IAuditTrailEntry.HasChanges),
                     Label = "Has Changes"
                 },

             };

        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            if (ValidateDateRange(filterCriteria))
            {
                _pageStateHandler._currentFilters = filterCriteria;
                _loadingService.ShowLoading();
                _viewModel.PageNumber = 1;
                await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
                _loadingService.HideLoading();
            }
            else
            {
                await _AlertMessgae.ShowErrorAlert("Alert", "Date range should not exceed 31 days");
            }

        }

        public bool ValidateDateRange(Dictionary<string, string> filterCriteria)
        {
            if (filterCriteria.TryGetValue("StartDate", out string? startDateStr) &&
                filterCriteria.TryGetValue("EndDate", out string? endDateStr) &&
                DateTime.TryParse(startDateStr, out DateTime startDate) &&
                DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return (endDate - startDate).Days <= 31;
            }

            return false; // Return false if parsing fails or dates are missing
        }

        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _viewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}
