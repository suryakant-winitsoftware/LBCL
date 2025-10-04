using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.DL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Classes
{
    public class MSSQLProvisioningItemViewDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IProvisioningItemViewDL
    {
        public MSSQLProvisioningItemViewDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<IProvisionItemView> GetProvisioningLineItemDetailsByUID(string uID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  uID}
                };
                var sql = """

                        """;
                return await ExecuteSingleAsync<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResponse<IProvisionItemView>> SelectProvisioningLineItemsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            DECLARE @UID NVARCHAR(50);
                                            SET @UID = @INPUTUID
                                            select * from (
                                            SELECT 
                                                     I.org_uid AS channelPartner, 
                                                    I.invoice_date AS orderdate, 
                                                    L.sell_in_p2_amount AS Amount, 
                                                    'Credit' AS Type, 
                                                    I.invoice_number AS TransactionCode,
                                                    L.sku_uid AS ModelNo, 
                                                    L.qty AS Qty, 
                                                    L.unit_price AS AmountPerSKU, 
                                                    'Provision Created Against Sales Order' AS Description 
                                                FROM wallet_ledger w
                                                INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                WHERE w.source_type = 'Invoice' AND w.type = 'P2' 
                                                AND I.org_uid = @UID
                                            
                                                UNION ALL 
                                            
                                                SELECT 
                                                    I.org_uid AS channelPartner, 
                                                    I.invoice_date AS date, 
                                                    L.sell_in_p3_amount AS Amount, 
                                                    'Credit' AS Type, 
                                                    I.invoice_number AS TransactionCode,
                                                    L.sku_uid AS ModelNo, 
                                                    L.qty AS Qty, 
                                                    L.unit_price AS AmountPerSKU, 
                                                    'Provision Created Against Sales Order' AS Description  
                                                FROM wallet_ledger w
                                                INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                WHERE w.source_type = 'Invoice' AND w.type = 'P3' 
                                                AND I.org_uid = @UID
                                            
                                                UNION ALL 
                                            
                                                SELECT 
                                                    I.org_uid AS channelPartner, 
                                                    I.invoice_date AS date, 
                                                    L.p3_standing_amount AS Amount, 
                                                    'Credit' AS Type, 
                                                    I.invoice_number AS TransactionCode,
                                                    L.sku_uid AS ModelNo, 
                                                    L.qty AS Qty, 
                                                    L.unit_price AS AmountPerSKU, 
                                                    'Provision Created Against Sales Order' AS Description  
                                                FROM wallet_ledger w
                                                INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                WHERE w.source_type = 'Invoice' AND w.type = 'P3S' 
                                                AND I.org_uid = @UID) as subQuery
                                            
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                DECLARE @UID NVARCHAR(50);
                                                SET @UID = @INPUTUID
                                                select count(*) from (
                                                SELECT 
                                                         I.org_uid AS channelPartner, 
                                                        I.invoice_date AS orderdate, 
                                                        L.sell_in_p2_amount AS Amount, 
                                                        'Credit' AS Type, 
                                                        I.invoice_number AS TransactionCode,
                                                        L.sku_uid AS ModelNo, 
                                                        L.qty AS Qty, 
                                                        L.unit_price AS AmountPerSKU, 
                                                        'Provision Created Against Sales Order' AS Description 
                                                    FROM wallet_ledger w
                                                    INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                    INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                    WHERE w.source_type = 'Invoice' AND w.type = 'P2' 
                                                    AND I.org_uid = @UID
                                                
                                                    UNION ALL 
                                                
                                                    SELECT 
                                                        I.org_uid AS channelPartner, 
                                                        I.invoice_date AS date, 
                                                        L.sell_in_p3_amount AS Amount, 
                                                        'Credit' AS Type, 
                                                        I.invoice_number AS TransactionCode,
                                                        L.sku_uid AS ModelNo, 
                                                        L.qty AS Qty, 
                                                        L.unit_price AS AmountPerSKU, 
                                                        'Provision Created Against Sales Order' AS Description  
                                                    FROM wallet_ledger w
                                                    INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                    INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                    WHERE w.source_type = 'Invoice' AND w.type = 'P3' 
                                                    AND I.org_uid = @UID
                                                
                                                    UNION ALL 
                                                
                                                    SELECT 
                                                        I.org_uid AS channelPartner, 
                                                        I.invoice_date AS date, 
                                                        L.p3_standing_amount AS Amount, 
                                                        'Credit' AS Type, 
                                                        I.invoice_number AS TransactionCode,
                                                        L.sku_uid AS ModelNo, 
                                                        L.qty AS Qty, 
                                                        L.unit_price AS AmountPerSKU, 
                                                        'Provision Created Against Sales Order' AS Description  
                                                    FROM wallet_ledger w
                                                    INNER JOIN invoice_line L ON L.invoice_uid = w.source_uid 
                                                    INNER JOIN invoice I ON I.uid = L.invoice_uid
                                                    WHERE w.source_type = 'Invoice' AND w.type = 'P3S' 
                                                    AND I.org_uid = @UID) as subQuery
    
                                                """);
                }
           //     var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView> provisioningItems = await ExecuteQueryAsync<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView> pagedResponse = new PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>
                {
                    PagedData = provisioningItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }   
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
