using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using WINITMobile.Data;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages
{
    public partial class OSOI : BaseComponentBase
    {
        private string selectedAssetType = "";
        private string PlanogramUID = Guid.NewGuid().ToString();
        public bool IsShelfArranged { get; set; }
        public bool IsShelfNotArranged { get; set; }
        [Parameter]
        public string From { get; set; }
        public bool IsPlanogram { get; set; }
        public string FileKey { get; set; } = "";
        private string ImgFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
        private Dictionary<string, List<IFileSys>> CapturedImagesByAssetType = new();
        private bool popupVisible = false;
        private IFileSys selectedImage;
        private IPlanogramSetupV1 planogramSetupV1 { get; set; } = new PlanogramSetupV1();
        public List<IFileSys> holdImg { get; set; } = new List<IFileSys>();
        public PlanogramExecutionV1 planogramExecutionObj = new PlanogramExecutionV1();
        public string Noimage { get; set; } = "/Images/noimage.png";
        public string planogramUID { get; set; } = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            try
            {
                if (From == "planogram")
                {
                    IsPlanogram = true;
                }
                if (IsPlanogram)
                {
                    Dictionary<string, List<string>> storeLinkedItemUIDs = await _selectionMapCriteriaBL.GetLinkedItemUIDByStore(LinkedItemType.PlanogramV1, new List<string>() { _appUser.SelectedCustomer.StoreUID });
                    if (storeLinkedItemUIDs == null || storeLinkedItemUIDs.Count == 0)
                    {
                        await _alertService.ShowErrorAlert("Error", "No planogram setup available. Please contact your manager");
                        _navigationManager.NavigateTo("CustomerCall");
                        return;
                    }
                    planogramUID = storeLinkedItemUIDs.Values.FirstOrDefault()?.FirstOrDefault();
                    if (string.IsNullOrEmpty(planogramUID))
                    {
                        await _alertService.ShowErrorAlert("Error", "No planogram setup available. Please contact your manager");
                        _navigationManager.NavigateTo("CustomerCall");
                        return;
                    }
                    planogramSetupV1 = await _planogramViewModel.GetPlanogramSetupByUID(planogramUID);
                }
                PrepareObjectForPlanogramOSOI(IsPlanogram, planogramUID);
                _backbuttonhandler.ClearCurrentPage();
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
        public List<ISelectionItem> assetTypeDataSource = new List<ISelectionItem>
        {
            new SelectionItem { UID = "Thaapi", Code = "Thaapi", Label = "Thaapi"},
            new SelectionItem { UID = "Endcap", Code = "Endcap", Label = "Endcap"},
            new SelectionItem { UID = "Bin", Code = "Bin", Label = "Bin"},
            new SelectionItem { UID = "POS", Code = "POS", Label = "POS"},
            new SelectionItem { UID = "Aisel", Code = "Aisel", Label = "Aisel"},
        };
        private void OnFeedbackTypeSelected(DropDownEvent dropDownEvent)
        {
            FileKey = Guid.NewGuid().ToString();

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    selectedAssetType = dropDownEvent.SelectionItems.FirstOrDefault()?.Label;

                    // Initialize list for selected asset type if not already
                    if (!CapturedImagesByAssetType.ContainsKey(selectedAssetType))
                        CapturedImagesByAssetType[selectedAssetType] = new List<IFileSys>();
                }
                else
                {
                    // Nothing selected — reset selectedAssetType
                    selectedAssetType = null;
                }
            }

            StateHasChanged();
        }

        private void OnRadioChange(string action)
        {
            if (action == "Yes")
            {
                IsShelfArranged = true;
                IsShelfNotArranged = false;
            }
            else if (action == "No")
            {
                IsShelfNotArranged = true;
                IsShelfArranged = false;
            }

            // Optional: store the selected gender as a string too
            //selectedGender = gender;
        }
        private async Task OnPlanogramOSOISubmit()
        {
            ShowLoader();
            try
            {

                if (CapturedImagesByAssetType != null &&
        CapturedImagesByAssetType.Any(g => g.Value != null && g.Value.Any()))
                {
                    bool result = await _planogramViewModel.OnPlanogramOSOISubmit(planogramExecutionObj);
                    if (result)
                    {
                        StartBackgroundSync();
                        await _alertService.ShowSuccessAlert("Success", $"{(!IsPlanogram ? "OSOI" : "Planogram")} submitted successfully");
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", $"Failed to submit {(!IsPlanogram ? "OSOI" : "Planogram")}. Please try again.");
                    }

                    string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");

                    if (_appUser.SelectedCustomer != null)
                    {
                        _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
                    }

                    _navigationManager.NavigateTo("CustomerCall");
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please add atleast one activity");
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Exception", ex.Message);
                // Optionally log it
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }

        public void PrepareObjectForPlanogramOSOI(bool isPlanogram, string planogramV1UID)
        {
            planogramExecutionObj = new PlanogramExecutionV1
            {
                JobPositionUID = _appUser.SelectedJobPosition.UID,
                EmpUID = _appUser.Emp.UID,
                BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                RouteUID = _appUser.SelectedRoute.UID,
                StoreHistoryUID = _appUser?.SelectedCustomer?.StoreHistoryUID,
                StoreUID = _appUser?.SelectedCustomer?.StoreUID,
                ScreenName = isPlanogram ? "Planogram" : "OSOI",
                PlanogramSetupV1UID = isPlanogram ? planogramV1UID : null,
                ExecutionTime = DateTime.Now,
                UID = PlanogramUID,
                CreatedBy = _appUser.Emp.UID,
                CreatedTime = DateTime.Now,
                ModifiedBy = _appUser.Emp.UID,
                ModifiedTime = DateTime.Now,
                SS = 1, // Assuming SS is a status or some identifier, set it to -1 initially
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now
            };
        }
        private void OnImageDeleteClick(string fileName)
        {
            IFileSys fileSys = _planogramViewModel.ImageFileSysList.Find
            (e => e.FileName == fileName);
            if (fileSys is not null) _planogramViewModel.ImageFileSysList.Remove(fileSys);
        }
        //private string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Images");
        private async Task OnImageCapture((string fileName, string folderPath) data)
        {
            ShowLoader();

            string relativePath = "";

            string selectedCustomerCode = _appUser?.SelectedJobPosition?.UID;

            if (!string.IsNullOrWhiteSpace(selectedCustomerCode))
            {
                relativePath = FileSysTemplateControles.GetPlanogramImageFolderPath(selectedCustomerCode) ?? "";
            }
            try
            {
                IFileSys fileSys = ConvertFileSys(IsPlanogram ? "Planogram" : "OSOI", PlanogramUID, selectedAssetType, "Image",
           data.fileName, _appUser.Emp?.Name, data.folderPath);

                string fullPath = Path.Combine(data.folderPath, data.fileName);
                if (File.Exists(fullPath))
                {
                    byte[] bytes = await File.ReadAllBytesAsync(fullPath);
                    fileSys.FileData = Convert.ToBase64String(bytes);
                }

                fileSys.SS = -1;
                fileSys.RelativePath = relativePath;
                _planogramViewModel.ImageFileSysList.Add(fileSys);
                holdImg.Add(fileSys);
                SaveCapturedImage();

                // _viewModel.FolderPathImages = folderPath;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", "Failed to capture image");
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        private void DeleteCapturedImage(string assetType, string fileName)
        {
            ShowLoader();
            try
            {
                if (CapturedImagesByAssetType.TryGetValue(assetType, out var list))
                {
                    var file = list.FirstOrDefault(f => f.FileName == fileName);
                    if (file != null)
                    {
                        list.Remove(file);
                        OnImageDeleteClick(fileName);
                    }
                }
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
        private void SaveCapturedImage()
        {

            if (holdImg.Count > 0)
            {
                if (!CapturedImagesByAssetType.ContainsKey(selectedAssetType))
                    CapturedImagesByAssetType[selectedAssetType] = new List<IFileSys>();

                CapturedImagesByAssetType[selectedAssetType].AddRange(holdImg);
                holdImg = new List<IFileSys>();
            }
            StateHasChanged();
        }
        private FileCaptureData fileCaptureData = new FileCaptureData
        {
            AllowedExtensions = new List<string> { ".jpg", ".png" },
            IsCameraAllowed = true,
            IsGalleryAllowed = true,
            MaxNumberOfItems = 5,
            MaxFileSize = 10 * 1024 * 1024, // 10 MB
            EmbedLatLong = true,
            EmbedDateTime = true,
            LinkedItemType = "Planogram",
            LinkedItemUID = "PlanogramUID",
            EmpUID = "EmployeeUID",
            JobPositionUID = "JobPositionUID",
            IsEditable = true,
            Files = new List<FileSys>()
        };
        private void ShowImage(IFileSys file)
        {
            // Show the selected image in a popup (using Blazor component state)
            selectedImage = file;
            popupVisible = true;
            //StateHasChanged();  // Request a re-render after state change
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
            bool result = await _imageUploadService.PostPendingImagesToServer(PlanogramUID);

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
