using Microsoft.AspNetCore.Components;
using Winit.Modules.Location.BL.Interfaces;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.LocationTypeHierarchy;

partial class LocationTypeHierarchyBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddLocationType { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView Context { get; set; } = new Winit.Modules.Location.Model.Classes.LocationTypeItemView();
    public bool ISFilterOpen = false;
    public string NameFilter = "";
    public string CodeFilter = "";
    public string SelectedLocationTypeTypeNameFilter = "";
    public List<FilterModel> FilterColumns = new();
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Location Type Hierarchy",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Location Type Hierarchy"},
         }
    };

    protected async override Task OnInitializedAsync()
    {
        ShowLoader();
        LoadResources(null, _languageService.SelectedCulture);
        try
        {
            await _LocationTypeViewModel.PopulateViewModel();
            PrepareFilter();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
        HideLoader();
    }
   
    public void AddLocationTypePOPupClosed()
    {
        IsAddLocationType = false;
    }
    public async void GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        await _LocationTypeViewModel.GetChildGrid(locationTypeItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        bool status = await _LocationTypeViewModel.CreateLocationType(locationTypeItemView);
        StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        return await _LocationTypeViewModel.DeleteLocationType(locationTypeItemView);
    }

    public async Task<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView> getClonedILocationTypeItemView(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        return await _LocationTypeViewModel.CreateClone(locationTypeItemView);
    }
    public async Task<bool> UpdateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        if (await _LocationTypeViewModel.UpdateLocationType(locationTypeItemView))
        {
            ShowSuccessSnackBar(@Localizer["success"], @Localizer["location_type_updated_successfully"]);
            return true;
        }
        else
        {
            ShowErrorSnackBar(@Localizer["error"], @Localizer["location_type_update_failed"]);
            return false;
        }
    }

    public async void CreateClickedFromHome()
    {
        bool IsNew = !_LocationTypeViewModel.LocationTypeItemViews.Any(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateLocationType(Context);
            if (status)
            {
                _LocationTypeViewModel.LocationTypeItemViews.Add(Context);
                _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["location_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddLocationType = false;
            }
            else
            {
                _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["location_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["location_type_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged(); ;
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code/name"],
                    ColumnName = "locationhierarchy_level_code_name"},
            });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _LocationTypeViewModel.ApplyFilter(keyValuePairs);
    }

}

