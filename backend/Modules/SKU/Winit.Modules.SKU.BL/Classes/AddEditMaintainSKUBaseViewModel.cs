using Elasticsearch.Net;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.CustomSKUField.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.UOM.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.CustomControls;

namespace Winit.Modules.SKU.BL.Classes
{
    public abstract class AddEditMaintainSKUBaseViewModel : IAddEditMaintainSKUViewModel
    {
        public IEnumerable<IOrg> SupplierOrg = new List<IOrg>();
        public List<SKUGroupTypeView> SKUGroupTypeAttributeList = new();
        public List<SKUGroupView> SKUGroupAttributeList = new();
        public List<ISelectionItem> ORGSupplierSelectionItems { get; set; }
        private readonly Dictionary<string, ISKUAttributes> SkuAttributesDict = new();
        public List<ISKU> SKUViews { get; set; }
        public ISKUMaster SkuMaster { get; set; }
        private string msg = "";
        public ISKU SKU { get; set; }
        public Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields SKUCUSTOM1 { get; set; }
        public Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField SKUCUSTOMForDynamic { get; set; }
        public ICustomSKUFields SKUCUSTOM { get; set; }
        public Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields SKUCUSTOMFieldsforView { get; set; }
        public List<CustomField> dbData { get; set; }
        public ISKU SelectedSKU { get; set; }
        public List<ISKUMaster> SKUMasterList { get; set; }
        public List<SortCriteria> SKUSortCriterials { get; set; }
        public IEnumerable<IOrg> DistributionMappingList = new List<IOrg>();
        public List<ISelectionItem> oRGDistributionSelectionItems { get; set; }
        public List<ISKUUOM> SKUUOMList { get; set; }
        public List<ISKUConfig> SKUCONFIGLIST { get; set; }
        public List<CustomField> CustomFieldlist { get; set; }
        public ISKUConfig SKUCONFIG { get; set; }
        public ISKUUOM SKUUOM { get; set; }
        public bool IsAddNewSKUVisiblebtn { get; set; }
        public List<ISelectionItem> SKUUOMTypeSelectionItems { get; set; }
        public List<ISelectionItem> BuyingUOMSelectionItems { get; set; }
        public List<ISelectionItem> SellingUOMSelectionItems { get; set; }
        private readonly IListItem VOLUMEUNITLISTITEM;
        public List<IListItem> VolumeUnitList = new();
        public List<ISelectionItem> VoumeUnitSelectionItems { get; set; }
        public List<IUOMType> UOMTypeList = new();
        //GrossVolumeUnit
        private readonly IListItem GrossVolumeUnit;
        public List<IListItem> GrossWeightUnitList = new();
        public List<ISelectionItem> GrossWeightUnitSelectionItems { get; set; }
        public List<IListItem> WeightUnitList = new();

        public List<ISelectionItem> WeightUnitSelectionItems { get; set; }

        //SelectionManagers for DRopDown
        private SelectionManager SKUUOMTypeSM { get; set; }
        private SelectionManager VolumeUnitSM { get; set; }
        private SelectionManager GrossWeightUnitSM { get; set; }
        private SelectionManager WeightUnitSM { get; set; }
        private SelectionManager DistributionTypeSM { get; set; }
        private SelectionManager BuyingUOMSM { get; set; }
        private SelectionManager SellingUOMSM { get; set; }
        public DropDown DropDown { get; set; }
        private SelectionManager ItemGroupSM { get; set; }
        private SelectionManager ProductGroupSM { get; set; }
        public List<SKUAttributeDropdownModel> SKUAttributeData { get; set; }
        public List<ISKUAttributes> SKUAttributes { get; set; }
        private SelectionManager MCLProductGroupSM { get; set; }
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly List<string> _propertiesToSearch = new();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private readonly Winit.Modules.Base.BL.ApiService _apiService;
        public bool IsAddSkuAttribute { get; set; }
        public AddEditMaintainSKUBaseViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager,
              IAppConfig appConfigs,
              Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            // Initialize common properties or perform other common setup
            SKU = _serviceProvider.CreateInstance<ISKU>();
            SKUUOM = new Winit.Modules.SKU.Model.Classes.SKUUOM();
            SKUCONFIG = new Winit.Modules.SKU.Model.Classes.SKUConfig();
            //SKUCUSTOM = new CustomSKUFields();
            SKUSortCriterials = new List<SortCriteria>();
            SKUUOMTypeSelectionItems = new List<ISelectionItem>();
            VoumeUnitSelectionItems = new List<ISelectionItem>();
            GrossWeightUnitSelectionItems = new List<ISelectionItem>();
            WeightUnitSelectionItems = new List<ISelectionItem>();
            oRGDistributionSelectionItems = new List<ISelectionItem>();
            BuyingUOMSelectionItems = new List<ISelectionItem>();
            SellingUOMSelectionItems = new List<ISelectionItem>();
            ORGSupplierSelectionItems = new List<ISelectionItem>();
            SKUUOMList = new List<ISKUUOM>();
            SKUCONFIGLIST = new List<ISKUConfig>();
            SkuMaster = new Winit.Modules.SKU.Model.Classes.SKUMaster();
            SKUAttributeData = new List<SKUAttributeDropdownModel>();
            SKUAttributes = new List<ISKUAttributes>();
            // folderImageDetails1 = new List<IFolderImageDetails>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public virtual async Task PopulateViewModel()
        {

            SKUGroupAttributeList = await GetSKUGroupAttributeData();
            SKUGroupTypeAttributeList = await GetSKUGroupTypeAttributeData();
            UOMTypeList = await GetSKUUOMData();
            VolumeUnitList = await GetSKUUOMVolumeUnitData("Volume_Unit");
            GrossWeightUnitList = await GetSKUUOMGrossWeightUnitData("Weight_Unit");// GetDDListItemsFromAPIAsync("GrossWeight_Unit");
            DistributionMappingList = await GetDistributionMappingData();
            SKUUOMInitializePopUp();
            DistributionInializePopUp();
            await PopulateSKUAttributeData();
        }

