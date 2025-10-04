using System.Collections.Generic;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.BL.UIInterfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIClasses;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.SKUClass.BL.UIClasses;

public abstract class SKUClassGroupItemsBaseViewModel : ISKUClassGroupItemsViewModel
{
    // Injection
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SKUClassGroupUID { get; set; }
    public List<ISelectionItem> OrgSelectionItems { get; set; }
    public List<ISelectionItem> DistributionChannelSelectionItems { get; set; }
    public List<ISelectionItem> PlantSelectionItems { get; set; }
    public List<ISKUAttributes> SkuAttributesList { get; set; }
    public List<ISKU> SKUList { get; set; }
    public Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster SKUClassGroupMaster { get; set; }
    public bool IsEdit { get; set; }
    protected SKUClassGroupItemsBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting
            )
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        OrgSelectionItems = new List<ISelectionItem>();
        DistributionChannelSelectionItems = new List<ISelectionItem>();
        PlantSelectionItems = new List<ISelectionItem>();
        SkuAttributesList = new List<ISKUAttributes>();
        SKUList = new List<ISKU>();
        SKUClassGroupMaster = new SKUClassGroupMaster();
        SKUClassGroupMaster.SKUClassGroup = new SKUClassGroup();
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
        OrgSelectionItems.Clear();
        OrgSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems
            (await GetOrgs(new List<FilterCriteria> { new FilterCriteria("OrgTypeUID", "FR", FilterType.Equal) })
            , new List<string> { "UID", "Code", "Name" }));
        PlantSelectionItems.Clear();
        PlantSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems
            (await GetOrgs(new List<FilterCriteria> { new FilterCriteria("OrgTypeUID", "PLNT", FilterType.Equal),
                new FilterCriteria("ParentUID", _appUser?.SelectedJobPosition?.OrgUID ?? "N/A", FilterType.Equal) })
            , new List<string> { "UID", "Code", "Name" }));
        List<ISKUMaster>? sKUMastersData = await GetSKUMasters(new());
        if (sKUMastersData != null)
        {
            SkuAttributesList.Clear();
            SkuAttributesList.AddRange(sKUMastersData.SelectMany(e => e.SKUAttributes));
            SKUList.Clear();
            SKUList.AddRange(sKUMastersData.Select(e => e.SKU));
        }
        if (!string.IsNullOrEmpty(skuClassGroupUID))
        {
            IsEdit = true;
            await BindEditPageDDL();
        }
    }
    public async Task OnOrgSelect(ISelectionItem selectionItem)
    {
        SKUClassGroupMaster.SKUClassGroup!.OrgUID = selectionItem.UID;
        await BindDistributionChannelDDByOrgUID(selectionItem.UID);
    }
    public async Task BindDistributionChannelDDByOrgUID(string orgUID)
    {
        DistributionChannelSelectionItems.Clear();
        DistributionChannelSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IOrg>
            (
            await GetOrgs(new List<FilterCriteria> {
                new FilterCriteria("OrgTypeUID", "DC", FilterType.Equal),
                new FilterCriteria("ParentUID",orgUID,FilterType.Equal)
            }), new List<string> { "UID", "Code", "Name" }));
    }
    public async Task AddSKUsToGrid(List<ISelectionItem> selectionItems)
    {
        string msg = string.Empty; 
        foreach (ISelectionItem selectionItem in selectionItems)
        {
            ISKU? sku = SKUList.Find(e => e.UID == selectionItem.UID);
            if (sku is not null)
            {
                if(SKUClassGroupMaster.SKUClassGroupItems!.Any(e => e.SKUCode == sku.Code))
                {

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
            ISelectionItem? DistributionChannelselectionItem = DistributionChannelSelectionItems.
                Find(e => e.UID == SKUClassGroupMaster.SKUClassGroup!.DistributionChannelUID);
            if (DistributionChannelselectionItem is not null)
            {
                DistributionChannelselectionItem.IsSelected = true;
            }
        }
    }
    public async Task<bool> OnSaveClick()
    {
        if (IsEdit)
        {
            AddUpdateFields(SKUClassGroupMaster.SKUClassGroup!);
            foreach (var item in SKUClassGroupMaster.SKUClassGroupItems!)
            {
                if (item.ActionType == ActionType.Add)
                {
                    AddCreateFields(item, true);
                }
                else
                {
                    AddUpdateFields(item);
                }
            }
        }
        else
        {
            AddCreateFields(SKUClassGroupMaster.SKUClassGroup!, true);
            SKUClassGroupMaster.SKUClassGroup!.SKUClassUID = "PORTFOLIO";
            foreach (var item in SKUClassGroupMaster.SKUClassGroupItems!)
            {
                AddCreateFields(item, true);
                item.SKUClassGroupUID = SKUClassGroupMaster.SKUClassGroup.UID;
            }
        }
        return await CUD_SKUClassGroupMaster(SKUClassGroupMaster);
    }
    protected abstract Task<ISKUClassGroupMaster?>
        GetSKUClassGroupMaster(string skuClassGroupUID);

    protected abstract Task<List<IOrg>> GetOrgs(List<FilterCriteria> filterCriterias);
    protected abstract Task<List<ISKUMaster>?> GetSKUMasters(List<string> orgs);
    protected abstract Task<bool> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster);
    #region Common Util Methods
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

