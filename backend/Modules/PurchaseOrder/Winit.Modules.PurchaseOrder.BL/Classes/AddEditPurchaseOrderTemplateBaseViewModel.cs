using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class AddEditPurchaseOrderTemplateBaseViewModel : IAddEditPurchaseOrderTemplateViewModel
{
    public bool IsAdd { get; set; } = true;
    public string ProductSearchString { get; set; } = string.Empty;
    public List<ISKUV1> SKUs { get; set; }
    public List<IPurchaseOrderTemplateItemView> PurchaseOrderTemplateItemViews { get; set; }
    public List<IPurchaseOrderTemplateItemView> FilteredPurchaseOrderTemplateItemViews { get; set; }
    public List<ISelectionItem> OrganisationUnitSelectionItems { get; set; }
    public List<ISelectionItem> DivisionSelectionItems { get; set; }
    public List<ISelectionItem> ProductCategorySelectionItems { get; set; }
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    public IPurchaseOrderTemplateMaster PurchaseOrderTemplateMaster { get; set; }
    private readonly IAppUser _appUser;
    private readonly IAddEditPurchaseOrderTemplateDataHelper _addEditPurchaseOrderTemplateDataHelper;
    private readonly IServiceProvider _serviceProvider;
    public AddEditPurchaseOrderTemplateBaseViewModel(IAppUser appUser,
        IAddEditPurchaseOrderTemplateDataHelper addEditPurchaseOrderTemplateDataHelper,
        IServiceProvider serviceProvider)
    {
        _appUser = appUser;
        _addEditPurchaseOrderTemplateDataHelper = addEditPurchaseOrderTemplateDataHelper;
        _serviceProvider = serviceProvider;
        SKUs = [];
        PurchaseOrderTemplateItemViews = [];
        FilteredPurchaseOrderTemplateItemViews = [];
        OrganisationUnitSelectionItems = [];
        DivisionSelectionItems = [];
        ProductCategorySelectionItems = [];
        SKUAttributeData = [];
        PurchaseOrderTemplateMaster = _serviceProvider.GetRequiredService<IPurchaseOrderTemplateMaster>();
    }
    public async Task PopulateViewModel(string purchaseOrderHeaderUID)
    {
        if (!string.IsNullOrEmpty(purchaseOrderHeaderUID))
        {
            IsAdd = false;
            var data = await _addEditPurchaseOrderTemplateDataHelper.GetPOTemplateMasterByUID(purchaseOrderHeaderUID);
            if (data != null) PurchaseOrderTemplateMaster = data;
            else throw new CustomException(ExceptionStatus.Failed, "Retriving the po template failed..");
        }

        SKUs.Clear();
        PagingRequest pagingRequest = new PagingRequest();
        pagingRequest.FilterCriterias = [new FilterCriteria("OrgUID", _appUser.OrgUIDs, FilterType.Equal)];
        SKUs.AddRange(await _addEditPurchaseOrderTemplateDataHelper.GetAllSKUs(pagingRequest));
        if (!IsAdd)
        {
            List<string> skuUIds = PurchaseOrderTemplateMaster.PurchaseOrderTemplateLines.Select(e => e.SKUUID).ToList();
            if (skuUIds != null && skuUIds.Any())
            {
                await AddProductsToGridBySKUUIDs(skuUIds);
            }
            foreach (IPurchaseOrderTemplateLine line in PurchaseOrderTemplateMaster.PurchaseOrderTemplateLines)
            {
                UpdatePurchaseOrderLineByPurchaseOrderLine(line);
            }
            PurchaseOrderTemplateMaster.ActionType = ActionType.Update;
            PurchaseOrderTemplateItemViews.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
            FilteredPurchaseOrderTemplateItemViews.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
        }
        _ = PrepareFilters();
    }
    private void UpdatePurchaseOrderLineByPurchaseOrderLine(IPurchaseOrderTemplateLine purchaseOrderTemplateLine)
    {
        IPurchaseOrderTemplateItemView? purchaseOrderTemplateItemView = PurchaseOrderTemplateItemViews
            .Find(e => e.SKUUID == purchaseOrderTemplateLine.SKUUID);
        if (purchaseOrderTemplateItemView is not null)
        {
            purchaseOrderTemplateItemView.UID = purchaseOrderTemplateLine.UID;
            purchaseOrderTemplateItemView.CreatedBy = purchaseOrderTemplateLine.CreatedBy;
            purchaseOrderTemplateItemView.ModifiedBy = purchaseOrderTemplateLine.ModifiedBy;
            purchaseOrderTemplateItemView.CreatedTime = purchaseOrderTemplateLine.CreatedTime;
            purchaseOrderTemplateItemView.ModifiedTime = purchaseOrderTemplateLine.ModifiedTime;
            purchaseOrderTemplateItemView.LineNumber = purchaseOrderTemplateLine.LineNumber;
            purchaseOrderTemplateItemView.Qty = purchaseOrderTemplateLine.Qty;
            purchaseOrderTemplateItemView.PurchaseOrderTemplateHeaderUID = purchaseOrderTemplateLine.PurchaseOrderTemplateHeaderUID;
        }
    }

    public async Task AddProductsToGridBySKUUIDs(List<string> sKUs)
    {
        _ = sKUs.RemoveAll(s => PurchaseOrderTemplateItemViews.Select(e => e.SKUUID).Contains(s));
        if (!sKUs.Any()) return;
        SKUMasterRequest sKUMasterRequest = new()
        {
            SKUUIDs = sKUs, OrgUIDs = _appUser.OrgUIDs
        };
        Task<List<ISKUMaster>> skuMasterData = _addEditPurchaseOrderTemplateDataHelper.GetSKUsMasterBySKUUIDs(sKUMasterRequest);
        PreparepurchaseOrderTemplateItemViewsBySKUMaster(await skuMasterData);

        ApplyGridFilter();
    }
    private void PreparepurchaseOrderTemplateItemViewsBySKUMaster(List<ISKUMaster> skuMasterData)
    {
        foreach (ISKUMaster skuMaster in skuMasterData)
        {
            IPurchaseOrderTemplateItemView purchaseOrderTemplateItemView = ConvertTopurchaseOrderTemplateItemView(skuMaster, PurchaseOrderTemplateItemViews.Count + 1);
            PurchaseOrderTemplateItemViews.Add(purchaseOrderTemplateItemView);
        }
    }
    public virtual IPurchaseOrderTemplateItemView ConvertTopurchaseOrderTemplateItemView(ISKUMaster sKUMaster, int lineNumber, List<string>? skuImages = null)
    {
        Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.FirstOrDefault();
        List<ISKUUOMView>? sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
        ISKUUOMView? defaultUOM = sKUUOMViews
            ?.FirstOrDefault(e => e.Code == sKUConfig?.SellingUOM);
        ISKUUOMView? baseUOM = sKUUOMViews
            ?.FirstOrDefault(e => e.IsBaseUOM);
        IPurchaseOrderTemplateItemView purchaseOrderTemplateItemView = _serviceProvider.GetService<IPurchaseOrderTemplateItemView>() ??
            throw new Exception("IPurchaseOrderTemplateItemView is not registered");
        //purchaseOrderTemplateItemView.UID = Guid.NewGuid().ToString();
        purchaseOrderTemplateItemView.LineNumber = lineNumber;
        purchaseOrderTemplateItemView.SKUUID = sKUMaster.SKU.UID;
        purchaseOrderTemplateItemView.SKUCode = sKUMaster.SKU.Code;
        purchaseOrderTemplateItemView.SKUName = sKUMaster.SKU.Name;
        purchaseOrderTemplateItemView.BaseUOM = baseUOM?.Code;
        purchaseOrderTemplateItemView.SelectedUOM = defaultUOM;
        purchaseOrderTemplateItemView.UOM = defaultUOM?.Code;
        purchaseOrderTemplateItemView.ModelDescription = $"[{sKUMaster.SKU.Code}]" + sKUMaster.SKU.Name;

        if (sKUMaster.SKU is ISKUV1 sKUV1)
        {
            purchaseOrderTemplateItemView.FilterKeys = sKUV1.FilterKeys;
            purchaseOrderTemplateItemView.ModelDescription = sKUV1.L2 ?? "" + $"[{sKUV1.ModelCode}]" + sKUV1.Name;
            purchaseOrderTemplateItemView.L2 = sKUV1.L2;
            purchaseOrderTemplateItemView.ModelCode = sKUV1.ModelCode;
        }
        else
        {
            sKUMaster.SKUAttributes?.ForEach(e => purchaseOrderTemplateItemView.FilterKeys.Add(e.Code));
        }

        return purchaseOrderTemplateItemView;
    }
    public virtual ISKUUOMView ConvertToISKUUOMView(ISKUUOM sKUUOM)
    {
        ISKUUOMView sKUUOMView = _serviceProvider.CreateInstance<ISKUUOMView>();
        sKUUOMView.SKUUID = sKUUOM.SKUUID;
        sKUUOMView.Code = sKUUOM.Code;
        sKUUOMView.Name = sKUUOM.Name;
        sKUUOMView.Label = sKUUOM.Label;
        sKUUOMView.Barcode = sKUUOM.Barcodes;
        sKUUOMView.IsBaseUOM = sKUUOM.IsBaseUOM;
        sKUUOMView.IsOuterUOM = sKUUOM.IsOuterUOM;
        sKUUOMView.Multiplier = sKUUOM.Multiplier;
        return sKUUOMView;
    }
    public List<ISKUUOMView> ConvertToISKUUOMView(List<ISKUUOM> sKUUOMs)
    {
        List<ISKUUOMView>? sKUUOMViews = null;
        if (sKUUOMs != null)
        {
            sKUUOMViews = [];
            foreach (ISKUUOM sKUUOM in sKUUOMs)
            {
                sKUUOMViews.Add(ConvertToISKUUOMView(sKUUOM));
            }
        }
        return sKUUOMViews;
    }

    public void ApplyGridFilter()
    {
        FilteredPurchaseOrderTemplateItemViews.Clear();
        List<IPurchaseOrderTemplateItemView> items = PurchaseOrderTemplateItemViews.Where(e => string.IsNullOrEmpty(ProductSearchString) ||
            (e.SKUCode?.Contains(ProductSearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.SKUName?.Contains(ProductSearchString, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        List<string>? selectedOrgs = OrganisationUnitSelectionItems
            .Where(e => e.IsSelected && e.UID != null)
            .Select(_ => _.UID!)
            .ToList();
        List<string>? selectedDivisions = DivisionSelectionItems
            .Where(e => e.IsSelected && e.UID != null)
            .Select(_ => _.UID!)
            .ToList();
        List<string>? selectedProductCategories = ProductCategorySelectionItems
            .Where(e => e.IsSelected && e.Code != null)
            .Select(_ => _.Code!)
            .ToList();
        if (items == null)
        {
            return;
        }

        List<IPurchaseOrderTemplateItemView> rmItems = new();
        foreach (IPurchaseOrderTemplateItemView? item in items)
        {
            if (selectedOrgs != null && selectedOrgs.Any() && !item.FilterKeys.Any(selectedOrgs.Contains))
            {
                rmItems.Add(item);
                continue;
            }
            if (selectedDivisions != null && selectedDivisions.Any() && !item.FilterKeys.Any(selectedDivisions.Contains))
            {
                rmItems.Add(item);
                continue;
            }
            if (selectedProductCategories != null && selectedProductCategories.Any() && !item.FilterKeys.Any(selectedProductCategories.Contains))
            {
                rmItems.Add(item);
                continue;
            }
        }
        _ = items.RemoveAll(rmItems.Contains);
        FilteredPurchaseOrderTemplateItemViews.AddRange(items);
    }
    public async Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID)
    {
        List<SKUGroupSelectionItem> data = await _addEditPurchaseOrderTemplateDataHelper.GetSKUGroupSelectionItemBySKUGroupTypeUID(null, selectedItemUID);
        return data != null && data.Any() ? data.ToList<ISelectionItem>() : [];
    }
    private async Task PrepareFilters()
    {
        await PopulateSKUAttributeData();
        await Task.Run(PrepareGridFilters);
    }
    private async Task PrepareGridFilters()
    {
        OrganisationUnitSelectionItems.Clear();
        DivisionSelectionItems.Clear();
        ProductCategorySelectionItems.Clear();

        var orgUnits = _addEditPurchaseOrderTemplateDataHelper.GetProductOrgSelectionItems();
        var orgDivisions = _addEditPurchaseOrderTemplateDataHelper.GetProductDivisionSelectionItems();
        await Task.WhenAll(orgUnits, orgDivisions);
        if (orgUnits.Result != null) OrganisationUnitSelectionItems.AddRange(orgUnits.Result);
        if (orgDivisions.Result != null) DivisionSelectionItems.AddRange(orgDivisions.Result);
        var productCategorySelectionItems = SKUAttributeData.Find(e => e.DropDownTitle == "Product Category");
        if (productCategorySelectionItems != null) ProductCategorySelectionItems.AddRange(productCategorySelectionItems.DropDownDataSource);
    }
    public async Task PopulateSKUAttributeData()
    {
        SKUAttributeData.Clear();
        List<SKUAttributeDropdownModel> sKUAttributeDropdownModels = await _addEditPurchaseOrderTemplateDataHelper.GetSKUAttributeDropDownData();
        if (sKUAttributeDropdownModels != null && sKUAttributeDropdownModels.Any())
        {
            SKUAttributeData.AddRange(sKUAttributeDropdownModels);
            for (int i = 0; i < SKUAttributeData.Count; i++)
            {
                SKUAttributeDropdownModel item = SKUAttributeData[i];
                for (int j = 0; j < SKUAttributeData.Count; j++)
                {
                    if (item.UID == SKUAttributeData[j].ParentUID)
                    {
                        SKUAttributeDropdownModel childItem = SKUAttributeData[j];
                        SKUAttributeData.RemoveAt(j);
                        SKUAttributeData.Insert(i + 1, childItem);
                        i++;
                        break;
                    }
                }
            }
            List<Task> tasks = [];
            foreach (SKUAttributeDropdownModel item in SKUAttributeData)
            {
                tasks.Add(AddDataSourceToDD(item));
            }
        }
    }
    private async Task AddDataSourceToDD(SKUAttributeDropdownModel item)
    {
        List<SKUGroupSelectionItem> data = await _addEditPurchaseOrderTemplateDataHelper.GetSKUGroupSelectionItemBySKUGroupTypeUID(item.UID, null);
        if (data != null && data.Any())
        {
            item.DropDownDataSource.AddRange(data.ToList<ISelectionItem>());
        }
    }

    public void Validate()
    {
        List<string> errorMSG = [];

        if (string.IsNullOrEmpty(PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.TemplateName) || string.IsNullOrWhiteSpace(PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.TemplateName))
        {
            errorMSG.Add("Template name");
        }

        if (!PurchaseOrderTemplateItemViews.Any())
        {
            errorMSG.Add("Items");
        }

        if (errorMSG.Any())
        {
            throw new CustomException(ExceptionStatus.Failed, $"Please select the following fields: {string.Join(", ", errorMSG)}");
        }
    }

    public async Task OnSaveOrUpdateClick()
    {

        PurchaseOrderTemplateMaster.PurchaseOrderTemplateLines = PurchaseOrderTemplateItemViews.ToList<IPurchaseOrderTemplateLine>();
        PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.OrgUID = _appUser.SelectedJobPosition.OrgUID;
        if (_appUser.Role.IsDistributorRole)
        {
            PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.StoreUid = _appUser.SelectedJobPosition.OrgUID;
            PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.IsCreatedByStore = true;
        }
        if (IsAdd)
        {
            AddCreateFields(PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader);
            PurchaseOrderTemplateMaster.PurchaseOrderTemplateLines.ForEach(e =>
            {
                e.PurchaseOrderTemplateHeaderUID = PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.UID;
                AddCreateFields(e);
            });
        }
        else
        {
            AddUpdateFields(PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader);
            PurchaseOrderTemplateMaster.PurchaseOrderTemplateLines.ForEach(e =>
            {
                if (string.IsNullOrEmpty(e.UID))
                {
                    AddCreateFields(e);
                    e.PurchaseOrderTemplateHeaderUID = PurchaseOrderTemplateMaster.PurchaseOrderTemplateHeader.UID;
                }
                else
                {
                    AddUpdateFields(e);
                }
            });
        }

        if (await _addEditPurchaseOrderTemplateDataHelper.CUD_POTemplate(PurchaseOrderTemplateMaster))
            throw new CustomException(ExceptionStatus.Success, $"Purchase order template {(IsAdd ? "created" : "updated")} successfully");
        else throw new CustomException(ExceptionStatus.Failed, $"Purchase order template {(IsAdd ? "create" : "update")} failed");
    }

    public async Task OnDeleteSelectedItems()
    {
        List<string> purchaseOrderLineUids = PurchaseOrderTemplateItemViews.Where(e => e.IsSelected).Select(e => e.UID).ToList();
        if (!IsAdd)
        {
            await _addEditPurchaseOrderTemplateDataHelper.DeletePurchaseOrderTemplateLinesByUIDs(purchaseOrderLineUids);
        }
        List<string> selectedSKUs = PurchaseOrderTemplateItemViews.Where(e => e.IsSelected).Select(e => e.SKUUID).ToList();
        int count = 1;
        PurchaseOrderTemplateItemViews.RemoveAll(e => selectedSKUs.Contains(e.SKUUID));
        FilteredPurchaseOrderTemplateItemViews.RemoveAll(e => selectedSKUs.Contains(e.SKUUID));
        PurchaseOrderTemplateItemViews.ForEach(e => e.LineNumber = count++);
    }
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool isUIDRequired = true)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID;
        baseModel.ModifiedBy = _appUser?.Emp?.UID;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (isUIDRequired)
        {
            baseModel.UID = Guid.NewGuid().ToString();
        }
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
}