        public async Task PopulateSKUAttributeData()
        {
            List<SKUAttributeDropdownModel> sKUAttributeDropdownModels = await GetSKUAttribute_Data();
            if (sKUAttributeDropdownModels != null && sKUAttributeDropdownModels.Any())
            {
                SKUAttributeData.AddRange(sKUAttributeDropdownModels);
                foreach (SKUAttributeDropdownModel item in SKUAttributeData)
                {
                    if (item.ParentUID == null)
                    {
                        List<SKUGroupSelectionItem> data = await GetSKUGroupSelectionItemBySKUGroupTypeUID(item.UID, null);
                        if (data != null && data.Any())
                        {
                            item.DropDownDataSource.AddRange(data.ToList<ISelectionItem>());
                        }
                    }
                }
            }
        }
        protected abstract Task<List<SKUAttributeDropdownModel>> GetSKUAttribute_Data();
        protected abstract Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID);
        public async Task<List<CustomField>> PopulateCustomSkuFieldsDynamic()
        {
            return CustomFieldlist = await GetCustomSkuFieldsDynamicData();
        }
        #region Business Logics  
        public async Task SaveSKUCustomFieldsForDynamic(List<Winit.Modules.CustomSKUField.Model.Classes.CustomField> customField)
        {
            Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField = new()
            {
                SKUUID = SKU.UID,
                CustomField = customField
            };
            _ = await CreateUpdateCustomSKuField(customSKUField);
        }
        public void SKUUOMInitializePopUp()
        {
            if (UOMTypeList != null && UOMTypeList.Any())
            {
                SKUUOMTypeSelectionItems.Clear();
                SKUUOMTypeSelectionItems.AddRange(ConvertUOMTYpeToSelectionItem(UOMTypeList));
                SKUUOMTypeSM = new SelectionManager(SKUUOMTypeSelectionItems, SelectionMode.Single);
            }
            else
            {
                SKUUOMTypeSM = new SelectionManager(new(), SelectionMode.Single);

            }
            if (VolumeUnitList != null && VolumeUnitList.Any())
            {
                VoumeUnitSelectionItems.Clear();
                VoumeUnitSelectionItems.AddRange(ConvertListItemToSelectionItem(VolumeUnitList));
                VolumeUnitSM = new SelectionManager(VoumeUnitSelectionItems, SelectionMode.Single);
            }
            else
            {
                VolumeUnitSM = new SelectionManager(new(), SelectionMode.Single);

            }
            if (GrossWeightUnitList != null && GrossWeightUnitList.Any())
            {
                GrossWeightUnitSelectionItems.Clear();
                GrossWeightUnitSelectionItems.AddRange(ConvertListItemToSelectionItem(GrossWeightUnitList));
                GrossWeightUnitSM = new SelectionManager(GrossWeightUnitSelectionItems, SelectionMode.Single);
            }
            else
            {
                GrossWeightUnitSM = new SelectionManager(new(), SelectionMode.Single);
            }
            if (GrossWeightUnitList != null && GrossWeightUnitList.Any())
            {
                WeightUnitSelectionItems.Clear();
                WeightUnitSelectionItems.AddRange(ConvertListItemToSelectionItem(GrossWeightUnitList));
                WeightUnitSM = new SelectionManager(WeightUnitSelectionItems, SelectionMode.Single);
            }
            else
            {
                WeightUnitSM = new SelectionManager(new(), SelectionMode.Single);
            }
        }
        public void DistributionInializePopUp()
        {
            if (DistributionMappingList != null && DistributionMappingList.Any())
            {
                oRGDistributionSelectionItems.Clear();
                oRGDistributionSelectionItems.AddRange(ConvertDistributionChannelDropDownToSelectionItem(DistributionMappingList));
                DistributionTypeSM = new SelectionManager(oRGDistributionSelectionItems, SelectionMode.Single);
            }
            else
            {
                DistributionTypeSM = new SelectionManager(new(), SelectionMode.Single);
            }
        }
        public void SetDropDownDataForDistributionChannel()
        {
            if (SKUUOMList != null && SKUUOMList.Any())
            {
                BuyingUOMSelectionItems.Clear();
                BuyingUOMSelectionItems.AddRange(ConvertSKUUUOMToSelectionItem(SKUUOMList));
                BuyingUOMSM = new SelectionManager(BuyingUOMSelectionItems, SelectionMode.Single);
            }
            if (SKUUOMList != null && SKUUOMList.Any())
            {
                SellingUOMSelectionItems.Clear();
                SellingUOMSelectionItems.AddRange(ConvertSKUUUOMToSelectionItem(SKUUOMList));
                SellingUOMSM = new SelectionManager(SellingUOMSelectionItems, SelectionMode.Single);
            }
        }
        public async Task<ISKUUOM> CreateSKUUOMClone(ISKUUOM ogSKUUOM)
        {
            ISKUUOM sKUUOM = _serviceProvider.CreateInstance<ISKUUOM>();
            sKUUOM.UID = ogSKUUOM.UID;
            sKUUOM.Code = ogSKUUOM.Code;
            sKUUOM.Name = ogSKUUOM.Name;
            sKUUOM.Label = ogSKUUOM.Name;
            sKUUOM.Barcodes = ogSKUUOM.Barcodes;
            sKUUOM.IsBaseUOM = ogSKUUOM.IsBaseUOM;
            sKUUOM.IsOuterUOM = ogSKUUOM.IsOuterUOM;
            sKUUOM.Multiplier = ogSKUUOM.Multiplier;
            sKUUOM.Length = ogSKUUOM.Length;
            sKUUOM.Depth = ogSKUUOM.Depth;
            sKUUOM.Width = ogSKUUOM.Width;
            sKUUOM.Height = ogSKUUOM.Height;
            sKUUOM.Volume = ogSKUUOM.Volume;
            sKUUOM.Weight = ogSKUUOM.Weight;
            sKUUOM.GrossWeight = ogSKUUOM.GrossWeight;
            sKUUOM.DimensionUnit = ogSKUUOM.DimensionUnit;
            sKUUOM.VolumeUnit = ogSKUUOM.VolumeUnit;
            sKUUOM.WeightUnit = ogSKUUOM.WeightUnit;
            sKUUOM.GrossWeightUnit = ogSKUUOM.GrossWeightUnit;
            sKUUOM.Liter = ogSKUUOM.Liter;
            sKUUOM.KGM = ogSKUUOM.KGM;
            sKUUOM.Id = ogSKUUOM.Id;
            sKUUOM.SS = ogSKUUOM.SS;
            sKUUOM.CreatedBy = ogSKUUOM.CreatedBy;
            sKUUOM.CreatedTime = ogSKUUOM.CreatedTime;
            sKUUOM.ModifiedBy = ogSKUUOM.ModifiedBy;
            sKUUOM.ModifiedTime = ogSKUUOM.ModifiedTime;
            sKUUOM.ServerAddTime = ogSKUUOM.ServerAddTime;
            sKUUOM.ServerModifiedTime = ogSKUUOM.ServerModifiedTime;
            return await Task.Run(() => sKUUOM);
        }
        public async Task<ISKUConfig> CreateDistributionClone(ISKUConfig ogSKUConfig)
        {
            ISKUConfig sKUConfig = _serviceProvider.CreateInstance<ISKUConfig>();
            // Assuming ogSKUConfig is an instance of a class with relevant properties
            sKUConfig.UID = ogSKUConfig.UID;
            sKUConfig.OrgUID = ogSKUConfig.OrgUID;
            sKUConfig.Name = ogSKUConfig.Name;
            sKUConfig.DistributionChannelOrgUID = ogSKUConfig.DistributionChannelOrgUID;
            sKUConfig.SKUUID = ogSKUConfig.SKUUID;
            sKUConfig.CanBuy = ogSKUConfig.CanBuy;
            sKUConfig.CanSell = ogSKUConfig.CanSell;
            sKUConfig.BuyingUOM = ogSKUConfig.BuyingUOM;
            sKUConfig.SellingUOM = ogSKUConfig.SellingUOM;
            sKUConfig.IsActive = ogSKUConfig.IsActive;
            sKUConfig.Id = ogSKUConfig.Id;
            sKUConfig.SS = ogSKUConfig.SS;
            sKUConfig.CreatedBy = ogSKUConfig.CreatedBy;
            sKUConfig.CreatedTime = ogSKUConfig.CreatedTime;
            sKUConfig.ModifiedBy = ogSKUConfig.ModifiedBy;
            sKUConfig.ModifiedTime = ogSKUConfig.ModifiedTime;
            sKUConfig.ServerAddTime = ogSKUConfig.ServerAddTime;
            sKUConfig.ServerModifiedTime = ogSKUConfig.ServerModifiedTime;
            return await Task.Run(() => sKUConfig);
        }
        private List<ISelectionItem> ConvertSKUUUOMToSelectionItem(List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> skuUom)
        {
            List<ISelectionItem> selectionItems = new();
            foreach (ISKUUOM volume in skuUom)
            {
                SelectionItem si = new()
                {
                    UID = volume.UID,
                    Code = volume.Code,
                    Label = volume.Name
                };
                selectionItems.Add(si);
            }
            return selectionItems;
        }

