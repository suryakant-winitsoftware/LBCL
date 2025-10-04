using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;

namespace WinIt.Pages.PurchaseOrderTemplate;

partial class PurchaseOrderTemplateManagement
{
    private List<DataGridColumn> GridColumns = [];
    public List<FilterModel> ColumnsForFilter;
    public Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
    {
        BreadcrumList =
        [
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Maintain Purchase Order Template", IsClickable = false },
        ],
        HeaderText = "Maintain Purchase Order Template"
    };

    protected override async Task OnInitializedAsync()
    {
        GridColumns =
        [
             new DataGridColumn { Header = "Template Name", IsSortable = true, GetValue = s => ((IPurchaseOrderTemplateHeader)s).TemplateName, SortField="templatename" },
            new DataGridColumn { Header = "Is Active", IsSortable = true,GetValue = s => ((IPurchaseOrderTemplateHeader)s).IsActive  , SortField="isactive"    },
            new DataGridColumn { Header = "Created Date", IsSortable = true,GetValue = s => CommonFunctions.GetDateTimeInFormat(((IPurchaseOrderTemplateHeader)s).CreatedTime)  , SortField="CreatedTime"    },
            new DataGridColumn
            {
                IsButtonColumn = true,
                Header = "Action", IsSortable = false,
                ButtonActions =
                [   new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = e => OnEditbtnClick((IPurchaseOrderTemplateHeader) e)
                    },new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = async e =>  await DeleteItem((e as IPurchaseOrderTemplateHeader).UID)
                    }
                ],
            }
         ];
        _viewModel.PageNumber = 1;
        _viewModel.PageSize = 50;
        FilterInitialized();
        await _viewModel.PopulateViewModel();
        await StateChageHandler();
    }
    private async Task StateChageHandler()
    {
        _navigationManager.LocationChanged += (sender, args) => SavePageState();
        bool stateRestored = _pageStateHandler.RestoreState("purchaseordertemplate", ref ColumnsForFilter, out PageState pageState);

        ///only work with filters
        await OnFilterApply(_pageStateHandler._currentFilters);

    }
    private void SavePageState()
    {
        _navigationManager.LocationChanged -= (sender, args) => SavePageState();
        _pageStateHandler.SaveCurrentState("purchaseordertemplate");
    }
    public void FilterInitialized()
    {
        ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Template Name", ColumnName = "templatename", IsForSearch=true, PlaceHolder="Search By Template Name", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox, Label = "Is Active", ColumnName = "isactive", Width=1000},
            };
    }
    private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
    {
        try
        {
            ShowLoader();
            _pageStateHandler._currentFilters = filterCriteria;
            await _viewModel.OnFilterApply(filterCriteria);
        }
        catch (Exception ex)
        {

        }
        finally
        {
            HideLoader();
            StateHasChanged();
        }
    }
    private async Task OnSortClick(SortCriteria sortCriteria)
    {
        ShowLoader();
        await _viewModel.OnSortClick(sortCriteria);
        HideLoader();
    }
    private async Task OnPageIndexChange(int index)
    {
        ShowLoader();
        await _viewModel.PageIndexChanged(index);
        HideLoader();
    }

    private void OnEditbtnClick(IPurchaseOrderTemplateHeader header)
    {
        _navigationManager.NavigateTo($"addeditpotemplate/{header.UID}/{true}");
    }

    private void OnViewbtnClick(IPurchaseOrderTemplateHeader header)
    {
        _navigationManager.NavigateTo($"addeditpotemplate/{header.UID}/{false}");
    }

    private async Task DeleteItem(string purchaseOrderHeaderUid)
    {
        try
        {
            if (await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete this template?"))
            {
                await _viewModel.OnDeleteClick([purchaseOrderHeaderUid]);
            }
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                StateHasChanged();
                ShowSuccessSnackBar("Success", ex.Message);
            }
            else
            {
                ShowErrorSnackBar("Failed", ex.Message);
            }
        }
        catch (Exception)
        {
        }
    }
}
