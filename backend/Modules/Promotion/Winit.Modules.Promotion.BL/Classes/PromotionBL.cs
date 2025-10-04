using Microsoft.VisualBasic;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Promotion.BL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.BL.Classes
{
    public class PromotionBL : IPromotionBL
    {
        protected readonly DL.Interfaces.IPromotionDL _promotionDL = null;
        public PromotionBL(DL.Interfaces.IPromotionDL promotionDL)
        {
            _promotionDL = promotionDL;
        }
        public async Task<PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion>> GetPromotionDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _promotionDL.GetPromotionDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CUDPromotionMaster(Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView)
        {
            int cnt = await _promotionDL.CUDPromotionMaster(promoMasterView);
            if (promoMasterView.IsFinalApproval)
            {
                Task.Run(() => _promotionDL.UpdateSchemeMappingData(promoMasterView.PromotionView.UID));
            }
            return cnt;

        }

        public async Task<int> PopulateItemPromotionMap(string promotionUID)
        {
            return await _promotionDL.PopulateItemPromotionMap(promotionUID);
        }
        public async Task<int> ChangeEndDate(PromotionView promotionView)
        {
            int cnt = await _promotionDL.ChangeEndDate(promotionView);

            if (cnt > 0)
                Task.Run(() => _promotionDL.UpdateSchemeMappingData(promotionView.UID));

            return cnt;
        }

        public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView,
            Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
        {
            List<AppliedPromotionView> appliedPromotions = new List<AppliedPromotionView>();
            List<PromotionItemView> appliedPromotionItems = new List<PromotionItemView>();

            if (!string.IsNullOrEmpty(applicablePromotionUIDs))
            {
                string[] promotionUIDs = applicablePromotionUIDs.Split(',');
                foreach (var uid in promotionUIDs)
                {
                    if (promoDictionary.ContainsKey(uid))
                    {
                        DmsPromotion dmsPromotion = promoDictionary[uid];
                        if (dmsPromotion.Type == "Invoice")
                        {
                            var invoicePromotions = ApplyInvoicePromotions(dmsPromotion, promoHeaderView);
                            appliedPromotions.AddRange(invoicePromotions
                            .Where(invoicePromotion => invoicePromotion.DiscountAmount > 0 || (invoicePromotion.FOCItems != null && invoicePromotion.FOCItems.Count > 0))
                            .Select(invoicePromotion => new AppliedPromotionView
                            {
                                PromotionUID = dmsPromotion.UID,
                                Priority = dmsPromotion.Priority,
                                IsFOC = invoicePromotion.FOCItems.Count > 0 ? true : false,
                                DiscountAmount = invoicePromotion.DiscountAmount,
                                FOCItems = invoicePromotion.FOCItems != null && invoicePromotion.FOCItems.Count > 0 ? invoicePromotion.FOCItems : null
                            }));
                        }
                        else
                        {

                            DmsPromoOrder bestFitPromoOrder = null;
                            decimal bestFitMin = 0;
                            List<PromotionItemView> eligibleItems = new List<PromotionItemView>();
                            foreach (var promoOrder in dmsPromotion.PromoOrders)
                            {
                                bool match = false;

                                decimal totalQuantities = 0;

                                var promoOrderItemSKUUIDs = promoOrder.PromoOrderItems.Select(poi => poi.ItemCriteriaSelected).ToList();

                                match = promoOrder.SelectionModel == "Any"
                                    ? promoOrder.PromoOrderItems.Any(poi =>
                                        promoHeaderView.promotionItemView.Any(piv => IsMatchWithAttribute(poi, piv)))
                                    : promoOrder.PromoOrderItems.All(poi =>
                                        promoHeaderView.promotionItemView.Any(piv => IsMatchWithAttribute(poi, piv)));

                                if (match)
                                {
                                    eligibleItems = promoOrder.SelectionModel == "Any"
                                        ? promoHeaderView.promotionItemView
                                            .Where(piv => promoOrder.PromoOrderItems.Any(poi => IsMatchWithAttribute(poi, piv)))
                                            .ToList()
                                        : promoHeaderView.promotionItemView
                                            .Where(piv => promoOrderItemSKUUIDs.Contains(piv.SKUUID))
                                            .ToList();

                                    totalQuantities = eligibleItems.Sum(item => item.QtyBU);
                                }

                                if (match)
                                {
                                    decimal minQuantity = promoOrder.objPromoCondition?.Min ?? 0.0m;

                                    if (promoOrder.SelectionModel == "All")
                                    {
                                        bool allItemsMeetMinQuantity = eligibleItems.All(eligibleItem =>
                                            promoOrder.PromoOrderItems.Any(poi =>
                                                poi.ItemCriteriaSelected == eligibleItem.SKUUID &&
                                                eligibleItem.QtyBU >= (poi.objPromoCondition?.Min ?? 0.0m))
                                        );

                                        if (!allItemsMeetMinQuantity)
                                        {
                                            match = false;
                                        }
                                        else
                                        {
                                            bestFitPromoOrder = promoOrder;
                                        }
                                    }
                                    if (promoOrder.SelectionModel == "Any")
                                    {
                                        //bool allItemsMeetMinQuantity = eligibleItems.All(eligibleItem =>
                                        //{
                                        //    bool itemMeetsMinQuantity = promoOrder.PromoOrderItems.Any(poi =>
                                        //    {
                                        //        bool isCompulsory = poi.IsCompulsory ?? false;

                                        //        return poi.ItemCriteriaSelected == eligibleItem.SKUUID &&
                                        //               (!isCompulsory || eligibleItem.Qty >= (poi.objPromoCondition?.Min ?? 0.0m));
                                        //    });

                                        //    return itemMeetsMinQuantity;
                                        //});
                                        //if (!allItemsMeetMinQuantity)
                                        //{
                                        //    match = false;
                                        //}
                                        bool allItemsMeetMinQuantity = true;

                                        foreach (var poi in promoOrder.PromoOrderItems)
                                        {
                                            bool isCompulsory = poi.IsCompulsory ?? false;

                                            if (isCompulsory)
                                            {
                                                var eligibleItem = eligibleItems.FirstOrDefault(ei => ei.SKUUID == poi.ItemCriteriaSelected);
                                                decimal itemQuantity = eligibleItem?.QtyBU ?? 0.0m;
                                                decimal minQuantityToCheck = poi.objPromoCondition?.Min ?? 0.0m;

                                                if (itemQuantity < minQuantityToCheck)
                                                {
                                                    allItemsMeetMinQuantity = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                decimal minQuantityToCheck = promoOrder.objPromoCondition?.Min ?? 0.0m;

                                                if (totalQuantities < minQuantityToCheck)
                                                {
                                                    allItemsMeetMinQuantity = false;
                                                    break;
                                                }
                                            }
                                        }

                                    }

                                    if (match && totalQuantities >= minQuantity)
                                    {
                                        if (minQuantity > bestFitMin)
                                        {
                                            bestFitMin = minQuantity;
                                            bestFitPromoOrder = promoOrder;
                                        }
                                    }
                                }
                            }
                            if (bestFitPromoOrder != null)
                            {

                                if (bestFitPromoOrder.SelectionModel == "Any")
                                {
                                    appliedPromotionItems = CheckPromoConditionForPromoOrder("Any", bestFitPromoOrder, promoHeaderView, eligibleItems);
                                }
                                else if (bestFitPromoOrder.SelectionModel == "All")
                                {
                                    appliedPromotionItems = CheckPromoConditionForPromoOrder("All", bestFitPromoOrder, promoHeaderView, eligibleItems);
                                }
                                bool isFOC = dmsPromotion.PromoFormat == "BQXF" || dmsPromotion.PromoFormat == "IQXF";
                                List<FOCItem> focItems = new List<FOCItem>();
                                if (isFOC)
                                {
                                    foreach (var item in appliedPromotionItems)
                                    {
                                        if (item.ItemType == "FOC")
                                        {
                                            FOCItem focItem = new FOCItem
                                            {
                                                ItemCode = item.SKUUID,
                                                UOM = item.UOM,
                                                Qty = item.QtyBU
                                            };
                                            focItems.Add(focItem);
                                        }
                                    }
                                    AppliedPromotionView appliedPromotionObjFOC = new AppliedPromotionView
                                    {
                                        PromotionUID = dmsPromotion.UID,
                                        Priority = dmsPromotion.Priority,
                                        PromoFormat = dmsPromotion.PromoFormat,
                                        IsFOC = isFOC,
                                        UniqueUID = appliedPromotionItems.Last().UniqueUId,
                                        DiscountAmount = 0,
                                        FOCItems = focItems.Count > 0 ? focItems : null
                                    };
                                    appliedPromotions.Add(appliedPromotionObjFOC);
                                }
                                else
                                {
                                    foreach (var item in appliedPromotionItems)
                                    {
                                        //if (item.ItemType == "FOC")
                                        //{
                                        //    FOCItem focItem = new FOCItem
                                        //    {
                                        //        ItemCode = item.SKUUID,
                                        //        UOM = item.UOM,
                                        //        Qty = item.QtyBU
                                        //    };
                                        //    focItems.Add(focItem);
                                        //}
                                        AppliedPromotionView appliedPromotionObj = null;
                                        //int occurence = 1;
                                        /*int firstOcc = 0;
                                        if (item.ItemType == "FOC")
                                        {
                                            firstOcc += 1;
                                        }
                                        if (firstOcc < 2)
                                        {*/

                                        appliedPromotionObj = new AppliedPromotionView();
                                        /*{
                                            PromotionUID = dmsPromotion.UID,
                                            Priority = dmsPromotion.Priority,
                                            IsFOC = isFOC,
                                            UniqueUID = item.SKUUID,
                                            DiscountAmount = item.TotalDiscount,
                                            //FOCItems = item.ItemType == "FOC" ? focItems : null
                                            //FOCItems = occurence == appliedPromotionItems.length &&  ? focItems
                                        };*/
                                        appliedPromotionObj.PromotionUID = dmsPromotion.UID;
                                        appliedPromotionObj.Priority = dmsPromotion.Priority;
                                        appliedPromotionObj.IsFOC = false;
                                        appliedPromotionObj.UniqueUID = item.UniqueUId;// item.SKUUID; vishal changed this on 22nd May 2024
                                        appliedPromotionObj.DiscountAmount += item.TotalDiscount;
                                        appliedPromotionObj.PromoFormat = dmsPromotion.PromoFormat;
                                        //if (occurence == appliedPromotionItems.Count)
                                        //{
                                        //    appliedPromotionObj.FOCItems = focItems;
                                        //}
                                        appliedPromotions.Add(appliedPromotionObj);
                                        /*}
                                        else
                                        {
                                            break;
                                        }*/
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                if (appliedPromotions.Count > 0)
                {
                    switch (promotionPriority)
                    {
                        case PromotionPriority.All:
                            break;
                        case PromotionPriority.MaxPriority:
                            appliedPromotions = appliedPromotions.OrderByDescending(ap => ap.Priority).Take(1).ToList();
                            break;

                        case PromotionPriority.MinPriority:
                            appliedPromotions = appliedPromotions.OrderBy(ap => ap.Priority).Take(1).ToList();
                            break;

                        default:
                            break;
                    }
                }
            }
            return appliedPromotions;
        }

        private bool IsMatchWithAttribute(DmsPromoOrderItem poi, PromotionItemView piv)
        {
            if (IsAttributeType(poi.ItemCriteriaType))
            {
                string attributeCode = GetAttributeCodeValue(piv, poi.ItemCriteriaType);
                return attributeCode == poi.ItemCriteriaSelected;
            }
            else
            {
                return poi.ItemCriteriaSelected == piv.SKUUID;
            }
        }

        private bool IsAttributeType(string itemCriteriaType)
        {
            return new List<string> { "Brand", "Category", "SubCategory", "Product Group" }.Contains(itemCriteriaType);
        }

        private string GetAttributeCodeValue(PromotionItemView piv, string itemCriteriaType)
        {
            if (piv.Attributes != null && piv.Attributes.ContainsKey(itemCriteriaType))
            {
                return piv.Attributes[itemCriteriaType].Code;
            }
            else
            {
                return string.Empty;
            }
        }

        private List<PromotionItemView> CheckPromoConditionForPromoOrder(String selectionModel, DmsPromoOrder promoOrder,
            PromotionHeaderView promoHeaderView, List<PromotionItemView> eligibleItems)
        {
            PromotionItemView objPromotionItemView = new PromotionItemView();
            List<PromotionItemView> lstPromotionItemView = new List<PromotionItemView>();
            List<PromotionItemView> lstPromotionItemViewFOC = new List<PromotionItemView>();
            if (selectionModel == "Any")
            {
                if (promoOrder.objPromoCondition != null)
                {
                    decimal minQuantity = promoOrder.objPromoCondition.Min.HasValue ? promoOrder.objPromoCondition.Min.Value : 0.0m;
                    decimal totalQuantity = eligibleItems.Sum(e => e.QtyBU);
                    //List<PromotionItemView> eligibleItems = new List<PromotionItemView>();

                    //foreach (var promoOrderItem in promoOrder.PromoOrderItems)
                    //{
                    //    var correspondingPromotionItemView = promoHeaderView.promotionItemView
                    //        .FirstOrDefault(piv => IsMatchWithAttribute(promoOrderItem, piv));

                    //    if (correspondingPromotionItemView != null)
                    //    {
                    //        totalQuantity += correspondingPromotionItemView.QtyBU;
                    //        eligibleItems.Add(correspondingPromotionItemView);
                    //    }
                    //}



                    if (eligibleItems.Any() && promoOrder.PromoOffers != null && totalQuantity >= minQuantity)
                    {
                        decimal itemMultiplier = totalQuantity / minQuantity;
                        if (itemMultiplier >= 1)
                        {
                            int numberOfDeals = Math.Min((int)itemMultiplier, promoOrder.MaxDealCount ?? int.MaxValue);
                            foreach (var promoOffer in promoOrder.PromoOffers)
                            {
                                if (promoOffer.objPromoCondition != null)
                                {
                                    if (promoOffer.objPromoCondition.ConditionType != "FOC")
                                    {
                                        var offerCondition = promoOffer.objPromoCondition;

                                        if (offerCondition.ReferenceUID == promoOffer.UID)
                                        {
                                            foreach (var eligibleItem in eligibleItems)
                                            {
                                                objPromotionItemView = ApplyPromotionOfferToItem(offerCondition, eligibleItem, numberOfDeals);
                                                lstPromotionItemView.Add(objPromotionItemView);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var offerCondition = promoOffer.objPromoCondition;

                                        if (offerCondition.ReferenceUID == promoOffer.UID)
                                        {
                                            lstPromotionItemViewFOC = ApplyFOCPromotionOfferToItem(promoOffer, promoHeaderView, numberOfDeals);
                                            if (lstPromotionItemViewFOC != null && lstPromotionItemViewFOC.Count > 0)
                                            {
                                                lstPromotionItemView.AddRange(lstPromotionItemViewFOC.Where(poi => poi.ItemType == "FOC"));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            else if (selectionModel == "All")
            {
                bool allItemsMeetMinQuantity = true;
                int minMultiplier = int.MaxValue;

                foreach (var promoOrderItem in promoOrder.PromoOrderItems)
                {
                    if (promoOrderItem.objPromoCondition != null)
                    {
                        decimal minQuantity = promoOrderItem.objPromoCondition.Min.HasValue
                            ? promoOrderItem.objPromoCondition.Min.Value
                            : 0.0m;

                        var correspondingPromotionItemView = promoHeaderView.promotionItemView
                            .FirstOrDefault(piv => piv.SKUUID == promoOrderItem.ItemCriteriaSelected);

                        if (correspondingPromotionItemView != null)
                        {
                            decimal itemMultiplier = correspondingPromotionItemView.QtyBU / minQuantity;

                            if (itemMultiplier < minMultiplier)
                            {
                                minMultiplier = (int)itemMultiplier;
                            }

                            if (itemMultiplier < 1.0m)
                            {
                                allItemsMeetMinQuantity = false;
                                break;
                            }
                        }
                        else
                        {
                            allItemsMeetMinQuantity = false;
                            break;
                        }
                    }
                }

                if (allItemsMeetMinQuantity)
                {
                    int numberOfDeals = Math.Min(minMultiplier, promoOrder.MaxDealCount ?? int.MaxValue);
                    foreach (var promoOffer in promoOrder.PromoOffers)
                    {
                        if (promoOffer.objPromoCondition != null)
                        {
                            if (promoOffer.objPromoCondition.ConditionType != "FOC")
                            {
                                if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                                {
                                    foreach (var promoOrderItem in promoOrder.PromoOrderItems)
                                    {
                                        var correspondingPromotionItemView = promoHeaderView.promotionItemView
                                            .FirstOrDefault(piv => piv.SKUUID == promoOrderItem.ItemCriteriaSelected);

                                        objPromotionItemView = ApplyPromotionOfferToItem(promoOffer.objPromoCondition, correspondingPromotionItemView, numberOfDeals);
                                        lstPromotionItemView.Add(objPromotionItemView);
                                    }
                                }
                            }
                            else
                            {
                                var offerCondition = promoOffer.objPromoCondition;

                                if (offerCondition.ReferenceUID == promoOffer.UID)
                                {
                                    lstPromotionItemViewFOC = ApplyFOCPromotionOfferToItem(promoOffer, promoHeaderView, numberOfDeals);
                                    if (lstPromotionItemViewFOC != null && lstPromotionItemViewFOC.Count > 0)
                                    {
                                        lstPromotionItemView.AddRange(lstPromotionItemViewFOC);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return lstPromotionItemView;
        }

        /*public List<AppliedInvoicePromotion> ApplyInvoicePromotions(DmsPromotion promotion, PromotionHeaderView promotionHeaderView)
        {
            var appliedPromotions = new List<AppliedInvoicePromotion>();
            PromotionHeaderView objPromotionHeaderView = new PromotionHeaderView();

            foreach (var promoOrder in promotion.PromoOrders)
            {
                switch (promotion.PromoFormat)
                {
                    case "ANYVALUE":
                        if (promoOrder.PromoOffers != null)
                        {
                            foreach (var promoOffer in promoOrder.PromoOffers)
                            {
                                if (promoOffer.objPromoCondition == null)
                                {
                                    continue;
                                }
                                if (promoOffer.objPromoCondition.ConditionType != "FOC")
                                {
                                    if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                                    {
                                        var offerCondition = promoOffer.objPromoCondition;
                                        objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                        break;

                    case "BRANDCOUNT":
                        break;

                    case "BYQTY":
                        decimal? minQuantityRequired = promoOrder.objPromoCondition.Min;

                        if (promotionHeaderView.TotalQty >= minQuantityRequired)
                        {
                            if (promoOrder.PromoOffers == null)
                            {
                                continue;
                            }
                            foreach (var promoOffer in promoOrder.PromoOffers)
                            {
                                if (promoOffer.objPromoCondition == null)
                                {
                                    continue;
                                }
                                if (promoOffer.objPromoCondition.ConditionType != "FOC")
                                {
                                    if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                                    {
                                        var offerCondition = promoOffer.objPromoCondition;
                                        objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                        break;

                    case "BYVALUE":

                        decimal? minValueRequired = promoOrder.objPromoCondition.Min;

                        if (promotionHeaderView.TotalAmount >= minValueRequired)
                        {
                            if (promoOrder.PromoOffers == null)
                            {
                                continue;
                            }
                            foreach (var promoOffer in promoOrder.PromoOffers)
                            {
                                if (promoOffer.objPromoCondition == null)
                                {
                                    continue;
                                }
                                if (promoOffer.objPromoCondition.ConditionType != "FOC")
                                {
                                    if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                                    {
                                        var offerCondition = promoOffer.objPromoCondition;
                                        objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                        break;

                    default:

                        break;

                }
                var appliedPromotion = new AppliedInvoicePromotion
                {
                    PromotionUID = promotion.UID,
                    DiscountAmount = objPromotionHeaderView.TotalDiscount
                };

                appliedPromotions.Add(appliedPromotion);
            }

            return appliedPromotions;
        }*/


        public List<AppliedInvoicePromotion> ApplyInvoicePromotions(DmsPromotion promotion, PromotionHeaderView promotionHeaderView)
        {
            var appliedPromotions = new List<AppliedInvoicePromotion>();

            foreach (var promoOrder in promotion.PromoOrders)
            {
                bool shouldApply = false;
                string condition = string.Empty;
                switch (promotion.PromoFormat)
                {
                    case "LINECOUNT":
                        shouldApply = promotionHeaderView.LineCount >= promoOrder.objPromoCondition?.Min;
                        condition = "LINECOUNT";
                        break;
                    case "BRANDCOUNT":
                        shouldApply = promotionHeaderView.BrandCount >= promoOrder.objPromoCondition?.Min;
                        condition = "BRANDCOUNT";
                        break;
                    case "BYQTY":
                        shouldApply = promotionHeaderView.TotalQty >= promoOrder.objPromoCondition?.Min;
                        condition = "BYQTY";
                        break;
                    case "BYVALUE":
                        shouldApply = promotionHeaderView.TotalAmount >= promoOrder.objPromoCondition?.Min;
                        condition = "BYVALUE";
                        break;

                    default:
                        break;
                }

                if (shouldApply)
                {
                    InvoicePromotionApplication(promoOrder, promotionHeaderView, appliedPromotions, condition, promotion.PromoFormat);
                }
            }

            return appliedPromotions;
        }

        private void InvoicePromotionApplication(DmsPromoOrder promoOrder, PromotionHeaderView promotionHeaderView, List<AppliedInvoicePromotion> appliedPromotions, string condition, string promoFormat)
        {
            if (promoOrder.PromoOffers == null)
            {
                return;
            }

            foreach (var promoOffer in promoOrder.PromoOffers)
            {
                if (promoOffer.objPromoCondition == null)
                {
                    continue;
                }
                if (promoOffer.objPromoCondition.ConditionType != "FOC")
                {
                    if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                    {
                        //var offerCondition = promoOffer.objPromoCondition;
                        var objPromotionHeaderView = ApplyPromotionOfferToInvoice(promoOffer, promotionHeaderView);

                        if (objPromotionHeaderView.TotalDiscount > 0)
                        {
                            var appliedPromotion = new AppliedInvoicePromotion
                            {
                                PromotionUID = promoOrder.PromotionUID,
                                DiscountAmount = objPromotionHeaderView.TotalDiscount,
                                PromoFormat = promoFormat,
                                FOCItems = new List<FOCItem>()
                            };

                            appliedPromotions.Add(appliedPromotion);
                        }
                    }
                }
                else
                {
                    if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
                    {
                        //var offerCondition = promoOffer.objPromoCondition;
                        var objPromotionHeaderView = ApplyPromotionOfferToInvoice(promoOffer, promotionHeaderView, condition, promoOrder);

                        var focItems = new List<FOCItem>();
                        foreach (var item in objPromotionHeaderView.promotionItemView)
                        {
                            var focItem = new FOCItem
                            {
                                PromotionUID = promoOrder.PromotionUID,
                                ItemCode = item.SKUUID,
                                UOM = item.UOM,
                                Qty = item.QtyBU
                            };
                            focItems.Add(focItem);
                        }

                        var appliedPromotion = new AppliedInvoicePromotion
                        {
                            PromotionUID = promoOrder.PromotionUID,
                            DiscountAmount = 0,
                            PromoFormat = promoFormat,
                            FOCItems = focItems
                        };

                        appliedPromotions.Add(appliedPromotion);
                    }
                }

            }
        }



        public List<PromotionItemView> ApplyFOCPromotionOfferToItem(DmsPromoOffer promoOffer, PromotionHeaderView promotionHeaderView, int numberOfDeals)
        {
            List<PromotionItemView> promotionItemViews = null;
            if (promoOffer != null && promoOffer.PromoOfferItems != null)
            {
                promotionItemViews = new List<PromotionItemView>();
                foreach (var newItem in promoOffer.PromoOfferItems)
                {
                    for (int i = 0; i < numberOfDeals; i++)
                    {
                        PromotionItemView newPromotionItem = new PromotionItemView
                        {
                            SKUUID = newItem.ItemCriteriaSelected,
                            UOM = newItem.ItemUOM,
                            QtyBU = (int)newItem.objPromoCondition.Min,
                            ItemType = "FOC"
                        };
                        promotionItemViews.Add(newPromotionItem);
                        //promotionHeaderView.promotionItemView.Add(newPromotionItem);
                    }

                }
            }
            return promotionItemViews;
        }

        public PromotionHeaderView ApplyPromotionOfferToInvoice(DmsPromoOffer promoOffer, PromotionHeaderView promotionHeaderView, string condition = null, DmsPromoOrder promoOrder = null)
        {
            var promoCondition = promoOffer.objPromoCondition;
            if (promoOffer.objPromoCondition.ConditionType == "Percent")
            {
                if (promoCondition.Min.HasValue && promoCondition.Min >= 0 && promoCondition.Min <= 100)
                {
                    decimal promotionPercentage = promoCondition.Min.Value / 100;
                    decimal discountAmount = promotionHeaderView.TotalAmount * promotionPercentage;
                    promotionHeaderView.TotalDiscount += discountAmount;
                    promotionHeaderView.TotalAmount -= discountAmount;
                }
            }
            else if (promoCondition.ConditionType == "Value")
            {
                if (promoCondition.Min.HasValue && promoCondition.Min >= 0)
                {
                    decimal discountAmount = promoCondition.Min.Value;
                    promotionHeaderView.TotalDiscount += discountAmount;
                    promotionHeaderView.TotalAmount -= discountAmount;
                }
            }
            else if (promoCondition.ConditionType == "FOC")
            {
                int numberOfDeals = 0;
                decimal? promoApplicationRequirement = promoOrder.objPromoCondition.Min.Value;
                if (promoApplicationRequirement != null && promoApplicationRequirement > 0)
                {
                    switch (condition)
                    {
                        case "LINECOUNT":
                            numberOfDeals = (int)(promotionHeaderView.LineCount / promoApplicationRequirement);
                            break;
                        case "BRANDCOUNT":
                            numberOfDeals = (int)(promotionHeaderView.BrandCount / promoApplicationRequirement);
                            break;
                        case "BYQTY":
                            numberOfDeals = (int)(promotionHeaderView.TotalQty / promoApplicationRequirement);
                            break;
                        case "BYVALUE":
                            numberOfDeals = (int)(promotionHeaderView.TotalAmount / promoApplicationRequirement);
                            break;
                        default:
                            numberOfDeals = 0;
                            break;
                    }
                }
                if (numberOfDeals >= 1)
                {
                    promotionHeaderView.promotionItemView = ApplyFOCPromotionOfferToItem(promoOffer, promotionHeaderView, numberOfDeals);
                }

            }

            return promotionHeaderView;
        }

        public PromotionItemView ApplyPromotionOfferToItem(DmsPromoCondition promoCondition, PromotionItemView promoItemView, int numOfDeals, PromotionHeaderView promotionHeaderView = null)
        {
            if (promoCondition.ConditionType == "Percentage")
            {
                if (promoCondition.Min.HasValue && promoCondition.Min >= 0 && promoCondition.Min <= 100)
                {
                    decimal promotionPercentage = promoCondition.Min.Value / 100;

                    decimal discountAmount = promoItemView.TotalAmount * promotionPercentage;
                    promoItemView.TotalDiscount += discountAmount;
                }
            }
            else if (promoCondition.ConditionType == "Value")
            {
                if (promoCondition.Min.HasValue && promoCondition.Min >= 0)
                {
                    for (int dealCount = 0; dealCount < numOfDeals; dealCount++)
                    {
                        decimal discountAmount = promoCondition.Min.Value;
                        promoItemView.TotalDiscount += discountAmount;
                    }
                }
            }

            else if (promoCondition.ConditionType == "ReplacePrice")
            {
                if (promoCondition.Min.HasValue && promoCondition.Min >= 0)
                {
                    for (int dealCount = 0; dealCount < numOfDeals; dealCount++)
                    {
                        decimal discountAmount = promoCondition.Min.Value;
                        promoItemView.TotalDiscount += discountAmount;
                    }
                }
            }

            return promoItemView;
        }

        //public async Task<Winit.Modules.Promotion.Model.Interfaces.IPromoMaster> GetPromotionMasterByUID(string UID)
        //{
        //    return await _promotionDL.GetPromotionMasterByUID(UID);
        //}
        public async Task<int> CreateDMSPromotionByJsonData(List<string> applicablePromotions)
        {
            return await _promotionDL.CreateDMSPromotionByJsonData(applicablePromotions);
        }
        public async Task<Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>?> GetDMSPromotionByPromotionUIDs(List<string> promotionUIDs)
        {
            return await _promotionDL.GetDMSPromotionByPromotionUIDs(promotionUIDs);
        }
        public async Task<List<string>?> GetApplicablePromotionUIDs(List<string> orgUIDs)
        {
            return await _promotionDL.GetApplicablePromotionUIDs(orgUIDs);
        }
        public async Task<IEnumerable<Winit.Modules.Promotion.Model.Classes.DmsPromotion>> CreateDMSPromotionByPromotionUID(string PromotionUID)
        {
            return await _promotionDL.CreateDMSPromotionByPromotionUID(PromotionUID);
        }

        //public async Task<List<Dictionary<string, DmsPromotion>>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType)
        //public async Task<Dictionary<string, DmsPromotion>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType)
        //{
        //    return await _promotionDL.CreateDMSPromotionByPromoUID(applicablePromotioListCommaSeparated, promotionType);
        //}

        public async Task<Winit.Modules.Promotion.Model.Classes.PromoMasterView> GetPromotionDetailsByUID(string UID)
        {
            return await _promotionDL.GetPromotionDetailsByUID(UID);
        }

        //public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView, 
        //    Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
        //{
        //    return _promotionDL.ApplyPromotion(applicablePromotionUIDs, promoHeaderView, promoDictionary, promotionPriority);
        //}
        public Task<int> GetPromotionDetailsValidated(string PromotionUID, string OrgUID, string PromotionCode, string PriorityNo, bool isNew)
        {
            return _promotionDL.GetPromotionDetailsValidated(PromotionUID, OrgUID, PromotionCode, PriorityNo, isNew);
        }
        public Task<int> DeletePromotionDetailsByUID(string PromotionUID)
        {
            return _promotionDL.DeletePromotionDetailsByUID(PromotionUID);
        }
        public Task<Dictionary<string, List<string>>?> LoadStorePromotionMap(List<string> orgUIDs)
        {
            return _promotionDL.LoadStorePromotionMap(orgUIDs);
        }
        public Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>> SelectItemPromotionMapByPromotionUIDs(List<string> promotionUIDs)
        {
            return _promotionDL.SelectItemPromotionMapByPromotionUIDs(promotionUIDs);
        }
        public Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromotionData>?> GetPromotionData()
        {
            return _promotionDL.GetPromotionData();
        }
        public async Task<int> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView)
        {
            return await _promotionDL.UpdatePromotion(updatePromotionView);
        }
        public async Task<int> DeletePromoOrderItems(List<string> UIDs)
        {
            return await _promotionDL.DeletePromoOrderItems(UIDs);
        }
        public async Task<int> DeletePromotionSlabByPromoOrderUID(string promoOrderUID)
        {
            return await _promotionDL.DeletePromotionSlabByPromoOrderUID(promoOrderUID);
        }

    }
}
