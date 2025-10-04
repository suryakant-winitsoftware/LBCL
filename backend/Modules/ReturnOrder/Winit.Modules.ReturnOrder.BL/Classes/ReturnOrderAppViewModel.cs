using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderAppViewModel : ReturnOrderBaseViewModel, IReturnOrderAppViewModel
{
    protected Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL _returnOrderBL;
    protected Winit.Modules.SKU.BL.Interfaces.ISKUBL _sKUBL;
    protected Winit.Modules.ListHeader.BL.Interfaces.IListHeaderBL _ListHeaderBL;
    protected Winit.Modules.SKU.BL.Interfaces.ISKUPriceBL _SkuPriceBL;
    protected readonly IFileSysBL _fileSysBL;
    private IStringLocalizer<LanguageKeys> _localizer;
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    protected readonly IDataManager _dataManager;
    List<SKUGroupSelectionItem> SKUGroupTypeSelectionItems { get; set; }
    Dictionary<string, List<SKUGroupSelectionItem>> SKUGroupSelectionItemsDict { get; set; }


    #region ImageCapturing
    public List<IFileSys> ImageFileSysList { get; set; }
    public string ImageFolderPath { get; set; }
    #endregion
    public ReturnOrderAppViewModel(IServiceProvider serviceProvider,
       IFilterHelper filter,
       ISortHelper sorter,
       Interfaces.IReturnOrderAmountCalculator amountCalculator,
       IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
       Winit.Modules.Base.BL.ApiService apiService,
       IAppUser appUser, ISKUBL sKUBL, IListHeaderBL listHeaderBL, ISKUPriceBL sKUPriceBL,
       IReturnOrderBL returnOrderBL, IFileSysBL fileSysBL, IStringLocalizer<LanguageKeys> localizer, IDataManager dataManager) : base(serviceProvider,
       filter, sorter, amountCalculator, listHelper, appConfigs, apiService, appUser)
    {
        _sKUBL = sKUBL;
        _ListHeaderBL = listHeaderBL;
        _SkuPriceBL = sKUPriceBL;
        _returnOrderBL = returnOrderBL;
        _fileSysBL = fileSysBL;
        _localizer = localizer;
        _dataManager = dataManager;


        ImageFileSysList = new List<IFileSys>();
        SignatureFileSysList = new List<IFileSys>();
        FilterDataList = new Dictionary<ISelectionItem, List<ISelectionItem>>();
        SKUGroupTypeSelectionItems = new List<SKUGroupSelectionItem>();
        SKUGroupSelectionItemsDict = new Dictionary<string, List<SKUGroupSelectionItem>>();
    }


    #region overrided methods
    public override async Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "")
    {
        await base.PopulateViewModel(source, isNewOrder, returnOrderUID);
        PrepareSignatureFields();
        SelectedStoreViewModel.IsCaptureSignatureRequired = true;
        RouteUID = _appUser.SelectedRoute.UID;
        ImageFolderPath = Path.Combine(_appConfigs.BaseFolderPath,
            FileSysTemplateControles.GetReturnOrderImageFolderPath(ReturnOrderUID));
        await Task.Run(PopulateFilterData);
    }

    protected override async Task<List<IListItem>> Reasons_Data(string reason)
    {
        return await GetReasonDataFromSqLite(reason);
    }
    protected override async Task<List<ISKUMaster>> SKUMasters_Data(List<string> orgs, List<string>? skuUIDs = null)
    {
        return await _sKUBL.PrepareSKUMaster(orgs, default, skuUIDs, null);
    }
    protected override async Task<List<ISKUPrice>> GetSKuPrices_Data()
    {
        return await PopulatePriceMasterData();
    }
    protected override async Task<bool> PostData_ReturnOrder(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO
        returnOrderViewModel)
    {
        if (SelectedStoreViewModel.IsCaptureSignatureRequired || ImageFileSysList.Any())
        {
            SignatureFileSysList.AddRange(ImageFileSysList);
            if (await _fileSysBL.CreateFileSysForBulk(SignatureFileSysList) <= 0)
                throw new Exception("Failed to save FileSys");
        }
        return await PostData_ReturnOrderBL(returnOrderViewModel) > 0;
    }
    protected override Task<IReturnOrderMaster> GetReturnOrder_Data()
    {
        throw new NotImplementedException();
    }
    #endregion
    #region BL calling Methods
    private async Task<List<IListItem>> GetReasonDataFromSqLite(string reasonType)
    {
        try
        {
            List<string> salebleReasonsRequestList = new List<string>() { reasonType };
            PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> reasonsData =
                await _ListHeaderBL.GetListItemsByCodes(salebleReasonsRequestList, false);
            await Task.Run(() => Console.WriteLine("skhbvhk"));
            return reasonsData.PagedData.ToList();
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while retirving the Reasons");
        }
    }

    private async Task<List<ISKUPrice>> PopulatePriceMasterData()
    {
        try
        {
            List<string> orgUID = new List<string>();
            orgUID.Add(SelectedStoreViewModel.SelectedOrgUID);
            PagedResponse<ISKUPrice> pagedResponse = await _SkuPriceBL.SelectAllSKUPriceDetails(null, 0, 0, null, false);
            if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Count() > 0)
            {
                return pagedResponse.PagedData.ToList();
            }
            return new List<ISKUPrice>();
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while Retriving the sku prices data from api");
        }
    }
    private async Task<int> PostData_ReturnOrderBL(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO
        returnOrderViewModel)
    {
        return await _returnOrderBL.CreateReturnOrderMaster(returnOrderViewModel);
    }
    #endregion
    public void PopulateFilterData()
    {
        FilterDataList.Add(

                new Winit.Shared.Models.Common.SelectionItemFilter
                {
                    Label = "sort_by",
                    Code = "Sort-By",
                    ActionType = FilterActionType.Sort,
                    Mode = Winit.Shared.Models.Enums.SelectionMode.Single,
                    ImgPath = ""
                },
                new List<Winit.Shared.Models.Common.ISelectionItem>
                {
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {UID="NAME_ASC", Label =  "name_ascending",
                        Code = "SKULabel", Direction = SortDirection.Asc,
                        IsSelected = true, DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {UID="NAME_DESC",  Label = "name_descending", Code = "SKULabel",
                        Direction = SortDirection.Desc, DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter {UID="PRICE_ASC",
                        Label="price_ascending",Code="UnitPrice", Direction = SortDirection.Asc,
                        DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {UID="PRICE_DESC",  Label = "price_descending"  , Code = "UnitPrice",
                        Direction = SortDirection.Desc, DataType = typeof(string) },
                });

        var skuAttributesData = (List<SKUGroupSelectionItem>)_dataManager.GetData("skuAttributes");
        if (skuAttributesData != null) SKUGroupTypeSelectionItems.AddRange(skuAttributesData);
        SKUGroupSelectionItemsDict =
            (Dictionary<string, List<SKUGroupSelectionItem>>)_dataManager.GetData("skuTypes");
        if (SKUGroupTypeSelectionItems != null && SKUGroupTypeSelectionItems.Any() &&
            SKUGroupSelectionItemsDict != null && SKUGroupSelectionItemsDict.Any())
        {
            foreach (var filterLeft in SKUGroupTypeSelectionItems)
            {
                if (filterLeft.Code != null && SKUGroupSelectionItemsDict.ContainsKey(filterLeft.Code))
                {
                    FilterDataList.Add(ConvertToSKUGroupSelectionToSelectionItemFilter(filterLeft).First(),
                        ConvertToSKUGroupSelectionToSelectionItemFilter(SKUGroupSelectionItemsDict[filterLeft.Code].ToArray()));
                }
            }
        }
    }
    public bool FilterAction(List<FilterCriteria> filterCriterias, IReturnOrderItemView itemView)
    {
        if (filterCriterias == null || !filterCriterias.Any()) return true;
        var fieldFilterCriteria = filterCriterias.Where(e => e.FilterGroup == FilterGroupType.Field).ToList();
        List<IReturnOrderItemView>? items = null;
        if (fieldFilterCriteria != null && fieldFilterCriteria.Any())
        {
            var filteredItems = async () => items = await _filter.ApplyFilter<IReturnOrderItemView>(new List<IReturnOrderItemView> { itemView },
                fieldFilterCriteria, FilterMode.And);
            if (items == null || !items.Any()) return false; 
        }

        // Group attribute filters by Name and get a list of values
        var attributeFilters = filterCriterias
            .Where(e => e.FilterGroup == FilterGroupType.Attribute)
            .GroupBy(e => e.Name)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Value.ToString()).ToList());

        // Apply attribute-based filters
        foreach (var attribute in attributeFilters)
        {
            var attributeKeys = new HashSet<string>(attribute.Value);
            if (!itemView.FilterKeys.Any(key => attributeKeys.Contains(key))) return false;
        }
        return true;
    }

    private List<ISelectionItem> ConvertToSKUGroupSelectionToSelectionItemFilter(params SKUGroupSelectionItem[] selectionItems)
    {
        return selectionItems.Select(e => new SelectionItemFilter()
        {
            UID = e.UID,
            Code = e.Code,
            Label = e.Label,
            ExtData = e.ExtData,
            ImgPath = e.ExtData?.ToString(),
            DataType = typeof(List<ISKUAttributes>),
            ActionType = FilterActionType.Filter,
            Mode = SelectionMode.Multiple,
            FilterGroup = FilterGroupType.Attribute
        }
        ).ToList<ISelectionItem>();
    }

    protected void PrepareSignatureFields()
    {
        string baseFolder = Path.Combine(_appConfigs.BaseFolderPath,
            FileSysTemplateControles.GetSignatureFolderPath("ReturnOrder", ReturnOrderUID));
        CustomerSignatureFolderPath = baseFolder;
        UserSignatureFolderPath = baseFolder;
        CustomerSignatureFileName = $"Customer_{ReturnOrderUID}.png";
        UserSignatureFileName = $"User_{ReturnOrderUID}.png";
    }
}
