using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.DL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Promotion.DL.Classes
{
    public class PGSQLPromotionDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPromotionDL
    {
        public PGSQLPromotionDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<int> GetPromotionDetailsValidated(string PromotionUID, string OrgUID, string PromotionCode, string PriorityNo, bool isNew)
        {
            // Convert PriorityNo to integer for proper database comparison
            int priorityInt = 0;
            if (!string.IsNullOrEmpty(PriorityNo) && int.TryParse(PriorityNo, out int parsedPriority))
            {
                priorityInt = parsedPriority;
            }
            
            var parameters = new Dictionary<string, object>()
            {
                {"PromotionUID", PromotionUID },
                {"OrgUID", OrgUID },
                {"PromotionCode", PromotionCode },
                {"PriorityNo", priorityInt }, // Pass as integer instead of string
            };
            
            int retVal = Winit.Shared.Models.Constants.Promotions.None;
            try
            {
                // Updated to make promotion codes globally unique (not just per organization)
                var Query = @"SELECT 1 as Status FROM promotion WHERE code = @PromotionCode LIMIT 1;
                             SELECT 1 as Status FROM promotion WHERE org_uid = @OrgUID AND priority = @PriorityNo LIMIT 1";

                var Query2 = @"SELECT 1 as Status FROM promotion WHERE code = @PromotionCode AND uid != @PromotionUID LIMIT 1;
                              SELECT 1 as Status FROM promotion WHERE org_uid = @OrgUID AND priority = @PriorityNo AND uid != @PromotionUID LIMIT 1";

                DataSet ds = await ExecuteQueryDataSetAsync(isNew ? Query : Query2, parameters);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            retVal = isNew ? Winit.Shared.Models.Constants.Promotions.Code : Winit.Shared.Models.Constants.Promotions.Priority;
                        }
                    }
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1].Rows.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            retVal = Winit.Shared.Models.Constants.Promotions.Code_Priority;
                        }
                        else if (ds.Tables[1].Rows.Count > 0)
                        {
                            retVal = Winit.Shared.Models.Constants.Promotions.Priority;
                        }
                    }
                }
                return retVal;


            }
            catch
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion>> GetPromotionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                //var sql = new StringBuilder(@"SELECT * FROM ""Promotion""");
                var sql = new StringBuilder(@"select * from (SELECT 
                                                    li.Name as PromoFormatLabel, 
                                                        p.id AS Id,
                                                        p.uid AS Uid,
                                                        p.company_uid AS CompanyUid,
                                                        p.org_uid AS OrgUid,
                                                        p.code AS Code,
                                                        p.name AS Name,
                                                        p.remarks AS Remarks,
                                                        lit.Name AS Category,
                                                        p.has_slabs AS HasSlabs,
                                                        p.created_by_emp_uid AS CreatedByEmpUid,
                                                        p.valid_from AS ValidFrom,
                                                        p.valid_upto AS ValidUpto,
                                                        p.type AS Type,
                                                        p.promo_format AS PromoFormat,
                                                        p.is_active AS IsActive,
                                                        p.promo_title AS PromoTitle,
                                                        p.promo_message AS PromoMessage,
                                                        p.has_fact_sheet AS HasFactSheet,
                                                        p.priority AS Priority,
                                                        p.ss AS Ss,
                                                        p.created_time AS CreatedTime,
                                                        p.modified_time AS ModifiedTime,
                                                        p.server_add_time AS ServerAddTime,
                                                        p.server_modified_time AS ServerModifiedTime
    
                                                    FROM 
                                                        public.promotion p
                                                     Left join list_item li on li.uid=p.promo_format
                                                      Left join list_item  lit on lit.uid=p.category) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) from (SELECT 
                                                    li.Name as PromoFormatLabel, 
                                                        p.id AS Id,
                                                        p.uid AS Uid,
                                                        p.company_uid AS CompanyUid,
                                                        p.org_uid AS OrgUid,
                                                        p.code AS Code,
                                                        p.name AS Name,
                                                        p.remarks AS Remarks,
                                                        lit.Name AS Category,
                                                        p.has_slabs AS HasSlabs,
                                                        p.created_by_emp_uid AS CreatedByEmpUid,
                                                        p.valid_from AS ValidFrom,
                                                        p.valid_upto AS ValidUpto,
                                                        p.type AS Type,
                                                        p.promo_format AS PromoFormat,
                                                        p.is_active AS IsActive,
                                                        p.promo_title AS PromoTitle,
                                                        p.promo_message AS PromoMessage,
                                                        p.has_fact_sheet AS HasFactSheet,
                                                        p.priority AS Priority,
                                                        p.ss AS Ss,
                                                        p.created_time AS CreatedTime,
                                                        p.modified_time AS ModifiedTime,
                                                        p.server_add_time AS ServerAddTime,
                                                        p.server_modified_time AS ServerModifiedTime
    
                                                    FROM 
                                                        public.promotion p
                                                     Left join list_item li on li.uid=p.promo_format
                                                      Left join list_item  lit on lit.uid=p.category) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Promotion.Model.Interfaces.IPromotion>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromotion>().GetType();

                IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromotion> PromotionDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromotion>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion> pagedResponse = new PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion>
                {
                    PagedData = PromotionDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.Promotion.Model.Classes.PromoMasterView> GetPromotionDetailsByUID(string UID)
        {
            Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView = new PromoMasterView();
            using (var connection = PostgreConnection())
            {

                var sql = @"SELECT 
    p.id AS ""Id"",
    p.uid AS ""UID"",
    p.company_uid AS ""CompanyUid"",
    p.org_uid AS ""OrgUid"",
    p.code AS ""Code"",
    p.name AS ""Name"",
    p.remarks AS ""Remarks"",
    p.category AS ""Category"",
    p.has_slabs AS ""HasSlabs"",
    p.created_by_emp_uid AS ""CreatedByEmpUid"",
    p.valid_from AS ""ValidFrom"",
    p.valid_upto AS ""ValidUpto"",
    p.type AS ""Type"",
    p.promo_format AS ""PromoFormat"",
    p.is_active AS ""IsActive"",
    p.promo_title AS ""PromoTitle"",
    p.promo_message AS ""PromoMessage"",
    p.has_fact_sheet AS ""HasFactSheet"",
    p.priority AS ""Priority"",
    p.ss AS ""SS""
FROM public.promotion p
WHERE p.uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    promotion_uid AS ""PromotionUid"",
    selection_model AS ""SelectionModel"",
    qualification_level AS ""QualificationLevel"",
    min_deal_count AS ""MinDealCount"",
    max_deal_count AS ""MaxDealCount"",
    ss AS ""Ss""
FROM public.promo_order
WHERE promotion_uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    promo_order_uid AS ""PromoOrderUid"",
    parent_uid AS ""ParentUid"",
    item_criteria_type AS ""ItemCriteriaType"",
    item_criteria_selected AS ""ItemCriteriaSelected"",
    is_compulsory AS ""IsCompulsory"",
    item_uom AS ""ItemUom"",
    promo_split AS ""PromoSplit"",
    ss AS ""Ss"",
    promotion_uid AS ""PromotionUid""
FROM public.promo_order_item
WHERE promotion_uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    promo_order_uid AS ""PromoOrderUID"",
    type AS ""Type"",
    qualification_level AS ""QualificationLevel"",
    application_level AS ""ApplicationLevel"",
    selection_model AS ""SelectionModel"",
    has_offer_item_selection AS ""HasOfferItemSelection"",
    ss AS ""Ss"",
    promotion_uid AS ""PromotionUID""
FROM promo_offer
WHERE promotion_uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    promo_offer_uid AS ""PromoOfferUID"",
    item_criteria_type AS ""ItemCriteriaType"",
    item_criteria_selected AS ""ItemCriteriaSelected"",
    is_compulsory AS ""IsCompulsory"",
    item_uom AS ""ItemUOM"",
    ss AS ""SS"",
    promotion_uid AS ""PromotionUID""
FROM promo_offer_item
WHERE promotion_uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    reference_type AS ""ReferenceType"",
    reference_uid AS ""ReferenceUID"",
    condition_type AS ""ConditionType"",
    min AS ""Min"",
    max AS ""Max"",
    max_deal_count AS ""MaxDealCount"",
    uom AS ""UOM"",
    all_uom_conversion AS ""AllUOMConversion"",
    value_type AS ""ValueType"",
    is_prorated AS ""IsProrated"",
    ss AS ""SS"",
    discount_type AS ""DiscountType"",
    discount_percentage AS ""DiscountPercentage"",
    discount_amount AS ""DiscountAmount"",
    free_quantity AS ""FreeQuantity"",
    promotion_uid AS ""PromotionUID""
FROM public.promo_condition
WHERE promotion_uid = @PromotionUID;

SELECT
    id AS ""Id"",
    uid AS ""UID"",
    sku_type AS ""SKUType"",
    sku_type_uid AS ""SKUTypeUID"",
    promotion_uid AS ""PromotionUID"",
    ss AS ""SS"",
    created_time AS ""CreatedTime"",
    modified_time AS ""ModifiedTime"",
    server_add_time AS ""ServerAddTime"",
    server_modified_time AS ""ServerModifiedTime""
FROM public.item_promotion_map
WHERE promotion_uid = @PromotionUID;

";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                  {
                     {"PromotionUID",  UID}
                  };
                DataSet ds = await ExecuteQueryDataSetAsync(sql, parameters);
                if (ds != null)
                {
                    //Promotion
                    if (ds.Tables.Count >= 1)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                promoMasterView.PromotionView = ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromotionView>(row);
                            }
                        }
                    }
                    //PromoOrderView
                    if (ds.Tables.Count >= 2)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            promoMasterView.PromoOrderViewList = new();
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                promoMasterView.PromoOrderViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromoOrderView>(row));
                            }
                        }
                    }
                    //PromoOrderItemView
                    if (ds.Tables.Count >= 3)
                    {
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            promoMasterView.PromoOrderItemViewList = new();
                            foreach (DataRow row in ds.Tables[2].Rows)
                            {
                                promoMasterView.PromoOrderItemViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromoOrderItemView>(row));
                            }
                        }
                    }
                    //PromoOfferViewList
                    if (ds.Tables.Count >= 4)
                    {
                        if (ds.Tables[3].Rows.Count > 0)
                        {
                            promoMasterView.PromoOfferViewList = new();
                            foreach (DataRow row in ds.Tables[3].Rows)
                            {
                                promoMasterView.PromoOfferViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromoOfferView>(row));
                            }
                        }
                    }
                    //PromoOfferItemViewList
                    if (ds.Tables.Count >= 5)
                    {
                        if (ds.Tables[4].Rows.Count > 0)
                        {
                            promoMasterView.PromoOfferItemViewList = new();
                            foreach (DataRow row in ds.Tables[4].Rows)
                            {
                                promoMasterView.PromoOfferItemViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromoOfferItemView>(row));
                            }
                        }
                    }
                    //PromoConditionViewList
                    if (ds.Tables.Count >= 6)
                    {
                        if (ds.Tables[5].Rows.Count > 0)
                        {
                            promoMasterView.PromoConditionViewList = new();
                            foreach (DataRow row in ds.Tables[5].Rows)
                            {
                                promoMasterView.PromoConditionViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.PromoConditionView>(row));
                            }
                        }
                    }
                    //ItemPromotionMapViewList
                    if (ds.Tables.Count >= 7)
                    {
                        if (ds.Tables[6].Rows.Count > 0)
                        {
                            promoMasterView.ItemPromotionMapViewList = new();
                            foreach (DataRow row in ds.Tables[6].Rows)
                            {
                                promoMasterView.ItemPromotionMapViewList.Add(ConvertDataTableToObject<Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView>(row));
                            }
                        }
                    }


                }

                return promoMasterView;
            }
        }
        public async Task<int> DeletePromotionDetailsByUID(string PromotionUID)
        {
            var Query = @"DELETE FROM promotion WHERE valid_from>= now() and uid=@UID";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", PromotionUID },
                        };

            int count = await ExecuteNonQueryAsync(Query, Parameters);
            if (count > 0)
            {
                Query = @"Delete  from promo_order  where promotion_uid=@PromotionUID;
                              Delete  from promo_order_item where promotion_uid=@PromotionUID;
                              Delete  from promo_offer where promotion_uid=@PromotionUID;
                              Delete  from promo_offer_item where promotion_uid=@PromotionUID;
                              Delete  from promo_condition where promotion_uid=@PromotionUID;";
                Parameters = new Dictionary<string, object>
                        {
                           { "@PromotionUID", PromotionUID },
                        };
                count += await ExecuteNonQueryAsync(Query, Parameters);
            }
            return count;
        }
        /*public async Task<DmsPromotion> CreateDMSPromotionByPromoUID(string PromoUID)
        {
            try
            {
                DmsPromotion objDmsPromotion = new DmsPromotion();
                var sql = @"Select * from Promotion where UID = ""PromoUID""";

            }
            catch (Exception ex)
            {
                throw;
            }

        }*/

        public async Task<int> CUDPromotion(Winit.Modules.Promotion.Model.Classes.PromotionView promotionView)
        {
            int count = 0;

            try
            {
                bool exists = false;
                var existingRec = await SelectPromotionByUID(promotionView.UID);


                switch (promotionView.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        if (existingRec != null)
                        {
                            exists = (existingRec.UID == promotionView.UID);
                        }

                        count += exists ? await UpdatePromotion(promotionView) : await CreatePromotion(promotionView);
                        break;

                    case Shared.Models.Enums.ActionType.Update:
                        count += await UpdatePromotion(promotionView);
                        break;

                    case Shared.Models.Enums.ActionType.Delete:
                        count += await DeletePromotion(promotionView.UID);
                        break;
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<int> CUDPromoOrder(List<Winit.Modules.Promotion.Model.Classes.PromoOrderView> lstPromoOrder)
        {
            int count = 0;
            if (lstPromoOrder == null || lstPromoOrder.Count == 0)
            {
                return count;
            }
            List<string> uidList = lstPromoOrder.Select(po => po.UID).ToList();
            List<string> deletedUidList = lstPromoOrder.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<IPromoOrder> existingRec = await SelectPromoOrderByUID(uidList);

                foreach (PromoOrderView promoOrder in lstPromoOrder)
                {
                    switch (promoOrder.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == promoOrder.UID);
                            count += exists ?
                                await UpdatePromoOrder(promoOrder) :
                                await CreatePromoOrder(promoOrder);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromoOrder(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<int> CUDPromoOrderItem(List<Winit.Modules.Promotion.Model.Classes.PromoOrderItemView> promoOrderItemViewlst)
        {
            int count = 0;

            if (promoOrderItemViewlst == null || promoOrderItemViewlst.Count == 0)
            {
                return count;
            }

            List<string> uidList = promoOrderItemViewlst.Select(po => po.UID).ToList();

            List<string> deletedUidList = promoOrderItemViewlst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
            try
            {
                IEnumerable<IPromoOrderItem> existingRec = await SelectPromoOrderItemByUID(uidList);

                foreach (PromoOrderItemView promoOrderItem in promoOrderItemViewlst)
                {
                    switch (promoOrderItem.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == promoOrderItem.UID);
                            count += exists ?
                                await UpdatePromoOrderItem(promoOrderItem) :
                                await CreatePromoOrderItem(promoOrderItem);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromoOrderItem(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> CUDPromoOffer(List<Winit.Modules.Promotion.Model.Classes.PromoOfferView> promoOfferiewlst)
        {
            int count = 0;

            if (promoOfferiewlst == null || promoOfferiewlst.Count == 0)
            {
                return count;
            }

            List<string> uidList = promoOfferiewlst.Select(po => po.UID).ToList();
            List<string> deletedUidList = promoOfferiewlst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<IPromoOffer> existingRec = await SelectPromoOfferByUID(uidList);

                foreach (PromoOfferView promoOffer in promoOfferiewlst)
                {
                    switch (promoOffer.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == promoOffer.UID);
                            count += exists ?
                                await UpdatePromoOffer(promoOffer) :
                                await CreatePromoOffer(promoOffer);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromoOffer(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<int> CUDPromoOfferItem(List<Winit.Modules.Promotion.Model.Classes.PromoOfferItemView> promoOfferItemlst)
        {
            int count = 0;

            if (promoOfferItemlst == null || promoOfferItemlst.Count == 0)
            {
                return count;
            }

            List<string> uidList = promoOfferItemlst.Select(po => po.UID).ToList();
            List<string> deletedUidList = promoOfferItemlst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<IPromoOfferItem> existingRec = await SelectPromoOfferItemByUID(uidList);
                foreach (PromoOfferItemView promoOfferItem in promoOfferItemlst)
                {
                    switch (promoOfferItem.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == promoOfferItem.UID);
                            count += exists ?
                                await UpdatePromoOfferItem(promoOfferItem) :
                                await CreatePromoOfferItem(promoOfferItem);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromoOfferItem(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> CUDPromoCondition(List<Winit.Modules.Promotion.Model.Classes.PromoConditionView> PromoConditionlst)
        {
            int count = 0;

            if (PromoConditionlst == null || PromoConditionlst.Count == 0)
            {
                return count;
            }

            List<string> uidList = PromoConditionlst.Select(po => po.UID).ToList();
            List<string> deletedUidList = PromoConditionlst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
            try
            {
                IEnumerable<IPromoCondition> existingRec = await SelectPromoConditionByUID(uidList);

                foreach (PromoConditionView PromoCondition in PromoConditionlst)
                {
                    switch (PromoCondition.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == PromoCondition.UID);
                            count += exists ?
                                await UpdatePromoCondition(PromoCondition) :
                                await CreatePromoCondition(PromoCondition);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromoCondition(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {

                throw;
            }
            return count;
        }
        public async Task<int> CUDItemPromotionMap(List<Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView> ItemPromotionMaplst)
        {
            int count = 0;

            if (ItemPromotionMaplst == null || ItemPromotionMaplst.Count == 0)
            {
                return count;
            }
            List<string> uidList = ItemPromotionMaplst.Select(po => po.UID).ToList();
            List<string> deletedUidList = ItemPromotionMaplst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
            try
            {
                IEnumerable<IItemPromotionMap> existingRec = await SelectItemPromotionMapByUID(uidList);

                foreach (ItemPromotionMapView ItemPromotionMap in ItemPromotionMaplst)
                {
                    switch (ItemPromotionMap.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.UID == ItemPromotionMap.UID);
                            count += exists ?
                                await UpdateItemPromotionMap(ItemPromotionMap) :
                                await CreateItemPromotionMap(ItemPromotionMap);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeleteItemPromotionMap(deletedUidList);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }


        public async Task<int> CUDPromotionMaster(Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView)
        {
            int count = 0;

            try
            {
                // Add null checks and logging
                if (promoMasterView == null)
                {
                    System.Console.WriteLine("[ERROR] CUDPromotionMaster: promoMasterView is null");
                    throw new ArgumentNullException(nameof(promoMasterView), "PromoMasterView cannot be null");
                }

                if (promoMasterView.PromotionView == null)
                {
                    System.Console.WriteLine("[ERROR] CUDPromotionMaster: PromotionView is null");
                    System.Console.WriteLine("[DEBUG] PromoMasterView properties received:");
                    System.Console.WriteLine($"  - IsNew: {promoMasterView.IsNew}");
                    System.Console.WriteLine($"  - PromoOrderViewList: {promoMasterView.PromoOrderViewList?.Count ?? 0} items");
                    System.Console.WriteLine($"  - PromoOfferViewList: {promoMasterView.PromoOfferViewList?.Count ?? 0} items");
                    throw new ArgumentNullException("promoMasterView.PromotionView", "PromotionView cannot be null");
                }

                // Log the promotion details being processed
                System.Console.WriteLine($"[DEBUG] Processing promotion: UID={promoMasterView.PromotionView.UID}, Code={promoMasterView.PromotionView.Code}, Name={promoMasterView.PromotionView.Name}");
                System.Console.WriteLine($"[DEBUG] OrgUID={promoMasterView.PromotionView.OrgUID}, CompanyUID={promoMasterView.PromotionView.CompanyUID}");
                System.Console.WriteLine($"[DEBUG] ActionType={promoMasterView.PromotionView.ActionType}");

                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            count += await CUDPromotion(promoMasterView.PromotionView);
                            count += await CUDPromoOrder(promoMasterView.PromoOrderViewList);
                            count += await CUDPromoOrderItem(promoMasterView.PromoOrderItemViewList);
                            count += await CUDPromoOffer(promoMasterView.PromoOfferViewList);
                            count += await CUDPromoOfferItem(promoMasterView.PromoOfferItemViewList);
                            count += await CUDPromoCondition(promoMasterView.PromoConditionViewList);
                            count += await CUDItemPromotionMap(promoMasterView.ItemPromotionMapViewList);
                            
                            // Handle volume cap if present
                            if (promoMasterView.PromotionVolumeCap != null)
                            {
                                count += await CUDPromotionVolumeCap(promoMasterView.PromotionVolumeCap);
                            }
                            
                            // Handle hierarchy caps if present
                            if (promoMasterView.PromotionHierarchyCapViewList != null && promoMasterView.PromotionHierarchyCapViewList.Count > 0)
                            {
                                count += await CUDPromotionHierarchyCapList(promoMasterView.PromotionHierarchyCapViewList);
                            }
                            
                            // Handle period caps if present
                            if (promoMasterView.PromotionPeriodCapViewList != null && promoMasterView.PromotionPeriodCapViewList.Count > 0)
                            {
                                count += await CUDPromotionPeriodCapList(promoMasterView.PromotionPeriodCapViewList);
                            }
                            
                            transaction.Commit();
                            
                            // Additional null check before accessing UID
                            if (!string.IsNullOrEmpty(promoMasterView.PromotionView?.UID))
                            {
                                List<string> promotionUIDList = new List<string>();
                                promotionUIDList.Add(promoMasterView.PromotionView.UID);
                                count += await CreateDMSPromotionByJsonData(promotionUIDList);
                                count += await PopulateItemPromotionMap(promoMasterView.PromotionView.UID);
                            }
                            
                            System.Console.WriteLine($"[SUCCESS] Promotion created successfully. Total operations: {count}");
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"[ERROR] Transaction failed: {ex.Message}");
                            System.Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] CUDPromotionMaster failed: {ex.Message}");
                throw;
            }

            return count;
        }
        //Promotion
        public async Task<int> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView)
        {
            // Log the IsActive value being updated
            Console.WriteLine($"[DEBUG] UpdatePromotion - UID: {updatePromotionView.UID}, IsActive: {updatePromotionView.IsActive}");
            
            var Query = @"UPDATE promotion SET 
                                 company_uid = @CompanyUID, 
                                 org_uid = @OrgUID, 
                                 code = @Code, 
                                 name = @Name, 
                                 remarks = @Remarks, 
                                 category = @Category, 
                                 has_slabs = @HasSlabs, 
                                 created_by_emp_uid = @CreatedByEmpUID, 
                                 valid_from = @ValidFrom, 
                                 valid_upto = @ValidUpto, 
                                 type = @Type, 
                                 promo_format = @PromoFormat, 
                                 is_active = @IsActive, 
                                 promo_title = @PromoTitle, 
                                 promo_message = @PromoMessage, 
                                 has_fact_sheet = @HasFactSheet, 
                                 priority = @Priority, 
                                 modified_time = @ModifiedTime, 
                                 server_modified_time = @ServerModifiedTime
                             WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@Id", updatePromotionView.Id },
                        { "@UID", updatePromotionView.UID },
                        { "@CompanyUID", updatePromotionView.CompanyUID },
                        { "@OrgUID", updatePromotionView.OrgUID },
                        { "@Code", updatePromotionView.Code },
                        { "@Name", updatePromotionView.Name },
                        { "@Remarks", updatePromotionView.Remarks },
                        { "@Category", updatePromotionView.Category },
                        { "@HasSlabs", updatePromotionView.HasSlabs },
                        { "@CreatedByEmpUID", updatePromotionView.CreatedByEmpUID },
                        { "@ValidFrom", updatePromotionView.ValidFrom },
                        { "@ValidUpto", updatePromotionView.ValidUpto },
                        { "@Type", updatePromotionView.Type },
                        { "@PromoFormat", updatePromotionView.PromoFormat },
                        { "@IsActive", updatePromotionView.IsActive },
                        { "@PromoTitle", updatePromotionView.PromoTitle },
                        { "@PromoMessage", updatePromotionView.PromoMessage },
                        { "@HasFactSheet", updatePromotionView.HasFactSheet },
                        { "@Priority", updatePromotionView.Priority },
                        { "@CreatedTime", updatePromotionView.CreatedTime ?? DateTime.UtcNow },
                        { "@ModifiedTime", DateTime.UtcNow },
                        { "@ServerAddTime", updatePromotionView.ServerAddTime ?? DateTime.UtcNow },
                        { "@ServerModifiedTime", DateTime.UtcNow },
                        };
            
            try
            {
                var result = await ExecuteNonQueryAsync(Query, Parameters);
                Console.WriteLine($"[DEBUG] UpdatePromotion - Result: {result} rows affected");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdatePromotion failed: {ex.Message}");
                throw;
            }
        }
        private async Task<int> CreatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView CreatePromotionView)
        {
            var Query = @"INSERT INTO promotion (uid, company_uid, org_uid, code, name, remarks, category, has_slabs, created_by_emp_uid,
                                              valid_from, valid_upto, type, promo_format, is_active, promo_title, promo_message, has_fact_sheet, priority,
                                                 created_time, modified_time, server_add_time, server_modified_time)
                          VALUES (@UID, @CompanyUID, @OrgUID, @Code, @Name, @Remarks, @Category, @HasSlabs,
                                  @CreatedByEmpUID, @ValidFrom, @ValidUpto, @Type, @PromoFormat, @IsActive, @PromoTitle, @PromoMessage, @HasFactSheet, @Priority,
                                  @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", CreatePromotionView.UID },
                        { "@CompanyUID", CreatePromotionView.CompanyUID },
                        { "@OrgUID", CreatePromotionView.OrgUID },
                        { "@Code", CreatePromotionView.Code },
                        { "@Name", CreatePromotionView.Name },
                        { "@Remarks", CreatePromotionView.Remarks },
                        { "@Category", CreatePromotionView.Category },
                        { "@HasSlabs", CreatePromotionView.HasSlabs },
                        { "@CreatedByEmpUID", CreatePromotionView.CreatedByEmpUID },
                        { "@ValidFrom", CreatePromotionView.ValidFrom },
                        { "@ValidUpto", CreatePromotionView.ValidUpto },
                        { "@Type", CreatePromotionView.Type },
                        { "@PromoFormat", CreatePromotionView.PromoFormat },
                        { "@IsActive", CreatePromotionView.IsActive },
                        { "@PromoTitle", CreatePromotionView.PromoTitle },
                        { "@PromoMessage", CreatePromotionView.PromoMessage },
                        { "@HasFactSheet", CreatePromotionView.HasFactSheet },
                        { "@Priority", CreatePromotionView.Priority },
                        { "@CreatedTime", CreatePromotionView.CreatedTime },
                        { "@ModifiedTime", CreatePromotionView.ModifiedTime },
                        { "@ServerAddTime", CreatePromotionView.ServerAddTime },
                        { "@ServerModifiedTime", CreatePromotionView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromotion(string UID)
        {
            var Query = @"DELETE FROM promotion WHERE uid=@UID";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", UID },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        public async Task<Winit.Modules.Promotion.Model.Interfaces.IPromotion> SelectPromotionByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT 
                                    p.id AS Id,
                                    p.uid AS Uid,
                                    p.company_uid AS CompanyUid,
                                    p.org_uid AS OrgUid,
                                    p.code AS Code,
                                    p.name AS Name,
                                    p.remarks AS Remarks,
                                    p.category AS Category,
                                    p.has_slabs AS HasSlabs,
                                    p.created_by_emp_uid AS CreatedByEmpUid,
                                    p.valid_from AS ValidFrom,
                                    p.valid_upto AS ValidUpto,
                                    p.type AS Type,
                                    p.promo_format AS PromoFormat,
                                    p.is_active AS IsActive,
                                    p.promo_title AS PromoTitle,
                                    p.promo_message AS PromoMessage,
                                    p.has_fact_sheet AS HasFactSheet,
                                    p.priority AS Priority,
                                    p.ss AS Ss,
                                    p.created_time AS CreatedTime,
                                    p.modified_time AS ModifiedTime,
                                    p.server_add_time AS ServerAddTime,
                                    p.server_modified_time AS ServerModifiedTime
                                    
                                FROM     public.promotion p where p.uid=@UID";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromotion>().GetType();
            Winit.Modules.Promotion.Model.Interfaces.IPromotion promotionDetails = await ExecuteSingleAsync<Winit.Modules.Promotion.Model.Interfaces.IPromotion>(sql, parameters, type);
            return promotionDetails;
        }
        //PromoOrder
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder>> SelectPromoOrderByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT
         id AS Id,
         uid AS Uid,
         promotion_uid AS PromotionUid,
         selection_model AS SelectionModel,
         qualification_level AS QualificationLevel,
         min_deal_count AS MinDealCount,
         max_deal_count AS MaxDealCount,
         ss AS Ss,
         created_time AS CreatedTime,
         modified_time AS ModifiedTime,
         server_add_time AS ServerAddTime,
         server_modified_time AS ServerModifiedTime
     FROM    public.promo_order  WHERE uid = ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder> promotionOrderDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder>(sql, parameters, type);
            return promotionOrderDetails;
        }
        private async Task<int> CreatePromoOrder(Winit.Modules.Promotion.Model.Classes.PromoOrderView CreatePromoOrderView)
        {
            try
            {
                var Query = @"INSERT INTO promo_order (uid, promotion_uid, selection_model, qualification_level, min_deal_count, max_deal_count,
                                                         ss, created_time, modified_time, server_add_time, server_modified_time)
                                VALUES (@UID, @PromotionUID, @SelectionModel, @QualificationLevel,
                                        @MinDealCount, @MaxDealCount, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", CreatePromoOrderView.UID },
                           { "@PromotionUID", CreatePromoOrderView.PromotionUID },
                           { "@SelectionModel", CreatePromoOrderView.SelectionModel },
                           { "@QualificationLevel", CreatePromoOrderView.QualificationLevel },
                           { "@MinDealCount", CreatePromoOrderView.MinDealCount },
                           { "@MaxDealCount", CreatePromoOrderView.MaxDealCount },
                           { "@ss", CreatePromoOrderView.SS },
                           { "@CreatedTime", CreatePromoOrderView.CreatedTime },
                           { "@ModifiedTime", CreatePromoOrderView.ModifiedTime },
                           { "@ServerAddTime", CreatePromoOrderView.ServerAddTime },
                           { "@ServerModifiedTime", CreatePromoOrderView.ServerModifiedTime },
            };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }


        }
        private async Task<int> UpdatePromoOrder(Winit.Modules.Promotion.Model.Classes.PromoOrderView updatePromoOrderView)
        {
            var Query = @"UPDATE promo_order SET 
                                    promotion_uid = @PromotionUID, 
                                    selection_model = @SelectionModel, 
                                    qualification_level = @QualificationLevel, 
                                    min_deal_count = @MinDealCount, 
                                    ss = @ss, 
                                    max_deal_count = @MaxDealCount, 
                                    modified_time = @ModifiedTime, 
                                    server_modified_time = @ServerModifiedTime
                                WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", updatePromoOrderView.UID },
                           { "@PromotionUID", updatePromoOrderView.PromotionUID },
                           { "@SelectionModel", updatePromoOrderView.SelectionModel },
                           { "@QualificationLevel", updatePromoOrderView.QualificationLevel },
                           { "@MinDealCount", updatePromoOrderView.MinDealCount },
                           { "@ss", updatePromoOrderView.SS },
                           { "@MaxDealCount", updatePromoOrderView.MaxDealCount },
                           { "@CreatedTime", updatePromoOrderView.CreatedTime },
                           { "@ModifiedTime", updatePromoOrderView.ModifiedTime },
                           { "@ServerAddTime", updatePromoOrderView.ServerAddTime },
                           { "@ServerModifiedTime", updatePromoOrderView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromoOrder(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_order WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
        //PromoOrderItem
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem>> SelectPromoOrderItemByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT
        id AS Id,
        uid AS Uid,
        promo_order_uid AS PromoOrderUid,
        parent_uid AS ParentUid,
        item_criteria_type AS ItemCriteriaType,
        item_criteria_selected AS ItemCriteriaSelected,
        is_compulsory AS IsCompulsory,
        item_uom AS ItemUom,
        promo_split AS PromoSplit,
        ss AS Ss,
        created_time AS CreatedTime,
        modified_time AS ModifiedTime,
        server_add_time AS ServerAddTime,
        server_modified_time AS ServerModifiedTime,
        promotion_uid AS PromotionUid
    FROM
        public.promo_order_item Where uid=  ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem> promotionOrderItemDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem>(sql, parameters, type);
            return promotionOrderItemDetails;
        }
        private async Task<int> CreatePromoOrderItem(Winit.Modules.Promotion.Model.Classes.PromoOrderItemView CreatePromoOrderItemView)
        {
            try
            {
                var Query = @"INSERT INTO promo_order_item (uid, promotion_uid, promo_order_uid, parent_uid, item_criteria_type, item_criteria_selected, is_compulsory, 
                                                            item_uom, promo_split, ss, min_qty, max_qty, created_time, modified_time, server_add_time, server_modified_time,
                                                            config_group_id, config_name, config_promotion_type) 
                                VALUES (@UID, @PromotionUID, @PromoOrderUID, @ParentUID, @ItemCriteriaType, @ItemCriteriaSelected, @IsCompulsory, @ItemUOM, @PromoSplit, @ss, @MinQty, @MaxQty, @CreatedTime,
                                                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId, @ConfigName, @ConfigPromotionType);";
                var Parameters = new Dictionary<string, object>
                        {
                         { "@UID", CreatePromoOrderItemView.UID },
                         { "@PromotionUID", CreatePromoOrderItemView.PromotionUID },
                         { "@PromoOrderUID", CreatePromoOrderItemView.PromoOrderUID },
                         { "@ParentUID", CreatePromoOrderItemView.ParentUID },
                         { "@ItemCriteriaType", CreatePromoOrderItemView.ItemCriteriaType },
                         { "@ItemCriteriaSelected", CreatePromoOrderItemView.ItemCriteriaSelected },
                         { "@IsCompulsory", CreatePromoOrderItemView.IsCompulsory },
                         { "@ItemUOM", CreatePromoOrderItemView.ItemUOM },
                         { "@PromoSplit", CreatePromoOrderItemView.PromoSplit },
                         { "@ss", CreatePromoOrderItemView.SS },
                         { "@MinQty", CreatePromoOrderItemView.MinQty },
                         { "@MaxQty", CreatePromoOrderItemView.MaxQty },
                         { "@CreatedTime", CreatePromoOrderItemView.CreatedTime },
                         { "@ModifiedTime", CreatePromoOrderItemView.ModifiedTime },
                         { "@ServerAddTime", CreatePromoOrderItemView.ServerAddTime },
                         { "@ServerModifiedTime", CreatePromoOrderItemView.ServerModifiedTime },
                         { "@ConfigGroupId", CreatePromoOrderItemView.ConfigGroupId },
                         { "@ConfigName", CreatePromoOrderItemView.ConfigName },
                         { "@ConfigPromotionType", CreatePromoOrderItemView.ConfigPromotionType },
            };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }


        }
        private async Task<int> UpdatePromoOrderItem(Winit.Modules.Promotion.Model.Classes.PromoOrderItemView updatePromoOrderItemView)
        {
            var Query = @"UPDATE promo_order_item SET 
                        item_criteria_type = @ItemCriteriaType, 
                        item_criteria_selected = @ItemCriteriaSelected, 
                        is_compulsory = @IsCompulsory, 
                        item_uom = @ItemUOM, 
                        promo_split = @PromoSplit, 
                        ss = @ss,
                        min_qty = @MinQty,
                        max_qty = @MaxQty, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime
                        WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", updatePromoOrderItemView.UID },
                         { "@ItemCriteriaType", updatePromoOrderItemView.ItemCriteriaType },
                         { "@ItemCriteriaSelected", updatePromoOrderItemView.ItemCriteriaSelected },
                         { "@IsCompulsory", updatePromoOrderItemView.IsCompulsory },
                         { "@ItemUOM", updatePromoOrderItemView.ItemUOM },
                         { "@PromoSplit", updatePromoOrderItemView.PromoSplit },
                         { "@ss", updatePromoOrderItemView.SS },
                         { "@MinQty", updatePromoOrderItemView.MinQty },
                         { "@MaxQty", updatePromoOrderItemView.MaxQty },
                         { "@ModifiedTime", updatePromoOrderItemView.ModifiedTime },
                         { "@ServerModifiedTime", updatePromoOrderItemView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromoOrderItem(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_order_item WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
        //PromoOffer
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer>> SelectPromoOfferByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT
                             id AS Id,
                             uid AS UID,
                             promo_order_uid AS PromoOrderUID,
                             type AS Type,
                             qualification_level AS QualificationLevel,
                             application_level AS ApplicationLevel,
                             selection_model AS SelectionModel,
                             has_offer_item_selection AS HasOfferItemSelection,
                             ss AS Ss,
                             created_time AS CreatedTime,
                             modified_time AS ModifiedTime,
                             server_add_time AS ServerAddTime,
                             server_modified_time AS ServerModifiedTime,
                             promotion_uid AS PromotionUID
                         FROM
                             promo_offer WHERE uid = ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer> promoOfferDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer>(sql, parameters, type);
            return promoOfferDetails;
        }
        private async Task<int> CreatePromoOffer(Winit.Modules.Promotion.Model.Classes.PromoOfferView CreatePromoOffer)
        {
            try
            {
                var Query = @"INSERT INTO promo_offer (uid, promotion_uid, promo_order_uid, type, qualification_level, application_level, selection_model, 
                            has_offer_item_selection, ss, created_time, modified_time, server_add_time, server_modified_time) 
                    VALUES (@UID, @PromotionUID, @PromoOrderUID, @Type, @QualificationLevel, 
                            @ApplicationLevel, @SelectionModel, @HasOfferItemSelection, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                var Parameters = new Dictionary<string, object>
                        {

                         { "@UID", CreatePromoOffer.UID },
                         { "@PromoOrderUID", CreatePromoOffer.PromoOrderUID },
                         { "@PromotionUID", CreatePromoOffer.PromotionUID },
                         { "@Type", CreatePromoOffer.Type },
                         { "@QualificationLevel", CreatePromoOffer.QualificationLevel },
                         { "@ApplicationLevel", CreatePromoOffer.ApplicationLevel },
                         { "@SelectionModel", CreatePromoOffer.SelectionModel },
                         { "@HasOfferItemSelection", CreatePromoOffer.HasOfferItemSelection },
                         { "@ss", CreatePromoOffer.SS },
                         { "@CreatedTime", CreatePromoOffer.CreatedTime },
                         { "@ModifiedTime", CreatePromoOffer.ModifiedTime },
                         { "@ServerAddTime", CreatePromoOffer.ServerAddTime },
                         { "@ServerModifiedTime", CreatePromoOffer.ServerModifiedTime },
            };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }

        }
        private async Task<int> UpdatePromoOffer(Winit.Modules.Promotion.Model.Classes.PromoOfferView updatePromoOffer)
        {
            var Query = @"UPDATE promo_offer SET 
                        type = @Type, 
                        qualification_level = @QualificationLevel, 
                        application_level = @ApplicationLevel, 
                        selection_model = @SelectionModel, 
                        has_offer_item_selection = @HasOfferItemSelection, 
                        ss = @ss, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime
                         WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                         { "@UID", updatePromoOffer.UID },
                         { "@Type", updatePromoOffer.Type },
                         { "@QualificationLevel", updatePromoOffer.QualificationLevel },
                         { "@ApplicationLevel", updatePromoOffer.ApplicationLevel },
                         { "@SelectionModel", updatePromoOffer.SelectionModel },
                         { "@HasOfferItemSelection", updatePromoOffer.HasOfferItemSelection },
                         { "@ss", updatePromoOffer.SS },
                         { "@CreatedTime", updatePromoOffer.CreatedTime },
                         { "@ModifiedTime", updatePromoOffer.ModifiedTime },
                         { "@ServerAddTime", updatePromoOffer.ServerAddTime },
                         { "@ServerModifiedTime", updatePromoOffer.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromoOffer(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_offer WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
        //PromoOfferItem
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem>> SelectPromoOfferItemByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"
                         SELECT
                                 id AS Id,
                                 uid AS UID,
                                 promo_offer_uid AS PromoOfferUID,
                                 item_criteria_type AS ItemCriteriaType,
                                 item_criteria_selected AS ItemCriteriaSelected,
                                 is_compulsory AS IsCompulsory,
                                 item_uom AS ItemUOM,
                                 quantity AS Quantity,
                                 ss AS SS,
                                 created_time AS CreatedTime,
                                 modified_time AS ModifiedTime,
                                 server_add_time AS ServerAddTime,
                                 server_modified_time AS ServerModifiedTime,
                                 promotion_uid AS PromotionUID
                             FROM
                                 promo_offer_item WHERE uid = ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem> promoOfferItemDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem>(sql, parameters, type);
            return promoOfferItemDetails;
        }
        private async Task<int> CreatePromoOfferItem(Winit.Modules.Promotion.Model.Classes.PromoOfferItemView CreatePromoOfferItem)
        {
            try
            {
                var Query = @"INSERT INTO promo_offer_item (uid, promotion_uid, promo_offer_uid, item_criteria_type, item_criteria_selected, is_compulsory, item_uom,
                                                           quantity, ss, created_time, modified_time, server_add_time, server_modified_time, config_group_id) 
                               VALUES (@UID, @PromotionUID, @PromoOfferUID, @ItemCriteriaType, 
                                                            @ItemCriteriaSelected, @IsCompulsory, @ItemUOM, @Quantity, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId);";
                var Parameters = new Dictionary<string, object>
                        {
                        { "@UID", CreatePromoOfferItem.UID },
                        { "@PromoOfferUID", CreatePromoOfferItem.PromoOfferUID },
                        { "@PromotionUID", CreatePromoOfferItem.PromotionUID },
                        { "@ItemCriteriaType", CreatePromoOfferItem.ItemCriteriaType },
                        { "@ItemCriteriaSelected", CreatePromoOfferItem.ItemCriteriaSelected },
                        { "@IsCompulsory", CreatePromoOfferItem.IsCompulsory },
                        { "@ItemUOM", CreatePromoOfferItem.ItemUOM },
                        { "@Quantity", CreatePromoOfferItem.Quantity },
                        { "@ss", CreatePromoOfferItem.SS },
                        { "@CreatedTime", CreatePromoOfferItem.CreatedTime },
                        { "@ModifiedTime", CreatePromoOfferItem.ModifiedTime },
                        { "@ServerAddTime", CreatePromoOfferItem.ServerAddTime },
                        { "@ServerModifiedTime", CreatePromoOfferItem.ServerModifiedTime },
                        { "@ConfigGroupId", CreatePromoOfferItem.ConfigGroupId },
            };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> UpdatePromoOfferItem(Winit.Modules.Promotion.Model.Classes.PromoOfferItemView updatePromoOfferItem)
        {
            var Query = @"UPDATE promo_offer_item SET 
                        item_criteria_type = @ItemCriteriaType, 
                        item_criteria_selected = @ItemCriteriaSelected, 
                        is_compulsory = @IsCompulsory, 
                        item_uom = @ItemUOM, 
                        quantity = @Quantity,
                        ss = @ss, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime
                         WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                         { "@UID", updatePromoOfferItem.UID },
                        { "@ItemCriteriaType", updatePromoOfferItem.ItemCriteriaType },
                        { "@ItemCriteriaSelected", updatePromoOfferItem.ItemCriteriaSelected },
                        { "@IsCompulsory", updatePromoOfferItem.IsCompulsory },
                        { "@ItemUOM", updatePromoOfferItem.ItemUOM },
                        { "@Quantity", updatePromoOfferItem.Quantity },
                        { "@ss", updatePromoOfferItem.SS },
                        { "@ModifiedTime", updatePromoOfferItem.ModifiedTime },
                        { "@ServerModifiedTime", updatePromoOfferItem.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromoOfferItem(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_offer_item WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
        //PromoCondition
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition>> SelectPromoConditionByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT
                           id AS Id,
                           uid AS UID,
                           reference_type AS ReferenceType,
                           reference_uid AS ReferenceUID,
                           condition_type AS ConditionType,
                           min AS Min,
                           max AS Max,
                           max_deal_count AS MaxDealCount,
                           uom AS UOM,
                           all_uom_conversion AS AllUOMConversion,
                           value_type AS ValueType,
                           is_prorated AS IsProrated,
                           ss AS SS,
                           discount_type AS DiscountType,
                           discount_percentage AS DiscountPercentage,
                           discount_amount AS DiscountAmount,
                           free_quantity AS FreeQuantity,
                           created_time AS CreatedTime,
                           modified_time AS ModifiedTime,
                           server_add_time AS ServerAddTime,
                           server_modified_time AS ServerModifiedTime,
                           promotion_uid AS PromotionUID
                       FROM public.promo_condition WHERE uid = ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition> PromoConditionDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition>(sql, parameters, type);
            return PromoConditionDetails;
        }
        private async Task<int> CreatePromoCondition(Winit.Modules.Promotion.Model.Classes.PromoConditionView CreatePromoConditionView)
        {
            try
            {
                var Query = @"INSERT INTO promo_condition (uid, promotion_uid ,reference_type, reference_uid, condition_type, min, max, max_deal_count, uom, 
                            all_uom_conversion, value_type, is_prorated, ss, discount_type, discount_percentage, discount_amount, free_quantity,
                            created_time, modified_time, server_add_time, server_modified_time, config_group_id, config_details) 
                            VALUES (@UID,@PromotionUID, @ReferenceType, @ReferenceUID, @ConditionType, @Min, @Max, @MaxDealCount, @UOM, @AllUOMConversion, 
                            @ValueType, @IsProrated, @ss, @DiscountType, @DiscountPercentage, @DiscountAmount, @FreeQuantity,
                            @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId, @ConfigDetails);";
                var Parameters = new Dictionary<string, object>
                             {
                             { "@UID", CreatePromoConditionView.UID },
                             { "@PromotionUID", CreatePromoConditionView.PromotionUID },
                             { "@ReferenceType", CreatePromoConditionView.ReferenceType },
                             { "@ReferenceUID", CreatePromoConditionView.ReferenceUID },
                             { "@ConditionType", CreatePromoConditionView.ConditionType },
                             { "@Min", CreatePromoConditionView.Min },
                             { "@Max", CreatePromoConditionView.Max },
                             { "@MaxDealCount", CreatePromoConditionView.MaxDealCount },
                             { "@UOM", CreatePromoConditionView.UOM },
                             { "@AllUOMConversion", CreatePromoConditionView.AllUOMConversion },
                             { "@ValueType", CreatePromoConditionView.ValueType },
                             { "@IsProrated", CreatePromoConditionView.IsProrated },
                             { "@ss", CreatePromoConditionView.SS },
                             { "@DiscountType", CreatePromoConditionView.DiscountType },
                             { "@DiscountPercentage", CreatePromoConditionView.DiscountPercentage },
                             { "@DiscountAmount", CreatePromoConditionView.DiscountAmount },
                             { "@FreeQuantity", CreatePromoConditionView.FreeQuantity },
                             { "@CreatedTime", CreatePromoConditionView.CreatedTime },
                             { "@ModifiedTime", CreatePromoConditionView.ModifiedTime },
                             { "@ServerAddTime", CreatePromoConditionView.ServerAddTime },
                             { "@ServerModifiedTime", CreatePromoConditionView.ServerModifiedTime },
                             { "@ConfigGroupId", CreatePromoConditionView.ConfigGroupId },
                             { "@ConfigDetails", CreatePromoConditionView.ConfigDetails },
                             };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> UpdatePromoCondition(Winit.Modules.Promotion.Model.Classes.PromoConditionView updatePromoConditionView)
        {
            var Query = @"UPDATE promo_condition SET 
                        reference_type = @ReferenceType, 
                        reference_uid = @ReferenceUID, 
                        condition_type = @ConditionType, 
                        min = @Min, 
                        max = @Max, 
                        max_deal_count = @MaxDealCount, 
                        uom = @UOM, 
                        all_uom_conversion = @AllUOMConversion, 
                        value_type = @ValueType, 
                        is_prorated = @IsProrated, 
                        ss = @ss, 
                        discount_type = @DiscountType,
                        discount_percentage = @DiscountPercentage,
                        discount_amount = @DiscountAmount,
                        free_quantity = @FreeQuantity,
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime 
                         WHERE uid = @UID";
            var Parameters = new Dictionary<string, object>
                        {
                             { "@UID", updatePromoConditionView.UID },
                             { "@ReferenceType", updatePromoConditionView.ReferenceType },
                             { "@ReferenceUID", updatePromoConditionView.ReferenceUID },
                             { "@ConditionType", updatePromoConditionView.ConditionType },
                             { "@Min", updatePromoConditionView.Min },
                             { "@Max", updatePromoConditionView.Max },
                             { "@MaxDealCount", updatePromoConditionView.MaxDealCount },
                             { "@UOM", updatePromoConditionView.UOM },
                             { "@AllUOMConversion", updatePromoConditionView.AllUOMConversion },
                             { "@ValueType", updatePromoConditionView.ValueType },
                             { "@IsProrated", updatePromoConditionView.IsProrated },
                             { "@ss", updatePromoConditionView.SS },
                             { "@DiscountType", updatePromoConditionView.DiscountType },
                             { "@DiscountPercentage", updatePromoConditionView.DiscountPercentage },
                             { "@DiscountAmount", updatePromoConditionView.DiscountAmount },
                             { "@FreeQuantity", updatePromoConditionView.FreeQuantity },
                             { "@CreatedTime", updatePromoConditionView.CreatedTime },
                             { "@ModifiedTime", updatePromoConditionView.ModifiedTime },
                             { "@ServerAddTime", updatePromoConditionView.ServerAddTime },
                             { "@ServerModifiedTime", updatePromoConditionView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeletePromoCondition(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_condition WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
        //ItemPromotionMap
        private async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>> SelectItemPromotionMapByUID(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT 
                            id AS Id,
                            uid AS UID,
                            sku_type AS SKUType,
                            sku_type_uid AS SKUTypeUID,
                            promotion_uid AS PromotionUID,
                            ss AS SS,
                            created_time AS CreatedTime,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime
                        FROM 
                            public.item_promotion_map
                         WHERE uid = ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap> ItemPromotionMapDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>(sql, parameters, type);
            return ItemPromotionMapDetails;
        }
        private async Task<int> CreateItemPromotionMap(Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView CreateItemPromotionMap)
        {
            try
            {
                var Query = @"INSERT INTO item_promotion_map (uid, sku_type, sku_type_uid, promotion_uid, ss, created_time, 
                          modified_time, server_add_time, server_modified_time) VALUES (@UID, @SKUType, @SKUTypeUID, @PromotionUID, @SS, @CreatedTime, @ModifiedTime
                          , @ServerAddTime, @ServerModifiedTime);";
                var Parameters = new Dictionary<string, object>
                             {
                             { "@UID", CreateItemPromotionMap.UID },
                             { "@SKUType", CreateItemPromotionMap.SKUType },
                             { "@SKUTypeUID", CreateItemPromotionMap.SKUTypeUID },
                             { "@PromotionUID", CreateItemPromotionMap.PromotionUID },
                             { "@SS", CreateItemPromotionMap.SS },
                             { "@CreatedTime", CreateItemPromotionMap.CreatedTime },
                             { "@ModifiedTime", CreateItemPromotionMap.ModifiedTime },
                             { "@ServerAddTime", CreateItemPromotionMap.ServerAddTime },
                             { "@ServerModifiedTime", CreateItemPromotionMap.ServerModifiedTime },
                             };
                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> UpdateItemPromotionMap(Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView updatePromoConditionView)
        {
            var Query = @"UPDATE item_promotion_map SET 
    sku_type = @SKUType, 
    ss = @SS, 
    modified_time = @ModifiedTime, 
    server_modified_time = @ServerModifiedTime
WHERE uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                             { "@UID", updatePromoConditionView.UID },
                             { "@SKUType", updatePromoConditionView.SKUType },
                             { "@SS", updatePromoConditionView.SS },
                             { "@ModifiedTime", updatePromoConditionView.ModifiedTime },
                             { "@ServerModifiedTime", updatePromoConditionView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<int> DeleteItemPromotionMap(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM item_promotion_map WHERE  uid =ANY(string_to_array(@UIDs, ','))";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UIDs", commaSeperatedUIDs },
                        };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }

        public async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>> SelectItemPromotionMapByPromotionUIDs(List<string> promotionUIDs)
        {
            string commaSeperatedUIDs = string.Join(",", promotionUIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"PromotionUIDs",  commaSeperatedUIDs}
            };

            var sql = @"SELECT * FROM item_promotion_map WHERE promotion_uid = ANY(string_to_array(@PromotionUIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap> ItemPromotionMapDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>(sql, parameters, type);
            return ItemPromotionMapDetails;
        }

        //public async Task<(List<Model.Interfaces.IPromotion>, List<Model.Interfaces.IPromoOrder>, List<Model.Interfaces.IPromoOrderItem>, List<Model.Interfaces.IPromoOffer>,
        //    List<Model.Interfaces.IPromoOfferItem>, List<Model.Interfaces.IPromoCondition>, List<Model.Interfaces.IItemPromotionMap>)> GetPromotionMasterByUID(string UID)
        //{
        //    try
        //    {
        //        Dictionary<string, object> Parameters = new Dictionary<string, object>
        //        {
        //            { "UID", UID },
        //        };
        //        var promotionSql = new StringBuilder(@"SELECT
        //                                           ""Id"", ""UID"", ""CompanyUID"", ""OrgUID"", ""Code"", ""Name"", ""Remarks"", ""Category"", ""HasSlabs"", ""CreatedByEmpUID"", 
        //                                            ""ValidFrom"", ""ValidUpto"", ""Type"", ""PromoFormat"", ""IsActive"", ""PromoTitle"", ""PromoMessage"", ""HasFactSheet"", 
        //                                            ""Priority"", ""ss"", ""CreatedTime"", ""ModifiedTime"", ""ServerAddTime"", ""ServerModifiedTime"" WHERE ""UID""=@UID");
        //        Type promotionType = _serviceProvider.GetRequiredService<Model.Interfaces.IPromotion>().GetType();
        //        List<Model.Interfaces.IPromotion> promotionList = await ExecuteQueryAsync<Model.Interfaces.IPromotion>(promotionSql.ToString(), Parameters, promotionType);
        //        var promoOrderSQL = new StringBuilder(@"SELECT
        //                                               ""Id"", ""UID"", ""PromotionUID"", ""SelectionModel"", ""QualificationLevel"", ""MinDealCount"", 
        //                                               ""MaxDealCount"", ""ss"", ""CreatedTime"", ""ModifiedTime"", ""ServerAddTime"", ""ServerModifiedTime"" WHERE ""PromotionUID""=@UID");
        //        Type promoOrderType = _serviceProvider.GetRequiredService<Model.Interfaces.IPromoOrder>().GetType();
        //        List<Model.Interfaces.IPromoOrder> promoOrderList = await ExecuteQueryAsync<Model.Interfaces.IPromoOrder>(promoOrderSQL.ToString(), Parameters, promoOrderType);
        //        var routeScheduleDayWiseSQL = new StringBuilder(@"SELECT
        //                                           ""Id"",""UID"",""CreatedBy"",""CreatedTime"",""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",
        //                                           ""ServerModifiedTime"",""SS"",""RouteScheduleUID"",""Monday"",""Tuesday"",""Wednesday"",""Thursday"",
        //                                           ""Friday"",""Saturday"",""Sunday"" From ""RouteScheduleDaywise""Where ""RouteScheduleUID"" =@UID");
        //        Type routeScheduleDayWiseType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleDaywise>().GetType();
        //        List<Model.Interfaces.IRouteScheduleDaywise> routeScheduleDayWiseList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleDaywise>(routeScheduleDayWiseSQL.ToString(), Parameters, routeScheduleDayWiseType);

        //        var routeScheduleFortnightSQL = new StringBuilder(@"SELECT 
        //                                           ""Id"",""UID"",""CreatedBy"",""CreatedTime"",""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",
        //                                           ""SS"",""CompanyUID"",""RouteScheduleUID"",""Monday"",""Tuesday"",""Wednesday"",""Thursday"",""Friday"",""Saturday"",
        //                                           ""Sunday"",""MondayFN"",""TuesdayFN"",""WednesdayFN"",""ThursdayFN"",""FridayFN"",""SaturdayFN"",""SundayFN"" FROM 
        //                                           ""RouteScheduleFortnight"" WHERE ""RouteScheduleUID""=@UID");
        //        Type routeScheduleFortnightType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleFortnight>().GetType();
        //        List<Model.Interfaces.IRouteScheduleFortnight> routeScheduleFortnightList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleFortnight>(routeScheduleFortnightSQL.ToString(), Parameters, routeScheduleFortnightType);

        //        var routeCustomerSQL = new StringBuilder(@"SELECT
        //                                           ""Id"", ""UID"", ""CreatedBy"", ""CreatedTime"", ""ModifiedBy"", ""ModifiedTime"", 
        //                                           ""ServerAddTime"", ""ServerModifiedTime"", ""RouteUID"", ""StoreUID"", ""SeqNo"", 
        //                                           ""VisitTime"", ""VisitDuration"", ""EndTime"", ""IsDeleted""
        //                                           FROM ""RouteCustomer"" WHERE ""RouteUID""=@UID");
        //        Type routeCustomerType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteCustomer>().GetType();
        //        List<Model.Interfaces.IRouteCustomer> routeCustomerList = await ExecuteQueryAsync<Model.Interfaces.IRouteCustomer>(routeCustomerSQL.ToString(), Parameters, routeCustomerType);
        //        return (routeList, routeSheduleList, routeScheduleDayWiseList, routeScheduleFortnightList, routeCustomerList);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<int> CreateDMSPromotionByJsonData(List<string> applicablePromotions)
        {
            int rowsAffected = 0;
            try
            {
                Dictionary<string, DmsPromotion> dmsPromotion = new Dictionary<string, DmsPromotion>();
                string commaSeparatedPromotion = String.Join(",", applicablePromotions);
                dmsPromotion = await CreateDMSPromotionByPromoUID(commaSeparatedPromotion);
                //string jsonData = JsonConvert.SerializeObject(dmsPromotion);
                //var Query = @"INSERT INTO promotion_data (uid, promotion_uid, data, created_time, modified_time, server_add_time, server_modified_time) 
                //VALUES (@UID, @PromotionUID, @Data::json, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                //var Parameters = new Dictionary<string, object>
                //{
                //    { "@UID", Guid.NewGuid() },
                //    { "@PromotionUID", dmsPromotion.UID },
                //    { "@Data", jsonData },
                //    { "@CreatedTime", DateTime.Now },
                //    { "@ModifiedTime", DateTime.Now },
                //    { "@ServerAddTime", DateTime.Now },
                //    { "@ServerModifiedTime", DateTime.Now },
                //};

                //return await ExecuteNonQueryAsync(Query, Parameters);
                foreach (var promo in dmsPromotion)
                {
                    string promoUID = promo.Key;
                    IEnumerable<Winit.Modules.Promotion.Model.Classes.DmsPromotion> existingDmsPromotion = await CreateDMSPromotionByPromotionUID(promoUID);
                    DmsPromotion promotion = promo.Value;
                    string jsonData = JsonConvert.SerializeObject(promotion);
                    string query;

                    var parameters = new Dictionary<string, object>
                    {
                        { "@UID", Guid.NewGuid() },
                        { "@PromotionUID", promoUID },
                        { "@Data", jsonData },
                        { "@CreatedTime", DateTime.Now },
                        { "@ModifiedTime", DateTime.Now },
                        { "@ServerAddTime", DateTime.Now },
                        { "@ServerModifiedTime", DateTime.Now },
                    };

                    if (existingDmsPromotion.Any())
                    {
                        query = @"UPDATE promotion_data
                          SET data = @Data::json, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime
                          WHERE promotion_uid = @PromotionUID;";

                        parameters.Remove("@UID");
                        parameters.Remove("@CreatedTime");
                        parameters.Remove("@ServerAddTime");
                    }

                    else
                    {
                        query = @"INSERT INTO promotion_data (uid, promotion_uid, data, created_time, modified_time, server_add_time, server_modified_time) 
                          VALUES (@UID, @PromotionUID, @Data::json, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                    }

                    rowsAffected += await ExecuteNonQueryAsync(query, parameters);
                }
            }
            catch
            {
                throw;
            }
            return rowsAffected;
        }

        public async Task<int> PopulateItemPromotionMap(string promotionUID)
        {
            try
            {
                var query = $@"
                CREATE TEMP TABLE temppromoitem (
                    promotion_uid VARCHAR(250),
                    item_type VARCHAR(50),
                    item_code VARCHAR(50)
                );

                INSERT INTO temppromoitem (promotion_uid, item_type, item_code)
                SELECT DISTINCT P.uid, POI.item_criteria_type, POI.item_criteria_selected 
                FROM promotion P
                INNER JOIN promo_order PO ON P.UID = PO.promotion_uid AND P.UID = @PromotionUID
                INNER JOIN promo_order_item POI ON PO.UID = POI.promo_order_uid;

                DELETE FROM item_promotion_map WHERE promotion_uid = @PromotionUID;

                INSERT INTO item_promotion_map (uid, sku_type, sku_type_uid, promotion_uid, 
                ss, created_time, modified_time, server_add_time, server_modified_time)
                SELECT uuid_generate_v4(), item_type, item_code, promotion_uid, 0, 
                current_timestamp, current_timestamp, current_timestamp, current_timestamp
                FROM temppromoitem;

                DROP TABLE temppromoitem;";

                var parameters = new Dictionary<string, object>
                {
                    { "@PromotionUID", promotionUID }
                };

                return await ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while populating the item promotion map.", ex);
            }
        }

        public async Task<IEnumerable<Winit.Modules.Promotion.Model.Classes.DmsPromotion>> CreateDMSPromotionByPromotionUID(string PromotionUID)
        {
            List<Winit.Modules.Promotion.Model.Classes.DmsPromotion> dmsPromotions = new List<Winit.Modules.Promotion.Model.Classes.DmsPromotion>();
            try
            {
                var Query = @"SELECT 
                                    id AS Id,
                                    uid AS UID,
                                    created_time AS CreatedTime,
                                    modified_time AS ModifiedTime,
                                    server_add_time AS ServerAddTime,
                                    server_modified_time AS ServerModifiedTime,
                                    promotion_uid AS PromotionUID,
                                    data AS Data
                                FROM 
                                    public.promotion_data
                                 WHERE promotion_uid = @PromotionUID; ";

                var Parameters = new Dictionary<string, object>
        {
            { "@PromotionUID", PromotionUID },
        };

                DataTable dt = await ExecuteQueryDataTableAsync(Query, Parameters);

                foreach (DataRow row in dt.Rows)
                {
                    string jsonData = row["Data"].ToString();

                    Winit.Modules.Promotion.Model.Classes.DmsPromotion promotion = JsonConvert.DeserializeObject<Winit.Modules.Promotion.Model.Classes.DmsPromotion>(jsonData);

                    dmsPromotions.Add(promotion);
                }

                return dmsPromotions;
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<Dictionary<string, DmsPromotion>>> GetPromotion(string applicablePromotioListCommaSeparated, string promotionType)
        //public async Task<List<Dictionary<string, DmsPromotion>>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType)
        public async Task<Dictionary<string, DmsPromotion>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated)
        {
            //List<Dictionary<string, DmsPromotion>> promoDictionary = new List<Dictionary<string, DmsPromotion>>();
            Dictionary<string, DmsPromotion> promoDictionary = new Dictionary<string, DmsPromotion>();
            try
            {
                //List<ItemPromotionMap> itemPromotionMapList = await LoadItemPromotionMap(promotionType, applicablePromotioListCommaSeparated);
                List<Model.Classes.Promotion> promotionList = await LoadPromotion(applicablePromotioListCommaSeparated);
                List<PromoOrder> promoOrderList = await LoadPromoOrder(applicablePromotioListCommaSeparated);
                List<PromoOrderItem> promoOrderItemList = await LoadPromoOrderItem(applicablePromotioListCommaSeparated);
                List<PromoOffer> promoOfferList = await LoadPromoOffer(applicablePromotioListCommaSeparated);
                List<PromoOfferItem> promoOfferItemList = await LoadPromoOfferItem(applicablePromotioListCommaSeparated);
                List<PromoCondition> promoConditionList = await LoadPromoCondition(promoOrderList, promoOrderItemList, promoOfferList, promoOfferItemList);
                if (promotionList != null)
                {
                    foreach (Model.Classes.Promotion objp in promotionList)
                    {
                        DmsPromotion objPromotion = new DmsPromotion();
                        objPromotion = GetDmsPromotionfromPromotion(objp);
                        if (promoOrderList != null)
                        {
                            foreach (PromoOrder PO in promoOrderList.Where(e => e.PromotionUID == objp.UID))
                            {
                                DmsPromoOrder DPO = GetDmsPromoOrderfromPromoOrder(PO);
                                if (promoConditionList != null)
                                {
                                    DPO.objPromoCondition = GetDmsPromoCondition(promoConditionList.Where(e => e.ReferenceUID == DPO.UID).FirstOrDefault());
                                }
                                if (promoOrderItemList != null)
                                {
                                    foreach (PromoOrderItem POI in promoOrderItemList.Where(e => e.PromoOrderUID == DPO.UID && (e.ParentUID == null || e.ParentUID == "")))
                                    {
                                        DmsPromoOrderItem DPOI = GetDmsPromoOrderItem(POI);
                                        if (promoConditionList != null && POI.IsCompulsory == true)
                                        {
                                            DPOI.objPromoCondition = GetDmsPromoCondition(promoConditionList.Where(e => e.ReferenceUID == POI.UID).FirstOrDefault());
                                        }
                                        DPO.PromoOrderItems.Add(DPOI);
                                        /*
                                        if (promoOrderItemList != null)
                                        {
                                            foreach (PromoOrderItem CPOI in promoOrderItemList.Where(e => e.PromoOrderUID == DPO.UID && e.ParentUID == POI.UID))
                                            {
                                                DmsPromoOrderItem CDPOI = GetDmsPromoOrderItem(CPOI);
                                                if (promoConditionList != null && POI.IsCompulsory == true)
                                                {
                                                    CDPOI.objPromoCondition = GetDmsPromoCondition(promoConditionList.Where(e => e.ReferenceUID == CDPOI.UID).FirstOrDefault());
                                                }
                                                DPOI.lstPromoOrderItem.Add(CDPOI);
                                            }
                                        }
                                        */
                                    }
                                }
                                if (promoOfferList != null)
                                {
                                    foreach (PromoOffer POF in promoOfferList.Where(e => e.PromoOrderUID == PO.UID))
                                    {
                                        DmsPromoOffer DPOF = GetDmsPromoOffer(POF);
                                        if (promoConditionList != null)
                                        {
                                            DPOF.objPromoCondition = GetDmsPromoCondition(promoConditionList.Where(e => e.ReferenceUID == DPOF.UID).FirstOrDefault());
                                        }
                                        if (promoOfferItemList != null)
                                        {
                                            foreach (var POFI in promoOfferItemList.Where(e => e.PromoOfferUID == POF.UID))
                                            {
                                                DmsPromoOfferItem DPOFI = GetDmsPromoOfferItem(POFI);
                                                if (promoConditionList != null)
                                                {
                                                    DPOFI.objPromoCondition = GetDmsPromoCondition(promoConditionList.Where(e => e.ReferenceUID == DPOFI.UID).FirstOrDefault());
                                                }
                                                DPOF.PromoOfferItems.Add(DPOFI);
                                            }
                                        }
                                        DPO.PromoOffers.Add(DPOF);
                                    }
                                }
                                objPromotion.PromoOrders.Add(DPO);
                            }
                        }
                        //Dictionary<string, DmsPromotion> promoItem = new Dictionary<string, DmsPromotion>
                        //{
                        //    { objPromotion.UID, objPromotion }
                        //};

                        //promoDictionary.Add(promoItem);
                        promoDictionary[objPromotion.UID] = objPromotion;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return promoDictionary;
        }



        public async Task<List<Model.Classes.Promotion>> LoadPromotion(string itemApplicablePrmotionUIDList)
        {
            List<Model.Classes.Promotion> promotionList = new List<Model.Classes.Promotion>();
            string query = string.Empty;
            /*string[] promotionUIDs = itemApplicablePrmotionUIDList.Split(',');
            string joinedPromotionUIDs = string.Join(",", promotionUIDs.Select(uid => $"'{uid.Trim()}'"));*/


            query = @"SELECT id AS Id,
                            uid AS Uid,company_uid AS CompanyUid,org_uid AS OrgUid,code AS Code,name AS Name,remarks AS Remarks,category AS Category,has_slabs AS HasSlabs,created_by_emp_uid AS CreatedByEmpUid,
                            valid_from AS ValidFrom,valid_upto AS ValidUpto,type AS Type,promo_format AS PromoFormat,is_active AS IsActive,promo_title AS PromoTitle,promo_message AS PromoMessage,
                            has_fact_sheet AS HasFactSheet,priority AS Priority,ss AS Ss,created_time AS CreatedTime,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime
                            FROM promotion
                            WHERE
                                uid = ANY(string_to_array(@PromotionUID, ','))
                            ORDER BY id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", itemApplicablePrmotionUIDList }
            };

            try
            {

                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
                foreach (DataRow row in dt.Rows)
                {
                    Model.Classes.Promotion objPromotion = new Model.Classes.Promotion();
                    objPromotion.UID = Convert.ToString(row["UID"]);
                    objPromotion.CompanyUID = Convert.ToString(row["CompanyUID"]);
                    objPromotion.OrgUID = Convert.ToString(row["OrgUID"]);
                    objPromotion.Code = Convert.ToString(row["Code"]);
                    objPromotion.Name = Convert.ToString(row["Name"]);
                    objPromotion.Remarks = Convert.ToString(row["Remarks"]);
                    objPromotion.Category = Convert.ToString(row["Category"]);
                    objPromotion.HasSlabs = Convert.ToBoolean(row["HasSlabs"]);
                    objPromotion.CreatedByEmpUID = Convert.ToString(row["CreatedByEmpUID"]);
                    objPromotion.ValidFrom = Convert.ToDateTime(row["ValidFrom"]);
                    objPromotion.ValidUpto = row["ValidUpto"] != DBNull.Value ? Convert.ToDateTime(row["ValidUpto"]) : null;
                    objPromotion.Type = Convert.ToString(row["Type"]);
                    objPromotion.PromoFormat = Convert.ToString(row["PromoFormat"]);
                    objPromotion.IsActive = Convert.ToBoolean(row["IsActive"]);
                    objPromotion.PromoTitle = Convert.ToString(row["PromoTitle"]);
                    objPromotion.PromoMessage = Convert.ToString(row["PromoMessage"]);
                    objPromotion.HasFactSheet = Convert.ToBoolean(row["HasFactSheet"]);
                    objPromotion.Priority = Convert.ToInt32(row["Priority"]);

                    promotionList.Add(objPromotion);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return promotionList;
        }

        public async Task<List<PromoOrder>> LoadPromoOrder(string itemApplicablePrmotionUIDList)
        {
            List<PromoOrder> promoOrderList = new List<PromoOrder>();
            string query = string.Empty;
            /*string[] promotionUIDs = itemApplicablePrmotionUIDList.Split(',');
            string joinedPromotionUIDs = string.Join(",", promotionUIDs.Select(uid => $"'{uid.Trim()}'"));*/
            query = @"SELECT po.id AS Id,
                            po.uid AS Uid,
                            po.promotion_uid AS PromotionUid,
                            po.selection_model AS SelectionModel,
                            po.qualification_level AS QualificationLevel,
                            po.min_deal_count AS MinDealCount,
                            po.max_deal_count AS MaxDealCount,
                            po.ss AS Ss,
                            po.created_time AS CreatedTime,
                            po.modified_time AS ModifiedTime,
                            po.server_add_time AS ServerAddTime,
                            po.server_modified_time AS ServerModifiedTime
                    FROM promo_order po
                    INNER JOIN promotion p ON p.uid = po.promotion_uid
                    WHERE p.uid = ANY(string_to_array(@PromotionUID, ','))
                    ORDER BY po.id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", itemApplicablePrmotionUIDList }
            };
            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                PromoOrder objPromoOrder = new PromoOrder();
                objPromoOrder.Id = Convert.ToInt32(row["Id"]);
                objPromoOrder.UID = Convert.ToString(row["UID"]);
                objPromoOrder.PromotionUID = Convert.ToString(row["PromotionUID"]);
                objPromoOrder.SelectionModel = Convert.ToString(row["SelectionModel"]);
                objPromoOrder.QualificationLevel = Convert.ToString(row["QualificationLevel"]);
                objPromoOrder.MinDealCount = Convert.ToInt32(row["MinDealCount"]);
                objPromoOrder.MaxDealCount = Convert.ToInt32(row["MaxDealCount"]);
                promoOrderList.Add(objPromoOrder);
            }

            return promoOrderList;
        }

        public async Task<List<PromoOrderItem>> LoadPromoOrderItem(string itemApplicablePrmotionUIDList)
        {
            List<PromoOrderItem> promoOrderItemList = new List<PromoOrderItem>();
            string query = string.Empty;
            /*string[] promotionUIDs = itemApplicablePrmotionUIDList.Split(',');
            string joinedPromotionUIDs = string.Join(",", promotionUIDs.Select(uid => $"'{uid.Trim()}'"));*/
            query = @"SELECT poi.id AS Id,
                               poi.uid AS Uid,
                               poi.promo_order_uid AS PromoOrderUid,
                               poi.parent_uid AS ParentUid,
                               poi.item_criteria_type AS ItemCriteriaType,
                               poi.item_criteria_selected AS ItemCriteriaSelected,
                               poi.is_compulsory AS IsCompulsory,
                               poi.item_uom AS ItemUom,
                               poi.promo_split AS PromoSplit,
                               poi.ss AS Ss,
                               poi.created_time AS CreatedTime,
                               poi.modified_time AS ModifiedTime,
                               poi.server_add_time AS ServerAddTime,
                               poi.server_modified_time AS ServerModifiedTime,
                               poi.promotion_uid AS PromotionUid
                        FROM promo_order_item poi
                        INNER JOIN promo_order po ON po.uid = poi.promo_order_uid
                        INNER JOIN promotion p ON p.uid = po.promotion_uid
                        WHERE p.uid = ANY(string_to_array(@PromotionUID, ','))
                        ORDER BY poi.id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", itemApplicablePrmotionUIDList }
            };
            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                PromoOrderItem objPromoOrderItem = new PromoOrderItem();
                objPromoOrderItem.Id = Convert.ToInt32(row["Id"]);
                objPromoOrderItem.UID = Convert.ToString(row["UID"]);
                objPromoOrderItem.PromoOrderUID = Convert.ToString(row["PromoOrderUID"]);
                objPromoOrderItem.ParentUID = Convert.ToString(row["ParentUID"]);
                objPromoOrderItem.ItemCriteriaType = Convert.ToString(row["ItemCriteriaType"]);
                objPromoOrderItem.ItemCriteriaSelected = Convert.ToString(row["ItemCriteriaSelected"]);
                objPromoOrderItem.IsCompulsory = Convert.ToBoolean(row["IsCompulsory"]);
                objPromoOrderItem.ItemUOM = Convert.ToString(row["ItemUOM"]);
                objPromoOrderItem.PromoSplit = Convert.ToDecimal(row["PromoSplit"]);
                promoOrderItemList.Add(objPromoOrderItem);
            }
            return promoOrderItemList;
        }

        public async Task<List<PromoOffer>> LoadPromoOffer(string itemApplicablePrmotionUIDList)
        {
            List<PromoOffer> promoOfferList = new List<PromoOffer>();
            string query = string.Empty;
            /*string[] promotionUIDs = itemApplicablePrmotionUIDList.Split(',');
            string joinedPromotionUIDs = string.Join(",", promotionUIDs.Select(uid => $"'{uid.Trim()}'"));*/
            query = @"SELECT pof.id AS Id,
                            pof.uid AS UID,
                            pof.promo_order_uid AS PromoOrderUID,
                            pof.type AS Type,
                            pof.qualification_level AS QualificationLevel,
                            pof.application_level AS ApplicationLevel,
                            pof.selection_model AS SelectionModel,
                            pof.has_offer_item_selection AS HasOfferItemSelection,
                            pof.ss AS Ss,
                            pof.created_time AS CreatedTime,
                            pof.modified_time AS ModifiedTime,
                            pof.server_add_time AS ServerAddTime,
                            pof.server_modified_time AS ServerModifiedTime,
                            pof.promotion_uid AS PromotionUID
                    FROM promo_offer pof
                    INNER JOIN promo_order po ON po.uid = pof.promo_order_uid
                    INNER JOIN promotion p ON p.uid = po.promotion_uid
                    WHERE p.uid = ANY(string_to_array(@PromotionUID, ','))
                    ORDER BY pof.id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", itemApplicablePrmotionUIDList }
            };
            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                PromoOffer objPromoOffer = new PromoOffer();
                objPromoOffer.Id = Convert.ToInt32(row["Id"]);
                objPromoOffer.UID = Convert.ToString(row["UID"]);
                objPromoOffer.PromoOrderUID = Convert.ToString(row["PromoOrderUID"]);
                objPromoOffer.Type = Convert.ToString(row["Type"]);
                objPromoOffer.QualificationLevel = Convert.ToString(row["QualificationLevel"]);
                objPromoOffer.ApplicationLevel = Convert.ToString(row["ApplicationLevel"]);
                objPromoOffer.SelectionModel = Convert.ToString(row["SelectionModel"]);
                objPromoOffer.HasOfferItemSelection = Convert.ToBoolean(row["HasOfferItemSelection"]);
                promoOfferList.Add(objPromoOffer);
            }

            return promoOfferList;
        }

        public async Task<List<PromoOfferItem>> LoadPromoOfferItem(string itemApplicablePrmotionUIDList)
        {
            List<PromoOfferItem> promoOfferItemList = new List<PromoOfferItem>();
            string query = string.Empty;
            /*string[] promotionUIDs = itemApplicablePrmotionUIDList.Split(',');
            string joinedPromotionUIDs = string.Join(",", promotionUIDs.Select(uid => $"'{uid.Trim()}'"));*/
            query = @"SELECT pofi.id AS Id,
        pofi.uid AS UID,
        pofi.promo_offer_uid AS PromoOfferUID,
        pofi.item_criteria_type AS ItemCriteriaType,
        pofi.item_criteria_selected AS ItemCriteriaSelected,
        pofi.is_compulsory AS IsCompulsory,
        pofi.item_uom AS ItemUOM,
        pofi.ss AS SS,
        pofi.created_time AS CreatedTime,
        pofi.modified_time AS ModifiedTime,
        pofi.server_add_time AS ServerAddTime,
        pofi.server_modified_time AS ServerModifiedTime,
        pofi.promotion_uid AS PromotionUID
FROM promo_offer_item pofi
INNER JOIN promo_offer pof ON pof.uid = pofi.promo_offer_uid
INNER JOIN promo_order po ON po.uid = pof.promo_order_uid
INNER JOIN promotion p ON p.uid = po.promotion_uid
WHERE p.uid = ANY(string_to_array(@PromotionUID, ','))
ORDER BY pofi.id;";

            var parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", itemApplicablePrmotionUIDList },
            };

            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                PromoOfferItem objPromoOfferItem = new PromoOfferItem();
                objPromoOfferItem.Id = Convert.ToInt32(row["Id"]);
                objPromoOfferItem.UID = Convert.ToString(row["UID"]);
                objPromoOfferItem.PromoOfferUID = Convert.ToString(row["PromoOfferUID"]);
                objPromoOfferItem.ItemCriteriaType = Convert.ToString(row["ItemCriteriaType"]);
                objPromoOfferItem.ItemCriteriaSelected = Convert.ToString(row["ItemCriteriaSelected"]);
                objPromoOfferItem.IsCompulsory = Convert.ToBoolean(row["IsCompulsory"]);
                objPromoOfferItem.ItemUOM = Convert.ToString(row["ItemUOM"]);

                promoOfferItemList.Add(objPromoOfferItem);
            }
            return promoOfferItemList;
        }

        public async Task<List<PromoCondition>> LoadPromoCondition(List<PromoOrder> promoOrderList, List<PromoOrderItem> promoOrderItemList,
            List<PromoOffer> promoOfferList, List<PromoOfferItem> promoOfferItemList)
        {
            List<PromoCondition> promoConditionList = new List<PromoCondition>();
            List<string> promoOrderListStr = promoOrderList?.Select(code => code.UID.Trim()).ToList();
            List<string> promoOrderItemListStr = promoOrderItemList?.Select(code => code.UID.Trim()).ToList();
            List<string> promoOfferListStr = promoOfferList?.Select(code => code.UID.Trim()).ToList();
            List<string> promoOfferItemListStr = promoOfferItemList?.Select(code => code.UID.Trim()).ToList();
            string commaSeparatedpromoOrderList = string.Join(",", promoOrderListStr);
            string commaSeparatedpromoOrderItemList = string.Join(",", promoOrderItemListStr);
            string commaSeparatedpromoOfferList = string.Join(",", promoOfferListStr);
            string commaSeparatedpromoOfferItemList = string.Join(",", promoOfferItemListStr);
            string query = string.Empty;
            query = @"SELECT pc.id AS Id,
                           uid AS UID,
                           reference_type AS ReferenceType,
                           reference_uid AS ReferenceUID,
                           condition_type AS ConditionType,
                           min AS Min,
                           max AS Max,
                           max_deal_count AS MaxDealCount,
                           uom AS UOM,
                           all_uom_conversion AS AllUOMConversion,
                           value_type AS ValueType,
                           is_prorated AS IsProrated,
                           ss AS SS,
                           discount_type AS DiscountType,
                           discount_percentage AS DiscountPercentage,
                           discount_amount AS DiscountAmount,
                           free_quantity AS FreeQuantity,
                           created_time AS CreatedTime,
                           modified_time AS ModifiedTime,
                           server_add_time AS ServerAddTime,
                           server_modified_time AS ServerModifiedTime,
                           promotion_uid AS PromotionUID
                    FROM promo_condition pc
                    WHERE (@PromoOrderList IS NULL OR (pc.reference_type = 'PromoOrder' AND pc.reference_uid = ANY(string_to_array(@PromoOrderList, ','))))
                    OR (@PromoOrderItemList IS NULL OR (pc.reference_type = 'PromoOrderItem' AND pc.reference_uid = ANY(string_to_array(@PromoOrderItemList, ','))))
                    OR (@PromoOfferList IS NULL OR (pc.reference_type = 'PromoOffer' AND pc.reference_uid = ANY(string_to_array(@PromoOfferList, ','))))
                    OR (@PromoOfferItemList IS NULL OR (pc.reference_type = 'PromoOfferItem' AND pc.reference_uid = ANY(string_to_array(@PromoOfferItemList, ','))));";

            var parameters = new Dictionary<string, object>
            {
                { "@PromoOrderList", commaSeparatedpromoOrderList },
                { "@PromoOrderItemList", commaSeparatedpromoOrderItemList },
                { "@PromoOfferList", commaSeparatedpromoOfferList },
                { "@PromoOfferItemList", commaSeparatedpromoOfferItemList },
            };
            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                PromoCondition objPromoCondition = new PromoCondition();
                objPromoCondition.Id = Convert.ToInt32(row["Id"]);
                objPromoCondition.UID = Convert.ToString(row["UID"]);
                objPromoCondition.ReferenceType = Convert.ToString(row["ReferenceType"]);
                objPromoCondition.ReferenceUID = Convert.ToString(row["ReferenceUID"]);
                objPromoCondition.ConditionType = Convert.ToString(row["ConditionType"]);
                objPromoCondition.Min = Convert.ToInt32(row["Min"]);
                objPromoCondition.Max = Convert.ToInt32(row["Max"]);
                objPromoCondition.MaxDealCount = Convert.ToInt32(row["MaxDealCount"]);
                objPromoCondition.UOM = Convert.ToString(row["UOM"]);
                objPromoCondition.AllUOMConversion = Convert.ToBoolean(row["AllUOMConversion"]);
                objPromoCondition.ValueType = Convert.ToString(row["ValueType"]);
                objPromoCondition.IsProrated = Convert.ToBoolean(row["IsProrated"]);

                promoConditionList.Add(objPromoCondition);
            }
            return promoConditionList;
        }

        /*public async Task<string> LoadPriority(string name)
        {
            string priority = string.Empty;
            string query = string.Empty;
            query = @"SELECT S.* FROM ""Setting"" S
              WHERE ('' = '' OR S.""Name"" = ANY(string_to_array(@Name, ',')))";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", name },
            };

            DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                priority = Convert.ToString(row["Value"]);
                break;
            }
            return priority;
        }*/

        public enum PriorityPreference
        {
            Highest,
            Lowest,
            All
        }

        //public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView, 
        //    Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
        //{
        //    List<AppliedPromotionView> appliedPromotions = new List<AppliedPromotionView>();
        //    List<PromotionItemView> appliedPromotionItems = new List<PromotionItemView>();

        //    if (!string.IsNullOrEmpty(applicablePromotionUIDs))
        //    {
        //        string[] promotionUIDs = applicablePromotionUIDs.Split(',');
        //        foreach (var uid in promotionUIDs)
        //        {
        //            if (promoDictionary.ContainsKey(uid))
        //            {
        //                DmsPromotion dmsPromotion = promoDictionary[uid];
        //                if (dmsPromotion.Type == "Invoice")
        //                {
        //                    var invoicePromotions = ApplyInvoicePromotions(dmsPromotion, promoHeaderView);
        //                    appliedPromotions.AddRange(invoicePromotions.Select(invoicePromotion => new AppliedPromotionView
        //                    {
        //                        PromotionUID = dmsPromotion.UID,
        //                        Priority = dmsPromotion.Priority,
        //                        IsFOC = false,
        //                        DiscountAmount = invoicePromotion.DiscountAmount,
        //                        FOCItems = null
        //                    }));
        //                }

        //                DmsPromoOrder bestFitPromoOrder = null; // Initialize bestFitPromoOrder
        //                decimal bestFitMin = 0;
        //                foreach (var promoOrder in dmsPromotion.PromoOrders)
        //                {
        //                    bool match = false;
        //                    List<PromotionItemView> eligibleItems = new List<PromotionItemView>();
        //                    int totalQuantities = 0;

        //                    var promoOrderItemSKUUIDs = promoOrder.PromoOrderItems.Select(poi => poi.ItemCriteriaSelected).ToList();

        //                    match = promoOrder.SelectionModel == "Any"
        //                        ? promoOrder.PromoOrderItems.Any(poi =>
        //                            promoHeaderView.promotionItemView.Any(piv => IsMatchWithAttribute(poi, piv)))
        //                        : promoOrder.PromoOrderItems.All(poi =>
        //                            promoHeaderView.promotionItemView.Any(piv => poi.ItemCriteriaSelected == piv.SKUUID));

        //                    if (match)
        //                    {
        //                        eligibleItems = promoOrder.SelectionModel == "Any"
        //                            ? promoHeaderView.promotionItemView
        //                                .Where(piv => promoOrder.PromoOrderItems.Any(poi => IsMatchWithAttribute(poi, piv)))
        //                                .ToList()
        //                            : promoHeaderView.promotionItemView
        //                                .Where(piv => promoOrderItemSKUUIDs.Contains(piv.SKUUID))
        //                                .ToList();

        //                        totalQuantities = eligibleItems.Sum(item => item.Qty);
        //                    }

        //                    if (match)
        //                    {
        //                        decimal minQuantity = promoOrder.objPromoCondition?.Min ?? 0.0m;

        //                        if (promoOrder.SelectionModel == "All")
        //                        {
        //                            bool allItemsMeetMinQuantity = eligibleItems.All(eligibleItem =>
        //                                promoOrder.PromoOrderItems.Any(poi =>
        //                                    poi.ItemCriteriaSelected == eligibleItem.SKUUID &&
        //                                    eligibleItem.Qty >= (poi.objPromoCondition?.Min ?? 0.0m))
        //                            );

        //                            if (!allItemsMeetMinQuantity)
        //                            {
        //                                match = false;
        //                            }
        //                            else
        //                            {
        //                                bestFitPromoOrder = promoOrder;
        //                            }
        //                        }
        //                        if (promoOrder.SelectionModel == "Any")
        //                        {
        //                            bool allItemsMeetMinQuantity = eligibleItems.All(eligibleItem =>
        //                            {
        //                                bool itemMeetsMinQuantity = promoOrder.PromoOrderItems.Any(poi =>
        //                                {
        //                                    bool isCompulsory = poi.IsCompulsory ?? false;

        //                                    return poi.ItemCriteriaSelected == eligibleItem.SKUUID &&
        //                                           (!isCompulsory || eligibleItem.Qty >= (poi.objPromoCondition?.Min ?? 0.0m));
        //                                });

        //                                return itemMeetsMinQuantity;
        //                            });
        //                            if (!allItemsMeetMinQuantity)
        //                            {
        //                                match = false;
        //                            }
        //                        }

        //                        if (match && totalQuantities >= minQuantity)
        //                        {
        //                            if (minQuantity > bestFitMin)
        //                            {
        //                                bestFitMin = minQuantity;
        //                                bestFitPromoOrder = promoOrder;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (bestFitPromoOrder != null)
        //                {

        //                    if (bestFitPromoOrder.SelectionModel == "Any")
        //                    {
        //                        appliedPromotionItems = CheckPromoConditionForPromoOrder("Any", bestFitPromoOrder, promoHeaderView);
        //                    }
        //                    else if (bestFitPromoOrder.SelectionModel == "All")
        //                    {
        //                        appliedPromotionItems = CheckPromoConditionForPromoOrder("All", bestFitPromoOrder, promoHeaderView);
        //                    }
        //                    bool isFOC = dmsPromotion.PromoFormat == "BQXF" || dmsPromotion.PromoFormat == "IQXF";
        //                    List<FOCItem> focItems = new List<FOCItem>();
        //                    foreach (var item in appliedPromotionItems)
        //                    {
        //                        if (item.ItemType == "FOC")
        //                        {
        //                            FOCItem focItem = new FOCItem
        //                            {
        //                                ItemCode = item.SKUUID,
        //                                UOM = item.UOM,
        //                                Qty = item.Qty
        //                            };
        //                            focItems.Add(focItem);
        //                        }
        //                    }
        //                    AppliedPromotionView appliedPromotionObj = null;
        //                    int occurence = 1;
        //                    foreach (var item in appliedPromotionItems)
        //                    {
        //                        /*int firstOcc = 0;
        //                        if (item.ItemType == "FOC")
        //                        {
        //                            firstOcc += 1;
        //                        }
        //                        if (firstOcc < 2)
        //                        {*/

        //                        appliedPromotionObj = new AppliedPromotionView();
        //                        /*{
        //                            PromotionUID = dmsPromotion.UID,
        //                            Priority = dmsPromotion.Priority,
        //                            IsFOC = isFOC,
        //                            UniqueUID = item.SKUUID,
        //                            DiscountAmount = item.TotalDiscount,
        //                            //FOCItems = item.ItemType == "FOC" ? focItems : null
        //                            //FOCItems = occurence == appliedPromotionItems.length &&  ? focItems
        //                        };*/
        //                        appliedPromotionObj.PromotionUID = dmsPromotion.UID;
        //                        appliedPromotionObj.Priority = dmsPromotion.Priority;
        //                        appliedPromotionObj.IsFOC = item.ItemType == "FOC";
        //                        appliedPromotionObj.UniqueUID = item.SKUUID;

        //                        if (occurence == appliedPromotionItems.Count)
        //                        {
        //                            appliedPromotionObj.FOCItems = focItems;
        //                        }
        //                        appliedPromotions.Add(appliedPromotionObj);
        //                        occurence++;
        //                        /*}
        //                        else
        //                        {
        //                            break;
        //                        }*/
        //                    }
        //                }
        //                if (appliedPromotions.Count > 0)
        //                {
        //                    switch (promotionPriority)
        //                    {
        //                        case PromotionPriority.MaxPriority:
        //                            appliedPromotions = appliedPromotions.OrderByDescending(ap => ap.Priority).Take(1).ToList();
        //                            break;

        //                        case PromotionPriority.MinPriority:
        //                            appliedPromotions = appliedPromotions.OrderBy(ap => ap.Priority).Take(1).ToList();
        //                            break;

        //                        default:
        //                            break;
        //                    }

        //                    return appliedPromotions;
        //                }
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }
        //    }
        //    return appliedPromotions;
        //}

        //private bool IsMatchWithAttribute(DmsPromoOrderItem poi, PromotionItemView piv)
        //{
        //    if (IsAttributeType(poi.ItemCriteriaType))
        //    {
        //        string attributeCode = GetAttributeCodeValue(piv, poi.ItemCriteriaType);
        //        return attributeCode == poi.ItemCriteriaSelected;
        //    }
        //    else
        //    {
        //        return poi.ItemCriteriaSelected == piv.SKUUID;
        //    }
        //}

        //private bool IsAttributeType(string itemCriteriaType)
        //{
        //    return new List<string> { "Brand", "Category", "SubCategory", "Product Group" }.Contains(itemCriteriaType);
        //}

        //private string GetAttributeCodeValue(PromotionItemView piv, string itemCriteriaType)
        //{
        //    if (piv.Attributes != null && piv.Attributes.ContainsKey(itemCriteriaType))
        //    {
        //        return piv.Attributes[itemCriteriaType].Code;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        //private List<PromotionItemView> CheckPromoConditionForPromoOrder(String selectionModel, DmsPromoOrder promoOrder, PromotionHeaderView promoHeaderView)
        //{
        //    PromotionItemView objPromotionItemView = new PromotionItemView();
        //    List<PromotionItemView> lstPromotionItemView = new List<PromotionItemView>();
        //    List<PromotionItemView> lstPromotionItemViewFOC = new List<PromotionItemView>();
        //    if (selectionModel == "Any")
        //    {
        //        if (promoOrder.objPromoCondition != null)
        //        {
        //            decimal minQuantity = promoOrder.objPromoCondition.Min.HasValue ? promoOrder.objPromoCondition.Min.Value : 0.0m;
        //            decimal totalQuantity = 0.0m;
        //            List<PromotionItemView> eligibleItems = new List<PromotionItemView>();

        //            foreach (var promoOrderItem in promoOrder.PromoOrderItems)
        //            {
        //                var correspondingPromotionItemView = promoHeaderView.promotionItemView
        //                    .FirstOrDefault(piv => IsMatchWithAttribute(promoOrderItem, piv));

        //                if (correspondingPromotionItemView != null)
        //                {
        //                    totalQuantity += correspondingPromotionItemView.Qty;
        //                    eligibleItems.Add(correspondingPromotionItemView);
        //                }
        //            }

        //            if (eligibleItems.Any() && promoOrder.PromoOffers != null && totalQuantity >= minQuantity)
        //            {
        //                decimal itemMultiplier = totalQuantity / minQuantity;
        //                if (itemMultiplier >= 1)
        //                {
        //                    int numberOfDeals = Math.Min((int)itemMultiplier, promoOrder.MaxDealCount ?? int.MaxValue);
        //                    foreach (var promoOffer in promoOrder.PromoOffers)
        //                    {
        //                        if (promoOffer.objPromoCondition != null)
        //                        {
        //                            if (promoOffer.objPromoCondition.ConditionType != "FOC")
        //                            {
        //                                var offerCondition = promoOffer.objPromoCondition;

        //                                if (offerCondition.ReferenceUID == promoOffer.UID)
        //                                {
        //                                    foreach (var eligibleItem in eligibleItems)
        //                                    {
        //                                        objPromotionItemView = ApplyPromotionOfferToItem(offerCondition, eligibleItem, numberOfDeals);
        //                                        lstPromotionItemView.Add(objPromotionItemView);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var offerCondition = promoOffer.objPromoCondition;

        //                                if (offerCondition.ReferenceUID == promoOffer.UID)
        //                                {
        //                                    lstPromotionItemViewFOC = ApplyFOCPromotionOfferToItem(promoOffer, promoHeaderView);
        //                                    lstPromotionItemView.AddRange(lstPromotionItemViewFOC.Where(poi => poi.ItemType == "FOC"));
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    else if (selectionModel == "All")
        //    {
        //        bool allItemsMeetMinQuantity = true;
        //        int minMultiplier = int.MaxValue;

        //        foreach (var promoOrderItem in promoOrder.PromoOrderItems)
        //        {
        //            if (promoOrderItem.objPromoCondition != null)
        //            {
        //                decimal minQuantity = promoOrderItem.objPromoCondition.Min.HasValue
        //                    ? promoOrderItem.objPromoCondition.Min.Value
        //                    : 0.0m;

        //                var correspondingPromotionItemView = promoHeaderView.promotionItemView
        //                    .FirstOrDefault(piv => piv.SKUUID == promoOrderItem.ItemCriteriaSelected);

        //                if (correspondingPromotionItemView != null)
        //                {
        //                    decimal itemMultiplier = correspondingPromotionItemView.TotalQuantity / minQuantity;

        //                    if (itemMultiplier < minMultiplier)
        //                    {
        //                        minMultiplier = (int)itemMultiplier;
        //                    }

        //                    if (itemMultiplier < 1.0m)
        //                    {
        //                        allItemsMeetMinQuantity = false;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    allItemsMeetMinQuantity = false;
        //                    break;
        //                }
        //            }
        //        }

        //        if (allItemsMeetMinQuantity)
        //        {
        //            int numberOfDeals = Math.Min(minMultiplier, promoOrder.MaxDealCount ?? int.MaxValue);
        //            foreach (var promoOffer in promoOrder.PromoOffers)
        //            {
        //                if (promoOffer.objPromoCondition != null)
        //                {
        //                    if (promoOffer.objPromoCondition.ConditionType != "FOC")
        //                    {
        //                        if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
        //                        {
        //                            foreach (var promoOrderItem in promoOrder.PromoOrderItems)
        //                            {
        //                                var correspondingPromotionItemView = promoHeaderView.promotionItemView
        //                                    .FirstOrDefault(piv => piv.SKUUID == promoOrderItem.ItemCriteriaSelected);

        //                                objPromotionItemView = ApplyPromotionOfferToItem(promoOffer.objPromoCondition, correspondingPromotionItemView, numberOfDeals);
        //                                lstPromotionItemView.Add(objPromotionItemView);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var offerCondition = promoOffer.objPromoCondition;

        //                        if (offerCondition.ReferenceUID == promoOffer.UID)
        //                        {
        //                            lstPromotionItemViewFOC = ApplyFOCPromotionOfferToItem(promoOffer, promoHeaderView);
        //                            lstPromotionItemView.AddRange(lstPromotionItemViewFOC);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return lstPromotionItemView;
        //}

        //public List<PromotionItemView> ApplyFOCPromotionOfferToItem(DmsPromoOffer promoOffer, PromotionHeaderView promotionHeaderView)
        //{
        //    if (promoOffer != null && promoOffer.PromoOfferItems != null)
        //    {
        //        foreach (var newItem in promoOffer.PromoOfferItems)
        //        {
        //            PromotionItemView newPromotionItem = new PromotionItemView
        //            {
        //                SKUUID = newItem.ItemCriteriaSelected,
        //                UOM = newItem.ItemUOM,
        //                Qty = (int)newItem.objPromoCondition.Min,
        //                ItemType = "FOC"
        //            };

        //            promotionHeaderView.promotionItemView.Add(newPromotionItem);
        //        }
        //    }
        //    return promotionHeaderView.promotionItemView;
        //}

        //public List<AppliedInvoicePromotion> ApplyInvoicePromotions(DmsPromotion promotion, PromotionHeaderView promotionHeaderView)
        //{
        //    var appliedPromotions = new List<AppliedInvoicePromotion>();
        //    PromotionHeaderView objPromotionHeaderView = new PromotionHeaderView();

        //    foreach (var promoOrder in promotion.PromoOrders)
        //    {
        //        switch (promotion.PromoFormat)
        //        {
        //            case "ANYVALUE":
        //                if (promoOrder.PromoOffers != null)
        //                {
        //                    foreach (var promoOffer in promoOrder.PromoOffers)
        //                    {
        //                        if (promoOffer.objPromoCondition == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (promoOffer.objPromoCondition.ConditionType != "FOC")
        //                        {
        //                            if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
        //                            {
        //                                var offerCondition = promoOffer.objPromoCondition;
        //                                objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
        //                            }
        //                        }
        //                        else
        //                        {
        //                        }
        //                    }
        //                }
        //                break;

        //            case "BRANDCOUNT":
        //                break;

        //            case "BYQTY":
        //                decimal? minQuantityRequired = promoOrder.objPromoCondition.Min;

        //                if (promotionHeaderView.TotalQty >= minQuantityRequired)
        //                {
        //                    if (promoOrder.PromoOffers == null)
        //                    {
        //                        continue;
        //                    }
        //                    foreach (var promoOffer in promoOrder.PromoOffers)
        //                    {
        //                        if (promoOffer.objPromoCondition == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (promoOffer.objPromoCondition.ConditionType != "FOC")
        //                        {
        //                            if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
        //                            {
        //                                var offerCondition = promoOffer.objPromoCondition;
        //                                objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
        //                            }
        //                        }
        //                        else
        //                        {
        //                        }
        //                    }
        //                }
        //                break;

        //            case "BYVALUE":

        //                decimal? minValueRequired = promoOrder.objPromoCondition.Min;

        //                if (promotionHeaderView.TotalAmount >= minValueRequired)
        //                {
        //                    if (promoOrder.PromoOffers == null)
        //                    {
        //                        continue;
        //                    }
        //                    foreach (var promoOffer in promoOrder.PromoOffers)
        //                    {
        //                        if (promoOffer.objPromoCondition == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (promoOffer.objPromoCondition.ConditionType != "FOC")
        //                        {
        //                            if (promoOffer.objPromoCondition.ReferenceUID == promoOffer.UID)
        //                            {
        //                                var offerCondition = promoOffer.objPromoCondition;
        //                                objPromotionHeaderView = ApplyPromotionOfferToInvoice(offerCondition, promotionHeaderView);
        //                            }
        //                        }
        //                        else
        //                        {
        //                        }
        //                    }
        //                }
        //                break;

        //            default:

        //                break;

        //        }
        //        var appliedPromotion = new AppliedInvoicePromotion
        //        {
        //            PromotionUID = promotion.UID,
        //            DiscountAmount = objPromotionHeaderView.TotalDiscount
        //        };

        //        appliedPromotions.Add(appliedPromotion);
        //    }

        //    return appliedPromotions;
        //}

        public PromotionHeaderView ApplyPromotionOfferToInvoice(DmsPromoCondition promoCondition, PromotionHeaderView promotionHeaderView)
        {
            if (promoCondition.ConditionType == "Percentage")
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
                    promoItemView.TotalAmount -= discountAmount;
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
                        promoItemView.TotalAmount -= discountAmount;
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
                        promoItemView.TotalAmount -= discountAmount;
                    }
                }
            }

            return promoItemView;
        }

        private DmsPromotion GetDmsPromotionfromPromotion(Model.Classes.Promotion P)
        {

            DmsPromotion o = new DmsPromotion();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.OrgUID = P.OrgUID;
                o.Code = P.Code;
                o.Name = P.Name;
                o.Remarks = P.Remarks;
                o.Category = P.Category;
                o.HasSlabs = P.HasSlabs;
                o.CreatedByEmpUID = P.CreatedByEmpUID;
                o.ValidFrom = P.ValidFrom;
                o.ValidUpto = P.ValidUpto;
                o.Type = P.Type;
                o.PromoFormat = P.PromoFormat;
                o.IsActive = P.IsActive;
                o.Priority = P.Priority;
            }
            return o;
        }

        private DmsPromoOrder GetDmsPromoOrderfromPromoOrder(PromoOrder P)
        {
            DmsPromoOrder o = new DmsPromoOrder();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.PromotionUID = P.PromotionUID;
                o.SelectionModel = P.SelectionModel;
                o.QualificationLevel = P.QualificationLevel;
                o.MinDealCount = P.MinDealCount;
                o.MaxDealCount = P.MaxDealCount;
            }
            return o;
        }

        private DmsPromoCondition GetDmsPromoCondition(PromoCondition P)
        {
            DmsPromoCondition o = new DmsPromoCondition();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.ReferenceType = P.ReferenceType;
                o.ReferenceUID = P.ReferenceUID;
                o.ConditionType = P.ConditionType;
                o.Min = P.Min;
                o.Max = P.Max;
                o.MaxDealCount = P.MaxDealCount;
                o.UOM = P.UOM;
                o.AllUOMConversion = P.AllUOMConversion;
                o.ValueType = P.ValueType;
                o.IsProrated = P.IsProrated;
                //o.ss = P.ss;

            }
            return o;
        }

        private DmsPromoOrderItem GetDmsPromoOrderItem(PromoOrderItem P)
        {
            DmsPromoOrderItem o = new DmsPromoOrderItem();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.PromoOrderUID = P.PromoOrderUID;
                o.ParentUID = P.ParentUID;
                o.ItemCriteriaType = P.ItemCriteriaType;
                o.ItemCriteriaSelected = P.ItemCriteriaSelected;
                o.IsCompulsory = P.IsCompulsory;
                o.ItemUOM = P.ItemUOM;
                o.PromoSplit = P.PromoSplit;
            }
            return o;
        }

        private DmsPromoOffer GetDmsPromoOffer(PromoOffer P)
        {
            DmsPromoOffer o = new DmsPromoOffer();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.PromoOrderUID = P.PromoOrderUID;
                o.Type = P.Type;
                o.QualificationLevel = P.QualificationLevel;
                o.ApplicationLevel = P.ApplicationLevel;
                o.SelectionModel = P.SelectionModel;
                o.HasOfferItemSelection = P.HasOfferItemSelection;
            }
            return o;
        }

        private DmsPromoOfferItem GetDmsPromoOfferItem(PromoOfferItem P)
        {
            DmsPromoOfferItem o = new DmsPromoOfferItem();
            if (P != null)
            {
                o.Id = P.Id;
                o.UID = P.UID;
                o.PromoOfferUID = P.PromoOfferUID;
                o.ItemCriteriaType = P.ItemCriteriaType;
                o.ItemCriteriaSelected = P.ItemCriteriaSelected;
                o.IsCompulsory = P.IsCompulsory;
                o.ItemUOM = P.ItemUOM;
            }
            return o;
        }
        public async Task<Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>?> GetDMSPromotionByPromotionUIDs(List<string> promotionUIDs)
        {
            Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>? dMSPromotionDictionary = null;
            try
            {

                string commaSeperatedUIDs = string.Join(",", promotionUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"PromotionUIDs",  commaSeperatedUIDs}
                };

                var query = @"SELECT * FROM promotion_data WHERE promotion_uid = ANY(string_to_array(@PromotionUIDs, ','))";

                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dMSPromotionDictionary = new Dictionary<string, DmsPromotion>();
                    foreach (DataRow row in dt.Rows)
                    {
                        string jsonData = ToString(row["Data"]);
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            Winit.Modules.Promotion.Model.Classes.DmsPromotion? dmsPromotion = JsonConvert.DeserializeObject<Winit.Modules.Promotion.Model.Classes.DmsPromotion>(jsonData);
                            if (dmsPromotion != null)
                            {
                                dMSPromotionDictionary[dmsPromotion.UID] = dmsPromotion;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return dMSPromotionDictionary;
        }
        public async Task<List<string>?> GetApplicablePromotionUIDs(List<string> orgUIDs)
        {
            List<string>? promotionUIDList = null;
            try
            {
                string commaSeperatedUIDs = string.Join(",", orgUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"OrgUIDs",  commaSeperatedUIDs}
                };

                var query = @"SELECT DISTINCT P.uid AS PromotionUID                   
                             FROM promotion P                        
                             INNER JOIN selection_map_criteria SC ON SC.linked_item_uid = P.uid  
                             AND P.org_uid = ANY(string_to_array(@OrgUIDs, ','))                 
                             AND SC.linked_item_type = 'Promotion'  
                             AND P.valid_from::date <= CURRENT_DATE
                             AND (P.valid_upto IS NULL OR P.valid_upto::date >= CURRENT_DATE)          
                             AND P.is_active = 1                        
                             INNER JOIN selection_map_details SD ON SC.uid = SD.selection_map_criteria_uid                     
                             AND SD.selection_value IS NOT NULL     ";

                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    promotionUIDList = new List<string>();
                    foreach (DataRow row in dt.Rows)
                    {
                        promotionUIDList.Add(ToString(row["PromotionUID"]));
                    }
                }
            }
            catch
            {
                throw;
            }
            return promotionUIDList;
        }
        public async Task<Dictionary<string, List<string>>?> LoadStorePromotionMap(List<string> orgUIDs)
        {
            Dictionary<string, List<string>>? storePromotionMapDictionary = null;

            try
            {
                string commaSeperatedUIDs = string.Join(",", orgUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"OrgUIDs",  commaSeperatedUIDs}
                };

                string query = string.Format(@"SELECT store_uid, promotion_uids FROM store_promotion_map SPM
                                            INNER JOIN store S ON S.uid = SPM.store_uid
                                            AND S.org_uid = ANY(string_to_array(@OrgUIDs, ','))  ");
                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    storePromotionMapDictionary = new Dictionary<string, List<string>>();
                    foreach (DataRow row in dt.Rows)
                    {
                        storePromotionMapDictionary[ToString(row["store_uid"])] = ToString(row["promotion_uids"]).Split(",").ToList();
                    }
                }
            }
            catch
            {
                throw;
            }
            return storePromotionMapDictionary;
        }


        public async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromotionData>?> GetPromotionData()
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                var query = @"SELECT * FROM promotion_data";

                Type type = _serviceProvider.GetRequiredService<IPromotionData>().GetType();
                IEnumerable<IPromotionData> promotionDataList = await ExecuteQueryAsync<IPromotionData>(query, parameters, type);
                return promotionDataList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Volume Cap CUD Operations
        public async Task<int> CUDPromotionVolumeCap(Winit.Modules.Promotion.Model.Classes.PromotionVolumeCapView volumeCapView)
        {
            int count = 0;
            if (volumeCapView == null)
            {
                return count;
            }

            try
            {
                switch (volumeCapView.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        // Check if it already exists
                        var existing = await SelectPromotionVolumeCapByPromotionUID(volumeCapView.PromotionUID);
                        count += existing != null ?
                            await UpdatePromotionVolumeCap(volumeCapView) :
                            await CreatePromotionVolumeCap(volumeCapView);
                        break;

                    case Shared.Models.Enums.ActionType.Delete:
                        count += await DeletePromotionVolumeCap(volumeCapView.UID);
                        break;
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> CreatePromotionVolumeCap(Winit.Modules.Promotion.Model.Classes.PromotionVolumeCapView volumeCapView)
        {
            try
            {
                var Query = @"INSERT INTO promotion_volume_cap (uid, promotion_uid, enabled, overall_cap_type, overall_cap_value, overall_cap_consumed, 
                               invoice_max_discount_value, invoice_max_quantity, invoice_max_applications, created_by, created_time, modified_by, 
                               modified_time, server_add_time, server_modified_time) 
                               VALUES (@UID, @PromotionUID, @Enabled, @OverallCapType, @OverallCapValue, @OverallCapConsumed, 
                               @InvoiceMaxDiscountValue, @InvoiceMaxQuantity, @InvoiceMaxApplications, @CreatedBy, @CreatedTime, @ModifiedBy, 
                               @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                var Parameters = new Dictionary<string, object>
                {
                    { "@UID", volumeCapView.UID },
                    { "@PromotionUID", volumeCapView.PromotionUID },
                    { "@Enabled", volumeCapView.Enabled },
                    { "@OverallCapType", volumeCapView.OverallCapType },
                    { "@OverallCapValue", volumeCapView.OverallCapValue },
                    { "@OverallCapConsumed", volumeCapView.OverallCapConsumed },
                    { "@InvoiceMaxDiscountValue", volumeCapView.InvoiceMaxDiscountValue },
                    { "@InvoiceMaxQuantity", volumeCapView.InvoiceMaxQuantity },
                    { "@InvoiceMaxApplications", volumeCapView.InvoiceMaxApplications },
                    { "@CreatedBy", volumeCapView.CreatedBy },
                    { "@CreatedTime", volumeCapView.CreatedTime },
                    { "@ModifiedBy", volumeCapView.ModifiedBy },
                    { "@ModifiedTime", volumeCapView.ModifiedTime },
                    { "@ServerAddTime", volumeCapView.ServerAddTime },
                    { "@ServerModifiedTime", volumeCapView.ServerModifiedTime }
                };

                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> UpdatePromotionVolumeCap(Winit.Modules.Promotion.Model.Classes.PromotionVolumeCapView volumeCapView)
        {
            var Query = @"UPDATE promotion_volume_cap SET 
                         enabled = @Enabled, 
                         overall_cap_type = @OverallCapType, 
                         overall_cap_value = @OverallCapValue, 
                         overall_cap_consumed = @OverallCapConsumed, 
                         invoice_max_discount_value = @InvoiceMaxDiscountValue, 
                         invoice_max_quantity = @InvoiceMaxQuantity, 
                         invoice_max_applications = @InvoiceMaxApplications, 
                         modified_by = @ModifiedBy, 
                         modified_time = @ModifiedTime, 
                         server_modified_time = @ServerModifiedTime 
                         WHERE promotion_uid = @PromotionUID";

            var Parameters = new Dictionary<string, object>
            {
                { "@PromotionUID", volumeCapView.PromotionUID },
                { "@Enabled", volumeCapView.Enabled },
                { "@OverallCapType", volumeCapView.OverallCapType },
                { "@OverallCapValue", volumeCapView.OverallCapValue },
                { "@OverallCapConsumed", volumeCapView.OverallCapConsumed },
                { "@InvoiceMaxDiscountValue", volumeCapView.InvoiceMaxDiscountValue },
                { "@InvoiceMaxQuantity", volumeCapView.InvoiceMaxQuantity },
                { "@InvoiceMaxApplications", volumeCapView.InvoiceMaxApplications },
                { "@ModifiedBy", volumeCapView.ModifiedBy },
                { "@ModifiedTime", volumeCapView.ModifiedTime },
                { "@ServerModifiedTime", volumeCapView.ServerModifiedTime }
            };

            return await ExecuteNonQueryAsync(Query, Parameters);
        }

        private async Task<int> DeletePromotionVolumeCap(string UID)
        {
            var sql = @"DELETE FROM promotion_volume_cap WHERE uid = @UID";
            var Parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }

        private async Task<Winit.Modules.Promotion.Model.Classes.PromotionVolumeCap> SelectPromotionVolumeCapByPromotionUID(string promotionUID)
        {
            try
            {
                var sql = @"SELECT * FROM promotion_volume_cap WHERE promotion_uid = @PromotionUID LIMIT 1";

                var parameters = new Dictionary<string, object>
                {
                    { "@PromotionUID", promotionUID }
                };

                var dataTable = await ExecuteQueryDataTableAsync(sql, parameters);
                
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    return new Winit.Modules.Promotion.Model.Classes.PromotionVolumeCap
                    {
                        UID = row["uid"]?.ToString(),
                        PromotionUID = row["promotion_uid"]?.ToString(),
                        Enabled = Convert.ToBoolean(row["enabled"]),
                        OverallCapType = row["overall_cap_type"]?.ToString(),
                        OverallCapValue = Convert.ToDecimal(row["overall_cap_value"]),
                        OverallCapConsumed = Convert.ToDecimal(row["overall_cap_consumed"]),
                        InvoiceMaxDiscountValue = Convert.ToDecimal(row["invoice_max_discount_value"]),
                        InvoiceMaxQuantity = Convert.ToInt32(row["invoice_max_quantity"]),
                        InvoiceMaxApplications = Convert.ToInt32(row["invoice_max_applications"]),
                        CreatedBy = row["created_by"]?.ToString(),
                        CreatedTime = Convert.ToDateTime(row["created_time"]),
                        ModifiedBy = row["modified_by"]?.ToString(),
                        ModifiedTime = Convert.ToDateTime(row["modified_time"]),
                        ServerAddTime = Convert.ToDateTime(row["server_add_time"]),
                        ServerModifiedTime = Convert.ToDateTime(row["server_modified_time"])
                    };
                }
                
                return null;
            }
            catch
            {
                throw;
            }
        }

        // Hierarchy Cap CUD Operations
        public async Task<int> CUDPromotionHierarchyCapList(List<Winit.Modules.Promotion.Model.Classes.PromotionHierarchyCapView> hierarchyCapList)
        {
            int count = 0;
            if (hierarchyCapList == null || hierarchyCapList.Count == 0)
                return count;

            try
            {
                foreach (var hierarchyCap in hierarchyCapList)
                {
                    switch (hierarchyCap.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            count += await CreatePromotionHierarchyCap(hierarchyCap);
                            break;
                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromotionHierarchyCap(hierarchyCap.UID);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> CreatePromotionHierarchyCap(Winit.Modules.Promotion.Model.Classes.PromotionHierarchyCapView hierarchyCapView)
        {
            try
            {
                var Query = @"INSERT INTO promotion_hierarchy_caps (uid, promotion_uid, hierarchy_type, hierarchy_uid, hierarchy_name, 
                               cap_type, cap_value, cap_consumed, is_active, created_by, created_time, modified_by, modified_time) 
                               VALUES (@UID, @PromotionUID, @HierarchyType, @HierarchyUID, @HierarchyName, 
                               @CapType, @CapValue, @CapConsumed, @IsActive, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime);";

                var Parameters = new Dictionary<string, object>
                {
                    { "@UID", hierarchyCapView.UID },
                    { "@PromotionUID", hierarchyCapView.PromotionUID },
                    { "@HierarchyType", hierarchyCapView.HierarchyType },
                    { "@HierarchyUID", hierarchyCapView.HierarchyUID },
                    { "@HierarchyName", hierarchyCapView.HierarchyName },
                    { "@CapType", hierarchyCapView.CapType },
                    { "@CapValue", hierarchyCapView.CapValue },
                    { "@CapConsumed", hierarchyCapView.CapConsumed },
                    { "@IsActive", hierarchyCapView.IsActive },
                    { "@CreatedBy", hierarchyCapView.CreatedBy },
                    { "@CreatedTime", hierarchyCapView.CreatedTime },
                    { "@ModifiedBy", hierarchyCapView.ModifiedBy },
                    { "@ModifiedTime", hierarchyCapView.ModifiedTime }
                };

                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> DeletePromotionHierarchyCap(string UID)
        {
            var sql = @"DELETE FROM promotion_hierarchy_caps WHERE uid = @UID";
            var Parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }

        // Period Cap CUD Operations
        public async Task<int> CUDPromotionPeriodCapList(List<Winit.Modules.Promotion.Model.Classes.PromotionPeriodCapView> periodCapList)
        {
            int count = 0;
            if (periodCapList == null || periodCapList.Count == 0)
                return count;

            try
            {
                foreach (var periodCap in periodCapList)
                {
                    switch (periodCap.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            count += await CreatePromotionPeriodCap(periodCap);
                            break;
                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeletePromotionPeriodCap(periodCap.UID);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> CreatePromotionPeriodCap(Winit.Modules.Promotion.Model.Classes.PromotionPeriodCapView periodCapView)
        {
            try
            {
                var Query = @"INSERT INTO promotion_period_caps (uid, promotion_uid, period_type, cap_type, cap_value, cap_consumed, 
                               start_date, end_date, is_active, created_by, created_time, modified_by, modified_time) 
                               VALUES (@UID, @PromotionUID, @PeriodType, @CapType, @CapValue, @CapConsumed, 
                               @StartDate, @EndDate, @IsActive, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime);";

                var Parameters = new Dictionary<string, object>
                {
                    { "@UID", periodCapView.UID },
                    { "@PromotionUID", periodCapView.PromotionUID },
                    { "@PeriodType", periodCapView.PeriodType },
                    { "@CapType", periodCapView.CapType },
                    { "@CapValue", periodCapView.CapValue },
                    { "@CapConsumed", periodCapView.CapConsumed },
                    { "@StartDate", periodCapView.StartDate },
                    { "@EndDate", periodCapView.EndDate },
                    { "@IsActive", periodCapView.IsActive },
                    { "@CreatedBy", periodCapView.CreatedBy },
                    { "@CreatedTime", periodCapView.CreatedTime },
                    { "@ModifiedBy", periodCapView.ModifiedBy },
                    { "@ModifiedTime", periodCapView.ModifiedTime }
                };

                return await ExecuteNonQueryAsync(Query, Parameters);
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> DeletePromotionPeriodCap(string UID)
        {
            var sql = @"DELETE FROM promotion_period_caps WHERE uid = @UID";
            var Parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, Parameters);
        }
    }
}
