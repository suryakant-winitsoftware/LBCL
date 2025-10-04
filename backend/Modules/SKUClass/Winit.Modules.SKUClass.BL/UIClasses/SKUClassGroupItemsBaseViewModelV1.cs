using iTextSharp.text;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Constants;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKUClass.BL.UIInterfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIClasses;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using WINITSharedObjects.Models;

namespace Winit.Modules.SKUClass.BL.UIClasses;

public abstract class SKUClassGroupItemsBaseViewModelV1 : ISKUClassGroupItemsViewModelV1
{
    // Injection
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    public readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SKUClassGroupUID { get; set; }
    public List<ISelectionItem> OrgSelectionItems { get; set; }
    public List<ISelectionItem> DistributionChannelSelectionItems { get; set; }
    public List<ISelectionItem> PlantSelectionItems { get; set; }
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; set; }
    public List<ISKUV1> SKUs { get; set; }
    public Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster SKUClassGroupMaster { get; set; }
    public bool IsEdit { get; set; }
    public string SKUSearchString { get; set; }
    public List<ISKUV1> GridSKUs { get; set; }
    public List<ISelectionItem> BroadClassificationSelectionItems { get; set; }
    public List<ISelectionItem> BranchDdlSelectionItems { get; set; }
    public List<ISelectionItem> ChannelPartners { get; set; }
    public List<string> SkuExcludeList { get; set; }
    public List<string> SkuDeleteList { get; set; }
    public List<ISelectionItem> SelectedBranches { get; set; } = [];
    public List<ISelectionItem> SelectedCP { get; set; } = [];
    public List<ISelectionItem> SelectedBC { get; set; } = [];
    public ISelectionMapMaster SelectionMapMaster = new SelectionMapMaster();

    public IEnumerable<ISKUV1> DisplayGridSKUs
    {
        get
        {
            return GridSKUs.Where(e => string.IsNullOrEmpty(SKUSearchString) ||
                (e.Name?.Contains(SKUSearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.Code?.Contains(SKUSearchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }
    private readonly Winit.Modules.SKU.BL.Interfaces.IAddProductPopUpDataHelper _addProductPopUpDataHelper;

    protected SKUClassGroupItemsBaseViewModelV1(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IAddProductPopUpDataHelper addProductPopUpDataHelper)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _addProductPopUpDataHelper = addProductPopUpDataHelper;
        OrgSelectionItems = new List<ISelectionItem>();
        DistributionChannelSelectionItems = new List<ISelectionItem>();
        PlantSelectionItems = new List<ISelectionItem>();
        SKUAttributeData = [];
        SKUs = [];
        GridSKUs = [];
        SkuExcludeList = [];
        SkuDeleteList = [];
        BroadClassificationSelectionItems = [];
        BranchDdlSelectionItems = [];
        ChannelPartners = [];
        SKUClassGroupMaster = new SKUClassGroupMaster();
        SKUClassGroupMaster.SKUClassGroup = new SKUClassGroup();
        SKUClassGroupMaster.SKUClassGroup.FromDate = DateTime.Now.AddDays(1);
        SKUClassGroupMaster.SKUClassGroup.UID = Guid.NewGuid().ToString();
        SKUClassGroupMaster.SKUClassGroup.ToDate = DateTime.Now.AddMonths(2).AddDays(1);
        SKUClassGroupMaster.SKUClassGroupItems = new List<ISKUClassGroupItemView>();
    }

    public async Task PopulateViewModel(string skuClassGroupUID)
    {
        SKUClassGroupUID = skuClassGroupUID;
        Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster? data =
            await GetSKUClassGroupMaster(skuClassGroupUID);
        if (data is not null && data.SKUClassGroupItems is not null && data.SKUClassGroup is not null)
        {
            SKUClassGroupMaster = data;
            data.SKUClassGroup.ActionType = ActionType.Update;
            data.SKUClassGroupItems.ForEach(e => e.ActionType = ActionType.Update);
        }

        List<ISKUV1>? skUs = await _addProductPopUpDataHelper.GetAllSKUs(new());
        if (skUs != null)
        {
            SKUs.Clear();
            SKUs.AddRange(skUs);
        }
        _ = PopulateSKUAttributeData();
        if (!string.IsNullOrEmpty(skuClassGroupUID))
        {
            IsEdit = true;
            ISelectionMapMaster? selectionMapMaster = await GetSelectionMapMasterByLinkedItemUID(skuClassGroupUID);
            if (selectionMapMaster is not null)
            {
                SelectionMapMaster = selectionMapMaster;
            }
            await BindEditPageDDL();
        }
        PopulateGridData();

    }
    public async Task PopulateSKUAttributeData()
    {
        SKUAttributeData.Clear();
        List<SKUAttributeDropdownModel>? skuAttibuteData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        if (skuAttibuteData != null && skuAttibuteData.Any())
        {
            SKUAttributeData.AddRange(skuAttibuteData);
        }
    }
    public async Task OnOrgSelect(ISelectionItem selectionItem)
    {
        SKUClassGroupMaster.SKUClassGroup!.OrgUID = selectionItem.UID;
        await BindDistributionChannelDDByOrgUID(selectionItem.UID);
    }

    private void PopulateGridData()
    {
        if (IsEdit)
        {
            GridSKUs.Clear();
            SKUClassGroupMaster.SKUClassGroupItems.ForEach(e =>
            {
                ISKUV1 sku = SKUs.Find(i => i.UID == e.SKUUID);
                GridSKUs.Add(sku);
                if (e.IsExcluded)
                    SkuExcludeList.Add(sku.UID);
            });
        }
    }
    public async Task BindDistributionChannelDDByOrgUID(string orgUID)
    {
        DistributionChannelSelectionItems.Clear();
        DistributionChannelSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IOrg>
        (
        await GetOrgs(new List<FilterCriteria>
        {
            new FilterCriteria("OrgTypeUID", "DC", FilterType.Equal),
            new FilterCriteria("ParentUID", orgUID, FilterType.Equal)
        }), new List<string> { "UID", "Code", "Name" }));
    }
    public bool FilterAction(List<FilterCriteria> filterCriterias, ISKUV1 sKUV1)
    {
        return _addProductPopUpDataHelper.FilterAction(filterCriterias, sKUV1);
    }
    public async Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID)
    {
        List<ISelectionItem> data =
            await _addProductPopUpDataHelper.OnSKuAttributeDropdownValueSelect(selectedItemUID);
        return data != null && data.Any() ? data.ToList<ISelectionItem>() : [];
    }
    public async Task AddSKUsToGrid(List<ISKUV1> skus)
    {
        string msg = string.Empty;

        foreach (ISKUV1 sku in skus)
        {
            if (SKUClassGroupMaster.SKUClassGroupItems!.Any(e => e.SKUCode == sku.Code))
            {
                //return;
            }
            else
            {
                ISKUClassGroupItemView sKUClassGroupItemView = new SKUClassGroupItemView
                {
                    SKUCode = sku.Code,
                    SKUName = sku.Name,
                    SKUClassGroupUID = this.SKUClassGroupUID!
                };
                AddCreateFields(sKUClassGroupItemView, true);
                SKUClassGroupMaster.SKUClassGroupItems!.Add(sKUClassGroupItemView);
            }
        }
        await Task.CompletedTask;
    }
    private async Task BindEditPageDDL()
    {
        ISelectionItem? seletedOrg = OrgSelectionItems.Find(e => e.UID == SKUClassGroupMaster.SKUClassGroup!.OrgUID);
        if (seletedOrg != null)
        {
            seletedOrg.IsSelected = true;
            await OnOrgSelect(seletedOrg);
            ISelectionItem? distributionChannelselectionItem = DistributionChannelSelectionItems.Find(e => e.UID == SKUClassGroupMaster.SKUClassGroup!.DistributionChannelUID);
            if (distributionChannelselectionItem is not null)
            {
                distributionChannelselectionItem.IsSelected = true;
            }
        }
        List<ISelectionMapDetails> broadClassificationSelectionItems = SelectionMapMaster.SelectionMapDetails.FindAll(e => e.TypeUID == "BroadClassification");
        foreach (ISelectionMapDetails selectionMapDetail in broadClassificationSelectionItems)
        {
            ISelectionItem selectedBroadClassification = BroadClassificationSelectionItems.Find(e => e.UID == selectionMapDetail.SelectionValue);
            if (selectedBroadClassification is not null)
            {
                selectedBroadClassification.IsSelected = true;
                SelectedBC.Add(selectedBroadClassification);
            }
        }

        List<ISelectionMapDetails> branchSelectionItems = SelectionMapMaster.SelectionMapDetails.FindAll(e => e.TypeUID == "Branch");
        foreach (ISelectionMapDetails selectionMapDetail in branchSelectionItems)
        {
            ISelectionItem selectedbranch = BranchDdlSelectionItems.Find(e => e.UID == selectionMapDetail.SelectionValue);
            if (selectedbranch is not null)
            {
                selectedbranch.IsSelected = true;
                SelectedBranches.Add(selectedbranch);
            }
        }

        List<ISelectionMapDetails> cahnnelPartnerSelectionItems = SelectionMapMaster.SelectionMapDetails.FindAll(e => e.TypeUID == "ChannelPartner");
        foreach (ISelectionMapDetails selectionMapDetail in cahnnelPartnerSelectionItems)
        {
            ISelectionItem selectedChannelPartners = ChannelPartners.Find(e => e.UID == selectionMapDetail.SelectionValue);
            if (selectedChannelPartners is not null)
            {
                selectedChannelPartners.IsSelected = true;
                SelectedCP.Add(selectedChannelPartners);
            }
        }
    }
    public async Task<ApiResponse<string>> OnSaveClick()
    {
        if (IsEdit)
        {
            AddUpdateFields(SKUClassGroupMaster.SKUClassGroup!);
            if (SKUClassGroupMaster.SKUClassGroup!.ToDate < DateTime.Now)
            {
                SKUClassGroupMaster.SKUClassGroup.IsActive = false;
            }
        }
        else
        {
            AddCreateFields(SKUClassGroupMaster.SKUClassGroup!, false);
            SKUClassGroupMaster.SKUClassGroup!.IsActive = true;
        }
        int x = 0;
        GridSKUs.ForEach(sku =>
        {
            ISKUClassGroupItemView? skuclassGroupItem = SKUClassGroupMaster.SKUClassGroupItems!.Find(e => e.SKUUID == sku.UID);
            if (skuclassGroupItem is not null)
            {
                if (IsEdit)
                {
                    skuclassGroupItem.ActionType = ActionType.Update;
                    AddUpdateFields(skuclassGroupItem);
                }
            }
            else
            {
                skuclassGroupItem = new SKUClassGroupItemView();
                skuclassGroupItem.ActionType = ActionType.Add;
                skuclassGroupItem.SKUCode = sku.Code;
                skuclassGroupItem.SKUUID = sku.UID;
                //skuclassGroupItem.SKUClassGroupUID = SKUClassGroupMaster.SKUClassGroup.UID;
                AddCreateFields(skuclassGroupItem, true);
                SKUClassGroupMaster.SKUClassGroupItems.Add(skuclassGroupItem);
            }
            skuclassGroupItem.SerialNumber = ++x;
            skuclassGroupItem.SKUClassGroupUID = SKUClassGroupMaster.SKUClassGroup.UID;
            skuclassGroupItem.IsExcluded = SkuExcludeList.Contains(sku.UID);

        });
        foreach (var skuClassGroupItem in SKUClassGroupMaster.SKUClassGroupItems)
        {
            if (!GridSKUs.Any(e => e.UID == skuClassGroupItem.SKUUID))
            {
                skuClassGroupItem.ActionType = ActionType.Delete;
            }
        }
        var apiResponse = await CUD_SKUClassGroupMaster(SKUClassGroupMaster);
        if (apiResponse != null && apiResponse.IsSuccess)
        {
            await SaveMapping();
            DataPreparation();
        }
        return apiResponse;
    }


    public async Task<bool> SaveMapping()
    {
        try
        {
            if (IsEdit)
            {
                SelectionMapMaster.SelectionMapCriteria!.ActionType = ActionType.Update;
                AddUpdateFields(SelectionMapMaster.SelectionMapCriteria);
            }
            else
            {
                SelectionMapMaster.SelectionMapCriteria = new SelectionMapCriteria()
                {
                    IsActive = true,
                    ActionType = ActionType.Add,
                };
                AddCreateFields(SelectionMapMaster.SelectionMapCriteria, true);
            }
            SelectionMapMaster.SelectionMapDetails = new List<ISelectionMapDetails>();
            SelectionMapMaster.SelectionMapCriteria.LinkedItemType = GroupConstant.SKUClassGroup;
            SelectionMapMaster.SelectionMapCriteria.LinkedItemUID = SKUClassGroupMaster.SKUClassGroup.UID;
            SelectionMapMaster.SelectionMapCriteria.HasLocation = false;
            SelectionMapMaster.SelectionMapCriteria.LocationCount = 0;
            SelectionMapMaster.SelectionMapCriteria.HasCustomer = false;
            SelectionMapMaster.SelectionMapCriteria.CustomerCount = 0;
            SelectionMapMaster.SelectionMapCriteria.HasOrganization = false;
            SelectionMapMaster.SelectionMapCriteria.OrgCount = 0;
            if (SelectedBranches != null && SelectedBranches.Any())
            {
                foreach (var row in SelectedBranches)
                {
                    ISelectionMapDetails selectionMap = SelectionMapMaster.SelectionMapDetails.Find(e => e.SelectionValue == row.UID);
                    if (selectionMap is not null)
                    {
                        selectionMap.ActionType = ActionType.Update;
                        AddUpdateFields(selectionMap);
                    }
                    else
                    {
                        selectionMap = CreateSelectionMapDetails(typeUID: GroupConstant.Branch, selectionGroup: GroupConstant.Location, selectedValue: row.UID);
                        SelectionMapMaster.SelectionMapDetails.Add(selectionMap);
                    }
                    ProcessSelectionMapCriteriaWithSelectionMapDetails(SelectionMapMaster.SelectionMapCriteria, selectionMap);
                }
            }
            if (SelectedBC != null && SelectedBC.Any())
            {
                foreach (var row in SelectedBC)
                {
                    ISelectionMapDetails selectionMap = SelectionMapMaster.SelectionMapDetails.Find(e => e.SelectionValue == row.UID);
                    if (selectionMap is not null)
                    {
                        selectionMap.ActionType = ActionType.Update;
                        AddUpdateFields(selectionMap);
                    }
                    else
                    {
                        selectionMap = CreateSelectionMapDetails(typeUID: GroupConstant.BroadClassification, selectionGroup: GroupConstant.SalesTeam, selectedValue: row.UID);
                        SelectionMapMaster.SelectionMapDetails.Add(selectionMap);
                    }
                    ProcessSelectionMapCriteriaWithSelectionMapDetails(SelectionMapMaster.SelectionMapCriteria, selectionMap);
                }
            }
            if (SelectedCP != null && SelectedCP.Any())
            {
                foreach (var row in SelectedCP)
                {
                    ISelectionMapDetails selectionMap = SelectionMapMaster.SelectionMapDetails.Find(e => e.SelectionValue == row.UID);
                    if (selectionMap is not null)
                    {
                        selectionMap.ActionType = ActionType.Update;
                        AddUpdateFields(selectionMap);
                    }
                    else
                    {
                        selectionMap = CreateSelectionMapDetails(typeUID: GroupConstant.Store, selectionGroup: GroupConstant.Customer, selectedValue: row.UID);
                        SelectionMapMaster.SelectionMapDetails.Add(selectionMap);
                    }
                    ProcessSelectionMapCriteriaWithSelectionMapDetails(SelectionMapMaster.SelectionMapCriteria, selectionMap);
                }
            }
            foreach (var item in SelectionMapMaster.SelectionMapDetails.Select(e => e.SelectionValue))
            {
                if (SelectedBranches.Any(e => e.UID == item) || SelectedBC.Any(e => e.UID == item) || SelectedCP.Any(e => e.UID == item))
                {
                    continue;
                }
                SelectionMapMaster.SelectionMapDetails.Find(e => e.UID == item).ActionType = ActionType.Delete;
            }
            //SelectionMapMaster.SelectionMapCriteria.HasCustomer = SelectionMapMaster.SelectionMapDetails.Count > 0;
            //SelectionMapMaster.SelectionMapCriteria.CustomerCount = SelectionMapMaster.SelectionMapDetails.Count(p => p.ActionType != ActionType.Delete);

            SelectionMapMaster.SelectionMapCriteria.IsActive = SKUClassGroupMaster.SKUClassGroup!.ToDate < DateTime.Now ? false : true;


            return await SaveMappings();
        }
        catch (Exception)
        {
            throw;
        }
    }
    private SelectionMapDetails CreateSelectionMapDetails(string typeUID, string selectionGroup, string selectedValue)
    {
        var selectionMap = new SelectionMapDetails
        {
            IsExcluded = false,
            SelectionMapCriteriaUID = SelectionMapMaster.SelectionMapCriteria.UID,
            SelectionGroup = selectionGroup,
            TypeUID = typeUID,
            SelectionValue = selectedValue,
            ActionType = ActionType.Add,
        };
        AddCreateFields(selectionMap, true);
        return selectionMap;
    }
    public abstract Task<ISelectionMapMaster?> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID);
    public abstract Task<bool> SaveMappings();
    public abstract Task DataPreparation();
    private void ProcessSelectionMapCriteriaWithSelectionMapDetails(ISelectionMapCriteria selectionMapCriteria,
        ISelectionMapDetails selectionMapDetails)
    {
        switch (selectionMapDetails.SelectionGroup)
        {
            case GroupConstant.Location:
                selectionMapCriteria.HasLocation = true;
                selectionMapCriteria.LocationCount++;
                break;
            case GroupConstant.Customer:
                selectionMapCriteria.HasCustomer = true;
                selectionMapCriteria.CustomerCount++;
                break;
            case GroupConstant.SalesTeam:
                selectionMapCriteria.HasSalesTeam = true;
                selectionMapCriteria.SalesTeamCount++;
                break;
        }
    }
    protected abstract Task GetBroadClassificationHeaderDetails(PagingRequest pagingRequest);
    protected abstract Task GetBranchDetails();
    public abstract Task GetAllChannelPartner();


    public void OnChannelpartnerSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null || dropDownEvent.SelectionItems == null || dropDownEvent.SelectionItems.Count == 0) return;
        SelectedCP.AddRange(dropDownEvent.SelectionItems);
    }
    public void OnBroadClassificationSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedBC.Clear();
            SelectedBC.AddRange(dropDownEvent.SelectionItems);
        }
    }
    public void AddProductsToGrid(List<ISKUV1> products)
    {
        products.ForEach(e =>
        {

            if (!GridSKUs.Contains(e))
            {
                GridSKUs.Add(e);
            }
        });
    }
    public async Task PopulateApplicableToCustomersAndSKU()
    {

        await Task.WhenAll(
        GetAllChannelPartner(),
        GetBranchDetails(),
        GetBroadClassificationHeaderDetails(new()
        {
            FilterCriterias = new()
            {
                new("IsActive", true, FilterType.Equal)
            },
        }));
        if (IsEdit)
        {
            SetEditModeForApplicableCustomers();
        }
    }
    private void SetEditModeForApplicableCustomers()
    {
        ChannelPartners.ForEach(e =>
        {
            e.IsSelected = SelectionMapMaster!.SelectionMapDetails!.Any(m => m.SelectionValue!.Equals(e.UID));
            if (e.IsSelected)
            {
                SelectedCP.Add(e);
            }
        });
        BranchDdlSelectionItems.ForEach(e =>
        {
            e.IsSelected = SelectionMapMaster!.SelectionMapDetails!.Any(m => m.SelectionValue!.Equals(e.UID));
            if (e.IsSelected)
            {
                SelectedBranches.Add(e);
            }
        });
        BroadClassificationSelectionItems.ForEach(e =>
        {
            e.IsSelected = SelectionMapMaster!.SelectionMapDetails!.Any(m => m.SelectionValue!.Equals(e.UID));
            if (e.IsSelected)
            {
                SelectedBC.Add(e);
            }
        });
    }
    public async Task OnBranchSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedBranches.Clear();
            SelectedBranches.AddRange(dropDownEvent.SelectionItems);
        }
        await Task.CompletedTask;
    }

    protected abstract Task<ISKUClassGroupMaster?>
        GetSKUClassGroupMaster(string skuClassGroupUID);

    protected abstract Task<List<IOrg>> GetOrgs(List<FilterCriteria> filterCriterias);
    protected abstract Task<List<ISKUMaster>?> GetSKUMasters(List<string> orgs);
    protected abstract Task<ApiResponse<string>> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster);
    #region Common Util Methods
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID;
        baseModel.ModifiedBy = _appUser?.Emp?.UID;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID;//_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
    #endregion
}
