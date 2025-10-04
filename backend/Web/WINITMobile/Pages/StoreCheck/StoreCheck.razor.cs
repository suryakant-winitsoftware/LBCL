
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WINITMobile.Models.TopBar;
using Winit.Shared.Models.Common;
using Winit.Modules.Common.BL;
using Winit.Shared.Models.Events;
using WINITMobile.Pages.Base;
using Microsoft.AspNetCore.Components;
using Winit.UIComponents.Common.CustomControls;
using Winit.Shared.Models.Enums;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Modules.CollectionModule.BL.Classes.CreatePayment;
using Winit.Modules.FileSys.Model.Interfaces;
using WINITMobile.Data;
using Winit.Shared.CommonUtilities.Common;



namespace WINITMobile.Pages.StoreCheck
{
    public partial class StoreCheck : BaseComponentBase, IDisposable
    {
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

        private WinitTextBox wtbSearch;

        bool IsStoreCheckBeforeImageMandatory = true;

        bool IsStoreCheckBeforeImageClickDone = false;
        public string ImgFolderPath { get; set; } = Path.Combine(FileSystem.AppDataDirectory, "Images");
        public bool IsCaputureImage { get; set; } = false;
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();

        private IStoreCheckItemHistoryViewList StoreRowUomDataSource;
        private List<SelectionItemFilter> TopScrollSelectionItems = new();

        private IStoreCheckItemHistoryViewList DRERowDataSource;
        private FileCaptureData fileCaptureData = new FileCaptureData
        {
            AllowedExtensions = new List<string> { ".jpg", ".png" },
            IsCameraAllowed = true,
            IsGalleryAllowed = true,
            MaxNumberOfItems = 5,
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

        protected override void OnInitialized()
        {
            _backbuttonhandler.ClearCurrentPage();
            _dataManager.SetData("StoreCheck", _appUser.SelectedCustomer.Code + "_" + sixGuidstring());
            //ImgFolderPath = Path.Combine(_appConfigs.BaseFolderPath,
            //FileSysTemplateControles.GetReturnOrderImageFolderPath(_dataManager.GetData("StoreCheck").ToString()));
            TopScrollSelectionItems = new List<SelectionItemFilter>()
            {

                new Winit.Shared.Models.Common.SelectionItemFilter
                {
                    UID = "Focus",
                    Label = "Focus",
                    Code = "Focus",
                    ImgPath = "/Images/icon_star.png"
                },
                /*
                new Winit.Shared.Models.Common.SelectionItemFilter
                {
                    Label = @Localizer["promo"],
                    Code = "IsPromo",
                    ImgPath = "/Images/icon_tag.png"
                },
                new Winit.Shared.Models.Common.SelectionItemFilter
                {
                    Label = @Localizer["new"],
                    Code = "New",
                    ImgPath = "/Images/icon_tag.png"
                }
                */
            };
            base.OnInitialized();
        }
        protected override async Task OnInitializedAsync()
        {
            /*
            if (await _alertService.ShowConfirmationReturnType("Alert !", "Please Capture Before Image to Continue"))
            {
                IsCaputureImage = true;
                await Task.Delay(100);
                IsCaputureImage = false;
            }
            */

            await _AddEditStoreCheck.PopulateViewModel();
            await _AddEditStoreCheck.GetStoreCheckHistory(_AddEditStoreCheck.BeatHistoryUID, _AddEditStoreCheck.StoreHistoryUID);
            if (_AddEditStoreCheck.DisplayStoreCheckHistoryView == null)
            {
                _AddEditStoreCheck.CreateUpdateStoreCheck(_AddEditStoreCheck.BeatHistoryUID, _AddEditStoreCheck.StoreHistoryUID);
                await _AddEditStoreCheck.GetStoreCheckHistory(_AddEditStoreCheck.BeatHistoryUID, _AddEditStoreCheck.StoreHistoryUID);
            }
            await _AddEditStoreCheck.GetStoreCheckItemHistory(_AddEditStoreCheck.DisplayStoreCheckHistoryView.UID);

            //7await SetTopBar();
            LoadResources(null, _languageService.SelectedCulture);

        }

        public string sixGuidstring()
        {
            Guid newGuid = Guid.NewGuid();

            // Convert the GUID to a string and take the first 8 characters without hyphens
            string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);
            return eightDigitGuid;
        }
        //async Task SetTopBar()
        //{
        //    MainButtons buttons = new MainButtons()
        //    {
        //        TopLabel = @Localizer["store_check"],
        //        UIButton1 = new Models.TopBar.Buttons
        //        {
        //            Action = CaptureStore,
        //            ButtonText = @Localizer["capture_store"],
        //            ButtonType = Models.TopBar.ButtonType.Text,
        //            IsVisible = true
        //        },

        //    };
        //    await Btnname.InvokeAsync(buttons);
        //}

        public async Task WinitTextBox_OnSearch(string searchString)
        {
            _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView = await _AddEditStoreCheck.ItemSearch(searchString);
            // StateHasChanged();
        }
        private void AfterBarcodeScanned(string scannedText)
        {
            wtbSearch.UpdateValue(scannedText);
        }

