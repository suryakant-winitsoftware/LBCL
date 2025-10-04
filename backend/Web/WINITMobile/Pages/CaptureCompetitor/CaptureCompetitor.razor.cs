using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.CustomControls;
using WINITMobile.Data;
namespace WINITMobile.Pages.CaptureCompetitor;

public partial class CaptureCompetitor
{
    private bool showCaptureForm = false;
    private bool showDetailsModal = false;
    private WinitTextBox wtbSearch;
    private List<ICaptureCompetitor> items = new();
    private List<ISelectionItem> brands = new();
    private ICaptureCompetitor selectedItem;
    //public string ImgFolderPath { get; set; }
    private string ImgFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
    public bool IsCaputureImage { get; set; }
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
    public string CaptureCompetitorUID { get; set; } = Guid.NewGuid().ToString();
    protected override async Task OnInitializedAsync()
    {
        // Load items and brands from the view model
        // items = await _viewmodel.GetItemsAsync();
        // brands = await _viewmodel.GetBrandsAsync();
        await _viewmodel.PopulateViewModel();
        _viewmodel.CreateCaptureCompetitor.UID = CaptureCompetitorUID;
        if (_appUser?.SelectedCustomer?.Code != null)
        {
            _dataManager.SetData("CaptureCompetitor", _appUser.SelectedCustomer.Code + "_" + sixGuidstring());
        }
        _backbuttonhandler.ClearCurrentPage();
        //ImgFolderPath = Path.Combine(_appConfigs.BaseFolderPath,
        //FileSysTemplateControles.GetCaptureCapitatorImageFolderPath(_dataManager.GetData("CaptureCompetitor").ToString()));
    }

