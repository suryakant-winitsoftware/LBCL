using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.NewsActivity.BL.Classes;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.News
{
    public partial class MaintainNews
    {
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Manage News Activity",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Manage News Activity"},
            }
        };
        List<FilterModel> ColumnsForFilter = [];
        List<DataGridColumn> GridColumns = [];
        protected override async void OnInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(INewsActivity.Title),
                     Label = nameof(INewsActivity.Title)
                 },
                  new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox,
                     ColumnName =nameof(INewsActivity.IsActive),
                     Label = "Is Active"
                 },
                  new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName =nameof(INewsActivity.PublishDate),
                     Label = "Start Date"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName =nameof(INewsActivity.PublishDate),
                     Label = "End Date"
                 },

            };
            GridColumns.Add(new DataGridColumn { Header = "Title", GetValue = s => ((INewsActivity)s).Title, IsSortable = false });
            GridColumns.Add(new DataGridColumn
            {
                Header = "Published Date",
                GetValue = s => CommonFunctions.GetDateTimeInFormat(((INewsActivity)s).PublishDate),
                IsSortable = false
            });
            GridColumns.Add(new DataGridColumn
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            Text = "View",
                             URL = "images/view.png",
                            ButtonType=ButtonTypes.Image,

                            Action = async item =>  View((INewsActivity)item)
                        },
                    new ButtonAction
                        {
                            URL = "images/delete.png",
                            Text = "Delete",
                            ButtonType=ButtonTypes.Image,
                            Action = async item =>await  Delete((INewsActivity)item)
                        },
                    }

            });
            base.OnInitialized();
           
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("MaintainNewsActivity", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainNewsActivity");
        }
        private async Task Delete(INewsActivity newsActivity)
        {
            if (await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete?"))
            {
                var response = await ((ManageNewsActivityWebViewModel)_viewModel).DeleteNewsActivityByUID(newsActivity.UID);
                if (response != null)
                {
                    if (response.IsSuccess)
                    {
                        _tost.Add("Success", "Deleted Successfully;");
                        _viewModel.NewsActivities.Remove(newsActivity);
                    }
                    else
                    {
                        _tost.Add("Error", "Deleted Successfully;");
                    }
                }
            }
        }
        private void View(INewsActivity newsActivity)
        {
            _dataManager.SetData(newsActivity.UID, newsActivity);
            _navigationManager.NavigateTo($"Activity?{PageType.Type}={_viewModel.TabItems.FirstOrDefault(p => p.IsSelected)?.Code}&{PageType.Page}={PageType.View}&UID={newsActivity.UID}");
        }
        protected override async Task OnInitializedAsync()
        {
            await _viewModel.PopulateviewModel();
            await StateChageHandler();
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            _pageStateHandler._currentFilters = filterCriteria;
            await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
        }
        async Task OnTabSelect(ISelectionItem selectedItem)
        {
            ShowLoader();
            await _viewModel.OnTabSelect(selectedItem);
            StateHasChanged();
            HideLoader();
        }
    }
}
