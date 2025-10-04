using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Globalization;
using System.Resources;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.MaintainSKU;

partial class SKUAttribute
{
    [Parameter]
    public required List<SKUAttributeDropdownModel> DataSource { get; set; }
    [Parameter]
    public bool IsSave { get; set; }
    [Parameter]
    public EventCallback<Dictionary<string, ISelectionItem>> OnSaveClick { get; set; }
    [Parameter]
    public Func<ISelectionItem, Task<List<ISelectionItem>>>? OnDropdownValueSelectSKUAtrribute { get; set; }

    public string ErrorMsg = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        await base.OnInitializedAsync();
    }

    private async Task OnSave_Click()
    {
        Dictionary<string, ISelectionItem> selectedvalues = Validate();
        if (string.IsNullOrEmpty(ErrorMsg))
        {
            await OnSaveClick.InvokeAsync(selectedvalues);
        }
        StateHasChanged();
    }

    private Dictionary<string, ISelectionItem> Validate()
    {
        try
        {
            ErrorMsg = string.Empty;
            var errorTitles = new List<string>();
            Dictionary<string, ISelectionItem> selectedvalues = new Dictionary<string, ISelectionItem>();
            foreach (var dropDown in DataSource)
            {
                if (dropDown != null)
                {
                    var selected = dropDown.DropDownDataSource.Find(e => e.IsSelected);
                    if (selected == null)
                    {
                        errorTitles.Add(dropDown.DropDownTitle);
                        continue;
                    }
                    selectedvalues.Add(dropDown.DropDownTitle, selected);
                }
            }
            if (errorTitles.Count > 0)
            {
                ErrorMsg = string.Join(", ", errorTitles);
            }
            return selectedvalues;
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            StateHasChanged();
        }
    }
    private async Task OnDropDownSelect(SKUAttributeDropdownModel sKUAttributeDropdownModel, DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ResetDropDowns(sKUAttributeDropdownModel.UID);
            var data = await OnDropdownValueSelectSKUAtrribute?.Invoke(dropDownEvent.SelectionItems.First());
            if (data != null)
            {
                var selectedDropDown = DataSource.Find(e => e.ParentUID == sKUAttributeDropdownModel.UID);
                if (selectedDropDown != null)
                {
                    selectedDropDown.DropDownDataSource.Clear();
                    selectedDropDown.DropDownDataSource.AddRange(data);
                }
            }
        }
        StateHasChanged();
    }
    private void ResetDropDowns(string uid)
    {
        bool isAllCleared = false;
        string child = uid;
        while (!isAllCleared)
        {
            var dropDown = DataSource.Find(e => (e.ParentUID == child));
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
}
