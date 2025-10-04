using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.UIComponents.Mobile.DialogBox;


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
    public Func<(List<FilterCriteria>, T), bool> FilterAction { get; set; }

    [Parameter]
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    private string SearchString = string.Empty;
    private IEnumerable<T> FilteredItems
    {
        get
        {
            var filteredItems = DataSource.Where(e => FilterAction.Invoke((FilterCriterias, e)));
            return filteredItems.Where(e => string.IsNullOrEmpty(SearchString) ||
            (SKUCode?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (SKUHeader?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }
    private List<FilterCriteria> FilterCriterias = new List<FilterCriteria>();
    private List<SortCriteria> SortCriterias = new List<SortCriteria>();
    private List<T> SelectedItems { get; set; } = new List<T>();
    private FilterDialog FilterDialog;
    private void OnCloseClick()
    {

    }

    private async Task HandleOkClick()
    {
        await OnOkClick.InvokeAsync(SelectedItems);
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

