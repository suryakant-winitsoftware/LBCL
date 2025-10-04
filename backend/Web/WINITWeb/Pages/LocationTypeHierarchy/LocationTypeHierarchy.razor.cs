using Microsoft.AspNetCore.Components;

namespace WinIt.Pages.LocationTypeHierarchy;

partial class LocationTypeHierarchy
{
    [Parameter]
    public List<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView>? HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView, Task<bool>>? OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView, Task<bool>>? OnDeleteLocationType { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView, Task<bool>>? OnUpdateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView,
        Task<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView>>? OnCreateClone
    { get; set; }

    public string? title { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView? SelectedElement { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView? Context { get; set; }

    public async Task ToggleCollapse(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView hierarchy)
    {
        if (!hierarchy.IsOpen)
        {
            await OnBtnExpandClick.InvokeAsync(hierarchy);
        }
        if (hierarchy.ChildGrids != null && hierarchy.ChildGrids.Count > 0)
        {
            hierarchy.IsOpen = !hierarchy.IsOpen;
            StateHasChanged();
        }
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
    public async void CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        bool IsNew = true;
        if (locationTypeItemView != null && locationTypeItemView.ChildGrids != null && locationTypeItemView.ChildGrids.Any())
        {
            var item = locationTypeItemView.ChildGrids.Find(e => e.Code.Equals(Context?.Code, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                IsNew = false;
            }
        }
        if (IsNew && Context != null && OnCreateClick != null)
        {
            if (await OnCreateClick.Invoke(Context))
            {
                var parentgrid = HierarchyGrids?.Find(i => i.UID == Context.ParentUID);
                if (parentgrid != null)
                {
                    if (parentgrid.ChildGrids != null) parentgrid.ChildGrids.Add(Context);
                    parentgrid.IsCreatePopUpOpen = false;
                    _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["locationtype_successfully_created"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
            }
            else
            {
                _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["failed_to_delete_location_type"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task DeleteClicked(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        if (OnDeleteLocationType == null) return;
        bool result = await OnDeleteLocationType.Invoke(locationTypeItemView);
        if (result)
        {
            var deletedgrid = HierarchyGrids?.Find(i => i.UID == locationTypeItemView.UID);
            if (deletedgrid != null)
            {
                HierarchyGrids?.Remove(deletedgrid);
            }
            _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["locationtype_successfully_deleted"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["failed_to_delete_locationtype"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    public async Task UpdatePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        if (OnCreateClone == null) return;
        Context = await OnCreateClone.Invoke(locationTypeItemView);
        title = @Localizer["edit_locationtype_hierarchy"]; locationTypeItemView.IsUpdatePopUpOpen = true;
    }
    public async Task CreatePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        title = @Localizer["add_locationtype_hierarchy"];
        locationTypeItemView.IsCreatePopUpOpen = true;
        SelectedElement = locationTypeItemView;
        Context = new Winit.Modules.Location.Model.Classes.LocationTypeItemView();
        Context.ParentUID = locationTypeItemView.UID;
        Context.ParentName = locationTypeItemView.Name;
        Context.LevelNo = locationTypeItemView.LevelNo + 1;
        await Task.CompletedTask;
    }
    public async Task DeletePopIconClicked(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        title = @Localizer["delete_locationtype_hierarchy"];
        locationTypeItemView.IsDeletePopUpOpen = true;
        SelectedElement = locationTypeItemView;
        await Task.CompletedTask;
    }

    public async Task UpdateLocationTypeClick()
    {
        if (OnUpdateClick == null || Context == null) return;
        bool IsUpdated = await OnUpdateClick.Invoke(Context);
        if (IsUpdated)
        {
            var ogGrid = HierarchyGrids?.Find(i => i.UID == Context.UID);
            if (ogGrid != null)
            {
                ogGrid.Name = Context.Name;
                ogGrid.IsUpdatePopUpOpen = false;
            }
        }
        Context.IsUpdatePopUpOpen = false;
        StateHasChanged();
    }
}

