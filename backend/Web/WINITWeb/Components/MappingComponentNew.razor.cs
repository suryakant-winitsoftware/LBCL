using Microsoft.AspNetCore.Components;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.Modules.Mapping.Model.Classes;

namespace WinIt.Components;

/// <summary>
/// Reusable mapping component that uses event callbacks for all operations
/// </summary>
public partial class MappingComponentNew
{
    /// <summary>
    /// The mapping data to display
    /// </summary>
    [Parameter]
    public MappingComponentDTO MappingDTO { get; set; }

    /// <summary>
    /// Callback triggered when mapping is saved
    /// </summary>
    [Parameter]
    public EventCallback<List<IMappingItemView>> OnSaveMapping { get; set; }

    /// <summary>
    /// Callback triggered when close button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnCloseClick { get; set; }

    /// <summary>
    /// Callback triggered when tab is clicked
    /// </summary>
    [Parameter]
    public EventCallback<ISelectionItem> OnTabClick { get; set; }

    /// <summary>
    /// Callback triggered when location type is selected
    /// </summary>
    [Parameter]
    public EventCallback<ISelectionItem> OnLocationTypeDDClick { get; set; }

    /// <summary>
    /// Callback triggered when store group type is selected
    /// </summary>
    [Parameter]
    public EventCallback<ISelectionItem> OnStoreGroupTypeDDClick { get; set; }

    /// <summary>
    /// Callback triggered when location is added
    /// </summary>
    [Parameter]
    public EventCallback<ISelectionItem> OnLocationAddClick { get; set; }

    /// <summary>
    /// Callback triggered when store group is added
    /// </summary>
    [Parameter]
    public EventCallback OnStoreGroupAddClick { get; set; }

    /// <summary>
    /// Callback triggered when item is deleted from grid
    /// </summary>
    [Parameter]
    public EventCallback<IMappingItemView> OnDeleteItem { get; set; }

    /// <summary>
    /// Whether to show the add new button
    /// </summary>
    [Parameter]
    public bool ShowAddNewButton { get; set; }
    List<DataGridColumn> DataGridColumns = [];
    private bool ShowMapping { get; set; }
    protected override void OnInitialized()
    {

    }
    public void DeleteItemFromGrid(IMappingItemView mappingItemView)
    {
        //_MappingViewModel.GridDataSource?.Remove(mappingItemView);
        mappingItemView.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
        _tost.Add("Mapping", mappingItemView.Value + @Localizer["deleted_from_grid"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
    }

    private async Task OnSaveMappingClick()
    {
        ShowMapping = false;
        if (MappingDTO.GridDataSource != null && MappingDTO.GridDataSource.Any())
        {
            await OnSaveMapping.InvokeAsync(MappingDTO.GridDataSource);
        }
    }
    public void OnLocationAdd()
    {
        List<string> duplicates = MappingDTO.OnLocationClick.Invoke();
        if (duplicates.Any()) duplicates.ForEach(d => _tost.Add(@Localizer["mapping"], d + @Localizer["is_already_added"], Winit.UIComponents.SnackBar.Enum.Severity.Error));
    }
    public void OnStoreGroupAdd()
    {
        List<string> duplicates = MappingDTO.OnStoreGroupClick.Invoke();
        if (duplicates.Any()) duplicates.ForEach(d => _tost.Add(@Localizer["mapping"], d + @Localizer["is_already_added"], Winit.UIComponents.SnackBar.Enum.Severity.Error));
    }
    public void OnDistributorAdd()
    {
        List<string> duplicates = MappingDTO.OnDistributorClick.Invoke();
        if (duplicates.Any()) duplicates.ForEach(d => _tost.Add(@Localizer["mapping"], d + @Localizer["is_already_added"], Winit.UIComponents.SnackBar.Enum.Severity.Error));
    }
    public void OnUserAdd()
    {
        List<string> duplicates = MappingDTO.OnUserClick.Invoke();
        if (duplicates.Any()) duplicates.ForEach(d => _tost.Add(@Localizer["mapping"], d + @Localizer["is_already_added"], Winit.UIComponents.SnackBar.Enum.Severity.Error));
    }
    protected void OnClose()
    {
        ShowMapping = false;
        OnCloseClick.InvokeAsync();
    }
}
