using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Winit.Modules.Location.BL.Interfaces;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.SKUGroupTypeHierarchy;

partial class SKUGroupTypeHierarchyBasePage:BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddSKUGroupType { get; set; }
    public Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView Context { get; set; } = new Winit.Modules.SKU.Model.Classes.SKUGroupTypeItemView();
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "SKU Group Type Hierarchy",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="SKU Group Type Hierarchy"},
         }
    };

    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            await _SKUGroupTypeViewModel.PopulateViewModel();
            PrepareFilter();
            HideLoader();
            StateHasChanged();
        }
        catch(Exception ex) 
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }
    
    public void AddSKUGroupTypePOPupClosed()
    {
        IsAddSKUGroupType = false;
    }
    public async void GetChildGrid(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        await _SKUGroupTypeViewModel.GetChildGrid(sKUGroupTypeItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        bool status = await _SKUGroupTypeViewModel.CreateSKUGroupType(sKUGroupTypeItemView);
        base.StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        return await _SKUGroupTypeViewModel.DeleteSKUGroupType(sKUGroupTypeItemView);
    }

    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView> getClonedISKUGroupTypeItemView(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        return await _SKUGroupTypeViewModel.CreateClone(sKUGroupTypeItemView);
    }
    public async Task<bool> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        return await _SKUGroupTypeViewModel.UpdateSKUGroupType(sKUGroupTypeItemView);
    }

    public async void CreateClickedFromHome()
    {
        bool IsNew = !_SKUGroupTypeViewModel.SKUGroupTypeItemViews.Any(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateSKUGroupType(Context);
            if (status)
            {
                _SKUGroupTypeViewModel.SKUGroupTypeItemViews.Add(Context);
                _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_group_type_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddSKUGroupType = false;
            }
            else
            {
                _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_group_type_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
        {
            new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["code/name"],
                ColumnName = @"sku_group_type_hierarchy_level_code_name"},
        });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _SKUGroupTypeViewModel.ApplyFilter(keyValuePairs);
    }
}
