using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITSharedObjects.Models.RuleEngine;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderAppViewModel : PurchaseOrderBaseViewModel
{
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    List<SKUGroupSelectionItem> SKUGroupTypeSelectionItems { get; set; }
    Dictionary<string, List<SKUGroupSelectionItem>> SKUGroupSelectionItemsDict { get; set; }


    public PurchaseOrderAppViewModel(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IPurchaseOrderLevelCalculator purchaseOrderLevelCalculator,
        IAppConfig appConfigs,
        IPurchaseOrderDataHelper purchaseOrderDataHelper,
        IAddProductPopUpDataHelper addProductPopUpDataHelper, JsonSerializerSettings jsonSerializerSettings)
        : base(serviceProvider, filter, sorter,
            listHelper, appUser, appSetting,
            dataManager, purchaseOrderLevelCalculator,
            appConfigs, purchaseOrderDataHelper, addProductPopUpDataHelper, jsonSerializerSettings)
    {
        FilterDataList = [];
        SKUGroupTypeSelectionItems = [];
        SKUGroupSelectionItemsDict = [];
    }

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
}
