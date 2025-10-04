using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Promotion.DL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;


namespace Winit.Modules.Promotion.DL.Classes
{
    public class MSSQLPromotionDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IPromotionDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLPromotionDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }
        public async Task<int> GetPromotionDetailsValidated(string PromotionUID, string OrgUID, string PromotionCode, string PriorityNo, bool isNew)
        {
            var parametres = new Dictionary<string, object>()
            {
                {"PromotionUID",PromotionUID },
                {"OrgUID",OrgUID },
                {"PromotionCode",PromotionCode },
                {"PriorityNo",PriorityNo },
            };
            int retVal = Winit.Shared.Models.Constants.Promotions.None;
            try
            {
                // Updated to make promotion codes globally unique (not just per organization)
                var Query = @$"SELECT Top 1 1 as Status FROM promotion WHERE code = @PromotionCode;
                               SELECT Top 1 1 as Status FROM promotion WHERE org_uid = @OrgUID AND priority = @PriorityNo";
                var Query2 = @$"SELECT Top 1 1 as Status FROM promotion WHERE code = @PromotionCode AND uid != @PromotionUID;
                                SELECT Top 1 1 as Status FROM promotion WHERE org_uid = @OrgUID AND priority = @PriorityNo AND uid != @PromotionUID";

                DataSet ds = await ExecuteQueryDataSetAsync(isNew ? Query : Query2, parametres);
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
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion>> GetPromotionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * From (SELECT 
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
                                                        p.status,
                                                        p.created_time AS CreatedTime,
                                                        p.modified_time AS ModifiedTime,
                                                        p.server_add_time AS ServerAddTime,
                                                        p.server_modified_time AS ServerModifiedTime,
                                                        p.contribution_level1 AS ContributionLevel1,
                                                        p.contribution_level2 AS ContributionLevel2,
                                                        p.contribution_level3 AS ContributionLevel3
    
                                                    FROM 
                                                        promotion p
                                                     Left join list_item li on li.uid=p.promo_format
                                                      Left join list_item  lit on lit.uid=p.category)As SubQuery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
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
                                                        p.status,
                                                        p.created_time AS CreatedTime,
                                                        p.modified_time AS ModifiedTime,
                                                        p.server_add_time AS ServerAddTime,
                                                        p.server_modified_time AS ServerModifiedTime,
                                                        p.contribution_level1 AS ContributionLevel1,
                                                        p.contribution_level2 AS ContributionLevel2,
                                                        p.contribution_level3 AS ContributionLevel3
    
                                                    FROM 
                                                        promotion p
                                                     Left join list_item li on li.uid=p.promo_format
                                                      Left join list_item  lit on lit.uid=p.category)As SubQuery");
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
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromotion> PromotionDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromotion>(sql.ToString(), parameters);
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
            using (var connection = CreateConnection())
            {

                var sql = @"SELECT 
                                                p.id AS Id,
                                                p.uid AS UID,
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
                                                p.ss AS SS,
                                                p.status,
                                                p.created_time AS CreatedTime,
                                                p.modified_time AS ModifiedTime,
                                                p.server_add_time AS ServerAddTime,
                                                p.server_modified_time AS ServerModifiedTime,
                                                p.contribution_level1 AS ContributionLevel1,
                                                p.contribution_level2 AS ContributionLevel2,
                                                p.contribution_level3 AS ContributionLevel3
                                            FROM promotion p
                                            WHERE p.uid = @PromotionUID;
                                            
                                            SELECT
                                                id AS Id,
                                                uid AS UID,
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
                                            FROM promo_order
                                            WHERE promotion_uid = @PromotionUID;
                                            
                                            SELECT
                                                id AS Id,
                                                uid AS UID,
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
                                            FROM promo_order_item
                                            WHERE promotion_uid = @PromotionUID;
                                            
                                            SELECT
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
                                            FROM promo_offer
                                            WHERE promotion_uid = @PromotionUID;
                                            
                                            SELECT
                                                id AS Id,
                                                uid AS UID,
                                                promo_offer_uid AS PromoOfferUID,
                                                item_criteria_type AS ItemCriteriaType,
                                                item_criteria_selected AS ItemCriteriaSelected,
                                                is_compulsory AS IsCompulsory,
                                                item_uom AS ItemUOM,
                                                ss AS SS,
                                                created_time AS CreatedTime,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                promotion_uid AS PromotionUID
                                            FROM promo_offer_item
                                            WHERE promotion_uid = @PromotionUID;
                                            
                                            SELECT
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
                                                created_time AS CreatedTime,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                promotion_uid AS PromotionUID
                                            FROM promo_condition
                                            WHERE promotion_uid = @PromotionUID; ";

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

        public async Task<int> CUDPromotion(PromoMasterView promoMasterView)
        {
            int count = 0;

            try
            {
                string? uid = await CheckIfUIDExistsInDB(DbTableName.Promotion, promoMasterView.PromotionView.UID);
                if (promoMasterView.PromotionView.ActionType == Winit.Shared.Models.Enums.ActionType.Add)
                {
                    if (!string.IsNullOrEmpty(uid))
                    {
                        count += await UpdatePromotion(promoMasterView.PromotionView);

                    }
                    else
                    {
                        count += await CreatePromotion(promoMasterView.PromotionView);
                        //if (count > 0)
                        //{
                        //    if (await CreateApprovalRequest(promoMasterView))
                        //    {
                        //        promoMasterView.PromotionView.IsApprovalCreated = true;
                        //        count += await UpdatePromotion(promoMasterView.PromotionView);
                        //    }
                        //}
                    }
                }
                if (promoMasterView.PromotionView.ActionType == Winit.Shared.Models.Enums.ActionType.Delete)
                {
                    count += await DeletePromotion(promoMasterView.PromotionView.UID);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        private async Task<bool> CreateApprovalRequest(PromoMasterView promoMasterView)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = "StandingProvision";
                approvalRequest.LinkedItemUID = promoMasterView.PromotionView.UID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(promoMasterView.ApprovalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
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
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.PromoOrder, uidList);
                List<PromoOrderView>? existingPromoOrders = null;
                List<PromoOrderView>? newPromoOrders = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingPromoOrders = lstPromoOrder.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newPromoOrders = lstPromoOrder.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newPromoOrders = lstPromoOrder;
                }
                if (lstPromoOrder.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && existingUIDs.Any())
                    {
                        count += await UpdatePromoOrderList(existingPromoOrders);
                    }
                    if (newPromoOrders != null && newPromoOrders.Any())
                    {
                        count += await CreatePromoOrderList(newPromoOrders);
                    }
                }
                if (deletedUidList != null && lstPromoOrder.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeletePromoOrder(deletedUidList);
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
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.PromoOrderItem, uidList);
                List<PromoOrderItemView>? existingPromoOrderItemList = null;
                List<PromoOrderItemView>? newPromoOrderItemList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingPromoOrderItemList = promoOrderItemViewlst.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newPromoOrderItemList = promoOrderItemViewlst.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newPromoOrderItemList = promoOrderItemViewlst;
                }

                if (promoOrderItemViewlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && existingUIDs.Any() && existingUIDs.Count > 0)
                    {

                        count += await UpdatePromoOrderItemList(existingPromoOrderItemList);
                    }
                    if (newPromoOrderItemList != null && newPromoOrderItemList.Any())
                    {
                        count += await CreatePromoOrderItemList(newPromoOrderItemList);
                    }
                }
                if (deletedUidList != null && promoOrderItemViewlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeletePromoOrderItem(deletedUidList);
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
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.PromoOffer, uidList);
                List<PromoOfferView>? existingPromoOfferList = null;
                List<PromoOfferView>? newPromoOfferList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingPromoOfferList = promoOfferiewlst.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newPromoOfferList = promoOfferiewlst.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newPromoOfferList = promoOfferiewlst;
                }

                if (promoOfferiewlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && existingUIDs.Any())
                    {
                        count += await UpdatePromoOfferList(existingPromoOfferList);
                    }
                    if (newPromoOfferList != null && newPromoOfferList.Any())
                    {
                        count += await CreatePromoOfferList(newPromoOfferList);
                    }
                }
                if (deletedUidList != null && promoOfferiewlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeletePromoOffer(deletedUidList);
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
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.PromoOfferItem, uidList);
                List<PromoOfferItemView>? existingPromoOfferItemList = null;
                List<PromoOfferItemView>? newPromoOfferItemList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingPromoOfferItemList = promoOfferItemlst.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newPromoOfferItemList = promoOfferItemlst.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newPromoOfferItemList = promoOfferItemlst;
                }
                if (promoOfferItemlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && existingUIDs.Any())
                    {
                        count += await UpdatePromoOfferItemList(existingPromoOfferItemList);
                    }
                    if (newPromoOfferItemList != null && newPromoOfferItemList.Any())
                    {
                        count += await CreatePromoOfferItemList(newPromoOfferItemList);
                    }
                }
                if (deletedUidList != null && promoOfferItemlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeletePromoOfferItem(deletedUidList);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> CUDPromoCondition(List<Winit.Modules.Promotion.Model.Classes.PromoConditionView> promoConditionlst)
        {
            int count = 0;

            if (promoConditionlst == null || promoConditionlst.Count == 0)
            {
                return count;
            }
            List<string> uidList = promoConditionlst.Select(po => po.UID).ToList();
            List<string> deletedUidList = promoConditionlst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
            try
            {
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.PromoCondition, uidList);
                List<PromoConditionView>? existingPromoConditionList = null;
                List<PromoConditionView>? newPromoConditionList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingPromoConditionList = promoConditionlst.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newPromoConditionList = promoConditionlst.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newPromoConditionList = promoConditionlst;
                }
                if (promoConditionlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && existingUIDs.Any())
                    {
                        count += await UpdatePromoConditionList(existingPromoConditionList);
                    }
                    if (newPromoConditionList != null && newPromoConditionList.Any())
                    {
                        count += await CreatePromoConditionList(newPromoConditionList);
                    }
                }
                if (deletedUidList != null && promoConditionlst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeletePromoCondition(deletedUidList);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> CUDItemPromotionMap(List<Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView> itemPromotionMaplst)
        {
            int count = 0;

            if (itemPromotionMaplst == null || itemPromotionMaplst.Count == 0)
            {
                return count;
            }
            List<string> uidList = itemPromotionMaplst.Select(po => po.UID).ToList();
            List<string> deletedUidList = itemPromotionMaplst.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
            try
            {
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.ItemPromotionMap, uidList);
                List<ItemPromotionMapView>? existingItemPromotionMapList = null;
                List<ItemPromotionMapView>? newItemPromotionMapList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    existingItemPromotionMapList = itemPromotionMaplst.Where(e => existingUIDs.Contains(e.UID)).ToList();
                    newItemPromotionMapList = itemPromotionMaplst.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newItemPromotionMapList = itemPromotionMaplst;
                }
                if (itemPromotionMaplst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Add))
                {
                    if (existingUIDs != null && !existingUIDs.Any())
                    {
                        count += await UpdateItemPromotionMapList(existingItemPromotionMapList);
                    }
                    else
                    {
                        count += await CreateItemPromotionMapList(newItemPromotionMapList);
                    }
                }
                if (deletedUidList != null && itemPromotionMaplst.Any(e => e.ActionType == Shared.Models.Enums.ActionType.Delete))
                {
                    count += await DeleteItemPromotionMap(deletedUidList);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> UpdateSchemeMappingData(string schemeUID)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@scheme_uid", schemeUID);
                return await ExecuteProcedureAsync("usp_scheme_customer_mapping_data_insert_for_qps", parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> CUDPromotionMaster(Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView)
        {
            int count = 0;

            try
            {
                using (var connection = CreateConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            count += await CUDPromotion(promoMasterView);
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
                            List<string> promotionUIDList = new List<string>();
                            promotionUIDList.Add(promoMasterView.PromotionView.UID);
                            count += await CreateDMSPromotionByJsonData(promotionUIDList);
                            count += await PopulateItemPromotionMap(promoMasterView.PromotionView.UID);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        //Promotion
        public async Task<int> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView)
        {
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
                                 ss = @ss, 
                                 modified_time = @ModifiedTime, 
                                 server_modified_time = @ServerModifiedTime,
                                 contribution_level1 = @ContributionLevel1,
                                 contribution_level2 = @ContributionLevel2,
                                 contribution_level3 = @ContributionLevel3,
                                 is_approval_created = @IsApprovalCreated,
                                 status = @status
                             WHERE uid = @UID;";

            int retVal = await ExecuteNonQueryAsync(Query, updatePromotionView);

            return retVal;
        }
        private async Task<int> CreatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView CreatePromotionView)
        {
            var Query = @"INSERT INTO promotion (uid, company_uid, org_uid, code, name, remarks, category, has_slabs, created_by_emp_uid,
                                              valid_from, valid_upto, type, promo_format, is_active, promo_title, promo_message, has_fact_sheet, priority,
                                                 created_time, modified_time, server_add_time, server_modified_time,contribution_level1,contribution_level2,
                                            contribution_level3,is_approval_created,status)
                          VALUES (@UID, @CompanyUID, @OrgUID, @Code, @Name, @Remarks, @Category, @HasSlabs,
                                  @CreatedByEmpUID, @ValidFrom, @ValidUpto, @Type, @PromoFormat, @IsActive, @PromoTitle, @PromoMessage, @HasFactSheet, @Priority,
                                  @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@ContributionLevel1,@ContributionLevel2,@ContributionLevel3,@IsApprovalCreated,@Status);";

            return await ExecuteNonQueryAsync(Query, CreatePromotionView);
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
                                    p.server_modified_time AS ServerModifiedTime,
                                    p.contribution_level1 AS ContributionLevel1,
                                    p.contribution_level2 AS ContributionLevel2,
                                    p.contribution_level3 AS ContributionLevel3
                                    
                                FROM     promotion p where p.uid=@UID";
            Winit.Modules.Promotion.Model.Interfaces.IPromotion promotionDetails = await ExecuteSingleAsync<Winit.Modules.Promotion.Model.Interfaces.IPromotion>(sql, parameters);
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
     FROM    promo_order  WHERE uid = ANY(string_to_array(@UIDs, ','))";
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder> promotionOrderDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder>(sql, parameters);
            return promotionOrderDetails;
        }
        private async Task<int> CreatePromoOrderList(List<Winit.Modules.Promotion.Model.Classes.PromoOrderView> CreatePromoOrderViews)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO promo_order (uid, promotion_uid, selection_model, qualification_level, min_deal_count, max_deal_count,
                                                         ss, created_time, modified_time, server_add_time, server_modified_time)
                                VALUES (@UID, @PromotionUID, @SelectionModel, @QualificationLevel,
                                        @MinDealCount, @MaxDealCount, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                count = await ExecuteNonQueryAsync(Query, CreatePromoOrderViews);
            }
            catch
            {
                throw;
            }
            return count;


        }
        private async Task<int> UpdatePromoOrderList(List<Winit.Modules.Promotion.Model.Classes.PromoOrderView> updatePromoOrderViews)
        {
            int count = -1;
            try
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

                count = await ExecuteNonQueryAsync(Query, updatePromoOrderViews);
            }
            catch
            {
                throw;
            }
            return count;


        }
        private async Task<int> DeletePromoOrder(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_order WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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
        promo_order_item Where uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem> promotionOrderItemDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem>(sql, parameters, type);
            return promotionOrderItemDetails;
        }
        private async Task<int> CreatePromoOrderItemList(List<Winit.Modules.Promotion.Model.Classes.PromoOrderItemView> CreatePromoOrderItemViews)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO promo_order_item (uid, promotion_uid, promo_order_uid, parent_uid, item_criteria_type, item_criteria_selected, is_compulsory, 
                              item_uom, promo_split, ss, created_time, modified_time, server_add_time, server_modified_time,
                              config_group_id, config_name, config_promotion_type) 
                                VALUES 
                            (@UID, @PromotionUID, @PromoOrderUID, @ParentUID, @ItemCriteriaType, @ItemCriteriaSelected, @IsCompulsory, @ItemUOM, @PromoSplit,
                             @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId, @ConfigName, @ConfigPromotionType);";

                count = await ExecuteNonQueryAsync(Query, CreatePromoOrderItemViews);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;


        }
        private async Task<int> UpdatePromoOrderItemList(List<Winit.Modules.Promotion.Model.Classes.PromoOrderItemView> updatePromoOrderItemViews)
        {
            int count = -1;
            try
            {
                var Query = @"UPDATE promo_order_item SET 
                        item_criteria_type = @ItemCriteriaType, 
                        item_criteria_selected = @ItemCriteriaSelected, 
                        is_compulsory = @IsCompulsory, 
                        item_uom = @ItemUOM, 
                        promo_split = @PromoSplit, 
                        ss = @ss, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime
                        WHERE uid = @UID;";

                count = await ExecuteNonQueryAsync(Query, updatePromoOrderItemViews);
            }
            catch
            {
                throw;
            }
            return count;

        }
        private async Task<int> DeletePromoOrderItem(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_order_item WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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
                             promo_offer WHERE uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer> promoOfferDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer>(sql, parameters);
            return promoOfferDetails;
        }
        private async Task<int> CreatePromoOfferList(List<Winit.Modules.Promotion.Model.Classes.PromoOfferView> CreatePromoOffers)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO promo_offer (uid, promotion_uid, promo_order_uid, type, qualification_level, application_level, selection_model, 
                            has_offer_item_selection, ss, created_time, modified_time, server_add_time, server_modified_time) 
                            VALUES (@UID, @PromotionUID, @PromoOrderUID, @Type, @QualificationLevel, 
                            @ApplicationLevel, @SelectionModel, @HasOfferItemSelection, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                count = await ExecuteNonQueryAsync(Query, CreatePromoOffers);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;

        }
        private async Task<int> UpdatePromoOfferList(List<Winit.Modules.Promotion.Model.Classes.PromoOfferView> updatePromoOffers)
        {
            int count = -1;
            try
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

                count = await ExecuteNonQueryAsync(Query, updatePromoOffers);
            }
            catch
            {
                throw;
            }
            return count;

        }
        private async Task<int> DeletePromoOffer(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_offer WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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
                                 ss AS SS,
                                 created_time AS CreatedTime,
                                 modified_time AS ModifiedTime,
                                 server_add_time AS ServerAddTime,
                                 server_modified_time AS ServerModifiedTime,
                                 promotion_uid AS PromotionUID
                             FROM
                                 promo_offer_item WHERE uid = ANY(string_to_array(@UIDs, ','))";
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem> promoOfferItemDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem>(sql, parameters);
            return promoOfferItemDetails;
        }
        private async Task<int> CreatePromoOfferItemList(List<Winit.Modules.Promotion.Model.Classes.PromoOfferItemView> CreatePromoOfferItems)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO promo_offer_item (uid, promotion_uid, promo_offer_uid, item_criteria_type, item_criteria_selected, 
                               is_compulsory, item_uom,ss, created_time, modified_time, server_add_time, server_modified_time, config_group_id) 
                               VALUES (@UID, @PromotionUID, @PromoOfferUID, @ItemCriteriaType, 
                               @ItemCriteriaSelected, @IsCompulsory, @ItemUOM, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId);";

                count = await ExecuteNonQueryAsync(Query, CreatePromoOfferItems);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;
        }
        private async Task<int> UpdatePromoOfferItemList(List<Winit.Modules.Promotion.Model.Classes.PromoOfferItemView> updatePromoOfferItems)
        {
            int count = -1;
            try
            {
                var Query = @"UPDATE promo_offer_item SET 
                        item_criteria_type = @ItemCriteriaType, 
                        item_criteria_selected = @ItemCriteriaSelected, 
                        is_compulsory = @IsCompulsory, 
                        item_uom = @ItemUOM, 
                        ss = @ss, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime
                         WHERE uid = @UID;";
                count = await ExecuteNonQueryAsync(Query, updatePromoOfferItems);
            }
            catch
            {
                throw;
            }
            return count;

        }
        private async Task<int> DeletePromoOfferItem(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_offer_item WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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
                           created_time AS CreatedTime,
                           modified_time AS ModifiedTime,
                           server_add_time AS ServerAddTime,
                           server_modified_time AS ServerModifiedTime,
                           promotion_uid AS PromotionUID
                       FROM promo_condition WHERE uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition> PromoConditionDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition>(sql, parameters, type);
            return PromoConditionDetails;
        }
        private async Task<int> CreatePromoConditionList(List<Winit.Modules.Promotion.Model.Classes.PromoConditionView> CreatePromoConditionViews)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO promo_condition (uid, promotion_uid ,reference_type, reference_uid, condition_type, min, max, max_deal_count, uom, 
                            all_uom_conversion, value_type, is_prorated, ss, created_time, modified_time, server_add_time, server_modified_time,
                            config_group_id, config_details) 
                            VALUES (@UID,@PromotionUID, @ReferenceType, @ReferenceUID, @ConditionType, @Min, @Max, @MaxDealCount, @UOM, @AllUOMConversion, 
                            @ValueType, @IsProrated, @ss, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ConfigGroupId, @ConfigDetails);";

                count = await ExecuteNonQueryAsync(Query, CreatePromoConditionViews);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;
        }
        private async Task<int> UpdatePromoConditionList(List<Winit.Modules.Promotion.Model.Classes.PromoConditionView> updatePromoConditionViews)
        {
            int count = -1;
            try
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
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime 
                         WHERE uid = @UID";

                count = await ExecuteNonQueryAsync(Query, updatePromoConditionViews);
            }
            catch
            {
                throw;
            }
            return count;


        }
        private async Task<int> DeletePromoCondition(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM promo_condition WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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
                            item_promotion_map
                         WHERE uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap> ItemPromotionMapDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>(sql, parameters, type);
            return ItemPromotionMapDetails;
        }
        private async Task<int> CreateItemPromotionMapList(List<Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView> CreateItemPromotionMaps)
        {
            int count = -1;
            try
            {
                var Query = @"INSERT INTO item_promotion_map (uid, sku_type, sku_type_uid, promotion_uid, ss, created_time, 
                              modified_time, server_add_time, server_modified_time) VALUES (@UID, @SKUType, @SKUTypeUID, 
                              @PromotionUID, @SS, @CreatedTime, @ModifiedTime,@ServerAddTime, @ServerModifiedTime);";
                count = await ExecuteNonQueryAsync(Query, CreateItemPromotionMaps);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return count;
        }
        private async Task<int> UpdateItemPromotionMapList(List<Winit.Modules.Promotion.Model.Classes.ItemPromotionMapView> updatePromoConditionViews)
        {
            int count = -1;
            try
            {
                var Query = @"UPDATE item_promotion_map SET 
                          sku_type = @SKUType, 
                          ss = @SS, 
                          modified_time = @ModifiedTime, 
                          server_modified_time = @ServerModifiedTime
                          WHERE uid = @UID;";

                count = await ExecuteNonQueryAsync(Query, updatePromoConditionViews);
            }
            catch
            {
                throw;
            }
            return count;

        }
        private async Task<int> DeleteItemPromotionMap(List<string> UIDs)
        {
            string commaSeperatedUIDs = string.Join(",", UIDs);
            var sql = @"DELETE FROM item_promotion_map WHERE  uid IN (
        SELECT value 
        FROM STRING_SPLIT(@UIDs, ',')
    );";
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

            var sql = @"SELECT * FROM item_promotion_map WHERE promotion_uid IN (
        SELECT value 
        FROM STRING_SPLIT(@PromotionUIDs, ',')
    );";
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
                          SET data = CAST(@Data AS nvarchar(max)), modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime
                          WHERE promotion_uid = @PromotionUID;";

                        parameters.Remove("@UID");
                        parameters.Remove("@CreatedTime");
                        parameters.Remove("@ServerAddTime");
                    }

                    else
                    {
                        query = @"INSERT INTO promotion_data (uid, promotion_uid, data, created_time, modified_time, server_add_time, server_modified_time) 
                          VALUES (@UID, @PromotionUID, CAST(@Data AS nvarchar(max)), @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                    }

                    rowsAffected += await ExecuteNonQueryAsync(query, parameters);
                }
            }
            catch (Exception Ex)
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
                CREATE  TABLE #temppromoitem (
                    promotion_uid VARCHAR(250),
                    item_type VARCHAR(50),
                    item_code VARCHAR(50)
                );

                INSERT INTO #temppromoitem (promotion_uid, item_type, item_code)
                SELECT  P.uid, POI.item_criteria_type, POI.item_criteria_selected 
                FROM promotion P
                INNER JOIN promo_order PO ON P.UID = PO.promotion_uid AND P.UID = @PromotionUID
                INNER JOIN promo_order_item POI ON PO.UID = POI.promo_order_uid;

                DELETE FROM item_promotion_map WHERE promotion_uid = @PromotionUID;

                INSERT INTO item_promotion_map (uid, sku_type, sku_type_uid, promotion_uid, 
                ss, created_time, modified_time, server_add_time, server_modified_time)
                SELECT newid(), item_type, item_code, promotion_uid, 0, 
                current_timestamp, current_timestamp, current_timestamp, current_timestamp
                FROM #temppromoitem;

                DROP TABLE #temppromoitem;";

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
                                    promotion_data
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
            catch (Exception Ex)
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
                                uid IN (SELECT value FROM STRING_SPLIT(@PromotionUID, ','))
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
                    WHERE p.uid IN (
        SELECT value 
        FROM STRING_SPLIT(@PromotionUID, ',')
    )
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
                        WHERE p.uid IN (
        SELECT value 
        FROM STRING_SPLIT(@PromotionUID, ',')
    )
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
                    WHERE p.uid IN (SELECT value FROM STRING_SPLIT(@PromotionUID, ','))
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
WHERE p.uid IN (SELECT value FROM STRING_SPLIT(@PromotionUID, ','))
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
                           created_time AS CreatedTime,
                           modified_time AS ModifiedTime,
                           server_add_time AS ServerAddTime,
                           server_modified_time AS ServerModifiedTime,
                           promotion_uid AS PromotionUID
                    FROM promo_condition pc
                    WHERE (@PromoOrderList IS NULL OR (pc.reference_type = 'PromoOrder' AND pc.reference_uid IN (SELECT value FROM STRING_SPLIT(@PromoOrderList, ','))))
                    OR (@PromoOrderItemList IS NULL OR (pc.reference_type = 'PromoOrderItem' AND pc.reference_uid IN (SELECT value FROM STRING_SPLIT(@PromoOrderItemList, ','))))
                    OR (@PromoOfferList IS NULL OR (pc.reference_type = 'PromoOffer' AND pc.reference_uid IN (SELECT value FROM STRING_SPLIT(@PromoOfferList, ','))))
                    OR (@PromoOfferItemList IS NULL OR (pc.reference_type = 'PromoOfferItem' AND pc.reference_uid IN (SELECT value FROM STRING_SPLIT(@PromoOfferItemList, ','))));";

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

                var query = @"SELECT * FROM promotion_data WHERE promotion_uid IN (SELECT value FROM STRING_SPLIT(@PromotionUIDs, ','))";

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
            catch (Exception Ex)
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
                                INNER JOIN selection_map_criteria SC
                                    ON SC.linked_item_uid = P.uid
                                    AND P.org_uid IN (SELECT value FROM STRING_SPLIT(@OrgUIDs, ','))
                                    AND SC.linked_item_type = 'Promotion'
                                    AND CAST(P.valid_from AS DATE) <= CAST(GETDATE() AS DATE)
                                    AND (P.valid_upto IS NULL OR CAST(P.valid_upto AS DATE) >= CAST(GETDATE() AS DATE))
                                    AND P.is_active = 1
                                INNER JOIN selection_map_details SD
                                    ON SC.uid = SD.selection_map_criteria_uid
                                    AND SD.selection_value IS NOT NULL;
                                     ";

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
            catch (Exception Ex)
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
                                            AND S.org_uid IN (SELECT value FROM STRING_SPLIT(@OrgUIDs, ','))  ");
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
            catch (Exception Ex)
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
        public async Task<int> DeletePromotionSlabByPromoOrderUID(string promoOrderUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"PRomoOrderUID" ,promoOrderUID}
            };
            string sql = """
                                delete pc from promo_condition pc 
                inner join   (
                select uid from promo_order where 
                uid=@PRomoOrderUID
                union 
                select uid from promo_order_item where 
                promo_order_uid=@PRomoOrderUID
                union
                select uid from promo_offer where promo_order_uid=@PRomoOrderUID
                union

                select uid from promo_offer_item where promo_offer_uid in 
                (select uid from promo_offer where promo_order_uid=@PRomoOrderUID)

                ) rr on rr.uid=pc.reference_uid

                delete poi from promo_offer_item poi  
                inner join promo_offer po on poi.promo_offer_uid=po.uid and po.promo_order_uid=@PRomoOrderUID


                delete from promo_offer where promo_order_uid=@PRomoOrderUID

                delete from promo_order_item where 
                promo_order_uid=@PRomoOrderUID

                delete from promo_order where 
                uid=@PRomoOrderUID
                """;

            int count = await ExecuteNonQueryAsync(sql, parameters);
            return count;
        }
        public async Task<int> DeletePromoOrderItems(List<string> UIDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"UIDs" ,UIDs}
            };
            string sql = """delete from promo_order_item where uid in @UIDs""";

            int count = await ExecuteNonQueryAsync(sql, parameters);
            return count;
        }
        public async Task<int> ChangeEndDate(PromotionView promotionView)
        {
            string sql = """
                update promotion set 
                status=@Status,
                has_history=@HasHistory,
                valid_upto=@ValidUpto,
                end_date_remarks=@EndDateRemarks,
                end_date_updated_by_emp_uid=@EndDateUpdatedByEmpUID,
                end_date_updated_on=@EndDateUpdatedOn,
                modified_by=@ModifiedBy,
                modified_time=@ModifiedTime,
                server_modified_time=@ServerModifiedTime
                where uid=@UID
                """;

            int count = await ExecuteNonQueryAsync(sql, promotionView);
            return count;
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

                return await ExecuteNonQueryAsync(Query, volumeCapView);
            }
            catch (Exception Ex)
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

            return await ExecuteNonQueryAsync(Query, volumeCapView);
        }

        private async Task<int> DeletePromotionVolumeCap(string UID)
        {
            var sql = @"DELETE FROM promotion_volume_cap WHERE uid = @UID";
            var parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        private async Task<Winit.Modules.Promotion.Model.Classes.PromotionVolumeCap> SelectPromotionVolumeCapByPromotionUID(string promotionUID)
        {
            try
            {
                var sql = @"SELECT TOP 1 * FROM promotion_volume_cap WHERE promotion_uid = @PromotionUID";

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
            catch (Exception Ex)
            {
                throw;
            }
        }

        // Hierarchy Cap CUD Operations
        public async Task<int> CUDPromotionHierarchyCapList(List<Winit.Modules.Promotion.Model.Classes.PromotionHierarchyCapView> hierarchyCapViewList)
        {
            int count = 0;
            if (hierarchyCapViewList == null || hierarchyCapViewList.Count == 0)
            {
                return count;
            }

            try
            {
                foreach (var hierarchyCapView in hierarchyCapViewList)
                {
                    count += await CUDPromotionHierarchyCap(hierarchyCapView);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<int> CUDPromotionHierarchyCap(Winit.Modules.Promotion.Model.Classes.PromotionHierarchyCapView hierarchyCapView)
        {
            int count = 0;
            if (hierarchyCapView == null)
            {
                return count;
            }

            try
            {
                switch (hierarchyCapView.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        count += await CreatePromotionHierarchyCap(hierarchyCapView);
                        break;

                    case Shared.Models.Enums.ActionType.Delete:
                        count += await DeletePromotionHierarchyCap(hierarchyCapView.UID);
                        break;
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
                               cap_type, cap_value, cap_consumed, is_active, created_by, created_time, modified_by, modified_time, 
                               server_add_time, server_modified_time) 
                               VALUES (@UID, @PromotionUID, @HierarchyType, @HierarchyUID, @HierarchyName, @CapType, @CapValue, 
                               @CapConsumed, @IsActive, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                return await ExecuteNonQueryAsync(Query, hierarchyCapView);
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        private async Task<int> DeletePromotionHierarchyCap(string UID)
        {
            var sql = @"DELETE FROM promotion_hierarchy_caps WHERE uid = @UID";
            var parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        // Period Cap CUD Operations
        public async Task<int> CUDPromotionPeriodCapList(List<Winit.Modules.Promotion.Model.Classes.PromotionPeriodCapView> periodCapViewList)
        {
            int count = 0;
            if (periodCapViewList == null || periodCapViewList.Count == 0)
            {
                return count;
            }

            try
            {
                foreach (var periodCapView in periodCapViewList)
                {
                    count += await CUDPromotionPeriodCap(periodCapView);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<int> CUDPromotionPeriodCap(Winit.Modules.Promotion.Model.Classes.PromotionPeriodCapView periodCapView)
        {
            int count = 0;
            if (periodCapView == null)
            {
                return count;
            }

            try
            {
                switch (periodCapView.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        count += await CreatePromotionPeriodCap(periodCapView);
                        break;

                    case Shared.Models.Enums.ActionType.Delete:
                        count += await DeletePromotionPeriodCap(periodCapView.UID);
                        break;
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
                               start_date, end_date, is_active, created_by, created_time, modified_by, modified_time, 
                               server_add_time, server_modified_time) 
                               VALUES (@UID, @PromotionUID, @PeriodType, @CapType, @CapValue, @CapConsumed, @StartDate, @EndDate, 
                               @IsActive, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                return await ExecuteNonQueryAsync(Query, periodCapView);
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        private async Task<int> DeletePromotionPeriodCap(string UID)
        {
            var sql = @"DELETE FROM promotion_period_caps WHERE uid = @UID";
            var parameters = new Dictionary<string, object>
            {
                { "@UID", UID }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

    }
}
