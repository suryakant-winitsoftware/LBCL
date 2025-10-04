using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.Modules.Common.UIState.Classes;
namespace WinIt.Pages.Tax;

public partial class TaxBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public required List<DataGridColumn> DataGridColumns { get; set; }
    public required List<ITaxItemView> Datasource { get; set; }
    public bool IsLoaded { get; set; }
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    private List<ISelectionItem> StatusSelectionItems = new List<ISelectionItem>
    {
        new SelectionItem{UID="Active",Label="Active"},
        new SelectionItem{UID="InActive",Label="InActive"},
    };
    private List<ISelectionItem> ApplicableAtSelectionItems = new List<ISelectionItem>
    {
        new SelectionItem{UID="Item",Label="Item"},
        new SelectionItem{UID="Invoice",Label="Invoice"},
    };
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Maintain Tax",
        BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Tax"},
            }
    };
    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            _taxViewModel.PageSize = 5;
            await _taxViewModel.PopulateViewModel();
            LoadResources(null, _languageService.SelectedCulture);
            PrepareFilter();
            GenerateGridColumns();
          
            IsLoaded = true;
            await StateChageHandler();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
        HideLoader();
    }
    private async Task StateChageHandler()
    {
        _navigationManager.LocationChanged += (sender, args) => SavePageState();
        bool stateRestored = _pageStateHandler.RestoreState("MaintainTax", ref FilterColumns, out PageState pageState);
       
            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);
        
    }
    private void SavePageState()
    {
        _navigationManager.LocationChanged -= (sender, args) => SavePageState();
        _pageStateHandler.SaveCurrentState("MaintainTax");
    }

    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
            new() { Header = @Localizer["name"], GetValue = s => ((ITaxItemView)s).Name },
            new() { Header = @Localizer["applicable_at"], GetValue = s => ((ITaxItemView)s).ApplicableAt },
            new() { Header =  @Localizer["status"], GetValue = s => ((ITaxItemView)s).Status },
            new() { Header =  @Localizer["valid_from"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((ITaxItemView)s).ValidFrom) },
            new() { Header =  @Localizer["validupto"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((ITaxItemView)s).ValidTo) },
            new() {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new() {
                        Text = @Localizer["edit"],
                        Action = item => OnEditClick((ITaxItemView)item)
                    }
                }
            }
        };
    }
    public void OnEditClick(ITaxItemView taxItemView)
    {
        _navigationManager.NavigateTo($"AddEditTax?TaxUID={taxItemView.UID}");
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["name"],
                    ColumnName = "Name"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["applicable_at"],
                    ColumnName = "ApplicableAt",DropDownValues = ApplicableAtSelectionItems},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["status"],
                    ColumnName = "Status",DropDownValues = StatusSelectionItems},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["valid_from"],
                    ColumnName = "ValidFrom"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["valid_upto"],
                    ColumnName = "ValidUpto"},
            });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        ShowLoader("Applying Filter");
        _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
        List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
        string fromDate = string.Empty;
        string toDate = string.Empty;
        if (keyValuePairs != null)
        {
            foreach (var item in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    if (item.Key == "ValidFrom")
                    {
                        fromDate = item.Value;
                    }
                    else if (item.Key == "ValidUpto")
                    {
                        toDate = item.Value;
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Like));
                    }
                }
            }
        }
        //if(!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
        //{
        //    filterCriterias.Add(new FilterCriteria(""))
        //}
        await _taxViewModel.ApplyFilter(filterCriterias);
        HideLoader();
    }
}