    public string sixGuidstring()
    {
        Guid newGuid = Guid.NewGuid();
        // Convert the GUID to a string and take the first 8 characters without hyphens
        string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);
        return eightDigitGuid;
    }
    private async Task WinitTextBox_OnSearch(string searchString)
    {
        // Filter items based on search
        // items = await _viewmodel.SearchItemsAsync(searchString);
        StateHasChanged();
    }
    private async Task HandleCompetitorBrandSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            // await _viewModel.OnChannelpartnerSelected(dropDownEvent);
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                _viewmodel.CreateCaptureCompetitor.OtherCompany = dropDownEvent.SelectionItems.FirstOrDefault().UID;
            }
            else
            {
                //_viewModel.PreviousOrdersList.Clear();
            }
        }
        //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
        StateHasChanged();
    }
    private async Task HandleChannelSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            // await _viewModel.OnChannelpartnerSelected(dropDownEvent);
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                _viewmodel.CreateCaptureCompetitor.OurBrand = dropDownEvent.SelectionItems.FirstOrDefault().UID;
            }
            else
            {
                //_viewModel.PreviousOrdersList.Clear();
            }
        }
        //_viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
        StateHasChanged();
    }

    //image
    private void OnImageDeleteClick(string fileName)
    {
        try
        {
            IFileSys fileSys = _viewmodel.ImageFileSysList.Find
            (e => e.FileName == fileName);
            if (fileSys is not null) _viewmodel.ImageFileSysList.Remove(fileSys);
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

            IFileSys fileSys = ConvertFileSys("CaptureCompetitor", CaptureCompetitorUID, "CaptureCompetitor", "Image",
                data.fileName, _appUser.Emp?.Name, data.folderPath);
            fileSys.SS = -1;
            fileSys.RelativePath = relativePath;
            _viewmodel.ImageFileSysList.Add(fileSys);
            _viewmodel.FolderPathImages = data.folderPath;
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
    private void ShowCaptureForm()
    {
        showCaptureForm = true;
    }

    private void CancelCaptureForm()
    {
        showCaptureForm = false;
    }

    private void ShowItemDetails(ICaptureCompetitor item)
    {
        selectedItem = item;
        showDetailsModal = true;
    }

    private void CloseModal()
    {
        showDetailsModal = false;
    }

    private async Task SaveData()
    {
        if(await IsValidateCaptureCompetitor())
        {
            if (await _viewmodel.SaveCompitator() >= 1)
            {
                StartBackgroundSync();
                await _alertService.ShowConfirmationAlert("Success", "Competitor Observation captured successfully.", null, "Ok", null);
                string StoreActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");
                if (_appUser.SelectedCustomer != null)
                {
                    _ = await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(StoreActivityHistoryUid, Winit.Modules.Base.Model.CommonConstant.COMPLETED);
                }
                await _viewmodel.GetAllCapatureCampitators();
                _viewmodel.CreateCaptureCompetitor = new Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor();
                _viewmodel.ImageFileSysList = new();
                showCaptureForm = false;
                StateHasChanged();
            }
            else
            {
                await _alertService.ShowErrorAlert("Failed", "Failed to capture Competitor Observation ");
            }
        }
        // Save capture data logic
        else
        {
            return;
        }

    }
    public async Task<bool> IsValidateCaptureCompetitor()
    {
        // Validate the CreateCaptureCompetitor object
        if (string.IsNullOrWhiteSpace(_viewmodel.CreateCaptureCompetitor.OtherCompany))
        {
            await _alertService.ShowErrorAlert("Validation Error", "Please select a competitor brand.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_viewmodel.CreateCaptureCompetitor.OurBrand))
        {
            await _alertService.ShowErrorAlert("Validation Error", "Please select our brand.");
            return false;
        }
        if (_viewmodel.CreateCaptureCompetitor.OtherPrice == null || _viewmodel.CreateCaptureCompetitor.OtherPrice == 0)
        {
            await _alertService.ShowErrorAlert("Validation Error", "Please enter a valid competitor price.");
            return false;
        }
        if (_viewmodel.CreateCaptureCompetitor.OtherSellingPrice == null || _viewmodel.CreateCaptureCompetitor.OtherSellingPrice == 0)
        {
            await _alertService.ShowErrorAlert("Validation Error", "Please enter a valid selling price.");
            return false;
        }
        
        if (_viewmodel.CreateCaptureCompetitor.OtherUOM == null || _viewmodel.CreateCaptureCompetitor.OtherUOM == 0)
        {
            await _alertService.ShowErrorAlert("Validation Error", "Please enter a valid UOM.");
            return false;
        }
        return true;
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
        bool result = await _imageUploadService.PostPendingImagesToServer(CaptureCompetitorUID);

        // Use passed tableGroups or default to FileSys and Merchandiser
        var groups = tableGroups ?? new List<string>
        {
            DbTableGroup.FileSys,
            DbTableGroup.Merchandiser
        };

        // Use the reusable method from base class (no alerts since this is part of checkout flow)
        var res = await UploadDataSilent(groups);
    }
    public List<ISelectionItem> CompetitorDataSource = new List<ISelectionItem>
{
    new SelectionItem { UID = "RTC", Code = "RTC", Label = "RTC" },
    new SelectionItem { UID = "DBS", Code = "DBS", Label = "DBS" },
    new SelectionItem { UID = "MinistryOfNuts", Code = "MinistryOfNuts", Label = "Ministry Of Nuts" },
    new SelectionItem { UID = "NutsAboutYou", Code = "NutsAboutYou", Label = "Nuts About You" },
    new SelectionItem { UID = "Sapphire", Code = "Sapphire", Label = "Sapphire" },
    new SelectionItem { UID = "MarkPremium", Code = "MarkPremium", Label = "Mark Premium" },
    new SelectionItem { UID = "JewelFarmer", Code = "JewelFarmer", Label = "Jewel Farmer" },
    new SelectionItem { UID = "JK", Code = "JK", Label = "JK" },
    new SelectionItem { UID = "TATA_Sampann", Code = "TATA_Sampann", Label = "TATA Sampann" },
    new SelectionItem { UID = "DelightNuts", Code = "DelightNuts", Label = "Delight Nuts" },
    new SelectionItem { UID = "MaiRasoi", Code = "MaiRasoi", Label = "Mai Rasoi" },
    new SelectionItem { UID = "Bolas", Code = "Bolas", Label = "Bolas" },
    new SelectionItem { UID = "MorePrivateLabel", Code = "MorePrivateLabel", Label = "More Private Label" },
    new SelectionItem { UID = "MetroPrivateLabel", Code = "MetroPrivateLabel", Label = "Metro Private Label" },
    new SelectionItem { UID = "ZioFit", Code = "ZioFit", Label = "ZioFit" },
    new SelectionItem { UID = "Wonderland", Code = "Wonderland", Label = "Wonderland" },
    new SelectionItem { UID = "WalmartPrivateLabel", Code = "WalmartPrivateLabel", Label = "Walmart Private Label" },
    new SelectionItem { UID = "VittalsBalaji", Code = "VittalsBalaji", Label = "Vittal's Balaji" },
    new SelectionItem { UID = "Tulsi", Code = "Tulsi", Label = "Tulsi" },
    new SelectionItem { UID = "Taali", Code = "Taali", Label = "Taali" },
    new SelectionItem { UID = "SparSelect", Code = "SparSelect", Label = "Spar Select" },
    new SelectionItem { UID = "SnackLorry", Code = "SnackLorry", Label = "Snack Lorry" },
    new SelectionItem { UID = "SmartChoice", Code = "SmartChoice", Label = "Smart Choice" },
    new SelectionItem { UID = "Saffola", Code = "Saffola", Label = "Saffola" },
    new SelectionItem { UID = "RUNUTZ", Code = "RUNUTZ", Label = "RUNUTZ" },
    new SelectionItem { UID = "RoyalZahidi", Code = "RoyalZahidi", Label = "Royal Zahidi" },
    new SelectionItem { UID = "Regency", Code = "Regency", Label = "Regency" },
    new SelectionItem { UID = "PROV", Code = "PROV", Label = "PROV" },
    new SelectionItem { UID = "PopularLotus", Code = "PopularLotus", Label = "Popular Lotus" },
    new SelectionItem { UID = "NuttyGritties", Code = "NuttyGritties", Label = "Nutty Gritties" },
    new SelectionItem { UID = "Nutraj", Code = "Nutraj", Label = "Nutraj" },
    new SelectionItem { UID = "NewTree", Code = "NewTree", Label = "New Tree" },
    new SelectionItem { UID = "Naturoz", Code = "Naturoz", Label = "Naturoz" },
    new SelectionItem { UID = "MrMakhana", Code = "MrMakhana", Label = "Mr. Makhana" },
    new SelectionItem { UID = "MPGold", Code = "MPGold", Label = "MP Gold" },
    new SelectionItem { UID = "Manna", Code = "Manna", Label = "Manna" },
    new SelectionItem { UID = "Lion", Code = "Lion", Label = "Lion" },
    new SelectionItem { UID = "Kalbavi", Code = "Kalbavi", Label = "Kalbavi" },
    new SelectionItem { UID = "Happilo", Code = "Happilo", Label = "Happilo" },
    new SelectionItem { UID = "Gourmia", Code = "Gourmia", Label = "Gourmia" },
    new SelectionItem { UID = "GloNuts", Code = "GloNuts", Label = "Glo Nuts" },
    new SelectionItem { UID = "Falcon", Code = "Falcon", Label = "Falcon" },
    new SelectionItem { UID = "Eighty7", Code = "Eighty7", Label = "Eighty7" },
    new SelectionItem { UID = "DoubleTick", Code = "DoubleTick", Label = "Double Tick" },
    new SelectionItem { UID = "Chakde", Code = "Chakde", Label = "Chakde" },
    new SelectionItem { UID = "Borges", Code = "Borges", Label = "Borges" },
    new SelectionItem { UID = "BAT", Code = "BAT", Label = "BAT" },
    new SelectionItem { UID = "ASD", Code = "ASD", Label = "ASD" },
    new SelectionItem { UID = "APIS", Code = "APIS", Label = "APIS" }
};


}
