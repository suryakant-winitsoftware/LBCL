using Microsoft.AspNetCore.Components;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Common.UIState.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.ManageApproval
{
    public partial class ViewAllChangeRequestApproval
    {
        public List<DataGridColumn>? DataGridColumns { get; set; }
        public bool IsPageLoading = true;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View All Request Change Approval",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo = 1, Text = "View all request change approval", IsClickable = false}
            }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                await _ApprovalEngine.GetAllChangeRequestDataAsync();
                GenerateGridColumns();
                HideLoader();
                FilterInitialization();
                await StateChageHandler();
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", ex.Message);
            }

            finally
            {
                IsPageLoading=false;
            }

        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("viewallchangerequestapprovalinfo", ref ColumnsForFilter, out PageState pageState);
           
                ///only work with filters
                await OnFilterApply(_pageStateHandler._currentFilters);
            
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("viewallchangerequestapprovalinfo");
        }

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn {Header = "Request Type", GetValue = s => ((IViewChangeRequestApproval)s)?.LinkedItemType ?? "N/A"},
                new DataGridColumn {Header = "Operation", GetValue = s => ((IViewChangeRequestApproval)s)?.OperationType ?? "N/A"},
                new DataGridColumn {Header = "Reference", GetValue = s => ((IViewChangeRequestApproval)s)?.Reference ?? "N/A"},
                new DataGridColumn {Header = "Requested By", GetValue = s => ((IViewChangeRequestApproval)s)?.RequestedBy ?? "N/A"},
                new DataGridColumn {Header = "Requested Date", GetValue = s => ((IViewChangeRequestApproval)s)?.RequestDate!},
                new DataGridColumn {Header = "Last Update Date", GetValue = s => ((IViewChangeRequestApproval)s)?.ApprovedDate?.ToString() ?? "N/A" },
                new DataGridColumn {Header = "Status", GetValue = s => ((IViewChangeRequestApproval)s)?.Status!},
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
                        Action = item => OnEditClick((IViewChangeRequestApproval)item)

                    }
                }
            }
             };
        }
        public void OnEditClick(IViewChangeRequestApproval viewChangeRequestApproval)
        {
            var uid = viewChangeRequestApproval.UID;
            NavigationManager.NavigateTo($"changerequestapprovalinfo?UID={uid}");
        }
        #region FilterLogic

        public List<FilterModel>? ColumnsForFilter;
        public void FilterInitialization()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                  new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = nameof(IViewChangeRequestApproval.RequestDate),
                     Label = "Requested Date"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = nameof(IViewChangeRequestApproval.ApprovedDate),
                     Label = "Last Modified Date"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=new List<ISelectionItem>()
                     {
                       new SelectionItem { UID = "Pending", Code = "Pending", Label = "Pending"},
                       new SelectionItem { UID = "Approved", Code = "Approved", Label = "Approved"},
                       new SelectionItem { UID = "Rejected", Code = "Rejected", Label = "Rejected"}
                     },
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IViewChangeRequestApproval.Status),
                     Label = "Status"
                 }

             };

        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            _ApprovalEngine.PageNumber = 1;
            _pageStateHandler._currentFilters = filterCriteria;
            await _ApprovalEngine.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
        }
        #endregion
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _ApprovalEngine.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}


