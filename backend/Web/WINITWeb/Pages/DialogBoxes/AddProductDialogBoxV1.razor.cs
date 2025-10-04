using System.Collections;
using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace WinIt.Pages.DialogBoxes;

public partial class AddProductDialogBoxV1<T>
{
    [Parameter]
    public string? Title { get; set; } = "Add Product";
    [Parameter]
    public string? Class { get; set; }
    [Parameter]
    public string? Style { get; set; }
    [Parameter]
    public List<T> DataSource { get; set; } = [];
    [Parameter]
    public Func<T, string>? L1 { get; set; }
    [Parameter]
    public Func<T, string>? L2 { get; set; }
    [Parameter]
    public Func<T, string>? L3 { get; set; }
    [Parameter]
    public Func<T, string>? L4 { get; set; }
    [Parameter]
    public Func<T, string>? L5 { get; set; }
    [Parameter]
    public Func<T, string>? L6 { get; set; }
    [Parameter]
    public Func<T, string>? L7 { get; set; }
    [Parameter]
    public Func<T, string>? L8 { get; set; }
    [Parameter]
    public Func<T, string>? UID { get; set; }
    [Parameter]
    public Func<T, string>? SKUImage { get; set; }

    [Parameter]
    public EventCallback<List<T>> OnOkClick { get; set; }
    [Parameter]
    public EventCallback OnCancelClick { get; set; }
    [Parameter]
    public Func<List<FilterCriteria>, T, bool>? FilterAction { get; set; }

    [Parameter]
    public Dictionary<ISelectionItem, List<ISelectionItem>>? FilterDataList { get; set; }
    [Parameter]
    public required List<SKUAttributeDropdownModel> DropDownDataSource { get; set; }
    [Parameter]
    public Func<ISelectionItem, Task<List<ISelectionItem>>>? OnDropdownValueSelectSKUAtrribute { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public Func<T, bool> OnSelect { get; set; }

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.Multiple;

    private string SearchString = string.Empty;

    public string Display = "none";

    private readonly List<T> FilteredItems = [];
    private bool IsFilterOpen = false;
    private IEnumerable<T>? DisplayedItems
    {
        get;
        set;
    }
    private readonly List<FilterCriteria> FilterCriterias = [];
    private readonly List<SortCriteria> SortCriterias = [];
    [Parameter]
    public List<T> SelectedItems { get; set; } = [];
    //private FilterDialog FilterDialog;
    private ElementReference SelectAllCheckbox;
    private bool IsAllSelected => DataSource.Count != 0 && DataSource.Count == SelectedItems.Count;
    protected override void OnInitialized()
    {
        ApplyFilter();
        ApplySearch();
        base.OnInitialized();
    }
    public void OnCloseClick()
    {
        Display = "none";
        //StateHasChanged();
    }
    public void OnOpenClick()
    {
        if (Disabled)
        {
            return;
        }

        ApplyFilter();
        ApplySearch();
        Display = "block";
        //StateHasChanged();
    }

    private async Task HandleOkClick()
    {
        Display = "none";
        await OnOkClick.InvokeAsync(SelectedItems);
    }

    private void OnItemSelect(T item, ChangeEventArgs e)
    {
        if ((OnSelect != null && !OnSelect(item)))
        {
            e.Value = false;
            StateHasChanged();
            return;
        }
        if ((bool)e.Value!)
        {
            if (SelectionMode == SelectionMode.Single)
            {
                SelectedItems.Clear();
            }

            SelectedItems.Add(item);
        }
        else
        {
            _ = SelectedItems.Remove(item);
        }
    }
    private void OnSelectAllClick(ChangeEventArgs e)
    {
        if ((bool)e.Value!)
        {
            SelectedItems.Clear();
            SelectedItems.AddRange(DisplayedItems.Where(e => OnSelect?.Invoke(e) ?? true).ToList());
        }
        else
        {
            SelectedItems.Clear();
        }
    }

    private void ApplyFilter((List<FilterCriteria> filterCriterias, List<SortCriteria> sortCriterias) data)
    {
        FilterCriterias.Clear();
        FilterCriterias.AddRange(data.filterCriterias);
        SortCriterias.Clear();
        SortCriterias.AddRange(data.sortCriterias);
    }
    private async Task OnDropDownSelect(SKUAttributeDropdownModel sKUAttributeDropdownModel, DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ResetDropDowns(sKUAttributeDropdownModel.UID);
            List<ISelectionItem> data = await OnDropdownValueSelectSKUAtrribute?.Invoke(dropDownEvent.SelectionItems.First());
            if (data != null)
            {
                SKUAttributeDropdownModel? selectedDropDown = DropDownDataSource.Find(e => e.ParentUID == sKUAttributeDropdownModel.UID);
                if (selectedDropDown != null)
                {
                    selectedDropDown.DropDownDataSource.Clear();
                    selectedDropDown.DropDownDataSource.AddRange(data);
                }
            }

            _ = FilterCriterias.RemoveAll(e => e.Name == sKUAttributeDropdownModel.DropDownTitle);
            FilterCriterias.Add(new FilterCriteria(sKUAttributeDropdownModel.DropDownTitle, dropDownEvent.SelectionItems.Select(e => e.Code + "_" + sKUAttributeDropdownModel.Code).ToList(), FilterType.In));
        }
        else
        {
            _ = FilterCriterias.RemoveAll(e => e.Name == sKUAttributeDropdownModel.DropDownTitle);
        }
        ApplyFilter();
        StateHasChanged();
    }
    private void ApplyFilter()
    {
        FilteredItems.Clear();
        if (FilterCriterias == null || !FilterCriterias.Any())
        {
            FilteredItems.AddRange(DataSource);
            return;
        }
        foreach (T? item in DataSource)
        {
            if (FilterAction == null || FilterAction.Invoke(FilterCriterias, item))
            {
                FilteredItems.Add(item);
            }
        }
    }
    private void ApplySearch()
    {
        if (FilteredItems == null)
        {
            return;
        }
        //if (!FilterCriterias.Any() && DataSource.Any()) FilteredItems.AddRange(DataSource);
        DisplayedItems = FilteredItems.Where(e => string.IsNullOrEmpty(SearchString) ||
            (L7?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (L8?.Invoke(e)?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false));
        //SelectAllCheckbox.set("checked", CheckIsAllSelected());
    }
    private void ResetDropDowns(string uid)
    {
        bool isAllCleared = false;
        string child = uid;
        while (!isAllCleared)
        {
            SKUAttributeDropdownModel? dropDown = DropDownDataSource.Find(e => e.ParentUID == child);
            if (dropDown != null)
            {
                dropDown.DropDownDataSource.Clear();
                child = dropDown.UID;
            }
            else
            {
                isAllCleared = true;
            }
        }
    }

    private void OnClearFilterBtnClick()
    {
        DropDownDataSource.ForEach(e =>
        {
            e.DropDownDataSource.ForEach(i => i.IsSelected = false);
        });
        FilterCriterias.Clear();
        ApplyFilter();
        StateHasChanged();
    }
}
