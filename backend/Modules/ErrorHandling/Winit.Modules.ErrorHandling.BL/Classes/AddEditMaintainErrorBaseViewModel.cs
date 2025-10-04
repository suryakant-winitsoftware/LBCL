using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Microsoft.Identity.Client;
using Winit.Modules.ErrorHandling.Model.Classes;
using Microsoft.Extensions.Localization;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;


namespace Winit.Modules.ErrorHandling.BL.Classes;
public abstract class AddEditMaintainErrorBaseViewModel : IAddEditMaintainErrorViewModel
{
    public List<ISelectionItem> ServeritySelectionItems { get; set; }
    public List<ISelectionItem> CategorySelectionItems { get; set; }
    public List<ISelectionItem> PlatformSelectionItems { get; set; }
    public List<ISelectionItem> ModuleSelectionItems { get; set; }
    public List<ISelectionItem> SubModuleSelectionItems { get; set; }
    public bool IsEditError { get; set; }
    public IErrorDetail ErrorDetail { get; set; }

    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private List<string> _propertiesToSearch = new List<string>();
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IDataManager _dataManager;
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;

    public AddEditMaintainErrorBaseViewModel(IServiceProvider serviceProvider,
          IFilterHelper filter,
          ISortHelper sorter,
          IListHelper listHelper,
          IAppUser appUser,
          IAppSetting appSetting,
          IDataManager dataManager, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        _localizer = Localizer;
        _languageService = languageService;
        // Initialize common properties or perform other common setup
        //SKU = new Winit.Modules.SKU.Model.Classes.SKU();
        //SKUUOM = new Winit.Modules.SKU.Model.Classes.SKUUOM();
        //SKUCONFIG = new Winit.Modules.SKU.Model.Classes.SKUConfig();
        //SKUCUSTOM = new CustomSKUFields();
        ErrorDetail = new ErrorDetail();
        CategorySelectionItems = new List<ISelectionItem>();
        ModuleSelectionItems = new List<ISelectionItem>();
        SubModuleSelectionItems = new List<ISelectionItem>();
        PrepareStaticDropdowns();
        // Property set for Search
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
    }

    public virtual async Task PopulateViewModel(string errorUID)
    {
        var submodules = await GetListItemsByCodes(new List<string> { "ERROR_SubModule" });
        if (submodules != null)
        {
            SubModuleSelectionItems.Clear();
            SubModuleSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>(submodules, new List<string> { "UID", "Code", "Name" }));
        }
        var modules = await GetListItemsByCodes(new List<string> { "ERROR_Module" });
        if (modules != null)
        {
            ModuleSelectionItems.Clear();
            ModuleSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>(modules, new List<string> { "UID", "Code", "Name" }));
        }
        var Categories = await GetListItemsByCodes(new List<string> { "ERROR_Category" });
        if (Categories != null)
        {
            CategorySelectionItems.Clear();
            CategorySelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>(Categories, new List<string> { "UID", "Code", "Name" }));
        }
        if (!string.IsNullOrEmpty(errorUID))
        {
            IErrorDetail? errodetails = await GetErrorDetailsByUID(errorUID);
            if (errodetails != null)
            {
                ErrorDetail = errodetails;
                await BindDropDownsForEditPage(errodetails);
            }
            else
            {
                throw new Exception(@_localizer["error_occured_while_retriving_the_details"]);
            }
        }
        await Task.CompletedTask;
    }

    private async Task BindDropDownsForEditPage(IErrorDetail errorDetail)
    {
        var selectedSeverity = ServeritySelectionItems.Find(e => e.Code == errorDetail.Severity.ToString());
        if (selectedSeverity != null)
        {
            selectedSeverity.IsSelected = true;
        }
        var selectedCategory = CategorySelectionItems.Find(e => e.Code == errorDetail.Category);
        if (selectedCategory != null)
        {
            selectedCategory.IsSelected = true;
        }
        var selectedplatform = PlatformSelectionItems.Find(e => e.Code == errorDetail.Platform);
        if (selectedplatform != null)
        {
            selectedplatform.IsSelected = true;
        }
        var selectedModule = ModuleSelectionItems.Find(e => e.Code == errorDetail.Module);
        if (selectedModule != null)
        {
            selectedModule.IsSelected = true;
        }
        var selectedSubModule = SubModuleSelectionItems.Find(e => e.Code == errorDetail.SubModule);
        if (selectedSubModule != null)
        {
            selectedSubModule.IsSelected = true;
        }
        await Task.CompletedTask;
    }
    public async void InstilizeFieldsForEditPage(IErrorDetail Item)
    {
        //if (IErrorDetail != null)
        //{
        //    await SetEditForSKU(IErrorDetail.SKU);
        //}
        //if (IErrorDetail.SKUAttributes != null)
        //{
        //    await SetEditForSKUAttribute(IErrorDetail.SKUAttributes);
        //}
        //if (IErrorDetail.SKUUOMs != null)
        //{
        //    await SetEditForSKUUOM(IErrorDetail.SKUUOMs);
        //}
        //if (IErrorDetail.SKUConfigs != null)
        //{
        //    await SetEditForSKUConfig(IErrorDetail.SKUConfigs);
        //}
        //if (IErrorDetail.customSKUFields != null)
        //{
        //    await SetEditForCustomSku(IErrorDetail.customSKUFields);
        //}
    }
    private void PrepareStaticDropdowns()
    {
        ServeritySelectionItems = new List<ISelectionItem>
        {
            new SelectionItem{UID="1",Label = "1", Code="1"},
            new SelectionItem{UID="2",Label = "2", Code="2"},
            new SelectionItem{UID="3",Label = "3", Code="3"},
            new SelectionItem{UID="4",Label = "4", Code="4"},
        };
        PlatformSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem{UID="ALL", Label = @_localizer["all"],Code="ALL"},
            new SelectionItem{UID = "WEB",  Label = @_localizer["web"], Code="WEB"},
            new SelectionItem{UID="MOBILE", Label = @_localizer["mobile"],Code="MOBILE"},
        };

    }
    public async Task<bool> Save()
    {
        AddCreateFields(ErrorDetail, true);
        return await CreateErrorDetails(ErrorDetail);
    }
    public async Task<bool> Update()
    {
        AddUpdateFields(ErrorDetail);
        return await UpdateErrorDetails(ErrorDetail);
    }
    #region abstract Methods
    protected abstract Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>?> GetListItemsByCodes(List<string> codes);

    protected abstract Task<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail?> GetErrorDetailsByUID(string errorUID);

    protected abstract Task<bool> CreateErrorDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail errorDetail);
    protected abstract Task<bool> UpdateErrorDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail errorDetail);
    #endregion
    #region Utils
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
    #endregion
}
