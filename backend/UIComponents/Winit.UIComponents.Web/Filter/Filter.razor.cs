using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common;
using Winit.UIModels.Common.Filter;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
namespace Winit.UIComponents.Web.Filter;

public partial class Filter
{
    [Parameter]
    public EventCallback<ChildDropdownChangedEventArgs> OnChildDropdownChanged { get; set; }

    public delegate void FilterAppliedEvent(Dictionary<string, string> filterCriteria);
    [Parameter]
    public EventCallback<Dictionary<string,string>> OnFilterApplied { get; set; }

    [Parameter]
    public List<FilterModel> ColumnsForFilter { get; set; }
    [Parameter]
    public bool IsSearchCriteriaNeeded { get; set; }

    private List<SelectionItem> FilterCriteria = new List<SelectionItem>();

    private bool showFilter = false;

    private Dictionary<string, List<string>> filtercriteria = new Dictionary<string, List<string>>();
    public Dictionary<string, Winit.UIComponents.Common.CustomControls.DropDown> DropDownRefs = new Dictionary<string, Winit.UIComponents.Common.CustomControls.DropDown>();

    public async Task ApplyFilter()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (var column in ColumnsForFilter)
        {
            string selectedValue = string.Empty;
            switch (column.FilterType)
            {
                case FilterConst.DropDown:
                    if (column.SelectedValues != null && column.SelectedValues.Count > 0)
                    {
                        selectedValue = string.Join(",", column.SelectedValues.Select(e => e.UID));
                    }
                    break;
                case FilterConst.TextBox:
                case FilterConst.Date:
                    selectedValue = column.SelectedValue;
                    break;
            }
            result[column.ColumnName] = selectedValue;
        }
        await OnFilterApplied.InvokeAsync(result);
    }
    public string columnName;

    private async Task ResetFilter()
    {
        foreach (var column in ColumnsForFilter)
        {
            if (column.FilterType == FilterConst.DropDown)
            {
                columnName = column.ColumnName;
                if (column.IsDependent)
                {
                    column.DropDownValues?.Clear();
                }
                else
                {
                    if (column.DropDownValues != null)
                    {
                        foreach (var item in column.DropDownValues)
                        {
                            item.IsSelected = false;
                        }
                    }
                }

                if (column.SelectedValues != null)
                {
                    column.SelectedValues.Clear();
                }
                var dropDown = DropDownRefs[column.ColumnName];
                await dropDown.UpdateDataSource(column.DropDownValues, column.SelectionMode);
            }
            else if (column.FilterType == FilterConst.TextBox || column.FilterType == FilterConst.Date)
            {  
                column.SelectedValue = "";
            }
        }
        await ApplyFilter();
    }

    private string GetSearchCriteriaDisplay()
    {
        var searchCriteria = new List<string>();

        foreach (var column in ColumnsForFilter)
        {
            string filterValue = "All";
            if (column.FilterType == FilterConst.DropDown)
            {
                if (column.SelectedValues != null && column.SelectedValues.Count > 0)
                {
                    filterValue = string.Join(", ", column.SelectedValues.Select(e => e.Label));
                }
            }
            else if (column.FilterType == FilterConst.TextBox || column.FilterType == FilterConst.Date)
            {
                filterValue = string.IsNullOrEmpty(column.SelectedValue) ? "N/A" : column.SelectedValue;
            }
            searchCriteria.Add($"<b>{column.Label}</b>: {filterValue}");

        }
        return string.Join(", ", searchCriteria);
    }

    public void OnDateChange(CalenderWrappedData calenderWrappedData)
    {
        FilterModel? filterModel = ColumnsForFilter.FirstOrDefault(c => c.ColumnName == calenderWrappedData.Id);
        if (filterModel != null)
        {
            filterModel.SelectedValue = calenderWrappedData.SelectedValue;
            StateHasChanged();
        }
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        await base.OnInitializedAsync();
    }


    public async Task HandleSelectionChange(string columnName, DropDownEvent dropDownEvent)
    {
        FilterModel? filterModel = ColumnsForFilter.FirstOrDefault(e => e.ColumnName == columnName);

        if (dropDownEvent != null)
        {
            if (filterModel != null)
            {
                if (filterModel.OnDropDownSelect != null) await Task.Run(() => filterModel.OnDropDownSelect.Invoke(dropDownEvent));
                filterModel.SelectedValues = dropDownEvent.SelectionItems;

                if (filterModel.HasChildDependency)
                {
                    var args = new ChildDropdownChangedEventArgs
                    {
                        ParentColumnName = columnName,
                        SelectionItems = dropDownEvent.SelectionItems
                    };

                    await OnChildDropdownChanged.InvokeAsync(args);
                }
            }
        }
    }
    public async Task UpdateDataSoure(string columnName, List<ISelectionItem> dataSource)
    {
        FilterModel? selectedFilterModel = ColumnsForFilter.FirstOrDefault(e => e.ColumnName == columnName);
        if (selectedFilterModel != null)
        {
            selectedFilterModel.DropDownValues = dataSource;

            if (DropDownRefs.ContainsKey(columnName))
            {
                var dropDown = DropDownRefs[columnName];
                await dropDown.UpdateDataSource(dataSource, selectedFilterModel.SelectionMode);
            }
        }
    }

    public async Task UpdateDropDownparams()
    {

    }

    public void ToggleFilter()
    {
        showFilter = !showFilter;

    }
  
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
}

