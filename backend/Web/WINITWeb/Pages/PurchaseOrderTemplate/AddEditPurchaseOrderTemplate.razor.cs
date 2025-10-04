using Microsoft.AspNetCore.Components;
using NPOI.OpenXmlFormats;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace WinIt.Pages.PurchaseOrderTemplate;

partial class AddEditPurchaseOrderTemplate
{
    [Parameter]
    public string UID { get; set; } = string.Empty;
    [Parameter]
    public bool IsView { get; set; }
    private WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1>? AddProductDialogBox;
    public Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
    {
        BreadcrumList =
        [
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
            {
                SlNo = 1,
                Text = "Add Edit PO Template",
                IsClickable = false
            },
        ],
        HeaderText = "Add Edit PO Template"
    };
    private readonly List<ISKUV1> SKUs = [];

    private bool IsFilterOpen = false;

    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            await _viewModel.PopulateViewModel(UID);
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            HideLoader();
        }
    }

    private async Task OnAddProductClick(List<ISKUV1> sKUs)
    {
        ShowLoader();
        await _viewModel.AddProductsToGridBySKUUIDs(sKUs.Select(e => e.UID).ToList());
        HideLoader();
    }
    private bool FilterAction(List<FilterCriteria> filterCriterias, ISKUV1 sKUV1)
    {
        if (filterCriterias == null || !filterCriterias.Any())
        {
            return true;
        }

        foreach (FilterCriteria filter in filterCriterias)
        {
            if (filter.Value is List<string> filterValues)
            {
                if (!sKUV1.FilterKeys.Any(filterValues.Contains))
                {
                    return false;
                }
            }
        }
        return true;
    }
    private async Task<List<ISelectionItem>> OnDropdownValueSelectSKUAtrributes(ISelectionItem selectedValue)
    {
        return await _viewModel.OnSKuAttributeDropdownValueSelect(selectedValue.UID);
    }

    private async Task OnSaveOrUpdateClick()
    {
        try
        {
            ShowLoader();
            _viewModel.Validate();
            await _viewModel.OnSaveOrUpdateClick();
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Success", ex.Message);
                _navigationManager.NavigateTo("purchaseordertemplate");
            }
            else
            {
                ShowErrorSnackBar("Failed", ex.Message);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            HideLoader();
        }
    }
    private async Task OnDeleteSelectedItems()
    {
        try
        {
            var items = _viewModel.PurchaseOrderTemplateItemViews.FindAll(e => e.IsSelected).Select(e => e.SKUUID);
            if (items == null || !items.Any()) throw new CustomException(ExceptionStatus.Failed, "Please select items..");
            if (await _alertService.ShowConfirmationReturnType("Confirmation", "Are you sure you want to delete selected items?"))
            {
                await _viewModel.OnDeleteSelectedItems();
                SKUs.RemoveAll(e => items.Contains(e.UID));
            }
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Success", ex.Message);
            }
            else
            {
                ShowErrorSnackBar("Failed", ex.Message);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