        public async Task CaptureStore()
        {

            IsCaputureImage = false;

            await Task.Yield();
        }
        private void OnUomChange(ChangeEventArgs e, IStoreCheckItemHistoryViewList item)
        {
            string selectedUOM = e.Value.ToString();
            item.UOM = selectedUOM;
            item = UpdateStoreCheckItemProperties(item);
        }
        private void CloneRow(IStoreCheckItemHistoryViewList item, bool isBackStore)
        {
            var clonedItem = item.Clone();
            clonedItem.Uid = Guid.NewGuid().ToString();
            if (isBackStore)
            {
                clonedItem.BackStoreQty = item.BackStoreQty;
            }
            else
            {
                clonedItem.StoreQty = item.StoreQty;
            }
            _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView.Add(clonedItem);
        }

        //private async Task OnQtyRowBtnClick(IStoreCheckItemHistoryViewList row, bool IsBackStoreBtnClick)
        //{
        //    row.IsBackStoreBtnClick = IsBackStoreBtnClick;
        //    if(row.StoreCheckBaseQtyDetails.StoreCheckItemHistoryUid == null)
        //    {
        //        row.StoreCheckBaseQtyDetails = await _AddEditStoreCheck.GetStoreCheckItemUomBaseQty(row);
        //    }
        //    if (row.StoreCheckOuterQtyDetails.StoreCheckItemHistoryUid == null)
        //    {
        //        row.StoreCheckOuterQtyDetails = await _AddEditStoreCheck.GetStoreCheckItemUomOuterQty(row);
        //    }
        //    StoreRowUomDataSource = row.Clone() as StoreCheckItemHistoryViewList; 

        //}
        private async Task OnQtyRowBtnClick(IStoreCheckItemHistoryViewList item, bool isBackStore)
        {
            item.IsBackStoreBtnClick = isBackStore;
            // Load base and outer qty details as needed
            if (item.StoreCheckBaseQtyDetails.StoreCheckItemHistoryUID == null)
            {
                item.StoreCheckBaseQtyDetails = await _AddEditStoreCheck.GetStoreCheckItemUomBaseQty(item);
            }
            if (item.StoreCheckOuterQtyDetails.StoreCheckItemHistoryUID == null)
            {
                item.StoreCheckOuterQtyDetails = await _AddEditStoreCheck.GetStoreCheckItemUomOuterQty(item);
            }
            StoreRowUomDataSource = item.Clone() as StoreCheckItemHistoryViewList;
        }

        private async Task OnDREBtnClick(IStoreCheckItemHistoryViewList row)
        {


            if (row.StoreCheckItemExpiryDREHistory.Count == 0 || row.StoreCheckItemExpiryDREHistory == null)
            {
                if (row.IsDRESelected)
                {
                    row.StoreCheckItemExpiryDREHistory = await _AddEditStoreCheck.GetStoreCheckItemExpiryDREHistory(row.Uid);
                }
                else
                {
                    row.StoreCheckItemExpiryDREHistory = _AddEditStoreCheck.CreateStoreCheckItemExpiryDREHistory(row);
                }

            }
            DRERowDataSource = row.Clone() as StoreCheckItemHistoryViewList;

        }
        private void UpdateRowDataSource(IStoreCheckItemHistoryViewList updatedRow)
        {
            if (updatedRow != null)
            {
                updatedRow = UpdateStoreCheckItemProperties(updatedRow);
                //Assuming DRERowDataSource is a collection that supports index - based operations like List<T> or ObservableCollection<T>
                var existingRow = _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView.FirstOrDefault(row => row.Uid == updatedRow.Uid);
                //_AddEditStoreCheck.DisplayStoreCheckItemExpiryDreHistory = updatedRow;

                if (existingRow != null)
                {
                    // Find the index of the existing row
                    int index = _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView.IndexOf(existingRow);

                    if (index != -1)
                    {
                        // Replace the existing row with the updated row
                        _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView[index] = updatedRow;
                    }
                }
            }
            StoreRowUomDataSource = null;
        }


        private void UpdateDRERowDataSource(IStoreCheckItemHistoryViewList updatedRow)
        {
            if (updatedRow != null)
            {
                updatedRow = UpdateStoreCheckItemProperties(updatedRow);
                //Assuming DRERowDataSource is a collection that supports index - based operations like List<T> or ObservableCollection<T>
                var existingRow = _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView.FirstOrDefault(row => row.Uid == updatedRow.Uid);
                //_AddEditStoreCheck.DisplayStoreCheckItemExpiryDreHistory = updatedRow;

                if (existingRow != null)
                {
                    // Find the index of the existing row
                    int index = _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView.IndexOf(existingRow);

                    if (index != -1)
                    {
                        // Replace the existing row with the updated row
                        _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView[index] = updatedRow;
                    }
                }
            }
            // Trigger UI update if necessary
            DRERowDataSource = null;
        }
        public IStoreCheckItemHistoryViewList UpdateStoreCheckItemProperties(IStoreCheckItemHistoryViewList storeCheckItemHistoryViewList)
        {
            storeCheckItemHistoryViewList.IsRowModified = true;
            storeCheckItemHistoryViewList.ModifiedTime = DateTime.Now;
            storeCheckItemHistoryViewList.ServerModifiedTime = DateTime.Now;
            return storeCheckItemHistoryViewList;
        }

