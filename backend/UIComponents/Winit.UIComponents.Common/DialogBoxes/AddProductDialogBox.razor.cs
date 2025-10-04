using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using System.Reflection.Metadata;
using Winit.Modules.Common.BL;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Common.DialogBoxes;

public partial class AddProductDialogBox
{
    [Parameter]
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SKUAttributesDataSource { get; set; }
    [Parameter]
    public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUDataSource { get; set; }
    [Parameter]
    public EventCallback<List<ISelectionItem>> GetSelectedSKU { get; set; }
    [Parameter]
    public bool Disabled { get; set; }
    [Parameter]
    public bool DisableAddButton { get; set; }
    [Parameter]
    public EventCallback OnDialogClose { get; set; }
    public List<Winit.Shared.Models.Common.ISelectionItem> SelectedListForCategory = new List<Winit.Shared.Models.Common.ISelectionItem>();
    public List<ExtendedSelectionItem> skuLIstForTable = new List<ExtendedSelectionItem>();
    public List<ExtendedSelectionItem> OriginalSKUList = new List<ExtendedSelectionItem>();
    public bool selectcatbutton = false;
    public bool selectcatbuttonCategory = false;
    public bool isDialogOpen { get; set; }
    bool AddSelectProductbool = false;
    public List<Winit.Shared.Models.Common.ISelectionItem> ListForddlCatType = new List<Winit.Shared.Models.Common.ISelectionItem>();
    public List<Winit.Shared.Models.Common.ISelectionItem> GetSKUListForAdd = new List<Winit.Shared.Models.Common.ISelectionItem>();
    private string searchTerm = "";
    public int SelectedCategory = 0;
    private bool CheckAllRows { get; set; } = false;
    public string SelectedCategoryType;
    private bool IsAllSelected { get; set; }

