using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.ApprovalEngine.BL.Classes;
using Winit.Modules.Store.Model.Classes;
using Dapper;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLQPSSchemeDL : SqlServerDBManager, IQPSSchemeDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLQPSSchemeDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }

        public async Task<List<IQPSSchemePO>> GetQPSSchemesByStoreUIDAndSKUUID(string storeUid, DateTime order_date, List<SKUFilter> filters)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                SELECT Scheme_UID, Scheme_Code, Store_UID, SKU_UID, Offer_Type, Offer_Value, MaxQty , MinQty, Keyword,
                ModifiedTime FROM (
                SELECT ROW_NUMBER() OVER (Partition BY scmd.scheme_uid, scmd.Store_UID, 
                OI.item_criteria_type +'_'+ CONVERT(VARCHAR,OI.item_criteria_selected) ORDER BY PC1.[Max] DESC, 
                scmd.approved_time DESC) AS RowNum,scmd.scheme_uid Scheme_UID, P.code Scheme_Code, scmd.Store_UID, 
                '' SKU_UID, PC.condition_type Offer_Type, PC.[max] Offer_Value, PC1.[Max] MaxQty , PC1.[Min] MinQty, 
                CONVERT(VARCHAR,OI.item_criteria_selected) +'_'+ OI.item_criteria_type AS Keyword,
                P.modified_time AS ModifiedTime
                FROM promotion P
                INNER JOIN promo_order O ON O.Promotion_UID = P.UID
                INNER JOIN promo_offer PO ON PO.promo_order_uid = O.UID
                INNER JOIN promo_condition PC ON PC.reference_uid = PO.UID
                INNER JOIN promo_condition PC1 ON PC1.reference_uid = PO.promo_order_uid
                INNER JOIN promo_order_item OI ON OI.Promotion_UID = P.UID
                INNER JOIN scheme_customer_mapping_data scmd ON scmd.scheme_uid = P.uid 
                AND scmd.scheme_type = 'QPS' AND scmd.store_uid = @storeUid  
                AND @order_date between CAST(start_date AS Date) and CAST(end_date AS Date)
                GROUP BY scmd.scheme_uid , P.code , scmd.Store_UID,  PC.condition_type , PC.[max],
                PC1.[Max],PC1.[Min], scmd.approved_time, OI.item_criteria_type , 
                CONVERT(VARCHAR,OI.item_criteria_selected), P.modified_time
                ) tbl WHERE RowNum =1 
                """);
                var parameters = new
                {
                    storeUid = storeUid,
                    order_date = order_date//, SKUUIds = sKUUIDs
                };
                //if (sKUUIDs != null && sKUUIDs.Any())
                //{
                //    sql.Append($" AND SKU.UID IN @SKUUIds ");
                //}
                List<IQPSSchemePO> list = await ExecuteQueryAsync<IQPSSchemePO>(sql.ToString(), parameters);
                List<IQPSSchemePO> listToReturn = new List<IQPSSchemePO>();
                foreach (SKUFilter filter in filters)
                {
                    foreach (IQPSSchemePO qps in list.OrderByDescending(e=>e.ModifiedTime))
                    {
                        if (qps.Keyword.Contains(filter.SKUUID))
                        {
                            IQPSSchemePO qpsObj = new QPSSchemePO((QPSSchemePO)qps);
                            qpsObj.SKU_UID = filter.SKUUID;
                            listToReturn.Add(qpsObj);
                            break;
                        }
                        else
                        {
                            if(filter.FilterKeys.Contains(qps.Keyword))
                            {
                                IQPSSchemePO qpsObj = new QPSSchemePO((QPSSchemePO)qps);
                                qpsObj.SKU_UID = filter.SKUUID;
                                listToReturn.Add(qpsObj);
                                break;
                            }
                        }
                    }
                }
                //var filteredList = listToReturn
                //    .GroupBy(item => item.SKU_UID)
                //    .Select(group => group.OrderByDescending(item => item.ModifiedTime).First())
                //    .ToList();
                return listToReturn;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<List<IQPSSchemePO>> GetQPSSchemesByPOUID(string pouid, List<SKUFilter> filters)
        {
            try
            {
                StringBuilder sql = new StringBuilder(
                """
                with Schemes as(
                select  distinct polp.scheme_code,pl.sku_uid,polp.provision_type,ph.org_uid from purchase_order_header ph 
                inner join purchase_order_line pl on ph.uid=pl.purchase_order_header_uid
                inner join purchase_order_line_provision polp on polp.purchase_order_line_uid=pl.uid 
                and polp.provision_type like '%QPS' 
                and ph.uid=@pouid 
                )
                --select * from Schemes
                 SELECT Scheme_UID, Scheme_Code, Store_UID, SKU_UID, Offer_Type, Offer_Value, MaxQty , MinQty, Keyword,
                ModifiedTime FROM (
                SELECT ROW_NUMBER() OVER (Partition BY scmd.scheme_uid, scmd.Store_UID, 
                OI.item_criteria_type +'_'+ CONVERT(VARCHAR,OI.item_criteria_selected) ORDER BY PC1.[Max] DESC, 
                scmd.approved_time DESC) AS RowNum,scmd.scheme_uid Scheme_UID, P.code Scheme_Code, scmd.Store_UID, 
                '' SKU_UID, PC.condition_type Offer_Type, PC.[max] Offer_Value, PC1.[Max] MaxQty , PC1.[Min] MinQty, 
                CONVERT(VARCHAR,OI.item_criteria_selected) +'_'+ OI.item_criteria_type AS Keyword,
                P.modified_time AS ModifiedTime
                FROM promotion P
                INNER JOIN promo_order O ON O.Promotion_UID = P.UID
                INNER JOIN promo_offer PO ON PO.promo_order_uid = O.UID
                INNER JOIN promo_condition PC ON PC.reference_uid = PO.UID
                INNER JOIN promo_condition PC1 ON PC1.reference_uid = PO.promo_order_uid
                INNER JOIN promo_order_item OI ON OI.Promotion_UID = P.UID
                INNER JOIN scheme_customer_mapping_data scmd ON scmd.scheme_uid = P.uid 
                inner join Schemes S on S.scheme_code=p.code and s.org_uid=scmd.Store_UID --and s.sku_uid=CAST(OI.item_criteria_selected AS VARCHAR(MAX))
                GROUP BY scmd.scheme_uid , P.code , scmd.Store_UID,  PC.condition_type , PC.[max],
                PC1.[Max],PC1.[Min], scmd.approved_time, OI.item_criteria_type , 
                CONVERT(VARCHAR,OI.item_criteria_selected), P.modified_time
                ) tbl WHERE RowNum =1
                """);
                var parameters = new
                {
                    pouid = pouid,
                    //, SKUUIds = sKUUIDs
                };
                //if (sKUUIDs != null && sKUUIDs.Any())
                //{
                //    sql.Append($" AND SKU.UID IN @SKUUIds ");
                //}
                List<IQPSSchemePO> list = await ExecuteQueryAsync<IQPSSchemePO>(sql.ToString(), parameters);
                List<IQPSSchemePO> listToReturn = new List<IQPSSchemePO>();
                foreach (SKUFilter filter in filters)
                {
                    foreach (IQPSSchemePO qps in list.OrderByDescending(e => e.ModifiedTime))
                    {
                        if (qps.Keyword.Contains(filter.SKUUID))
                        {
                            IQPSSchemePO qpsObj = new QPSSchemePO((QPSSchemePO)qps);
                            qpsObj.SKU_UID = filter.SKUUID;
                            listToReturn.Add(qpsObj);
                            break;
                        }
                        else
                        {
                            if (filter.FilterKeys.Contains(qps.Keyword))
                            {
                                IQPSSchemePO qpsObj = new QPSSchemePO((QPSSchemePO)qps);
                                qpsObj.SKU_UID = filter.SKUUID;
                                listToReturn.Add(qpsObj);
                                break;
                            }
                        }
                    }
                }

                //var filteredList = listToReturn
                //    .GroupBy(item => item.SKU_UID)
                //    .Select(group => group.OrderByDescending(item => item.ModifiedTime).First())
                //    .ToList();
                return listToReturn;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
