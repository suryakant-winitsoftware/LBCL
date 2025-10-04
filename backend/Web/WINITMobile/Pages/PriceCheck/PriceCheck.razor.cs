using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WINITMobile.Pages.Base;
using WINITMobile.Pages.DialogBoxes;
using WINITMobile.Pages.PriceCheck.Models;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Interfaces;
using Winit.Shared.Models.Enums;

namespace WINITMobile.Pages.PriceCheck
{
    public partial class PriceCheck : BaseComponentBase
    {
        private AddProductDialogBox<PriceCheckItem> addProductDialog;
        private bool showAddDialog;
        private List<PriceCheckItem> availableProducts;
        private List<PriceCheckItem> displayItems;
        private Dictionary<ISelectionItem, List<ISelectionItem>> filterDataList;
        private Func<List<FilterCriteria>, PriceCheckItem, bool> filterAction;

        public string StoreName { get; set; } = "Mossassa Beut Al JawharaTrading Est";
        public string StoreCode { get; set; } = "96604816";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            displayItems = new List<PriceCheckItem>();
            InitializeFilterDataList();
            InitializeFilterAction();
            await LoadAvailableProducts();
        }

        private void InitializeFilterAction()
        {
            filterAction = (filterCriteria, item) =>
            {
                if (filterCriteria == null || !filterCriteria.Any())
                    return true;

                return filterCriteria.All(criteria =>
                {
                    var value = criteria.Name switch
                    {
                        "SKUCode" => item.SKUCode,
                        "SKUName" => item.SKUName,
                        _ => null
                    };

                    return !string.IsNullOrEmpty(value) && 
                           value.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase);
                });
            };
        }

        private void InitializeFilterDataList()
        {
            filterDataList = new Dictionary<ISelectionItem, List<ISelectionItem>>();
            var filterItems = new List<ISelectionItem>
            {
                new SelectionItem 
                { 
                    UID = "SKUCode",
                    Code = "SKUCode",
                    Label = "Item Code"
                },
                new SelectionItem 
                { 
                    UID = "SKUName",
                    Code = "SKUName",
                    Label = "Description"
                }
            };

            foreach (var item in filterItems)
            {
                filterDataList.Add(item, new List<ISelectionItem>());
            }
        }

        private async Task LoadAvailableProducts()
        {
            // TODO: Replace with actual API call
            availableProducts = new List<PriceCheckItem>
            {
                new PriceCheckItem 
                { 
                    SKUCode = "1046814", 
                    SKUName = "7UP FREE PET BOTTLE 2.25L",
                    UOM = "PCS",
                    LastVisitPrice = 0.23M,
                    RSP = 0.23M,
                    ShelfPrice = 0,
                    SKUUID = "1",
                    StoreUID = StoreCode
                },
               
                new PriceCheckItem 
                { 
                    SKUCode = "1046816", 
                    SKUName = "8UP FREE PET BOTTLE 2.15L",
                    UOM = "PCS",
                    LastVisitPrice = 0.28M,
                    RSP = 0.28M,
                    ShelfPrice = 0,
                    SKUUID = "2",
                    StoreUID = StoreCode
                },
                
                new PriceCheckItem 
                { 
                    SKUCode = "1046819", 
                    SKUName = "9UP FREE PET BOTTLE 1.25L",
                    UOM = "PCS",
                    LastVisitPrice = 0.13M,
                    RSP = 0.13M,
                    ShelfPrice = 0,
                    SKUUID = "3",
                    StoreUID = StoreCode
                },
                // Add more sample products...
            };
        }

        private void ShowAddProductDialog()
        {
            addProductDialog.OnOpenClick();
            StateHasChanged();
        }

        private void CancelAddProducts()
        {
            addProductDialog.OnCloseClick();
            StateHasChanged();
        }

        private async Task AddProducts(List<PriceCheckItem> selectedProducts)
        {
            if (selectedProducts?.Any() == true)
            {
                foreach (var product in selectedProducts)
                {
                    if (!displayItems.Any(x => x.SKUUID == product.SKUUID))
                    {
                        // Get price data from DB
                        await GetPriceData(product);
                        displayItems.Add(product);
                    }
                }
            }
            addProductDialog.OnCloseClick();
            StateHasChanged();
        }

        private async Task GetPriceData(PriceCheckItem product)
        {
            // TODO: Replace with actual API call
            // This method should fetch LastVisitPrice and RSP from the database
            // using StoreUID and SKUUID
            await Task.Delay(100); // Simulate API call
        }

        private void SearchItems(string searchText)
        {
            // Implement local search in displayed items
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Reset to show all items
                StateHasChanged();
                return;
            }

            // Filter the displayed items
            StateHasChanged();
        }

        private void UpdateShelfPrice(PriceCheckItem item, ChangeEventArgs e)
        {
            if (decimal.TryParse(e.Value?.ToString(), out decimal newPrice))
            {
                item.ShelfPrice = newPrice;
            }
        }

        private async Task SubmitPrices()
        {
            try
            {
                // TODO: Implement API call to save prices
                await Task.Delay(100); // Simulate API call
                await _alertService.ShowSuccessAlert("Success", "Prices updated successfully");
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error","Failed to update prices");
            }
        }
    }
} 