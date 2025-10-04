using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;

namespace WinIt.Pages.SKUClassGroupItems;

partial class SKUClassGroupItemsBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    private string SKUClassGroupUID { get; set; } = string.Empty;
    public List<DataGridColumn>? DataGridColumns { get; set; }
    public bool IsInitialized { get; set; }
    private Winit.UIComponents.Common.CustomControls.DropDown? DistributionDDL;
    protected async override Task OnInitializedAsync()
    {
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            await SetHeaderName();
            ShowLoader();
            SKUClassGroupUID = _commonFunctions.GetParameterValueFromURL("SKuClassGroupUID");
            await _sKUClassGroupItemsViewModel.PopulateViewModel(SKUClassGroupUID);
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception)
        {
            HideLoader();
        }
    }
   
    public async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new();
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel
        { SlNo = 1, Text = @Localizer["allowed_sku"], IsClickable = true, URL = "/AllowedSKU" });
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel()
        { SlNo = 2, Text = @Localizer["sku_class_group_items"], IsClickable = false });
        _IDataService.HeaderText = @Localizer["sku_class_group_items"];
        await CallbackService.InvokeAsync(_IDataService);
    }

    private async Task OnOrgSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem selectedOrg = dropDownEvent.SelectionItems.First();
            await _sKUClassGroupItemsViewModel.OnOrgSelect(selectedOrg);
            DistributionDDL?.GetLoad();
        }
    }

    private async Task OnDistributionChannelSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem selectedDistributionChannel = dropDownEvent.SelectionItems.First();
            _sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroup!.DistributionChannelUID = selectedDistributionChannel.UID;
        }
        await Task.CompletedTask;
    }
    private async Task OnBackClick()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["are_you_sure_you_want_to_go_back_?"]))
        {
            _navigationManager.NavigateTo("/allowedsku");
        }
    }
    private async Task OnSaveClick()
    {
        ShowLoader();
        if (await Validate())
        {
            if (await _sKUClassGroupItemsViewModel.OnSaveClick())
            {
                ShowSuccessSnackBar(@Localizer["success"], @Localizer["saved_successfully"]);
                _navigationManager.NavigateTo("/allowedsku");
            }
            else
            {
                ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_save"]);
            }
        }
        HideLoader();
    }
    private async Task<bool> Validate()
    {
        if (string.IsNullOrEmpty(_sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroup!.OrgUID) ||
            string.IsNullOrEmpty(_sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroup!.DistributionChannelUID) ||
            string.IsNullOrEmpty(_sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroup!.Name)||
            string.IsNullOrEmpty(_sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroup!.Description) 
            )
        {
            ShowErrorSnackBar("","Missing Fields");
            return false;
        }
        else if(!_sKUClassGroupItemsViewModel.SKUClassGroupMaster.SKUClassGroupItems!.Any())
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    private async Task OnAddSKUClick(List<ISelectionItem> selectionItems)
    {
        await _sKUClassGroupItemsViewModel.AddSKUsToGrid(selectionItems);
    }

    private async Task PlantDDClicked(Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView sKUClassGroupItemView)
    {
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            DataSource = _sKUClassGroupItemsViewModel.PlantSelectionItems,
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnPlantDDSelect(sKUClassGroupItemView, eventArgs);
            },
        });
    }

    private async Task OnPlantDDSelect(Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView sKUClassGroupItemView
        , DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            sKUClassGroupItemView.SupplierOrgUID = dropDownEvent.SelectionItems.First().UID;
            sKUClassGroupItemView.PlantName = dropDownEvent.SelectionItems.First().Label;
        }
        StateHasChanged();
        await Task.CompletedTask;
    }
}

