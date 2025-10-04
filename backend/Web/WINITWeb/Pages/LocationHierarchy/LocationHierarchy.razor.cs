using Microsoft.AspNetCore.Components;
using Nest;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace WinIt.Pages.LocationHierarchy;

partial class LocationHierarchy
{
    [Parameter]
    public List<Winit.Modules.Location.Model.Interfaces.ILocationItemView> HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Location.Model.Interfaces.ILocationItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationItemView, Task<bool>> OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationItemView, Task<bool>> OnDeleteLocation { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationItemView, Task<bool>> OnUpdateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationItemView, Task<Winit.Modules.Location.Model.Interfaces.ILocationItemView>> OnCreateClone { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Location.Model.Interfaces.ILocationItemView> OnLocationTypebtnClicked { get; set; }
    [Parameter]
    public bool IsFirstGrid { get; set; }
    public string title { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationItemView SelectedElement { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationItemView Context { get; set; }

    public async Task ToggleCollapse(Winit.Modules.Location.Model.Interfaces.ILocationItemView hierarchy)
    {
        if (!hierarchy.IsOpen && (hierarchy.ChildGrids == null || hierarchy.ChildGrids.Count == 0))
        {
            ShowLoader();
            await OnBtnExpandClick.InvokeAsync(hierarchy);
        }
        if (hierarchy.ChildGrids != null && hierarchy.ChildGrids.Count > 0)
        {
            hierarchy.IsOpen = !hierarchy.IsOpen;
            StateHasChanged();
        }
        HideLoader();
    }

    public async Task CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        try
        {
            ValidateCode(locationItemView);
            ShowLoader();
            bool status = await OnCreateClick.Invoke(Context);
            if (status)
            {
                _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
            var parentgrid = HierarchyGrids.Find(i => i.UID == Context.ParentUID);
            if (parentgrid != null)
            {
                if (parentgrid.ChildGrids != null) parentgrid.ChildGrids.Add(Context);
                parentgrid.IsCreatePopUpOpen = false;
            }
        }
        catch (CustomException ex)
        {
            _tost.Add(@Localizer["location_hierarchy"], ex.Message, Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
        StateHasChanged();
    }

    public async Task UpdateLocationClick()
    {
        bool IsUpdated = await OnUpdateClick.Invoke(Context);
        if (IsUpdated)
        {
            ShowLoader();
            var ogGrid = HierarchyGrids.Find(i => i.UID == Context.UID);
            if (ogGrid != null)
            {
                //HierarchyGrids.Remove(ogGrid);
                //HierarchyGrids.Add(Context);
                ogGrid.LocationTypeUID = Context.LocationTypeUID;
                ogGrid.LocationTypeName = Context.LocationTypeName;
                ogGrid.Name = Context.Name;
                ogGrid.IsUpdatePopUpOpen = false;
            }
            _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            Context.IsUpdatePopUpOpen = false;
        }
        else
        {
            _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_update_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
        HideLoader();
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
    private void ValidateCode(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        List<string> errorMSG = new List<string>();
        if (string.IsNullOrEmpty(Context.Name)) errorMSG.Add("Name");
        if (string.IsNullOrEmpty(Context.Code)) errorMSG.Add("Code");
        if (string.IsNullOrEmpty(Context.LocationTypeName)) errorMSG.Add("LocationType");
        if (locationItemView != null && locationItemView.ChildGrids != null && locationItemView.ChildGrids.Any())
        {
            var item = locationItemView.ChildGrids.Find(e => e.Code == Context.Code);
            if (item != null)
            {
                errorMSG.Add("Code should be unique");
            }
        }
        if (errorMSG.Any())
        {
            throw new CustomException(ExceptionStatus.Failed, $"Please select the following fields: {string.Join(", ", errorMSG)}");
        }
    }

    public async Task DeleteClicked(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        ShowLoader();
        bool status = await OnDeleteLocation.Invoke(locationItemView);
        var deletedgrid = HierarchyGrids.Find(i => i.UID == locationItemView.UID);
        if (deletedgrid != null)
        {
            HierarchyGrids.Remove(deletedgrid);
        }
        if (status)
        {
            _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_deleting_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
    }

    public async Task UpdatePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        Context = await OnCreateClone.Invoke(locationItemView);
        title = @Localizer["edit_location_hierarchy"]; locationItemView.IsUpdatePopUpOpen = true;
    }
    public async Task CreatePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        title = @Localizer["add_location_hierarchy"];
        locationItemView.IsCreatePopUpOpen = true;
        SelectedElement = locationItemView;
        Context = new Winit.Modules.Location.Model.Classes.LocationItemView();
        Context.ParentUID = locationItemView.UID;
        Context.ParentName = locationItemView.Name;
        Context.ItemLevel = locationItemView.ItemLevel + 1;
        Context.ParentLocationTypeUID = locationItemView.LocationTypeUID;
        await Task.CompletedTask;
    }
    public async Task DeletePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        title = @Localizer["delete_location_hierarchy"];
        SelectedElement = locationItemView;
        if(await _alertService.ShowConfirmationReturnType("Alert","Are you sure you want to delete this location?"))
        {
            await DeleteClicked(locationItemView);
        }
    }


    public async Task LocationTypeDDClicked()
    {
        await OnLocationTypebtnClicked.InvokeAsync(Context);
    }
    public async Task UpdateLocationTypeDDClicked()
    {
        //Context.Level
        await OnLocationTypebtnClicked.InvokeAsync(Context);
    }
}

