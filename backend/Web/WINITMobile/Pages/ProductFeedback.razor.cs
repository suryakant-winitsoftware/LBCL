using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using WINITMobile.Data;
using WINITMobile.Pages.Base;
using WINITMobile.Pages.PriceCheck.Models;

namespace WINITMobile.Pages
{
    public partial class ProductFeedback : BaseComponentBase
    {
        [Inject]
        private IProductFeedbackBL _productFeedbackBL { get; set; }

        public ProductFeedbackItem productFeedbackItem { get; set; } = new ProductFeedbackItem();
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
        public bool showOthersTextBox { get; set; }
        public List<ISelectionItem> feedbackDataSource = new List<ISelectionItem>
        {
            new SelectionItem { UID = "Size", Code = "Size", Label = "Size"},
            new SelectionItem { UID = "Smell", Code = "Smell", Label = "Smell"},
            new SelectionItem { UID = "Insect", Code = "Insect", Label = "Insect"},
            new SelectionItem { UID = "Hair", Code = "Hair", Label = "Hair"},
            new SelectionItem { UID = "Other", Code = "Other", Label = "Other"}
        };
        public string ProductFeedbackUID { get; set; } = Guid.NewGuid().ToString();
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
                    productFeedbackItem.StoreUID = dropDownEvent.SelectionItems.FirstOrDefault().UID;
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
                    productFeedbackItem.SkuUID = dropDownEvent.SelectionItems.FirstOrDefault().UID;
                }
                else
                {
                    //_viewModel.PreviousOrdersList.Clear();
                }
            }
            //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
            StateHasChanged();
        }

        private void OnFeedbackTypeSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                // await _viewModel.OnChannelpartnerSelected(dropDownEvent);
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    productFeedbackItem.FeedbackTypes = dropDownEvent.SelectionItems.Select(e => e.Label).ToList();

                }
                else
                {
                    //_viewModel.PreviousOrdersList.Clear();
                }
            }
            if (productFeedbackItem.FeedbackTypes.Count > 0 && productFeedbackItem.FeedbackTypes.Contains("Other"))
            {
                showOthersTextBox = !showOthersTextBox;
            }
            //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
            StateHasChanged();
        }

        private void RemoveFeedbackType(string type)
        {
            productFeedbackItem.FeedbackTypes.Remove(type);
            StateHasChanged();
        }

        private void OnImageDeleteClick(string fileName)
        {
            ShowLoader();
            try
            {
                IFileSys fileSys = _viewModel.ImageFileSysList.Find
                (e => e.FileName == fileName);
                if (fileSys is not null) _viewModel.ImageFileSysList.Remove(fileSys);
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

                IFileSys fileSys = ConvertFileSys("ProductFeedback", ProductFeedbackUID, "ProductFeedback", "Image",
                    data.fileName, _appUser.Emp?.Name, data.folderPath);
                fileSys.SS = -1;
                fileSys.RelativePath = relativePath;
                _viewModel.ImageFileSysList.Add(fileSys);
                _viewModel.FolderPathImages = data.folderPath;
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
                if (string.IsNullOrEmpty(productFeedbackItem.StoreUID) ||
                    string.IsNullOrEmpty(productFeedbackItem.SkuUID) ||
                    productFeedbackItem.FeedbackTypes.Count == 0)
                {
                    await _alertService.ShowErrorAlert("Error", "Please select all fields.");
                    return;
                }
                if (string.IsNullOrEmpty(productFeedbackItem.MobileNumber) || productFeedbackItem.MobileNumber.Length < 10)
                {
                    await _alertService.ShowErrorAlert("Error", "Mobile number must be at least 10 digits.");
                    return;
                }
                if (productFeedbackItem.FeedbackTypes.Contains("Other"))
                {
                    if (string.IsNullOrEmpty(productFeedbackItem.OtherRemarks))
                    {
                        await _alertService.ShowErrorAlert("Error", "Please enter remarks for Other feedback type.");
                        return;
                    }
                }

                ShowLoader();

                var feedback = new Winit.Modules.Merchandiser.Model.Classes.ProductFeedback
                {
                    Id = 0,
                    UID = ProductFeedbackUID,
                    BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                    RouteUID = _appUser.SelectedRoute?.UID,
                    JobPositionUID = _appUser.SelectedJobPosition?.UID,
                    EmpUID = _appUser.Emp?.UID,
                    ExecutionTime = DateTime.Now,
                    StoreUID = productFeedbackItem.StoreUID,
                    SKUUID = productFeedbackItem.SkuUID,
                    FeedbackOn = string.Join(", ", productFeedbackItem.FeedbackTypes),
                    EndCustomerName = productFeedbackItem.EndCustomerName,
                    MobileNo = productFeedbackItem.MobileNumber,
                    CreatedBy = _appUser.Emp?.UID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = _appUser.Emp?.UID,
                    ModifiedTime = DateTime.Now,
                    SS = 1 // For new records
                };

                // If Other feedback type is selected, append the remarks
                if (productFeedbackItem.FeedbackTypes.Contains("Other"))
                {
                    feedback.FeedbackOn += $" - {productFeedbackItem.OtherRemarks}";
                }

                var result = await _productFeedbackBL.Insert(feedback);
                await _viewModel.SaveFileSys(result, ProductFeedbackUID);
                if (result)
                {
                    StartBackgroundSync();
                    await _alertService.ShowSuccessAlert("Success", "Product Feedback submitted successfully");
                    _navigationManager.NavigateTo("DashBoard");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Failed to save Product Feedback");
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
            bool result = await _imageUploadService.PostPendingImagesToServer(ProductFeedbackUID);

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

    public class ProductFeedbackItem
    {
        public string StoreUID { get; set; }
        public string SkuUID { get; set; }
        public List<string> FeedbackTypes { get; set; } = new List<string>();
        public string OtherRemarks { get; set; }
        public string EndCustomerName { get; set; }
        public string MobileNumber { get; set; }
    }
}