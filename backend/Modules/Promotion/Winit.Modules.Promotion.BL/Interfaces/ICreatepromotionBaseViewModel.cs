using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common;
using Winit.UIModels.Web.Promotions;

namespace Winit.Modules.Promotion.BL.Interfaces
{
    public interface ICreatepromotionBaseViewModel
    {
        IAddProductPopUpDataHelper _addProductPopUpDataHelper { get; }
        string PromotionUID { get; set; }
        bool IsAssorted { get; set; }
        bool IsLine { get; set; }
        bool IsInvoice { get; set; }
        bool ShowBundleQty { get; set; }
        decimal BundleQty { get; set; }
        string SelectionModel { get; set; }
        bool IsGroupTypeSKUType { get; set; }
        Model.Classes.PromotionView PromotionView { get; set; }
        bool IsLoad { get; set; }
        List<PromotionItemsModel> PromotionItemsModelList { get; set; }
        List<ISelectionItem> SKUGroupList { get; set; }
        string CategoryLabel { get; set; }
        bool IsCategoryDisplay { get; set; }
        List<ISelectionItem> SelectionModelList { get; set; }
        string SelectionModelLabel { get; set; }
        bool IsSelectionModelDisplay { get; set; }
        string OfferTypeCode { get; set; }
        bool IsNoEndDate { get; set; }
        List<ISelectionItem> Category { get; set; }
        bool IsInstantFormatsDisplay { get; set; }
        string InstantFormatsLabel { get; set; }
        List<ISelectionItem> InstantFormats { get; set; }
        bool IsTypeDisplay { get; set; }
        string TypeLabel { get; set; }
        List<ISelectionItem> Type { get; set; }
        List<ISelectionItem> SKUGroupTypeList { get; set; }
        List<ISelectionItem> SelectedItems { get; set; }
        List<IPromoOrderForSlabs> PromoOrderForSlabsList { get; set; }
        //List<ListItem> PROMOTION_TYPE { get; set; }
        //List<ListHeader.Model.Classes.ListItem> PROMOTION_CATEGORY { get; set; }
        //List<ListHeader.Model.Classes.ListItem> PROMO_INSTANT_FORMATS { get; set; }
        string SelectedGroupLabel { get; set; }
        string SelectedGroupTypeLabel { get; set; }
        string str1 { get; set; }
        string DiscountLabel { get; set; }
        bool IsDiscountShow { get; set; }
        bool IsSlabTypePromotion { get; set; }
        List<DataGridColumn> Columns { get; set; }
        bool IsNewPromotion { get; set; }
        bool IsGroupTypeSKU { get; set; }
        bool IsFOCSKU { get; set; }
        bool IsOfferFOCType { get; set; }
        bool IsGroupTypeVisible { get; set; }
        bool IsGroupVisible { get; set; }
        decimal EligibleOrBundleQty { get; set; }
        int MaxDealCount { get; set; }
        decimal Discount { get; set; }
        List<ISKUV1> SKUList { get; set; }
        List<ISKUV1> SelectedSKU { get; set; }
        bool IsOfferTypeDisplay { get; set; }
        string OfferTypeLabel { get; set; }
        List<ISelectionItem> OfferTypeList { get; set; }
        List<ISelectionItem> SlabOfferTypeList { get; set; }
        List<ISelectionItem> DisplaySlabOfferTypeList { get; set; }
        List<ISelectionItem> SlabOrderTypeList { get; set; }

        string SlabSkuUID { get; set; }
        string SlabSKULabel { get; set; }
        bool IsOrderTypeDisplay { get; set; }
        bool IsSlabOrderTypeDisplay { get; set; }
        string SlabOrderTypeLabel { get; set; }
        string OrderTypeLabel { get; set; }
        string SelectedOrderType { get; set; }
        List<ISelectionItem> OrderTypeList { get; set; }
        Task PopulateViewmodelAsync();
        Task DeleteOfferItem(ISKUV1 sKU);
        Task DeleteOrderItem(PromotionItemsModel promotionItemsModel);
        void OnGroupItemsSelected(DropDownEvent dropDownEvent);
        void OnCategorySelected(DropDownEvent dropDownEvent);
        void OnInstantFormatsSelected(DropDownEvent dropDownEvent);
        void OnOfferOrorderTypeSelected(DropDownEvent dropDownEvent);
        void OnSlabOrderTypeChanaged(DropDownEvent dropDownEvent);
        void OnSlabOfferTypeChanaged(DropDownEvent dropDownEvent, IPromoOrderForSlabs promoOrderForSlabs);
        void ShowFreeSlabSKU(IPromoOrderForSlabs promoOrderForSlabs);
        void OnGroupTypeSelected(DropDownEvent dropDownEvent);
        void OnSelectionModelSelected(DropDownEvent dropDownEvent);
        void AddSelectedItems();
        Task<Validation> SaveAsync();
        void GetSelectedItems(List<ISKUV1> SKU);
        void GetSelectedItemForSlab(List<ISKUV1> SKUList, IPromoOrderForSlabs promoOrderForSlabs);
        void AddSKUButtonType(bool isGroupTypeSKU);
        void AddTierOrSlab();
        void PopulateViewModel();

        List<SKUAttributeDropdownModel>? SKUAttributeData { get; }
    }
}
