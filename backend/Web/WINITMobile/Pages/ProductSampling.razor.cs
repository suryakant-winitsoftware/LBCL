using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using WINITMobile.Data;
using WINITMobile.Pages.Base;
using WINITMobile.Pages.PriceCheck.Models;

namespace WINITMobile.Pages
{
    public partial class ProductSampling : BaseComponentBase
    {
        [Inject]
        private IProductSamplingBL _productSamplingBL { get; set; }

        public ProductSamplingItem productSamplingItem { get; set; } = new ProductSamplingItem();
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
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public string FolderPathImages { get; set; }
        public string ProductSamplingUID { get; set; } = Guid.NewGuid().ToString();
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                await _viewModel.PopulateViewModel();
                await base.OnInitializedAsync();
                // Initialize any data needed when component loads
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", "Exception" + ex.Message);
            }
            finally
            {
                HideLoader();
            }
        }
        private void OnStoreSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                // await _viewModel.OnChannelpartnerSelected(dropDownEvent);
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    productSamplingItem.StoreUID = dropDownEvent.SelectionItems.FirstOrDefault().UID;
                }
                else
                {
                    //_viewModel.PreviousOrdersList.Clear();
                }
            }
            //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
            StateHasChanged();
        }

        private void OnSkuSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                // await _viewModel.OnChannelpartnerSelected(dropDownEvent);
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    productSamplingItem.SkuUID = dropDownEvent.SelectionItems.FirstOrDefault().UID;
                }
                else
                {
                    //_viewModel.PreviousOrdersList.Clear();
                }
            }
            //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
            StateHasChanged();
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

                IFileSys fileSys = ConvertFileSys("ProductSampling", ProductSamplingUID, "ProductSampling", "Image",
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
        private async Task HandleSubmit()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(productSamplingItem.StoreUID) ||
                    string.IsNullOrEmpty(productSamplingItem.SkuUID) ||
                    string.IsNullOrEmpty(productSamplingItem.SellingPrice) ||
                    string.IsNullOrEmpty(productSamplingItem.UnitsUsed) ||
                    string.IsNullOrEmpty(productSamplingItem.UnitsSold) ||
                    (productSamplingItem.NoOfCustomersApproached == 0))
                {
                    await _alertService.ShowErrorAlert("Error", "Please fill all required fields.");
                    return;
                }

                // Parse numeric values
                if (!decimal.TryParse(productSamplingItem.SellingPrice, out decimal sellingPrice) ||
                    !int.TryParse(productSamplingItem.UnitsUsed, out int unitsUsed) ||
                    !int.TryParse(productSamplingItem.UnitsSold, out int unitsSold))
                {
                    await _alertService.ShowErrorAlert("Error", "Invalid numeric values provided.");
                    return;
                }

                ShowLoader();

                // Create new product sampling object
                var productSampling = new Winit.Modules.Merchandiser.Model.Classes.ProductSampling
                {
                    UID = ProductSamplingUID,
                    SS = 1, // New record
                    CreatedBy = _appUser?.Emp?.UID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = _appUser?.Emp?.UID,
                    ModifiedTime = DateTime.Now,
                    BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                    RouteUID = _appUser.SelectedRoute?.UID,
                    JobPositionUID = _appUser.SelectedJobPosition?.UID,
                    EmpUID = _appUser.Emp?.UID,
                    ExecutionTime = DateTime.Now,
                    StoreUID = productSamplingItem.StoreUID,
                    SKUUID = productSamplingItem.SkuUID,
                    SellingPrice = sellingPrice,
                    UnitUsed = unitsUsed,
                    UnitSold = unitsSold,
                    NoOfCustomerApproached = productSamplingItem.NoOfCustomersApproached
                };

                // Call BL to insert the record
                var result = await _productSamplingBL.Insert(productSampling);

                if (result)
                {
                    // If images were captured, save them
                    if (ImageFileSysList != null && ImageFileSysList.Any())
                    {
                        try
                        {
                            ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{ProductSamplingUID}");
                            await SaveCapturedImagesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                            throw;
                        }
                    }
                    StartBackgroundSync();
                    await _alertService.ShowSuccessAlert("Success", "Product Sampling submitted successfully");
                    _navigationManager.NavigateTo("DashBoard");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Failed to submit Product Sampling");
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", $"An error occurred: {ex.Message}");
            }
            finally
            {
                HideLoader();
                StateHasChanged();
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
            bool result = await _imageUploadService.PostPendingImagesToServer(ProductSamplingUID);

            // Use passed tableGroups or default to FileSys and Merchandiser
            var groups = tableGroups ?? new List<string>
            {
                DbTableGroup.FileSys,
                DbTableGroup.Merchandiser
            };

            // Use the reusable method from base class (no alerts since this is part of checkout flow)
            var res = await UploadDataSilent(groups);
        }
    }
}

