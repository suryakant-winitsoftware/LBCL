using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.UOM.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;
using Winit.Modules.User.BL.Interfaces;

namespace WinIt.Pages.MaintainSKU;

public partial class AddEditMaintainSKU
{

    [CascadingParameter]
    public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    IDataService dataService = new DataServiceModel()
    {
        BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="Maintain SKU",URL="MaintainSKU",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text=" Maintain SKU"},
      }
    };
    public bool IsViewMode { get; set; } = true;
    private string? validationMessageSKU;
    private readonly string? validationMessageSKUAttribute;
    public bool IsAdd { get; set; } = true;
    public bool IsAddSKUAttribute { get; set; } = true;
    public bool IsCustomAdd { get; set; } = true;
    public bool IsAddSkuUomPopUp { get; set; }
    public bool IsLoaded { get; set; }
    private bool IsLoading = false;
    private readonly bool IsSKUAttributeLoading = false;
    private bool IsAddNewSKUUOMVisiblebtn = false;
    private bool IsAddNewDistributionVisiblebtn = false;
    public bool AddNewSKUItems { get; set; }
    public bool IsEditskuUOMPopUp { get; set; }
    public bool IsViewskuUOMPopUp { get; set; }
    public bool IsSupplierPopUp { get; set; }
    public bool IsDistributionPopUp { get; set; }
    public bool IsDistributionEDitPopUp = false;
    public string Msg = "";
    public List<CustomField> ListCustomSKUFields = new();

    //SKUUOM
    public List<IUOMType> UOMTypeList = new();
    //VolumeUnit
    private readonly IListItem? VOLUMEUNITLISTITEM;
    [Parameter]
    public string? SKUUID { get; set; }
    private List<IFileSys>? fileSysList { get; set; }
    private string? FilePath { get; set; }
    private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
    protected void CloseAddNewItemDialog()
    {
        AddNewSKUItems = false;
        IsDistributionEDitPopUp = false;
    }
    private List<SelectionItem> FilteredCustomer { get; set; } = new List<SelectionItem>();

    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        LoadResources(null, _languageService.SelectedCulture);
        await _addEditMaintainSKUViewModel.PopulateViewModel();
        ListCustomSKUFields = await _addEditMaintainSKUViewModel.PopulateCustomSkuFieldsDynamic();
        SKUUID = _commonFunctions.GetParameterValueFromURL("SKUUID");
        if (SKUUID != null)
        {
            IsAdd = false;
            _addEditMaintainSKUViewModel.IsAddSkuAttribute = false;
            IsAddNewSKUUOMVisiblebtn = true;
            IsAddNewDistributionVisiblebtn = true;
            ISKUMaster data = await _addEditMaintainSKUViewModel.PopulateSKUDetailsData(SKUUID);
            if (data != null)
            {
                if (data.DbDataList != null)
                {
                    ListCustomSKUFields = data.DbDataList;
                }
                if (data.FileSysList != null)
                {
                    fileSysList = data.FileSysList;
                }
            }
            if (_addEditMaintainSKUViewModel.SkuMaster?.SKUAttributes != null && _addEditMaintainSKUViewModel.SkuMaster.SKUAttributes.Any())
            {
                _addEditMaintainSKUViewModel.IsAddSkuAttribute = false;
            }

            if (_addEditMaintainSKUViewModel.SkuMaster?.CustomSKUFields != null && _addEditMaintainSKUViewModel.SkuMaster.CustomSKUFields.Any())
            {
                IsCustomAdd = false;
            }

            await _addEditMaintainSKUViewModel.InstilizeFieldsForEditPage(_addEditMaintainSKUViewModel.SkuMaster);
        }
        else
        {
            IsAdd = true;
            _addEditMaintainSKUViewModel.IsAddSkuAttribute = true;
            _addEditMaintainSKUViewModel.SKU.FromDate = DateTime.Today;
            _addEditMaintainSKUViewModel.SKU.ToDate = new DateTime(2099, 12, 31);
            _addEditMaintainSKUViewModel.SKU.ToDate = DateTime.Parse(_addEditMaintainSKUViewModel.SKU.ToDate.ToString("dd/MMM/yyyy"));
        }
        FilePath = FileSysTemplateControles.GetSKUFolderPath(Guid.NewGuid().ToString());
        IsLoaded = true;
        //await SetHeaderName();
        dataService.HeaderText = $"{(!IsAdd ? "Edit SKU" : "Add SKU")}";
        HideLoader();
    }
    /**************************************************************************************************************************************************/

    //public async Task SetHeaderName()
    //{
    //    _IDataService.BreadcrumList = new()
    //    {
    //        new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_sku"], IsClickable = true, URL = "MaintainSKU" },
    //        new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{(!IsAdd ? "Edit " : "Add ")} SKU ", IsClickable = false }
    //    };
    //    _IDataService.HeaderText = $"{(!IsAdd ? @Localizer["edit"] : @Localizer["add"])} SKU ";
    //    await CallbackService.InvokeAsync(_IDataService);
    //}

    //SKU
    private async Task<bool> UserExistsAsync(string code)
    {
        // Implement the logic to check if the user exists in the database
        // This could involve a call to a service or repository that queries the user data
        return await _skuViewModel.CheckUserExistsAsync(code);
    }
    public async Task CheckDuplication()
    {
        bool userExists = await UserExistsAsync(
              _addEditMaintainSKUViewModel.SKU.Code);
               
        if (userExists)
        {
            ShowErrorSnackBar("Error", "An SKU with the same Code already exists.");

            return; // Exit early if user already exists
        }
    }
    public async Task SaveUpdateSKUItem()
    {
        try
        {
            if(IsAdd)
            {
                await CheckDuplication();
                bool userExists = await UserExistsAsync(
            _addEditMaintainSKUViewModel.SKU.Code);
                if (userExists)
                {
                    // Exit early if the duplication check found a duplicate
                    _loadingService.HideLoading();
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.Code) ||
                string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.LongName) ||
                string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.Name))
            /*!IsSupplierSelectionValid()*/
            {
                validationMessageSKU = @Localizer["the_following_field(s)_have_invalid_value(s)"];
                //if (!IsSupplierSelectionValid())
                //{
                //    validationMessageSKU += "Supplier, ";
                //}
                if (string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.Code))
                {
                    validationMessageSKU += @Localizer["item_sku_code,"];
                }
                if (string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.LongName))
                {
                    validationMessageSKU += @Localizer["longname,"];
                }
                if (string.IsNullOrWhiteSpace(_addEditMaintainSKUViewModel.SKU.Name))
                {
                    validationMessageSKU += @Localizer["display_description,"];
                }
                validationMessageSKU = validationMessageSKU.TrimEnd(' ', ',');
            }
            else
            {
                IsLoading = true;
                if (IsAdd)
                {
                    ShowLoader();
                    (string, bool) createresponse = await _addEditMaintainSKUViewModel.SaveUpdateSKUItem(_addEditMaintainSKUViewModel.SKU, true);
                    if (_addEditMaintainSKUViewModel.SKU.UID != null)
                    {
                        IsAddNewSKUUOMVisiblebtn = true;
                        IsAddNewDistributionVisiblebtn = true;
                    }
                    Msg = createresponse.Item1;
                    if (string.IsNullOrEmpty(Msg))
                    {
                        _ = _tost.Add(@Localizer["sku"], "SKU saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _tost.Add(@Localizer["sku"], "SKU failed to save", Winit.UIComponents.SnackBar.Enum.Severity.Error);

                    }
                    HideLoader();
                }
                else
                {
                    ShowLoader();
                    (string, bool) createresponse = await _addEditMaintainSKUViewModel.SaveUpdateSKUItem(_addEditMaintainSKUViewModel.SKU, false);
                    Msg = createresponse.Item1;
                    if (string.IsNullOrEmpty(Msg))
                    {
                        _ = _tost.Add(@Localizer["sku"], "SKU updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _tost.Add(@Localizer["sku"], "SKU failed to update", Winit.UIComponents.SnackBar.Enum.Severity.Error);

                    }
                }
                _loadingService.HideLoading();
                // Reset validation message after successful save/update
                validationMessageSKU = string.Empty;
                IsLoading = false;
                IsAdd = false;

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            // Handle the error and set an error message
            validationMessageSKU = $"Error: {ex.Message}";
        }
    }

    public void OnORGSupplierSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem? selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
            // sku.Code = selecetedValue?.Code;
            _addEditMaintainSKUViewModel.SKU.SupplierOrgUID = selecetedValue?.UID;
        }
    }
    private bool IsSupplierSelectionValid()
    {
        // Check if SupplierOrgUID is not null or empty
        return !string.IsNullOrEmpty(_addEditMaintainSKUViewModel.SKU.SupplierOrgUID);
    }

    //SKUUOMMethods
    protected void OnSKUUOMAddBtnClicked()
    {
        Msg = string.Empty;
        _addEditMaintainSKUViewModel.SKUUOM = _serviceProvider.CreateInstance<ISKUUOM>();
        // VOLUMEUNITLISTITEM = _serviceProvider.CreateInstance<IListItem>();
        IsAddSkuUomPopUp = true;

    }
    public async Task OnSKUUOMEditBtnClick(ISKUUOM sKUUOM)
    {
        StateHasChanged();
        Msg = string.Empty;
        await _addEditMaintainSKUViewModel.OnSKUUOMEditClick(sKUUOM);
        IsEditskuUOMPopUp = true;


    }
    //public async Task OnSKUUOMViewBtnClick(ISKUUOM sKUUOM)
    //{
    //    Msg = string.Empty;
    //    await _addEditMaintainSKUViewModel.OnSKUUOMEditClick(sKUUOM);
    //    IsViewskuUOMPopUp = true;
    //}
    public void OnSKUUOMCancelBtnClickInPopUp()
    {
        _addEditMaintainSKUViewModel.OnSKUUOMCancelBtnClickInPopUp();
        IsEditskuUOMPopUp = false;
        IsViewskuUOMPopUp = false;
        IsAddSkuUomPopUp = false;
    }
    public async Task OnSKUUOMCreateBtnClickFromPopUp()
    {
        ShowLoader();
        (string, bool) createresponse = await _addEditMaintainSKUViewModel.OnSKUUOMCreateUpdateBtnClickFromPopUp(_addEditMaintainSKUViewModel.SKUUOM, true);
        Msg = createresponse.Item1;
        if (string.IsNullOrEmpty(Msg))
        {
            IsAddSkuUomPopUp = false;
            _ = _tost.Add(@Localizer["sku_uom"], "SKUUOM saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _ = _tost.Add(@Localizer["sku_uom"], "SKUUOM failed to save", Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
    }
    public async Task OnSKUUOMUpadteBtnClickFromPopUp()
    {
        ShowLoader();
        (string, bool) createresponse = await _addEditMaintainSKUViewModel.OnSKUUOMCreateUpdateBtnClickFromPopUp(_addEditMaintainSKUViewModel.SKUUOM, false);
        Msg = createresponse.Item1;
        if (string.IsNullOrEmpty(Msg))
        {
            IsEditskuUOMPopUp = false;
            _ = _tost.Add(@Localizer["sku_uom"], "SKUUOM updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _ = _tost.Add(@Localizer["sku_uom"], "SKUUOM failed to update", Winit.UIComponents.SnackBar.Enum.Severity.Error);

        }
        HideLoader();
        StateHasChanged();
    }
    /**************************************************************************************************************************************************/
    //SKUConfigDistributionChannel
    protected void OnDistributionAddBtnClicked()
    {
        Msg = string.Empty;
        _addEditMaintainSKUViewModel.SKUCONFIG = _serviceProvider.CreateInstance<ISKUConfig>();
        IsDistributionPopUp = true;
    }
    public async Task OnDistributionEditBtnClick(ISKUConfig sKUConfig)
    {
        await _addEditMaintainSKUViewModel.OnSKUConfigEditClick(sKUConfig);
        IsDistributionEDitPopUp = true;
    }
    public void OnSKUConfigCancelBtnClickInPopUp()
    {
        _addEditMaintainSKUViewModel.OnSKUConfigCancelBtnClickInPopUp();
        IsDistributionEDitPopUp = false;
        IsDistributionPopUp = false;
    }
    public async Task OnDistributionCreateBtnClickFromPopUp()
    {
        ShowLoader();
        (string, bool) createresponse = await _addEditMaintainSKUViewModel.OnDistributionCreateUpdateBtnClickFromPopUp(_addEditMaintainSKUViewModel.SKUCONFIG, true);
        Msg = createresponse.Item1;
        if (string.IsNullOrEmpty(Msg))
        {
            IsDistributionPopUp = false;
            _ = _tost.Add(@Localizer["sku_config"], "SKUConfig saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _ = _tost.Add(@Localizer["sku_config"], "SKUConfig failed to save", Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
    }
    public async Task OnDistributionUpadteBtnClickFromPopUp()
    {
        ShowLoader();
        (string, bool) createresponse = await _addEditMaintainSKUViewModel.OnDistributionCreateUpdateBtnClickFromPopUp(_addEditMaintainSKUViewModel.SKUCONFIG, false);
        Msg = createresponse.Item1;
        if (string.IsNullOrEmpty(Msg))
        {
            IsDistributionEDitPopUp = false;
            _ = _tost.Add(@Localizer["sku_config"], "SKUConfig updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _ = _tost.Add(@Localizer["sku_config"], "SKUConfig failed to update", Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
    }
    /**************************************************************************************************************************************************/
    //CustomSKU      
    //public async Task SaveSKUCustomFields()
    //{
    //    IsLoading = true;
    //    if (IsCustomAdd)
    //    {
    //        await _addEditMaintainSKUViewModel.SaveSKUCustomFields(_addEditMaintainSKUViewModel.SKUCUSTOM, true);
    //        _tost.Add("CustomSKU", "CustomSKU Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
    //    }
    //    else
    //    {
    //        await _addEditMaintainSKUViewModel.SaveSKUCustomFields(_addEditMaintainSKUViewModel.SKUCUSTOM, false);
    //        _tost.Add("CustomSKU", "CustomSKU Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
    //    }
    //    IsLoading = false;
    //    IsCustomAdd = false;
    //    StateHasChanged();
    //}
    public async Task SaveSKUCustomFieldsForDynamic()
    {
        ShowLoader();
        IsLoading = true;
        await _addEditMaintainSKUViewModel.SaveSKUCustomFieldsForDynamic(ListCustomSKUFields);
        _ = _tost.Add(@Localizer["custom_sku"], "CustomSKU saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        HideLoader();
        IsLoading = false;
        IsCustomAdd = false;
        StateHasChanged();
    }
    /**************************************************************************************************************************************************/
    private async Task BackBtnClicked()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["back"], @Localizer["are_you_sure_you_want_to_go_back?"], @Localizer["yes"], @Localizer["no"]))
        {
            _navigationManager.NavigateTo("MaintainSKU");
        }
    }
    /**************************************************************************************************************************************************/
    private void GetsavedImagePath(List<IFileSys> ImagePath)
    {
        fileSysList = ImagePath;
    }
    private void AfterDeleteImage()
    {

    }

    //public async Task PopulateUploadImage()
    //{
    //    List<Winit.Modules.FileSys.Model.Classes.FileSys> fileSys = new List<Winit.Modules.FileSys.Model.Classes.FileSys>();

    //    foreach (var imagePath in SavedImagePath)
    //    {
    //        string folderPath = Path.GetDirectoryName(imagePath);
    //        string fileName = Path.GetFileName(imagePath);

    //        Winit.Modules.FileSys.Model.Classes.FileSys file = new Winit.Modules.FileSys.Model.Classes.FileSys();
    //        file.UID = Guid.NewGuid().ToString();
    //        file.LinkedItemType = SKUConsonant.FileSysLinkedItmeType;
    //        file.LinkedItemUID = _addEditMaintainSKUViewModel.SKU.UID;
    //        file.FileSysType = SKUConsonant.FileSysLinkedItmeType;
    //        file.FileType = SKUConsonant.FileSysFileType;
    //        file.IsDirectory = false;
    //        file.FileName = fileName;
    //        file.DisplayName = fileName; // Assuming DisplayName is the same as FileName
    //        file.FileSize = 0;
    //        file.RelativePath = folderPath;
    //        file.Longitude = 0.ToString();
    //        file.Latitude = 0.ToString();
    //        file.CreatedBy = "WINIT";
    //        file.ModifiedBy = "WINIT";
    //        file.CreatedTime = DateTime.Now;
    //        file.ModifiedTime = DateTime.Now;
    //        file.ServerAddTime = DateTime.Now;
    //        file.ServerModifiedTime = DateTime.Now;
    //        fileSys.Add(file);
    //    }
    //    await CreateUpdateSKUImage(fileSysList);
    //}

    protected async Task SaveFileSys()
    {
        if (fileSysList == null || !fileSysList.Any())
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], "Please upload files");
            //_tost.Add("SKU Image", "SKU Image Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
            return;
        }

        Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await fileUploader.MoveFiles();
        if (apiResponse.IsSuccess)
        {
            await CreateUpdateSKUImage(fileSysList);
        }
        else
        {

        }
    }
    public async Task CreateUpdateSKUImage(List<IFileSys> fileSys)
    {
        if (fileSys.Count > 0)
        {
            ShowLoader();
            _ = await _addEditMaintainSKUViewModel.CreateUpdateFileSysData(fileSys, true);
            StateHasChanged();
            _ = _tost.Add(@Localizer["sku_image"], "SKUimage saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _ = _tost.Add(@Localizer["sku_image"], "SKUimage failed to save", Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        HideLoader();
    }

    private async Task OnSaveSKUAttributes(Dictionary<string, ISelectionItem> keyValuePairs)
    {
        ShowLoader();
        if (await _addEditMaintainSKUViewModel.SaveOrUpdateSKUAttributes(keyValuePairs))
        {
            ShowSuccessSnackBar(@Localizer["success"], "SKUattributes are saved successfully");
            _addEditMaintainSKUViewModel.IsAddSkuAttribute = false;
        }
        HideLoader();
    }
    private async Task<List<ISelectionItem>> OnDropdownValueSelectSKUAtrributes(ISelectionItem selectedValue)
    {
        return await _addEditMaintainSKUViewModel.OnSKuAttributeDropdownValueSelect(selectedValue.UID);
    }
}