using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;

namespace WinIt.Pages.TaxGroup;

public partial class AddEditTaxGroup : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    [Parameter]
    public required string TaxGroupUID { get; set; }
    public bool IsEditMode { get; set; }
    public required ITaxGroup TaxGroup { get; set; }
    public bool IsInitialized { get; set; }
    Winit.UIComponents.Common.CustomControls.DropDown? dropDown;
    IDataService dataService = new DataServiceModel()
    {
        BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel(){SlNo=1,Text="Maintain Tax Group",URL="MaintainTaxGroup",IsClickable=true},
            new BreadCrumModel(){SlNo=1,Text="Maintain Tax Group Details"},
        }
    };

    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        TaxGroupUID = _commonFunctions.GetParameterValueFromURL("TaxGroupUID");
        dataService.HeaderText = $"{(TaxGroupUID == null ? "Add Tax Group" : "Edit Tax Group")}";
        if (TaxGroupUID == null)
        {
            TaxGroupUID = Guid.NewGuid().ToString();
            IsEditMode = false;
            TaxGroup = _serviceProvider.CreateInstance<ITaxGroup>();
            TaxGroup.UID = TaxGroupUID;
            await _taxGroupViewModel.PrepareAddTaxGroupPage(TaxGroup);
        }
        else
        {
            IsEditMode = true;
            await _taxGroupViewModel.GetExistingTaxGroupWithByUID(TaxGroupUID);
            TaxGroup = _taxGroupViewModel.GetTaxGroup();
        }
        
        IsInitialized = true;
        dropDown?.GetLoad();
        
    }
    
   
    public void OnTaxSKUMapDeleteClick(ITaxSKUMapItemView sKU)
    {
        sKU.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
        StateHasChanged();
    }

    public async Task OnSaveClick()
    {
        try
        {
            if (IsEditMode)
            {
                ValidateTaxGroup(TaxGroup);
                if (await _taxGroupViewModel.UpdateTaxGroupMaster(TaxGroup))
                {
                    _ = _tost.Add("Tax group", "Tax group saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _ = _tost.Add("Tax group", "Tax group saving failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            else
            {
                ValidateTaxGroup(TaxGroup);
                if (await _taxGroupViewModel.CreateTaxGroupMaster(TaxGroup))
                {
                    _ = _tost.Add("Tax group", "Tax group saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _ = _tost.Add("Tax group", "Tax group saving failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _ = _tost.Add("AddEditTaxGroup", ex.Message, Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    private void ValidateTaxGroup(ITaxGroup taxGroup)
    {
        if (string.IsNullOrEmpty(taxGroup.Name))
        {
            throw new Exception("The \"Name\" field should not be left empty. Please ensure it is filled in.");
        }
        else if (string.IsNullOrEmpty(taxGroup.Code))
        {
            throw new Exception("The \"Code\" field must not be left empty. Please ensure it is entered.");
        }
        else if (_taxGroupViewModel.TaxSelectionItemsDD == null || !_taxGroupViewModel.TaxSelectionItemsDD.Any() || _taxGroupViewModel.TaxSelectionItemsDD.Find(e => e.IsSelected) == null)
        {
            throw new Exception("Please select at least one tax to save the tax group.");
        }
    }
}