        private List<ISelectionItem> ConvertDistributionChannelDropDownToSelectionItem(IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> DistributionChannel)
        {
            List<ISelectionItem> selectionItems = new();
            foreach (IOrg volume in DistributionChannel)
            {
                SelectionItem si = new()
                {
                    //si.Code = volume.Code;
                    Label = volume.Name,
                    UID = volume.UID
                };
                selectionItems.Add(si);
            }
            return selectionItems;
        }

        public async Task InstilizeFieldsForEditPage(ISKUMaster sKUMaster)
        {
            if (sKUMaster.SKU != null)
            {
                SetEditForSKU(sKUMaster.SKU);
            }
            if (sKUMaster.SKUAttributes != null)
            {
                await SetEditForSKUAttribute(sKUMaster.SKUAttributes);
            }
            if (sKUMaster.SKUUOMs != null)
            {
                SetEditForSKUUOM(sKUMaster.SKUUOMs);
            }
            if (sKUMaster.SKUConfigs != null)
            {
                SetEditForSKUConfig(sKUMaster.SKUConfigs);
            }
            if (sKUMaster.CustomSKUFields != null)
            {
                SetEditForCustomSku(sKUMaster.CustomSKUFields);
            }
        }
        public void SetEditForSKU(ISKU sku)
        {
            ISelectionItem? selctedOrgSupplier = ORGSupplierSelectionItems?.Find(e => e.UID == sku.SupplierOrgUID);
            if (selctedOrgSupplier != null)
            {
                selctedOrgSupplier.IsSelected = true;
            }
            SKU = sku;
        }
        private async Task SetEditForSKUAttribute(List<ISKUAttributes> sKUAttributes)
        {
            SKUAttributes.Clear();
            SKUAttributes.AddRange(sKUAttributes);
            foreach (var dropDown in SKUAttributeData)
            {
                var skuAttr = sKUAttributes.Find(e => e.Type == dropDown.DropDownTitle);
                if (skuAttr != null)
                {
                    var selectedvalue = dropDown.DropDownDataSource.Find(e => e.Code == skuAttr.Code);
                    if (selectedvalue != null)
                    {
                        selectedvalue.IsSelected = true;
                        var childDropDown = SKUAttributeData.Find(e => dropDown.UID == e.ParentUID);
                        if (childDropDown != null)
                        {
                            childDropDown.DropDownDataSource.AddRange(await OnSKuAttributeDropdownValueSelect(selectedvalue.UID));
                        }
                    }
                }
            }
        }
        public void SetEditForSKUUOM(List<ISKUUOM> sKUUOM)
        {
            SKUUOMList.Clear();
            SKUUOMList.AddRange(sKUUOM);
            SetDropDownDataForDistributionChannel();
        }
        public void SetEditForSKUConfig(List<ISKUConfig> sKUConfig)
        {
            SKUCONFIGLIST.Clear();
            SKUCONFIGLIST.AddRange(sKUConfig);
        }
        public void SetEditForCustomSku(List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields> customSKUFields)
        {
            if (customSKUFields?.FirstOrDefault() != null)
            {
                SKUCUSTOMFieldsforView = customSKUFields?.FirstOrDefault();
            }
        }
        public List<SKUGroupView> GetSKUAttributeData(string parentUid, int Requiredlevel)
        {
            List<SKUGroupView> result = new();

            foreach (SKUGroupView? item in SKUGroupAttributeList.Where(e => e.ParentUID == parentUid && e.ItemLevel == Requiredlevel))
            {
                result.Add(item);
            }

            return result;
        }
        public async Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID)
        {
            List<SKUGroupSelectionItem> data = await GetSKUGroupSelectionItemBySKUGroupTypeUID(null, selectedItemUID);
            return data != null && data.Any() ? data.ToList<ISelectionItem>() : new();
        }
        public async Task<bool> SaveOrUpdateSKUAttributes(Dictionary<string, ISelectionItem> keyValuePairs)
        {

            foreach (KeyValuePair<string, ISelectionItem> item in keyValuePairs)
            {
                var skuAttr = SKUAttributes.Find(e => e.Type == item.Key);
                if (skuAttr != null)
                {
                    skuAttr.Type = item.Key;
                    skuAttr.Code = item.Value.Code;
                    skuAttr.Value = item.Value.Label;
                    skuAttr.SKUUID = SKU.UID;
                    AddUpdateFields(skuAttr);
                    skuAttr.ActionType = ActionType.Update;
                }
                else
                {
                    SKUAttributes skuAttribute = new()
                    {
                        Type = item.Key,
                        Code = item.Value.Code,
                        Value = item.Value.Label,
                        SKUUID = SKU.UID,
                    };
                    AddCreateFields(skuAttribute, true);
                    skuAttribute.ActionType = ActionType.Add;
                    SKUAttributes.Add(skuAttribute);
                }
            }
            return await CUDSKUAttributes_API(SKUAttributes);
        }
        protected abstract Task<bool> CUDSKUAttributes_API(List<ISKUAttributes> sKUAttributes);
        private List<ISelectionItem> ConvertSKUAttributesToSelectionItem(List<Winit.Modules.SKU.Model.Classes.SKUGroupView> groupView)
        {
            List<ISelectionItem> selectionItems = new();
            foreach (SKUGroupView sKUGroupView in groupView)
            {
                SelectionItem si = new()
                {
                    Code = sKUGroupView.Code,
                    Label = sKUGroupView.Name,
                    UID = sKUGroupView.UID
                };
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        public void AddSelectionItemsToDD(ISelectionItem selectionItem, List<ISelectionItem> selectionItems, string skuAttrType)
        {
            SKUGroupView? selecteditem = SKUGroupAttributeList.Find(e => e.UID == selectionItem.UID);
            selectionItems.Clear();
            selectionItems.AddRange(ConvertSKUAttributesToSelectionItem(GetSKUAttributeData(selecteditem.UID, selecteditem.ItemLevel + 1)));
        }

        private List<ISelectionItem> ConvertListItemToSelectionItem(List<Winit.Modules.ListHeader.Model.Interfaces.IListItem> volumeGrossUnit)
        {
            List<ISelectionItem> selectionItems = new();
            foreach (IListItem volume in volumeGrossUnit)
            {
                SelectionItem si = new()
                {
                    Code = volume.Code,
                    Label = volume.Name,
                    UID = volume.ListHeaderUID

                };
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        private List<ISelectionItem> ConvertUOMTYpeToSelectionItem(List<Winit.Modules.UOM.Model.Interfaces.IUOMType> uOMTypes)
        {
            List<ISelectionItem> selectionItems = new();
            foreach (IUOMType uom in uOMTypes)
            {
                SelectionItem si = new()
                {
                    Label = uom.Name,
                    Code = uom.Name,
                    UID = uom.UID
                };
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        public async Task<(string, bool)> SaveUpdateSKUItem(ISKU sKU, bool Iscreate)
        {
            if (Iscreate)
            {
                sKU.OrgUID = _appUser.SelectedJobPosition.OrgUID;
                _ = await CreateUpdateSKUData(sKU, true);
            }
            else
            {
                _ = await CreateUpdateSKUData(sKU, false);
            }
            return ("", false);
        }
        public async Task<(string, bool)> OnSKUUOMCreateUpdateBtnClickFromPopUp(ISKUUOM sKUUOM, bool Iscreate)
        {
            if (Iscreate)
            {
                SKUUOMTypeSM.DeselectAll();


                bool isFirstTime = SKUUOMList.All(e => !e.IsBaseUOM);
                msg = string.Empty;
                if (isFirstTime && !sKUUOM.IsBaseUOM)
                {
                    msg += "IsBaseUOM is required for the first time, ";
                    return (msg, false);
                }
                if (sKUUOM.Code == null && sKUUOM.Multiplier == 0)
                {
                    msg += "The following field(s) have invalid value(s) : UOM,Multiplier ";
                    return (msg, false);
                }
                else if (sKUUOM.Code == null)
                {
                    msg += "The following field(s) have invalid value(s): UOM";
                    return (msg, false);
                }
                else if (sKUUOM.Multiplier == 0)
                {
                    msg += "The following field(s) have invalid value(s): Multiplier";
                    return (msg, false);
                }
                ISKUUOM? sKUUOMCodeDD = SKUUOMList.Find(e => e.Code == sKUUOM.Code);
                if (sKUUOMCodeDD == null)
                {
                    if (sKUUOM.IsBaseUOM || sKUUOM.IsOuterUOM)
                    {
                        if (sKUUOM.IsBaseUOM)
                        {
                            ISKUUOM? baseUOMs = SKUUOMList.Find(e => e.IsBaseUOM);
                            if (baseUOMs == null)
                            {
                                SKU.BaseUOM = sKUUOM.Code;
                                sKUUOM.SKUUID = SKU.UID;
                                sKUUOM.Label = sKUUOM.Name;
                                SKU.IsStockable = true;
                                _ = await CreateUpdateSKUUOMData(sKUUOM, true);
                                SKUUOMTypeSM.DeselectAll();
                                VolumeUnitSM.DeselectAll();
                                GrossWeightUnitSM.DeselectAll();
                                WeightUnitSM.DeselectAll();
                                SKUUOM = sKUUOM;
                                SKUUOMList.Add(SKUUOM);
                                SetDropDownDataForDistributionChannel();
                                //_ = await CreateUpdateSKUData(SKU, false);
                            }
                            else
                            {
                                msg += "Base UOM Is Already Available For the SKU, ";
                                return (msg, false);
                            }
                        }
                        else
                        {
                            ISKUUOM? outerUOMs = SKUUOMList.Find(e => e.IsOuterUOM);
                            if (outerUOMs == null)
                            {
                                SKU.OuterUOM = sKUUOM.Code;
                                sKUUOM.SKUUID = SKU.UID;
                                sKUUOM.Label = sKUUOM.Name;
                                SKU.IsStockable = true;
                                _ = await CreateUpdateSKUUOMData(sKUUOM, true);
                                SKUUOMTypeSM.DeselectAll();
                                VolumeUnitSM.DeselectAll();
                                GrossWeightUnitSM.DeselectAll();
                                WeightUnitSM.DeselectAll();
                                SKUUOM = sKUUOM;
                                SKUUOMList.Add(SKUUOM);
                                SetDropDownDataForDistributionChannel();
                                //_ = await CreateUpdateSKUData(SKU, false);
                            }
                            else
                            {
                                msg += "Outer UOM Is Already Available For the SKU, ";
                                return (msg, false);
                            }
                        }
                    }
                    else
                    {
                        sKUUOM.SKUUID = SKU.UID;
                        sKUUOM.Label = sKUUOM.Name;
                        SKU.IsStockable = true;
                        _ = await CreateUpdateSKUUOMData(sKUUOM, true);
                        SKUUOMTypeSM.DeselectAll();
                        VolumeUnitSM.DeselectAll();
                        GrossWeightUnitSM.DeselectAll();
                        WeightUnitSM.DeselectAll();
                        SKUUOM = sKUUOM;
                        SKUUOMList.Add(SKUUOM);
                        SetDropDownDataForDistributionChannel();
                    }
                }
                else
                {
                    msg += " UOM Is Already Available For the SKU, ";
                    return (msg, false);
                }
            }
            else
            {
                _ = await UpdateSKUUOM(sKUUOM);
                return (string.IsNullOrEmpty(msg) ? "" : msg, false);
            }

            return ("", false);
        }
        public async Task<(string, bool)> UpdateSKUUOM(ISKUUOM sKUUOM)
        {
            if (sKUUOM.IsBaseUOM || sKUUOM.IsOuterUOM)
            {
                if (sKUUOM.IsBaseUOM)
                {
                    ISKUUOM? baseUOM = SKUUOMList.Find(e => e.IsBaseUOM);
                    // var outerUOM = SKUUOMList.Find(e => e.IsOuterUOM);
                    if (!sKUUOM.IsBaseUOM || baseUOM == null || baseUOM?.UID == sKUUOM.UID)
                    {
                        msg = string.Empty;
                        sKUUOM.SKUUID = SKU.UID;
                        ISKUUOM? OriganalSKUUOM = SKUUOMList.Find(e => e.UID == sKUUOM.UID);
                        OriganalSKUUOM.IsBaseUOM = sKUUOM.IsBaseUOM;
                        OriganalSKUUOM.Multiplier = sKUUOM.Multiplier;
                        OriganalSKUUOM.Code = sKUUOM.Code;
                        OriganalSKUUOM.Name = sKUUOM.Name;
                        OriganalSKUUOM.Label = sKUUOM.Name;
                        OriganalSKUUOM.Barcodes = sKUUOM.Barcodes;
                        OriganalSKUUOM.WeightUnit = sKUUOM.WeightUnit;
                        OriganalSKUUOM.Weight = sKUUOM.Weight;
                        OriganalSKUUOM.VolumeUnit = sKUUOM.VolumeUnit;
                        OriganalSKUUOM.GrossWeightUnit = sKUUOM.GrossWeightUnit;
                        OriganalSKUUOM.GrossWeight = sKUUOM.GrossWeight;
                        OriganalSKUUOM.Volume = sKUUOM.Volume;
                        OriganalSKUUOM.IsOuterUOM = sKUUOM.IsOuterUOM;
                        _ = await CreateUpdateSKUUOMData(sKUUOM, false);
                    }
                    else
                    {
                        msg = "Base UOM Is Already Available For the SKU, ";

                    }
                }
                else
                {
                    ISKUUOM? outerUOM = SKUUOMList.Find(e => e.IsOuterUOM);
                    if (!sKUUOM.IsOuterUOM || outerUOM == null || outerUOM?.UID == sKUUOM.UID)
                    {
                        msg = string.Empty;
                        sKUUOM.SKUUID = SKU.UID;
                        ISKUUOM? OriganalSKUUOM = SKUUOMList.Find(e => e.UID == sKUUOM.UID);
                        OriganalSKUUOM.IsBaseUOM = sKUUOM.IsBaseUOM;
                        OriganalSKUUOM.Multiplier = sKUUOM.Multiplier;
                        OriganalSKUUOM.Code = sKUUOM.Code;
                        OriganalSKUUOM.Name = sKUUOM.Name;
                        OriganalSKUUOM.Label = sKUUOM.Name;
                        OriganalSKUUOM.Barcodes = sKUUOM.Barcodes;
                        OriganalSKUUOM.WeightUnit = sKUUOM.WeightUnit;
                        OriganalSKUUOM.Weight = sKUUOM.Weight;
                        OriganalSKUUOM.VolumeUnit = sKUUOM.VolumeUnit;
                        OriganalSKUUOM.GrossWeightUnit = sKUUOM.GrossWeightUnit;
                        OriganalSKUUOM.GrossWeight = sKUUOM.GrossWeight;
                        OriganalSKUUOM.Volume = sKUUOM.Volume;
                        OriganalSKUUOM.IsOuterUOM = sKUUOM.IsOuterUOM;
                        _ = await CreateUpdateSKUUOMData(sKUUOM, false);
                    }
                    else
                    {
                        msg = "Outer UOM Is Already Available For the SKU, ";

                    }
                }
            }
            else
            {
                msg = string.Empty;
                sKUUOM.SKUUID = SKU.UID;
                ISKUUOM? OriganalSKUUOM = SKUUOMList.Find(e => e.UID == sKUUOM.UID);
                OriganalSKUUOM.IsBaseUOM = sKUUOM.IsBaseUOM;
                OriganalSKUUOM.IsOuterUOM = sKUUOM.IsOuterUOM;
                OriganalSKUUOM.Multiplier = sKUUOM.Multiplier;
                OriganalSKUUOM.Code = sKUUOM.Code;
                OriganalSKUUOM.Name = sKUUOM.Name;
                OriganalSKUUOM.Label = sKUUOM.Name;
                OriganalSKUUOM.Barcodes = sKUUOM.Barcodes;
                OriganalSKUUOM.WeightUnit = sKUUOM.WeightUnit;
                OriganalSKUUOM.Weight = sKUUOM.Weight;
                OriganalSKUUOM.VolumeUnit = sKUUOM.VolumeUnit;
                OriganalSKUUOM.GrossWeightUnit = sKUUOM.GrossWeightUnit;
                OriganalSKUUOM.GrossWeight = sKUUOM.GrossWeight;
                OriganalSKUUOM.Volume = sKUUOM.Volume;
                _ = await CreateUpdateSKUUOMData(sKUUOM, false);
            }
            return ("", false);
        }
        public async Task OnSKUUOMEditClick(ISKUUOM sKUUOM)
        {
            ISKUUOM sKUUOM1 = await CreateSKUUOMClone(sKUUOM);
            SKUUOM = sKUUOM1;
            ISelectionItem? selecteditem = SKUUOMTypeSelectionItems?.Find(e => e.Code == sKUUOM.Code);
            if (selecteditem != null)
            {
                SKUUOMTypeSM.Select(selecteditem);
            }
            ISelectionItem? weightselecteditem = WeightUnitSelectionItems?.Find(e => e.Code == sKUUOM.WeightUnit);
            if (weightselecteditem != null)
            {
                WeightUnitSM.Select(weightselecteditem);
            }
            ISelectionItem? VolumeUnitselecteditem = VoumeUnitSelectionItems?.Find(e => e.Code == sKUUOM.VolumeUnit);
            if (VolumeUnitselecteditem != null)
            {
                VolumeUnitSM.Select(VolumeUnitselecteditem);
            }

            ISelectionItem? Grossweightselecteditem = GrossWeightUnitSelectionItems?.Find(e => e.Code == sKUUOM.GrossWeightUnit);
            if (Grossweightselecteditem != null)
            {
                GrossWeightUnitSM.Select(Grossweightselecteditem);
            }

        }
        public void OnSKUUOMCancelBtnClickInPopUp()
        {
            msg = string.Empty;
            SKUUOMTypeSM.DeselectAll();
            VolumeUnitSM.DeselectAll();
            GrossWeightUnitSM.DeselectAll();
            WeightUnitSM.DeselectAll();
        }
        //public async Task SaveSKUCustomFields(ICustomSKUFields sKUCustom, bool Iscreate)
        //{
        //    if (Iscreate)
        //    {
        //        sKUCustom.SKUUID = SKU.UID;
        //        await CreateUpdateCustomfieldsDataFromAPIAsync(sKUCustom, true);
        //    }
        //    else
        //    {
        //        await CreateUpdateCustomfieldsDataFromAPIAsync(sKUCustom, false);
        //    }
        //}
        public async Task<(string, bool)> OnDistributionCreateUpdateBtnClickFromPopUp(ISKUConfig sKUConfig, bool Iscreate)
        {
            if (sKUConfig.DistributionChannelOrgUID == null || sKUConfig.BuyingUOM == null || sKUConfig.SellingUOM == null)
            {
                msg = "The following field(s) have invalid value(s): ";

                if (sKUConfig.DistributionChannelOrgUID == null)
                {
                    msg += " DistributionChannel, ";
                }
                if (sKUConfig.BuyingUOM == null)
                {
                    msg += "BuyingUOM, ";
                }
                if (sKUConfig.SellingUOM == null)
                {
                    msg += "SellingUOM, ";
                }
                msg = msg.TrimEnd(' ', ',');
                return (msg, false);
            }
            if (Iscreate)
            {
                ISKUConfig? sKUConfigCodeDD = SKUCONFIGLIST.Find(e => e.DistributionChannelOrgUID == sKUConfig.DistributionChannelOrgUID);
                if (sKUConfigCodeDD == null)
                {
                    sKUConfig.SKUUID = SKU.UID;
                    sKUConfig.OrgUID = _appUser.SelectedJobPosition.OrgUID;
                    _ = await SaveDistributionData(sKUConfig, true);
                    DistributionTypeSM.DeselectAll();
                    BuyingUOMSM.DeselectAll();
                    SellingUOMSM.DeselectAll();
                    SKUCONFIG = sKUConfig;
                    SKUCONFIGLIST.Add(sKUConfig);
                }
                else
                {
                    msg = " There Is Already One Mapping, you cant add more than one, ";
                    return (msg, false);
                    //if (sKUConfig.DistributionChannelOrgUID == null)
                    //{
                    //    msg = " There Is Already One Mapping, you cant add more than one, ";
                    //    return (msg, false);
                    //}
                    //else
                    //{
                    //    msg = " There Is Already One Mapping, you cant add more than one, ";
                    //    return (msg, false);
                    //}
                }
            }
            else
            {
                ISKUConfig? OriganalSKUCONFIG = SKUCONFIGLIST.Find(e => e.UID == SKUCONFIG.UID);
                OriganalSKUCONFIG.DistributionChannelOrgUID = SKUCONFIG.DistributionChannelOrgUID;
                OriganalSKUCONFIG.Name = SKUCONFIG.Name;
                OriganalSKUCONFIG.BuyingUOM = SKUCONFIG.BuyingUOM;
                OriganalSKUCONFIG.SellingUOM = SKUCONFIG.SellingUOM;
                OriganalSKUCONFIG.IsActive = SKUCONFIG.IsActive;
                OriganalSKUCONFIG.CanBuy = SKUCONFIG.CanBuy;
                OriganalSKUCONFIG.CanSell = SKUCONFIG.CanSell;
                _ = await SaveDistributionData(sKUConfig, false);
            }
            return ("", false);
        }
        public async Task OnSKUConfigEditClick(ISKUConfig sKUConfig)
        {
            sKUConfig = await CreateDistributionClone(sKUConfig);
            SKUCONFIG = sKUConfig;
            ISelectionItem? selecteditem = oRGDistributionSelectionItems?.Find(e => e.Label == sKUConfig.DistributionChannelOrgUID);
            if (selecteditem != null)
            {
                DistributionTypeSM.Select(selecteditem);
            }

            ISelectionItem? BuyingUOMselecteditem = BuyingUOMSelectionItems?.Find(e => e.Label == sKUConfig.BuyingUOM);
            if (BuyingUOMselecteditem != null)
            {
                BuyingUOMSM.Select(BuyingUOMselecteditem);
            }

            ISelectionItem? SellingUOMtselecteditem = SellingUOMSelectionItems?.Find(e => e.Label == sKUConfig.SellingUOM);
            if (SellingUOMtselecteditem != null)
            {
                SellingUOMSM.Select(SellingUOMtselecteditem);
            }
        }
        public void OnSKUConfigCancelBtnClickInPopUp()
        {
            msg = string.Empty;
            DistributionTypeSM.DeselectAll();
            BuyingUOMSM?.DeselectAll();
            SellingUOMSM?.DeselectAll();
        }
        public async Task<ISKUMaster> PopulateSKUDetailsData(string orguid)
        {
            SkuMaster = await GetSKUDetailsData(orguid);
            return SkuMaster;
        }
        public void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {
            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired)
            {
                baseModel.UID = Guid.NewGuid().ToString();
            }
        }
        public void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        //public async Task PopulateUploadImage()
        //{
        //    List<FolderImageDetails> folderImageDetails = new List<FolderImageDetails>();
        //    List<Winit.Modules.FileSys.Model.Classes.FileSys> fileSys = new List<Winit.Modules.FileSys.Model.Classes.FileSys>();
        //    foreach(var sys in folderImageDetails)
        //    {
        //        Winit.Modules.FileSys.Model.Classes.FileSys file= new FileSys.Model.Classes.FileSys();
        //        file.UID=Guid.NewGuid().ToString();
        //        file.LinkedItemType = SKUConsonant.FileSysLinkedItmeType;
        //        file.LinkedItemUID = SKU.UID;
        //        file.FileSysType = SKUConsonant.FileSysLinkedItmeType;
        //        file.FileType = SKUConsonant.FileSysFileType;
        //        file.IsDirectory = false;
        //        file.FileName = sys.FileName;
        //        file.DisplayName = sys.FileName;
        //        file.FileSize = 0;
        //        file.RelativePath = sys.Folderpath;
        //        file.Longitude = 0.ToString();
        //        file.Latitude = 0.ToString();
        //        fileSys.Add(file);

        //    }
        //    //Call API
        //}
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupView>> GetSKUGroupAttributeData();
        public abstract Task<List<Winit.Modules.SKU.Model.Classes.SKUGroupTypeView>> GetSKUGroupTypeAttributeData();
        public abstract Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDistributionMappingData();
        public abstract Task<List<Winit.Modules.UOM.Model.Interfaces.IUOMType>> GetSKUUOMData();
        public abstract Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetSKUUOMVolumeUnitData(string code);
        public abstract Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetSKUUOMGrossWeightUnitData(string code);
        public abstract Task<List<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> GetCustomSkuFieldsDynamicData();
        public abstract Task<bool> CreateUpdateCustomSKuField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField);
        public abstract Task<bool> CreateUpdateSKUAttributeData(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuattributes, bool IsCreate);
        public abstract Task<bool> CreateUpdateSKUData(Winit.Modules.SKU.Model.Interfaces.ISKU sku, bool IsCreate);
        public abstract Task<bool> CreateUpdateSKUUOMData(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuuom, bool IsCreate);
        public abstract Task<bool> SaveDistributionData(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuconfig, bool IsCreate);
        public abstract Task<Winit.Modules.SKU.Model.Interfaces.ISKUMaster> GetSKUDetailsData(string orgUID);
        public abstract Task<List<CommonUIDResponse>> CreateUpdateFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate);
        #endregion
    }
}
