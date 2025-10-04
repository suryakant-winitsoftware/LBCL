using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using WinIt.Pages.Base;

namespace WinIt.Pages.Maintain_Van;

public partial class MaintainVan
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    private bool showFilter = false;
    public string RegistrationNo { get; set; }
    public string VehicleNo { get; set; }
    public string ModelNo { get; set; }
    private bool IsDeleteBtnPopUp { get; set; }
    public bool IsLoaded { get; set; }
    public List<Winit.Shared.Models.Enums.FilterCriteria> VehicleFilterCriterias { get; set; }
    public List<DataGridColumn> DataGridColumns { get; set; }
    public List<IVehicle> Datasource { get; set; }
    public IVehicle? SelectedVehicle { get; set; }
    private bool showFilterComponent = false;
    private Winit.UIComponents.Web.Filter.Filter filterRef;
    public List<FilterModel> ColumnsForFilter;
    private List<ISelectionItem> StatusSelectionItems = new List<ISelectionItem>
    {
        new SelectionItem{UID="Yes1",Code="Yes",Label="Yes"},
        new SelectionItem{UID="No1",Code="No",Label="No"},
    };
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Maintain Van",
        BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Van"},
            }
    };
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        FilterInitialized();
        _maintainvanViewModel.PageSize = 10;
        VehicleFilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
        _maintainvanViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
        //_maintainvanViewModel.OrgUID = "FR001";
        await _maintainvanViewModel.PopulateViewModel();
        await GenerateGridColumns();
        IsLoaded = true;
       // await SetHeaderName();
    }
   
    public async void ShowFilter()
    {
        showFilterComponent = !showFilterComponent;
        filterRef.ToggleFilter();
    }
    public void FilterInitialized()
    {
        ColumnsForFilter = new List<FilterModel>
    {
        new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["vehicle_no"],ColumnName = "VehicleNo"},
        new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["registration_no"],ColumnName = "RegistrationNo"},
        new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["model"],ColumnName = "Model"},
        new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["is_active"] , DropDownValues=StatusSelectionItems,ColumnName = "IsActive"},
    };
    }
    //public async Task SetHeaderName()
    //{
    //    _IDataService.BreadcrumList = new();
    //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_van"], IsClickable = false });
    //    _IDataService.HeaderText = @Localizer["maintain_van"];
    //    await CallbackService.InvokeAsync(_IDataService);
    //}

    public async void ApplyFilter(Dictionary<string, string> filterCriteria)
    {
        List<Winit.Shared.Models.Enums.FilterCriteria> filterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
        foreach (var keyValue in filterCriteria)
        {
            if (!string.IsNullOrEmpty(keyValue.Value))
            {
                if (keyValue.Key == "IsActive")
                {
                    if (keyValue.Value == "Yes1")
                    {
                        filterCriterias.Add(new Winit.Shared.Models.Enums.FilterCriteria(@$"{keyValue.Key}", true, Winit.Shared.Models.Enums.FilterType.Equal, typeof(bool)));
                    }
                    else
                    {
                        filterCriterias.Add(new Winit.Shared.Models.Enums.FilterCriteria(@$"{keyValue.Key}", false, Winit.Shared.Models.Enums.FilterType.Equal, typeof(bool)));
                    }
                }
                else
                {
                    filterCriterias.Add(new Winit.Shared.Models.Enums.FilterCriteria(@$"{keyValue.Key}", keyValue.Value, Winit.Shared.Models.Enums.FilterType.Like));
                }


            }
        }
        await _maintainvanViewModel.ApplyFilter(filterCriterias);
        StateHasChanged();
    }
    public async void ResetFilter()
    {
        // Clear the filter criteria
        RegistrationNo = null;
        VehicleNo = null;
        ModelNo = null;
        await _maintainvanViewModel.ResetFilter();
        StateHasChanged();
    }
    private async Task GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
            //new DataGridColumn { Header = "SlNO", GetValue = s => ((IVehicle)s)?.SlNO+1 },
            new DataGridColumn { Header = @Localizer["vehicle_no"], GetValue = s => ((IVehicle)s)?.VehicleNo ?? "N/A" ,IsSortable = true, SortField = "VehicleNo"},
            new DataGridColumn { Header = @Localizer["registration_no"], GetValue = s => ((IVehicle)s)?.RegistrationNo?? "N/A",IsSortable = true, SortField = "RegistrationNo" },
            new DataGridColumn {Header = @Localizer["model"], GetValue = s =>((IVehicle) s) ?.Model ?? "N/A", IsSortable = true, SortField = "Model"},
            new DataGridColumn { Header = @Localizer["is_active"], GetValue = s => ((IVehicle)s)?.IsActive  == true ? "Yes" : "No",IsSortable = true, SortField = "IsActive" },
         new DataGridColumn
        {
            Header = @Localizer["actions"],
            IsButtonColumn = true,
            ButtonActions = new List<ButtonAction>
            {
                new ButtonAction
                {
                    ButtonType = ButtonTypes.Image,
                    URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                    Action = item => OnEditClick((IVehicle)item)
                },
                  new ButtonAction
                  {
                    ButtonType = ButtonTypes.Image,
                    URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                    Action = s => OnDeleteClick((IVehicle)s)
                  }
            }
        }
         };
    }
    private async Task AddNewVehicle()
    {
        _navigationManager.NavigateTo($"AddEditVehicles");
    }
    public void OnEditClick(IVehicle code)
    {
        _navigationManager.NavigateTo($"AddEditVehicles?VehicleUID={code.UID}&IsEditPage=true");
    }
    public async void OnDeleteClick(IVehicle vehicle)
    {
        SelectedVehicle = vehicle;
        // IsDeleteBtnPopUp = true;
        if (await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item_?"], @Localizer["yes"], @Localizer["no"]))
        {
            try
            {
                string msg = await _maintainvanViewModel.DeleteVehicle(SelectedVehicle?.UID);
                if (msg.Contains("Failed"))
                {
                    await _AlertMessgae.ShowErrorAlert(@Localizer["failed"], msg);
                }
                else
                {
                    await _AlertMessgae.ShowSuccessAlert(@Localizer["success"], msg);
                    await _maintainvanViewModel.PopulateViewModel();
                }

            }
            catch (Exception ex)
            {

                await _AlertMessgae.ShowSuccessAlert(@Localizer["deleted"], ex.Message);
            }
        }
        StateHasChanged();
    }
    //public async Task OnOkFromDeleteBTnPopUpClick()
    //{
    //    IsDeleteBtnPopUp = false;
    //    string s = await _maintainvanViewModel.DeleteVehicle(SelectedVehicle?.UID);
    //    if (s.Contains("Failed"))
    //    {
    //        await _AlertMessgae.ShowErrorAlert("Failed", s);
    //    }
    //    else
    //    {
    //        await _AlertMessgae.ShowSuccessAlert("Success", s);
    //        await _maintainvanViewModel.PopulateViewModel();
    //    }
    //}
    private async Task OnSortApply(SortCriteria sortCriteria)
    {
        ShowLoader();
        await _maintainvanViewModel.ApplySort(sortCriteria);
        StateHasChanged();
        HideLoader();
    }
}
