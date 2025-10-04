using Microsoft.AspNetCore.Components;

using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using System.Reflection.Metadata;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL.Helper.Classes;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Mobile.DialogBox
{
    public partial class AddSKU
    {

        [Parameter]
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SKUAttributesDataSource { get; set; }
        [Parameter]
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUDataSource { get; set; }
        [Parameter]
        public EventCallback<List<ISelectionItem>> GetSelectedSKU { get; set; }
       // [Parameter]
       // public bool Disabled { get; set; }

        public List<Winit.Shared.Models.Common.ISelectionItem> SelectedListForCategory = new List<Winit.Shared.Models.Common.ISelectionItem>();
     
        public List<SelectionItem> DisplaySKUs = new List<SelectionItem>();
       
        public List<SelectionItem> FilterSKUs = new List<SelectionItem>();
       
        public bool selectcatbutton = false;
        
        public bool selectcatbuttonCategory = false;
       
        [Parameter]
        public bool isDialogOpen { get; set; }
           
        bool AddSelectProductbool = false;
       
        public List<Winit.Shared.Models.Common.ISelectionItem> ListForddlCatType = new List<Winit.Shared.Models.Common.ISelectionItem>();
      
        public List<Winit.Shared.Models.Common.ISelectionItem> GetSKUListForAdd = new List<Winit.Shared.Models.Common.ISelectionItem>();
      
        public int SelectedCategory = 0;
       
        public  IFilterHelper _filter = new FilterHelper();
       
        private bool CheckAllRows { get; set; } = false;
      
        public string SelectedCategoryType;
       
        List<string> _propertiesToSearch = new List<string>();
       
        // Adding items to the list
      
        protected override void OnInitialized()
        {
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Label");
            OpenAddProductDialog();
            base.OnInitialized();
            LoadResources(null, _languageService.SelectedCulture);

        }
       
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public void OnselectCategoryType(DropDownEvent dropDown)
        {
            if (dropDown != null)
            {
                try
                {

                    SelectedListForCategory.Clear();

                    foreach (var ddItem in dropDown.SelectionItems)
                    {
                        SelectedCategoryType = null;
                        SelectedCategoryType = ddItem.Label;
                        var matchingRows = SKUAttributesDataSource.Where(attr => attr.Type == ddItem.Label).ToList();
                        //SelectedListForCategory = new List<ISelectionItem>();
                        foreach (var item in matchingRows)
                        {
                            // Check if the Label already exists in SelectedListForCategory
                            //if (SelectedListForCategory != null || SelectedListForCategory.Count > 0)
                            //{
                            bool labelExists = SelectedListForCategory.Any(existingItem => existingItem.Label == item.Value);
                            //}
                            if (!labelExists)
                            {
                                SelectedListForCategory.Add(new Winit.Shared.Models.Common.SelectionItem
                                {
                                    Label = item.Value,
                                    UID = item.UID,
                                    Code = item.Code,
                                    IsSelected = false
                                });
                            }

                        }
                    }
                    selectcatbutton = false;
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }
        public void OnselectCategory(DropDownEvent dropDown)
        {
            if (dropDown != null)
            {
                try
                {
                    
                    SelectedCategory = 0;
                    DisplaySKUs.Clear();
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
                                DisplaySKUs.Add(new SelectionItem
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
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }
        public void InitialTableRows()
        {
            if (SKUDataSource != null)
            {
                foreach (var skuData in SKUDataSource)
                {
                    DisplaySKUs.Add(new SelectionItem
                    {
                        UID = skuData.UID,
                        Label = skuData.Name,
                        Code = skuData.Code
                    });
                }
                FilterSKUs = DisplaySKUs;
            }
        }

        
        private void ToggleAllRows(ChangeEventArgs e)
        {
            CheckAllRows = !CheckAllRows;

            foreach (var item in DisplaySKUs)
            {
                item.IsSelected = CheckAllRows;
                if (item.IsSelected)
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
        private void ToggleRow(SelectionItem row)
        {
            // Update the row's IsSelected property when the row checkbox is clicked
            row.IsSelected = !row.IsSelected;
            if (row.IsSelected)
            {
                // Add items to GetSKUListForAdd if they are checked
                if (!GetSKUListForAdd.Contains(row))
                {
                    GetSKUListForAdd.Add(row);
                }
            }
            else
            {
                // Remove items from GetSKUListForAdd if they are unchecked
                GetSKUListForAdd.Remove(row);
            }
        }
        private void CloseDialog()
        {
            isDialogOpen = false;
            selectcatbutton = false;
            selectcatbuttonCategory = false;
            SelectedCategory = 0;
            SelectedCategoryType = null;
            CheckAllRows = false;
            DisplaySKUs.Clear();
        }

        private async void AddSelectedProduct()
        {
            if (GetSKUListForAdd == null || GetSKUListForAdd.Count <= 0)
            {
                await _alertService.ShowErrorAlert(@Localizer["empty_alert"], @Localizer["select_atleast_one_sku"],null, @Localizer["ok"]);
            }
            else
            {
                AddSelectProductbool = true;
                isDialogOpen = false;


                GetSelectedSKU.InvokeAsync(GetSKUListForAdd);
                GetSKUListForAdd = new List<ISelectionItem>();
                SelectedCategory = 0;
                SelectedCategoryType = null;
                foreach (var item in DisplaySKUs)
                {
                    item.IsSelected = false;
                }
                CheckAllRows = false;
                DisplaySKUs.Clear();
            }
        }
        
        public async Task ApplySearch(string searchString)
        {
            try
            {

                DisplaySKUs = await _filter.ApplySearch<SelectionItem>(
                        FilterSKUs, searchString, _propertiesToSearch);
            }
            catch (Exception ex) { }
            
        }

        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
        }
        public async Task OpenAddProductDialog()
        {
            InitialTableRows();
            //if (!Disabled)
            //{
                ListForddlCatType = new List<Winit.Shared.Models.Common.ISelectionItem>();
                ListForddlCatType = FindListForddlCatType();
                GetSKUListForAdd = new List<ISelectionItem>();
                AddSelectProductbool = false;
                isDialogOpen = true;

            //}
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

            //showPromoRecords = (bool)e.Value;
            //if (showPromoRecords)
            //{
            //    OriginalDisplayedSKUList = DisplayedSKUList;
            //    DisplayedSKUList = DisplayedSKUList.Where(dItem => dItem.IsPromo).ToList();
            //}
            //else
            //{
            //    DisplayedSKUList = OriginalDisplayedSKUList;
            //}
        }
    }
}