    protected override async Task OnInitializedAsync()
    {
        InitialTableRows();
        if (DisableAddButton) await OpenAddProductDialog();
        LoadResources(null, _languageService.SelectedCulture);
    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    public void OnselectCategory(DropDownEvent dropDown)
    {

        SelectedCategory = 0;
        skuLIstForTable.Clear();
        foreach (var ddItem in dropDown.SelectionItems)
        {
            var matchingRows = SKUAttributesDataSource.Where(attr => attr.Value == ddItem.Label).GroupBy(attr => attr.SKUUID).Select(group => group.First()).ToList();
            // var matchingRows = SkuAttributesOriginal.Where(attr => attr.Value == ddItem.Label).ToList();
            SelectedCategory = SelectedCategory + 1;
            foreach (var matchingRow in matchingRows)
            {
                var matchingUid = SKUDataSource.FirstOrDefault(item => item.UID == matchingRow.SKUUID);
                if (matchingUid != null)
                {
                    // Add the matchingUid and related row from hardList to selectionList
                    skuLIstForTable.Add(new ExtendedSelectionItem
                    {
                        UID = matchingUid.UID,
                        Label = matchingUid.Name,
                        Code = matchingUid.Code,
                    });
                }
            }
        }
        selectcatbuttonCategory = false;
    }
    public void InitialTableRows()
    {
        if (SKUDataSource != null)
        {
            foreach (var skuData in SKUDataSource)
            {
                skuLIstForTable.Add(new ExtendedSelectionItem
                {
                    UID = skuData.UID,
                    Label = skuData.Name,
                    Code = skuData.Code
                });
            }
            OriginalSKUList = skuLIstForTable;
        }
    }

    public void OnselectCategoryType(DropDownEvent dropDown)
    {
        foreach (var ddItem in dropDown.SelectionItems)
        {
            SelectedCategoryType = null;
            SelectedCategoryType = ddItem.Label;
            var matchingRows = SKUAttributesDataSource.Where(attr => attr.Type == ddItem.Label).ToList();

            foreach (var item in matchingRows)
            {
                // Check if the Label already exists in SelectedListForCategory
                bool labelExists = SelectedListForCategory.Any(existingItem => existingItem.Label == item.Value);
                if (!labelExists)
                {
                    SelectedListForCategory.Add(new Winit.Shared.Models.Common.SelectionItem
                    {
                        Label = item.Value,
                        UID = item.Code,
                        Code = item.Code,
                        IsSelected = false
                    });
                }
            }
        }
        selectcatbutton = false;
    }

    private void CloseDialog()
    {
        isDialogOpen = false;
        selectcatbutton = false;
        selectcatbuttonCategory = false;
        SelectedCategory = 0;
        SelectedCategoryType = null;
        CheckAllRows = false;
        if (DisableAddButton)
        {
            OnDialogClose.InvokeAsync();
        }
    }

    private void btnSelectCategoryType()
    {
        selectcatbutton = true;
        SelectedListForCategory = new List<Winit.Shared.Models.Common.ISelectionItem>();
        SelectedCategoryType = null;
    }

    private void btnSelectCategory()
    {
        selectcatbuttonCategory = true;
    }
    private void AddSelectedProduct()
    {
        AddSelectProductbool = true;
        isDialogOpen = false;
        SelectionManager selectionManager = new SelectionManager(skuLIstForTable.OfType<ISelectionItem>().ToList(), Shared.Models.Enums.SelectionMode.Multiple);
        GetSelectedSKU.InvokeAsync(selectionManager.GetSelectedSelectionItems());
        GetSKUListForAdd = new List<ISelectionItem>();
        foreach (var item in skuLIstForTable)
        {
            item.IsSelected = false;
        }
        CheckAllRows = false;
    }
    //private void HandleRowSelectionChange(ExtendedSelectionItem context, bool newValue)
    //{
    //    context.IsSelected = newValue;
    //    if (newValue)
    //    {
    //        if (!GetSKUListForAdd.Contains(context))
    //        {
    //            GetSKUListForAdd.Add(context);
    //        }
    //    }
    //    else
    //    {
    //        if (GetSKUListForAdd.Contains(context))
    //        {
    //            GetSKUListForAdd.Remove(context);
    //        }
    //    }
    //}
    private void HandleRowSelectionChange()
    {
        IsAllSelected = !IsAllSelected;
        skuLIstForTable.ForEach(e => e.IsSelected = IsAllSelected);
    }

    private void SearchAmongSelectedProduct(string e)
    {
        searchTerm = e;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // If the search term is empty, reset the list to the original data
            skuLIstForTable = new List<ExtendedSelectionItem>(OriginalSKUList);
        }
        else
        {
            // Filter a copy of the original data based on the search term
            skuLIstForTable = OriginalSKUList
                .Where(product =>
                    product.UID.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    product.Label.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    product.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }

    private void HandleAllSelectionChange(bool newValue)
    {
        CheckAllRows = newValue;
        foreach (var item in skuLIstForTable)
        {
            if (item.IsSelected != newValue)
            {
                item.IsSelected = newValue;

                if (newValue)
                {
                    // Add items to GetSKUListForAdd if they are checked
                    if (!GetSKUListForAdd.Contains(item))
                    {
                        GetSKUListForAdd.Add(item);
                    }
                }
                else
                {
                    // Remove items from GetSKUListForAdd if they are unchecked
                    GetSKUListForAdd.Remove(item);
                }
            }
        }
    }


    private async Task OpenAddProductDialog()
    {
        if (!Disabled)
        {
            ListForddlCatType = new List<Winit.Shared.Models.Common.ISelectionItem>();
            ListForddlCatType = FindListForddlCatType();
            GetSKUListForAdd = new List<ISelectionItem>();
            AddSelectProductbool = false;
            isDialogOpen = true;
        }
    }

    public List<Winit.Shared.Models.Common.ISelectionItem> FindListForddlCatType()
    {
        // Create a list to store the distinct types
        List<ISelectionItem> distinctTypes = new List<ISelectionItem>();
        foreach (var skuAttribute in SKUAttributesDataSource)
        {
            // Check if the "Type" is not already in the distinctTypes list
            if (distinctTypes.All(item => item.UID != skuAttribute.Type))
            {
                // Append the distinct "Type" to the list
                distinctTypes.Add(new SelectionItem
                {
                    UID = skuAttribute.Type,
                    Code = skuAttribute.Type,
                    Label = skuAttribute.Type,
                    IsSelected = false
                });
            }
        }
        return distinctTypes;
    }

    //this method for IsPromo
    void OnCheckboxChange(ChangeEventArgs e)
    {

    }
}
public class ExtendedSelectionItem : SelectionItem
{
    public bool IsPromo { get; set; }
}

