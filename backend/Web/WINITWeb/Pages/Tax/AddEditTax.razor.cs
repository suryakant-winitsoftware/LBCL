using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;

namespace WinIt.Pages.Tax;

partial class AddEditTax : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public string TaxUID { get; set; }
    public bool IsEditMode { get; set; }
    public ITax Tax { get; set; }
    public bool IsInitialized { get; set; }
    public List<DataGridColumn> TaxSKuMapGridColumns { get; set; }
    public List<ISelectionItem> StatusSelectionItems { get; set; }
    public List<ISelectionItem> ApplicableAtSelectionItems { get; set; }
    public List<ISelectionItem> CalculationTypeSelectionItems { get; set; }
    IDataService dataService = new DataServiceModel()
    {
        BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel()
            {
                SlNo = 1,
                Text = "View Tax Details",
                URL = "MaintainTax",
                IsClickable = true
            },
            new BreadCrumModel()
            {
                SlNo = 1, Text = "Maintain Tax Details"
            },
        }
    };
    protected async override Task OnInitializedAsync()
    {
        ShowLoader();
        LoadResources(null, _languageService.SelectedCulture);
        await GenerateTaxSKuMapGridColumns();
        TaxUID = _commonFunctions.GetParameterValueFromURL("TaxUID");
        dataService.HeaderText = $"{(TaxUID == null ? "Add Tax Details" : "Edit Tax Details")}";
        GenerateStaticDropDowns();
        await _taxViewModel.PopulateAddEditTaxPage();
        if (TaxUID == null)
        {
            TaxUID = Guid.NewGuid().ToString();
            IsEditMode = false;
            Tax = new Winit.Modules.Tax.Model.Classes.Tax();
            Tax.UID = TaxUID;
            Tax.ValidFrom = DateTime.Now;
            Tax.ValidUpto = DateTime.Now.AddYears(1);
        }
        else
        {
            IsEditMode = true;
            await _taxViewModel.GetExistingTaxWithTaxSkuMaps(TaxUID);
            Tax = _taxViewModel.GetTax();
            BindDDForEditPage(Tax);
        }

        IsInitialized = true;
        HideLoader();
    }


    private void BindDDForEditPage(ITax tax)
    {
        //status
        ISelectionItem statusSelectionItem = StatusSelectionItems.Find(e => e?.Label == tax?.Status);
        if (statusSelectionItem != null) statusSelectionItem.IsSelected = true;

        //ApplicableAt
        ISelectionItem applicableAtSelectionItem = ApplicableAtSelectionItems.Find(e => e?.Label == tax?.ApplicableAt);
        if (applicableAtSelectionItem != null) applicableAtSelectionItem.IsSelected = true;

        //CalculationType
        ISelectionItem calculationTypeSelectionItem = CalculationTypeSelectionItems.Find(e => e?.Label == tax?.TaxCalculationType);
        if (calculationTypeSelectionItem != null) calculationTypeSelectionItem.IsSelected = true;
    }
    private async Task GenerateTaxSKuMapGridColumns()
    {
        TaxSKuMapGridColumns = new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header = @Localizer["itemcode"], GetValue = s => ((ITaxSKUMapItemView)s).SKUCode
            },
            new DataGridColumn
            {
                Header = @Localizer["item_name"], GetValue = s => ((ITaxSKUMapItemView)s).SKUName
            },
            new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        Text = @Localizer["delete"], Action = item => OnTaxSKUMapDeleteClick((ITaxSKUMapItemView)item)
                    }
                }
            }
        };

    }
    public void GenerateStaticDropDowns()
    {
        StatusSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem
            {
                Label = @Localizer["active"]
            },
            new SelectionItem
            {
                Label = @Localizer["inactive"]
            },
        };
        ApplicableAtSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem
            {
                Label = @Localizer["invoice"]
            },
            new SelectionItem
            {
                Label = @Localizer["item"]
            },
        };
        CalculationTypeSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem
            {
                Label = @Localizer["percentage"]
            },

        };
    }
    public void OnTaxSKUMapDeleteClick(ITaxSKUMapItemView sKU)
    {
        sKU.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
        StateHasChanged();
    }
    public void Product_AfterCheckBoxSelection(HashSet<object> hashSet)
    {
        _taxViewModel.TaxSKUMapItemViews.ForEach(e => e.IsChecked = false);
        foreach (var item in hashSet)
        {
            ((ITaxSKUMapItemView)item).IsChecked = true;
        }
    }
    public void SelectedSKus(List<ISelectionItem> selectionItems)
    {
        _taxViewModel.OnAddSKuSelectedItems(selectionItems, TaxUID, Winit.Shared.Models.Enums.ActionType.Add);
        StateHasChanged();
    }
    public async Task OnSaveClick()
    {
        ShowLoader();
        try
        {
            if (IsEditMode)
            {
                ValidateTax(Tax);
                if (await _taxViewModel.UpdateTaxMaster(Tax))
                {
                    _tost.Add(@Localizer["tax"], "Tax updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    _navigationManager.NavigateTo("MaintainTax");
                }
                else
                {
                    _tost.Add(@Localizer["tax"], "Failed to update tax", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            else
            {
                ValidateTax(Tax);
                if (await _taxViewModel.CreateTaxMaster(Tax))
                {
                    _tost.Add(@Localizer["tax"], "Tax saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    _navigationManager.NavigateTo("MaintainTax");
                }
                else
                {
                    _tost.Add(@Localizer["tax"], "Failed to save tax", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _tost.Add(@Localizer["add_edit_tax"], ex.Message, Winit.UIComponents.SnackBar.Enum.Severity.Error);
            HideLoader();
        }
        HideLoader();
    }
    private void DeleteCheckedTaxSKUMaps()
    {
        if (_taxViewModel.TaxSKUMapItemViews != null)
        {
            _taxViewModel.TaxSKUMapItemViews.ForEach(e =>
            {
                if (e.IsChecked)
                {
                    e.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
                }
            });
        }
    }
    private void ValidateTax(ITax tax)
    {
        if (string.IsNullOrEmpty(tax.Name)) throw new Exception("Name should not be empty");
        if (string.IsNullOrEmpty(tax.LegalName)) throw new Exception("Legal name should not be empty");
        if (string.IsNullOrEmpty(tax.LegalName)) throw new Exception("Code should not be empty");
        else if (string.IsNullOrEmpty(tax.ApplicableAt)) throw new Exception("The \"Applicable At\" field has not been selected. Please ensure it is properly specified.");
        else if (string.IsNullOrEmpty(tax.Status)) throw new Exception("Status is not selected");
        else if (tax.ValidFrom == default) throw new Exception("The \"Valid From\" has not been selected. Please ensure it is provided.");
        else if (tax.ValidUpto == default) throw new Exception("The \"Valid To\" has not been entered. Please ensure it is provided.");
        else if (string.IsNullOrEmpty(tax.TaxCalculationType)) throw new Exception("The \"Tax Calculation Type\" has not been selected. Please ensure it is appropriately specified.");
        else if (tax.BaseTaxRate == default) throw new Exception("The \"Base Tax Rate\" has not been entered. Please ensure it is provided.");
        //else if (_taxViewModel.TaxSKUMapItemViews == null || !_taxViewModel.TaxSKUMapItemViews.Any()) throw new Exception("you are not selected any Skus");
    }
}
