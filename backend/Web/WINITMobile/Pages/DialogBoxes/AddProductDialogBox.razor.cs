using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.CustomControls;
using Winit.UIComponents.Mobile.DialogBox;

namespace WINITMobile.Pages.DialogBoxes;
partial class AddProductDialogBox<T>
{
    [Parameter]
    public string Class { get; set; }
    [Parameter]
    public string Style { get; set; }
    [Parameter]
    public List<T> DataSource { get; set; } = new List<T>();
    [Parameter]
    public Func<T, string> SKUHeader { get; set; }
    [Parameter]
    public Func<T, string> SKUCode { get; set; }
    [Parameter]
    public Func<T, object> ImageUrl { get; set; }
    [Parameter]
    public EventCallback<List<T>> OnOkClick { get; set; }
    [Parameter]
    public EventCallback OnCancelClick { get; set; }
    [Parameter]
    public Func<List<FilterCriteria>, T, bool>? FilterAction { get; set; }

    [Parameter]
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    [Parameter]
    public bool Rerender { get; set; } = false;
    private string SearchString = string.Empty;
    private WinitTextBox wtbSearch;

    public string Display = "none";
    private IEnumerable<T> FilteredItems
    {
        get
        {
            if(DataSource != null)
            {
                var filteredItems = DataSource.Where(e => FilterAction.Invoke(FilterCriterias, e));
                return filteredItems.Where(e => string.IsNullOrEmpty(SearchString) ||
                (SKUCode?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (SKUHeader?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            else
            {
                return null;
            }
            
        }
    }
    private List<FilterCriteria> FilterCriterias = new List<FilterCriteria>();
    private List<SortCriteria> SortCriterias = new List<SortCriteria>();
    private List<T> SelectedItems { get; set; } = new List<T>();
    private FilterDialog FilterDialog;
    public void OnCloseClick()
    {
        Display = "none";
        StateHasChanged();
    }
    public void OnOpenClick()
    {
        Display = "block";
        StateHasChanged();
    }

    private async Task HandleOkClick()
    {
        await OnOkClick.InvokeAsync(SelectedItems);
        if (Rerender)
        {
            SelectedItems.Clear();
        }
    }

    private void OnItemSelect(T item, ChangeEventArgs e)
    {
        if ((bool)e.Value)
        {
            SelectedItems.Add(item);
        }
        else
        {
            SelectedItems.Remove(item);
        }
    }

    private void ApplyFilter((List<FilterCriteria> filterCriterias, List<SortCriteria> sortCriterias) data)
    {
        FilterCriterias.Clear();
        FilterCriterias.AddRange(data.filterCriterias);
        SortCriterias.Clear();
        SortCriterias.AddRange(data.sortCriterias);
    }
}
