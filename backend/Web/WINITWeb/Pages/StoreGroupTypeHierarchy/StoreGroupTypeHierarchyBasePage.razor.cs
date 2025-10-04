using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;

namespace WinIt.Pages.StoreGroupTypeHierarchy;

partial class StoreGroupTypeHierarchyBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddStoreGroupType { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView Context { get; set; } = new Winit.Modules.Store.Model.Classes.StoreGroupTypeItemView();
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            await _StoreGroupTypeViewModel.PopulateViewModel();
            HideLoader();
            await SetHeaderName();
            PrepareFilter();
            StateHasChanged();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }
    public async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new();
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["store_group_type_hierarchy"], IsClickable = false });
        _IDataService.HeaderText = @Localizer["store_group_type_hierarchy"];
        await CallbackService.InvokeAsync(_IDataService);
    }
    public void AddStoreGroupTypePOPupClosed()
    {
        IsAddStoreGroupType = false;
    }
    public async void GetChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        await _StoreGroupTypeViewModel.getChildGrid(storeGroupTypeItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        bool status = await _StoreGroupTypeViewModel.CreateStoreGroupType(storeGroupTypeItemView);
        base.StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        return await _StoreGroupTypeViewModel.DeleteStoreGroupType(storeGroupTypeItemView);
    }

    public async Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView> getClonedIStoreGroupTypeItemView(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        return await _StoreGroupTypeViewModel.CreateClone(storeGroupTypeItemView);
    }
    public async Task<bool> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        return await _StoreGroupTypeViewModel.UpdateStoreGroupType(storeGroupTypeItemView);
    }



    public async void CreateClickedFromHome()
    {
        bool IsNew = !_StoreGroupTypeViewModel.StoreGroupTypeItemViews.Any(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateStoreGroupType(Context);
            if (status)
            {
                _StoreGroupTypeViewModel.StoreGroupTypeItemViews.Add(Context);
                _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddStoreGroupType = false;
            }
            else
            {
                _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
        {
            new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code_&_name"],
                ColumnName ="customer_group_type_hierarchy_code_name"},
        });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _StoreGroupTypeViewModel.ApplyFilter(keyValuePairs);
    }

}