        private async Task SubmitStoreCheckData()
        {
            //await CaptureStore();
            if (await _AddEditStoreCheck.CreateUpdateStoreCheck(_AddEditStoreCheck.BeatHistoryUID, _AddEditStoreCheck.StoreHistoryUID))
            {
                await NavigateToCustomerDashboard();
            }//SubmitStoreCheckData
            else
            {
                await _alertService.ShowErrorAlert("Failed", "Failed to insert");
            }
        }

        private async Task NavigateToCustomerDashboard()
        {
            string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");
            if (_appUser.SelectedCustomer != null)
            {
                _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
            }
            NavigateTo("CustomerCall");
            await Task.CompletedTask;
        }
        public void OnRowDataChange(ChangeEventArgs e, IStoreCheckItemHistoryViewList row, bool isBackStoreQtyModified)
        {
            if (!isBackStoreQtyModified)
            {
                if (int.TryParse(e.Value.ToString(), out int newQty))
                {
                    row.StoreQty = newQty;
                }
            }
            else
            {
                if (int.TryParse(e.Value.ToString(), out int newQty))
                {
                    row.BackStoreQty = newQty;
                }
            }
            row = UpdateStoreCheckItemProperties(row);

        }
        public async Task InitializedStoreDataStatically()
        {
        }

        public void Dispose()
        {
            // Dispose of objects and perform cleanup here
        }

        //image
        private void OnImageDeleteClick(string fileName)
        {
            IFileSys fileSys = ImageFileSysList.Find
                (e => e.FileName == fileName);
            if (fileSys is not null) ImageFileSysList.Remove(fileSys);
        }
        //private async Task OnImageCapture((string fileName, string folderPath) data)
        //{
        //    IFileSys fileSys = ConvertFileSys("StoreCheck", "122", "Item", "Image",
        //        data.fileName, _appUser.Emp?.Name, data.folderPath);
        //    ImageFileSysList.Add(fileSys);
        //    await Task.CompletedTask;
        //}

        private async Task OnImageCapture((string fileName, string folderPath) data)
        {
            string relativePath = "";

            // Ensure UID is available and not empty
            string selectedStoreUID = _appUser?.SelectedCustomer?.StoreUID;

            if (!string.IsNullOrWhiteSpace(selectedStoreUID))
            {
                relativePath = FileSysTemplateControles.GetStoreCheckFolderPath(selectedStoreUID) ?? "";
            }

            IFileSys fileSys = ConvertFileSys("StoreCheck", "12", "StoreCheck", "Image",
                data.fileName, _appUser.Emp?.Name, data.folderPath);
            fileSys.SS = -1;
            fileSys.RelativePath = relativePath;
            _AddEditStoreCheck.ImageFileSysList.Add(fileSys);
            // _AddEditStoreCheck.FolderPathImages = data.folderPath;
            await Task.CompletedTask;
        }

        private async void OnTopScrollItmSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                selectionItem.IsSelected = !selectionItem.IsSelected;
                if (selectionItem.Code == "Focus" && selectionItem.IsSelected)
                {
                    _AddEditStoreCheck.IsFocusSelected = true;
                }
                else
                {
                    _AddEditStoreCheck.IsFocusSelected = false;
                }
                //TopScrollSelectionItems = TopScrollSelectionItems.OrderBy(equals => !equals.IsSelected).ToList();
                //if (selectionItem.IsSelected)
                //{
                //    if ((selectionItem as SelectionItemFilter).FilterGroup == FilterGroupType.Attribute)
                //    {
                //        _salesOrderViewModel.TopScrollFilterCriterias.Add(new FilterCriteria(selectionItem.UID,
                //            selectionItem.Code, FilterType.Equal, filterGroup: FilterGroupType.Attribute));
                //    }
                //    else
                //    {
                //        _salesOrderViewModel.TopScrollFilterCriterias.Add(new FilterCriteria(selectionItem.Code, true,
                //            FilterType.Equal, typeof(bool)));
                //    }
                //}
                //else
                //{
                //    if ((selectionItem as SelectionItemFilter).FilterGroup == FilterGroupType.Attribute)
                //    {
                //        _ = _salesOrderViewModel.TopScrollFilterCriterias.RemoveAll(e => e.Value == selectionItem.Code);
                //    }
                //    else
                //    {
                //        _ = _salesOrderViewModel.TopScrollFilterCriterias.RemoveAll(e => e.Name == selectionItem.Code);
                //    }
                //}
                //_ = _salesOrderViewModel.ApplyFilter(_salesOrderViewModel.TopScrollFilterCriterias,
                //    Winit.Shared.Models.Enums.FilterMode.And);
                
                _AddEditStoreCheck.DisplayStoreCheckItemHistoryListView = await _AddEditStoreCheck.ItemSearch(_AddEditStoreCheck.SearchString);
                StateHasChanged();
            }
            catch (Exception ex)
            {
            }
            HideLoader();
        }
    }

}


