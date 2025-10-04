using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.TaxGroup;

public partial class TaxGroupBasePage : WinIt.Pages.Base.BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public required List<DataGridColumn> DataGridColumns { get; set; }
    public required List<ITaxGroupItemView> Datasource { get; set; }
    public bool IsLoaded { get; set; }
    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public required List<FilterCriteria> TaxGroupFilterCriterials { get; set; }
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Maintain Tax Group",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Maintain Tax Group"},
         }
    };

    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            TaxGroupFilterCriterials = new List<FilterCriteria>();
            _taxGroupViewModel.PageSize = 5;
            await _taxGroupViewModel.PopulateViewModel();
            GenerateGridColumns();
           
            IsLoaded = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }
   
    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
        new() { Header = @Localizer["name"], GetValue = s => ((ITaxGroupItemView)s).Name },
        new() { Header = @Localizer["code"], GetValue = s => ((ITaxGroupItemView)s).Code },
        new() {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new() {
                        Text = @Localizer["edit"],
                        Action = item => OnEditClick((ITaxGroupItemView)item)
                    }
                }
            }
        };

    }
    public void OnEditClick(ITaxGroupItemView taxGroupItemView)
    {
        _navigationManager.NavigateTo($"AddEditTaxGroup?TaxGroupUID={taxGroupItemView.UID}");
    }
    public async Task ApplyFilter()
    {
        TaxGroupFilterCriterials.Clear();
        if (string.IsNullOrEmpty(CodeFilter))
        {
            TaxGroupFilterCriterials.Add(new FilterCriteria("Name", CodeFilter, FilterType.Equal));
        }

        if (string.IsNullOrEmpty(NameFilter))
        {
            TaxGroupFilterCriterials.Add(new FilterCriteria("Description", NameFilter, FilterType.Equal));
        }

        await _taxGroupViewModel.ApplyFilter(TaxGroupFilterCriterials);
        StateHasChanged();
    }
    public async Task ResetFilter()
    {
        NameFilter = null;
        CodeFilter = null;
        await _taxGroupViewModel.ResetFilter();
        StateHasChanged();
    }
}
