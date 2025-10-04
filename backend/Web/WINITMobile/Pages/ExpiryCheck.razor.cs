using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITMobile.Pages.Base;
using WINITMobile.Pages.PriceCheck.Models;

namespace WINITMobile.Pages
{
    public partial class ExpiryCheck : BaseComponentBase
    {
        private WINITMobile.Pages.DialogBoxes.AddProductDialogBox<ExpiryCheckItem> addProductDialog;
        private bool showAddDialog;


        private List<ExpiryCheckItem> displayItems;
        private Dictionary<ISelectionItem, List<ISelectionItem>> filterDataList;
        private Func<List<FilterCriteria>, ExpiryCheckItem, bool> filterAction;


        private List<ISKU> availableProductsSKU;
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            try
            {
                await base.OnInitializedAsync();
                await _expiryCheckViewModel.PopulateViewModel();
                displayItems = new List<ExpiryCheckItem>();
                InitializeFilterDataList();
                InitializeFilterAction();
                _backbuttonhandler.ClearCurrentPage();
            }
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        //public async Task<string> TestExpiryCheckCreation()
        //{
        //    try
        //    {
        //        // Test data with different scenarios for line items
        //        var testLineData = new[]
        //        {
        //            new { SkuCode = "Makhana_7-2745", Qty = 10.5m, ExpiryDate = DateTime.Now.AddDays(-10), Comment = "Already expired" },
        //            new { SkuCode = "Almonds_1-1748", Qty = 5.0m, ExpiryDate = DateTime.Now.AddDays(15), Comment = "Near expiry" },
        //            new { SkuCode = "Cashewnut_3-1988", Qty = 20.0m, ExpiryDate = DateTime.Now.AddMonths(1), Comment = "Expiring next month" },
        //            new { SkuCode = "Dates_4-30567", Qty = 15.0m, ExpiryDate = DateTime.Now.AddMonths(3), Comment = "Good shelf life" },
        //            new { SkuCode = "Dates_4-30567", Qty = 8.0m, ExpiryDate = DateTime.Now.AddMonths(6), Comment = "Long shelf life" }
        //        };

        //        // Create the header object
        //        IExpiryCheckExecution expiryCheckHeader = new ExpiryCheckExecution
        //        {
        //            Id = 0,
        //            UID = Guid.NewGuid().ToString(),
        //            CreatedBy = _appUser.Emp.UID,
        //            ModifiedBy = _appUser.Emp.UID,
        //            CreatedTime = DateTime.Now,
        //            ModifiedTime = DateTime.Now,
        //            ServerAddTime = DateTime.Now,
        //            ServerModifiedTime = DateTime.Now,
        //            BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
        //            StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID,
        //            RouteUID = _appUser.SelectedRoute.UID,
        //            StoreUID = _appUser.SelectedCustomer.StoreUID,
        //            JobPositionUID = _appUser.SelectedJobPosition.UID,
        //            EmpUID = _appUser.Emp.UID,
        //            ExecutionTime = DateTime.Now,
        //            SS = 1 // Active status
        //        };


        //        // Call the BL to create the record
        //        string createdUID = await _expiryCheckExecutionBL.CreateAsync(expiryCheckHeader);

        //        if (!string.IsNullOrEmpty(createdUID))
        //        {
        //            await _alertService.ShowSuccessAlert("Success", $"Created expiry check with {expiryCheckHeader.Lines.Count} items");
        //            return createdUID;
        //        }
        //        else
        //        {
        //            await _alertService.ShowErrorAlert("Error", "Failed to create expiry check");
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _alertService.ShowErrorAlert("Error", $"Error creating expiry check: {ex.Message}");
        //        return null;
        //    }
        //}
        public List<ExpiryCheckItem> ConvertToExpiryCheckItems(List<ISKU> skuList)
        {
            return skuList.Select(sku => new ExpiryCheckItem
            {
                SKUCode = sku.Code,
                SKUName = sku.Name,
                SKUImage = sku.SKUImage,
                Qty = sku.Qty,
                UOM = sku.BaseUOM
            }).ToList();
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
        private async Task AddProducts(List<ExpiryCheckItem> selectedProducts)
        {
            if (selectedProducts?.Any() == true)
            {
                foreach (var product in selectedProducts)
                {
                    // Get price data from DB             
                    await GetPriceData(product);
                    var productCopy = product.DeepClone();
                    displayItems.Add(productCopy);
                }
            }
            addProductDialog.OnCloseClick();
            StateHasChanged();
        }

        private async Task GetPriceData(ExpiryCheckItem product)
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
        public void ConvertIntoExecutionLine()
        {
            foreach (var data in displayItems)
            {
                // Find the SKU details from available products
                var skuDetails = _expiryCheckViewModel.availableProducts.FirstOrDefault(p => p.SKUCode == data.SKUCode);
                if (skuDetails == null) continue;

                // Create line item
                var line = new ExpiryCheckExecutionLine
                {
                    Id = 0,
                    UID = Guid.NewGuid().ToString(),
                    ExpiryCheckExecutionUID = _expiryCheckViewModel.expiryCheckHeader.UID,
                    LineNumber = displayItems.ToList().IndexOf(data) + 1,
                    SKUUID = skuDetails.SKUUID,
                    Qty = data.Qty,
                    ExpiryDate = data.ExpiryDate,
                    SS = 1 // Active status
                };

                _expiryCheckViewModel.expiryCheckHeader.Lines.Add(line);
            }
        }

        private async Task OnSubmitExpiryData()
        {
            try
            {
                if (displayItems.Count > 0 && displayItems.All(p => p.Qty > 0 && p.ExpiryDate != null))
                {
                    ConvertIntoExecutionLine();
                    string createdUID = await _expiryCheckViewModel.OnSubmitExpiryCheck();

                    if (!string.IsNullOrEmpty(createdUID))
                    {
                        StartBackgroundSync();
                        await _alertService.ShowSuccessAlert("Success", $"Ageing/Near Expiry Data submitted successfully.");
                        string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");
                        if (_appUser.SelectedCustomer != null)
                        {
                            _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
                        }
                        _navigationManager.NavigateTo("CustomerCall");
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", "Failed to save Ageing/Near Expiry");
                    }
                }
                else if (displayItems.Count == 0)
                {
                    await _alertService.ShowErrorAlert("Error", "Please select atleast one item.");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please ensure all selected items have both Quantity and Expiry Date filled.");
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", $"Failed to save Ageing/Near Expiry: {ex.Message}");
            }
        }
        public void StartBackgroundSync(List<string> tableGroups = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    await SyncDataInBackGround(tableGroups);
                }
                catch (Exception ex)
                {
                    // Log the exception since it won't bubble up
                    // Logger.LogError(ex, "Background sync failed");
                    Console.WriteLine($"Background sync error: {ex.Message}");
                }
            });
        }
        public async Task SyncDataInBackGround(List<string> tableGroups = null)
        {
            // Use passed tableGroups or default to FileSys and Merchandiser
            var groups = tableGroups ?? new List<string>
                    {
                        DbTableGroup.Merchandiser
                    };

            // Use the reusable method from base class (no alerts since this is part of checkout flow)
            var res = await UploadDataSilent(groups);
        }
    }
}
