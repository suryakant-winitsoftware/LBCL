using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;

namespace WinIt.Pages.RouteManagementMaster;

public partial class RouteManagement
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsInitialized;
    private List<DataGridColumn>? productColumns;
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    protected override async Task OnInitializedAsync()
    {
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            ShowLoader();
            _RouteManagement.PageSize = 50;
            PrepareGridColumns();
            PrepareFilterColumns();
            await _RouteManagement.PopulateRouteChangeLogGridData();
            await SetHeaderName();
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            HideLoader();
            Console.WriteLine(ex);
        }
    }
   
    public async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new()
        {
            new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["route_management"], IsClickable = false }
        };
        _IDataService.HeaderText = @Localizer["route_management"];
        await CallbackService.InvokeAsync(_IDataService);
    }

    private void NavigateToAddRoute()
    {
        _navigationManager.NavigateTo("addroute");
    }

    private void PrepareGridColumns()
    {
        productColumns = new List<DataGridColumn>
        {
            new() { Header = @Localizer["sn"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).serialNumber,
                IsSortable = false,  },
            new() { Header = @Localizer["route_code"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).Code,
                IsSortable = true, SortField = "Code" },
            new() { Header = @Localizer["route_name"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).Name,
                IsSortable = true, SortField = "Name" },
            new() { Header = @Localizer["from_period"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(
                ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).ValidFrom),
                IsSortable = true, SortField = "ValidFrom" },
            new() { Header = @Localizer["to_period"], GetValue = s =>  Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(
                ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).ValidUpto),IsSortable = true, SortField = "ValidUpto" },
            new() { Header = @Localizer["is_active"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).IsActive,
                IsSortable = true, SortField = "IsActive" },
            new() { Header = @Localizer["user"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).User},
            //new() { Header = "Vehicle", GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).VehicleUID, },
            //new() { Header = "Type", GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).Type, IsSortable = true,
            //    SortField = "Type" },
            new() { Header = @Localizer["total_customers"], GetValue = s => ((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)s).TotalCustomers
            ,IsSortable = true, SortField = "TotalCustomers"},
            new() {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new() {
                        URL="Images/edit.png",
                        ButtonType = ButtonTypes.Image,
                        IsVisible = true,
                        Action = (item) => HandleRowEditBtn_Click((Winit.Modules.Route.Model.Interfaces.IRouteChangeLog)item)
                    }
                }
            }
        };
    }

    private void PrepareFilterColumns()
    {
        FilterColumns.AddRange(new List<FilterModel> {
            new() {ColumnName="RouteCode",Label=@Localizer["route_code/name"],FilterType=Winit.Shared.Models.
            Constants.FilterConst.TextBox},
            new() {ColumnName="Date",Label=@Localizer["from_date"], FilterType = Winit.Shared.Models.Constants.FilterConst.Date},
            new() {ColumnName="Date",Label=@Localizer["to_date"], FilterType = Winit.Shared.Models.Constants.FilterConst.Date},
            new() {ColumnName="IsActive",Label=@Localizer["is_active"], FilterType =
            Winit.Shared.Models.Constants.FilterConst.DropDown},
            new() {ColumnName="Vehicle",Label=@Localizer["vehicle"], FilterType =
            Winit.Shared.Models.Constants.FilterConst.DropDown}
            });
    }

    private void HandleRowEditBtn_Click(Winit.Modules.Route.Model.Interfaces.IRouteChangeLog route)
    {
        if (route.UID != null)
        {
            _navigationManager.NavigateTo($"addroute?UID={route.UID}");
        }
    }

    private async Task OnFilterApply(Dictionary<string, string> filter)
    {
        await _RouteManagement.ApplyFilter(filter);
    }

    private async Task OnSortApply(SortCriteria sortCriteria)
    {
        ShowLoader();
        await _RouteManagement.ApplySort(sortCriteria);
        StateHasChanged();
        HideLoader();
    }
}
