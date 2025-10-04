using System.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.PO.BL.Interfaces;
using Winit.Modules.PO.Model.Classes;
using Winit.Modules.PO.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITMobile.Data;
using WINITMobile.Pages.Base;
using WINITMobile.Pages.PriceCheck.Models;

namespace WINITMobile.Pages
{
    public partial class CapturePO : BaseComponentBase
    {

        private WINITMobile.Pages.DialogBoxes.AddProductDialogBox<CapturePOItem> addProductDialog;
        private bool showAddDialog;
        private CapturePOItem captureItemPO { get; set; } = new CapturePOItem();    
        private List<CapturePOItem> availableProducts;
        private List<CapturePOItem> displayItems;
        private Dictionary<ISelectionItem, List<ISelectionItem>> filterDataList;
        private Func<List<FilterCriteria>, CapturePOItem, bool> filterAction;
        public string POExecutionHeaderUID = Guid.NewGuid().ToString();
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public string FolderPathImages { get; set; }

        private List<ISKU> availableProductsSKU;
        public string StoreName { get; set; } = "Mossassa Beut Al JawharaTrading Est";
        public string StoreCode { get; set; } = "96604816";
        public bool IsPageLoader = false;

        private string ImgFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
        private FileCaptureData fileCaptureData = new FileCaptureData
        {
            AllowedExtensions = new List<string> { ".jpg", ".png" },
            IsCameraAllowed = true,
            IsGalleryAllowed = true,
            MaxNumberOfItems = 1,
            MaxFileSize = 10 * 1024 * 1024,
            EmbedLatLong = true,
            EmbedDateTime = true,
            LinkedItemType = "ItemType",
            LinkedItemUID = "ItemUID",
            EmpUID = "EmployeeUID",
            JobPositionUID = "JobPositionUID",
            IsEditable = true,
            Files = new List<FileSys>()
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await base.OnInitializedAsync();
                //await PopulateViewModel();
                //availableProductsSKU = SkuItems;
                displayItems = new List<CapturePOItem>();
                //displayItems = ConvertToCapturePOItems(availableProductsSKU);
                InitializeFilterDataList();
                InitializeFilterAction();
                await LoadAvailableProducts();
                _backbuttonhandler.ClearCurrentPage();
                IsPageLoader = true;
            }
            catch (Exception)
            {

            }
            StateHasChanged();
        }
        public List<CapturePOItem> ConvertToCapturePOItems(List<ISKU> skuList)
        {
            return skuList.Select(sku => new CapturePOItem
            {
                SKUCode = sku.Code,
                SKUName = sku.Name,
                SKUImage = sku.SKUImage,
                Qty = (int)sku.Qty,
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

        //private async Task AddProducts(List<CapturePOItem> selectedProducts)
        //{
        //    if (selectedProducts?.Any() == true)
        //    {
        //        foreach (var product in selectedProducts)
        //        {

        //            // Get price data from DB
        //            await GetPriceData(product);
        //            displayItems.Add(product);

        //        }
        //    }
        //    addProductDialog.OnCloseClick();
        //    StateHasChanged();
        //}

        private async Task AddProducts(List<CapturePOItem> selectedProducts)
        {
            if (selectedProducts?.Any() == true)
            {
                foreach (var product in selectedProducts)
                {
                    if (!displayItems.Any(x => x.SKUCode == product.SKUCode))
                    {
                        // Get price data from DB             
                        await GetPriceData(product);

                        // Create a copy of the product instead of adding the same reference
                        var productCopy = new CapturePOItem // Replace 'Product' with your actual class name
                        {
                            SKUUID = product.SKUUID,
                            SKUCode = product.SKUCode,
                            SKUName = product.SKUName,
                            SKUImage = product.SKUImage,
                            Qty = product.Qty,
                            Value = product.Value,
                            // Copy all other properties you need 
                        };

                        displayItems.Add(productCopy);
                    }

                }
            }
            addProductDialog.OnCloseClick();
            StateHasChanged();
        }
        private async Task GetPriceData(CapturePOItem product)
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

        //private void UpdateShelfPrice(CapturePOItem item, ChangeEventArgs e)
        //{
        //    if (decimal.TryParse(e.Value?.ToString(), out decimal newPrice))
        //    {
        //        item.ShelfPrice = newPrice;
        //    }
        //}

        private async Task<(bool isValid, string errorMessage)> ValidateSubmission()
        {
            if (displayItems.Count == 0)
            {
                return (false, "Please add at least one item.");
            }

            if (string.IsNullOrEmpty(captureItemPO.PONumber))
            {
                return (false, "Please enter PO Number.");
            }

            if (!displayItems.All(p => p.Qty > 0))
            {
                return (false, "All items must have quantity greater than 0.");
            }

            if (!displayItems.All(p => p.Value > 0))
            {
                return (false, "All items must have value greater than 0.");
            }

            if (_appUser?.SelectedCustomer?.StoreUID == null)
            {
                return (false, "Store information is missing.");
            }

            if (_appUser?.Emp?.UID == null)
            {
                return (false, "Employee information is missing.");
            }

            return (true, string.Empty);
        }

        private async Task SubmitPOData()
        {
            try
            {
                var (isValid, errorMessage) = await ValidateSubmission();
                if (!isValid)
                {
                    await _alertService.ShowErrorAlert("Error", errorMessage);
                    return;
                }

                // Create PO execution object
                var poExecution = new POExecution
                {
                    UID = POExecutionHeaderUID,
                    SS = 1,
                    CreatedBy = _appUser.Emp.UID,
                    CreatedTime = DateTime.UtcNow,
                    ModifiedBy = _appUser.Emp.UID,
                    ModifiedTime = DateTime.UtcNow,
                    ServerAddTime = DateTime.UtcNow,
                    ServerModifiedTime = DateTime.UtcNow,
                    BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                    StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID,
                    RouteUID = _appUser.SelectedRoute.UID,
                    StoreUID = _appUser.SelectedCustomer.StoreUID,
                    JobPositionUID = _appUser.SelectedJobPosition.UID,
                    EmpUID = _appUser.Emp.UID,
                    ExecutionTime = DateTime.UtcNow,
                    PONumber = captureItemPO.PONumber,
                    LineCount = displayItems.Count,
                    QtyCount = displayItems.Sum(x => x.Qty),
                    TotalAmount = displayItems.Sum(x => x.Value * x.Qty),
                    Lines = displayItems.Select((item, index) => new POExecutionLine
                    {
                        UID = Guid.NewGuid().ToString(),
                        SS = 1,
                        LineNumber = index + 1,
                        SKUUID = item.SKUCode, /*item.SKUUID, it should be skuuid but currently uid kept wrong so taking skucode*/
                        Qty = item.Qty,
                        Price = item.Value,
                        TotalAmount = item.Value * item.Qty,
                        POExecutionUID = POExecutionHeaderUID // Will be set after header is saved
                    }).Cast<IPOExecutionLine>().ToList()
                };

                try
                {
                    var savedUID = await _poExecutionBL.CreateAsync(poExecution);
                    await SaveFileSys(savedUID);
                    if (!string.IsNullOrEmpty(savedUID))
                    {
                        StartBackgroundSync();
                        await _alertService.ShowSuccessAlert("Success", "Purchase Order captured successfully.");
                        string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");
                        if (_appUser.SelectedCustomer != null)
                        {
                            _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
                        }
                        _navigationManager.NavigateTo("CustomerCall");
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", "Failed to capture Purchase Order");
                    }
                }
                catch (ArgumentException ex)
                {
                    await _alertService.ShowErrorAlert("Validation Error", ex.Message);
                }
                catch (Exception ex)
                {
                    await _alertService.ShowErrorAlert("Error", $"Failed to capture Purchase Order: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", "Failed to capture Purchase Order");
            }
        }
        public async Task SaveFileSys(string UID)
        {
            try
            {
                if (!string.IsNullOrEmpty(UID) && ImageFileSysList != null && ImageFileSysList.Any())
                {
                    try
                    {
                        ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{POExecutionHeaderUID}");
                        await SaveCapturedImagesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task SaveCapturedImagesAsync()
        {
            // Save the images in bulk using the provided file system logic
            int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);

            // Throw an exception if the image save operation failed
            if (saveResult < 0)
            {
                throw new Exception("Failed to save the captured images.");
            }
        }
        private async Task TestPOExecution()
        {
            try
            {
                // Create sample PO data
                var samplePO = new POExecution
                {
                    UID = Guid.NewGuid().ToString(),
                    SS = -1,
                    CreatedBy = "TEST_USER",
                    CreatedTime = DateTime.UtcNow,
                    ModifiedBy = "TEST_USER",
                    ModifiedTime = DateTime.UtcNow,
                    ServerAddTime = DateTime.UtcNow,
                    ServerModifiedTime = DateTime.UtcNow,
                    BeatHistoryUID = "TEST_BEAT_HISTORY",
                    StoreHistoryUID = "TEST_STORE_HISTORY",
                    RouteUID = "TEST_ROUTE",
                    StoreUID = "TEST_STORE",
                    JobPositionUID = "TEST_JOB_POSITION",
                    EmpUID = "TEST_EMPLOYEE",
                    ExecutionTime = DateTime.UtcNow,
                    PONumber = "TEST_PO_001",
                    LineCount = 3,
                    QtyCount = 15,
                    TotalAmount = 750.00M,
                    Lines = new List<IPOExecutionLine>
                    {
                        new POExecutionLine
                        {
                            UID = Guid.NewGuid().ToString(),
                            SS = -1,
                            LineNumber = 1,
                            SKUUID = "Makhana_7-2663",
                            Qty = 5,
                            Price = 50.00M,
                            TotalAmount = 250.00M
                        },
                        new POExecutionLine
                        {
                            UID = Guid.NewGuid().ToString(),
                            SS = -1,
                            LineNumber = 2,
                            SKUUID = "Dates_4-2566",
                            Qty = 5,
                            Price = 60.00M,
                            TotalAmount = 300.00M
                        },
                        new POExecutionLine
                        {
                            UID = Guid.NewGuid().ToString(),
                            SS = -1,
                            LineNumber = 3,
                            SKUUID = "Seeds_11-2937",
                            Qty = 5,
                            Price = 40.00M,
                            TotalAmount = 200.00M
                        }
                    }
                };

                // Get BL instance
                var poExecutionBL = _serviceProvider.GetService<IPOExecutionBL>();

                // Test Create
                var savedUID = await poExecutionBL.CreateAsync(samplePO);
                if (!string.IsNullOrEmpty(savedUID))
                {
                    await _alertService.ShowSuccessAlert("Test Create", "Sample PO created successfully");

                    // Test Read
                    var retrievedPO = await poExecutionBL.GetByUIDAsync(savedUID);
                    if (retrievedPO != null && retrievedPO.Lines?.Count == 3)
                    {
                        await _alertService.ShowSuccessAlert("Test Read", "Sample PO retrieved successfully");

                        // Test Update
                        retrievedPO.PONumber = "TEST_PO_001_UPDATED";
                        retrievedPO.Lines.First().Qty = 10;
                        retrievedPO.Lines.First().TotalAmount = retrievedPO.Lines.First().Qty * retrievedPO.Lines.First().Price;
                        retrievedPO.QtyCount = retrievedPO.Lines.Sum(x => x.Qty);
                        retrievedPO.TotalAmount = retrievedPO.Lines.Sum(x => x.TotalAmount);

                        var updateSuccess = await poExecutionBL.UpdateAsync(retrievedPO);
                        if (updateSuccess)
                        {
                            await _alertService.ShowSuccessAlert("Test Update", "Sample PO updated successfully");

                            // Test Delete
                            var deleteSuccess = await poExecutionBL.DeleteAsync(savedUID);
                            if (deleteSuccess)
                            {
                                await _alertService.ShowSuccessAlert("Test Delete", "Sample PO deleted successfully");
                            }
                            else
                            {
                                await _alertService.ShowErrorAlert("Test Delete", "Failed to delete sample PO");
                            }
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert("Test Update", "Failed to update sample PO");
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Test Read", "Failed to retrieve sample PO");
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Test Create", "Failed to create sample PO");
                }
            }
            catch (ArgumentException ex)
            {
                await _alertService.ShowErrorAlert("Test Validation Error", ex.Message);
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Test Error", $"Test failed: {ex.Message}");
            }
        }

        private void OnImageDeleteClick(string fileName)
        {
            ShowLoader();
            try
            {
                IFileSys fileSys = ImageFileSysList.Find
                (e => e.FileName == fileName);
                if (fileSys is not null) ImageFileSysList.Remove(fileSys);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        private async Task OnImageCapture((string fileName, string folderPath) data)
        {
            ShowLoader();
            try
            {
                string relativePath = "";

                // Ensure UID is available and not empty
                string selectedCustomerCode = _appUser?.SelectedJobPosition?.UID;

                if (!string.IsNullOrWhiteSpace(selectedCustomerCode))
                {
                    relativePath = FileSysTemplateControles.GetCaptureCapitatorImageFolderPath(selectedCustomerCode) ?? "";
                }

                IFileSys fileSys = ConvertFileSys("CapturePO", POExecutionHeaderUID, "CapturePO", "Image",
                    data.fileName, _appUser.Emp?.Name, data.folderPath);
                fileSys.SS = -1;
                fileSys.RelativePath = relativePath;
                ImageFileSysList.Add(fileSys);
                FolderPathImages = data.folderPath;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();   
                StateHasChanged();
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
            bool result = await _imageUploadService.PostPendingImagesToServer(POExecutionHeaderUID);

            // Use passed tableGroups or default to FileSys and Merchandiser
                    var groups = tableGroups ?? new List<string>
                    {
                        DbTableGroup.FileSys,
                        DbTableGroup.Merchandiser
                    };

            // Use the reusable method from base class (no alerts since this is part of checkout flow)
            var res = await UploadDataSilent(groups);
        }
        private async Task LoadAvailableProducts()
        {
            try
            {
                // Get SKUs from SKUMaster
                List<ISKUMaster> SkuMasterItems = await _skuBL.PrepareSKUMaster(_appUser.OrgUIDs, null, null, null);

                // Filter SKUs based on AllowedSKUs if the list exists
                if (_appUser?.SelectedCustomer?.AllowedSKUs != null && _appUser.SelectedCustomer.AllowedSKUs.Any())
                {
                    SkuMasterItems = SkuMasterItems.Where(sku => _appUser.SelectedCustomer.AllowedSKUs.Contains(sku.SKU.UID)).ToList();
                }

                // Convert SKUMaster items to CapturePOItem format
                availableProducts = SkuMasterItems.Select(skuMaster => new CapturePOItem 
                { 
                    SKUCode = skuMaster.SKU.Code,
                    SKUName = skuMaster.SKU.Name,
                    UOM = skuMaster.SKU.BaseUOM,
                    LastVisitPrice = 0.0M,
                    RSP = 0.0M,
                    ShelfPrice = 0.0M,
                    SKUImage = !string.IsNullOrEmpty(skuMaster.SKU.SKUImage) 
                        ? _appConfigs.ApiDataBaseUrl + skuMaster.SKU.SKUImage 
                        : _appConfigs.ApiDataBaseUrl + "Data/SKU/no_image_available.jpg",
                    SKUUID = skuMaster.SKU.UID,
                    StoreUID = _appUser?.SelectedCustomer?.StoreUID
                }).ToList();

                /* Original hardcoded list commented out for reference
                availableProducts = new List<CapturePOItem>
                {
                    new CapturePOItem { SKUCode = "Makhana_7-2663", SKUName = "Aachari Munchies Farmley Pillow Pouch 33 g", UOM = "PCS", LastVisitPrice = 0.0M, RSP = 0.0M, ShelfPrice = 0.0M, SKUImage = _appConfigs.ApiDataBaseUrl + "Data/SKU/Makhana_7-2663.jpg", SKUUID = "Makhana_7-2663", StoreUID = StoreCode },
                    // ... rest of the hardcoded items ...
                };
                */
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}
