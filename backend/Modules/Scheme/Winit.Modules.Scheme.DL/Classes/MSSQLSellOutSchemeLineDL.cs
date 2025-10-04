using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class MSSQLSellOutSchemeLineDL : SqlServerDBManager, ISellOutSchemeLineDL
    {
        public MSSQLSellOutSchemeLineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<ISellOutSchemeLine>> SelectAllSellOutSchemeLine(
                        List<SortCriteria> sortCriterias,
                        int pageNumber,
                        int pageSize,
                        List<FilterCriteria> filterCriterias,
                        bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                             sell_out_SCHEME_header_uid, line_number, sku_uid, is_deleted, 
                                             qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                                             total_credit_note_amount, serial_nos,uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                                         FROM 
                                             sell_out_scheme_line
                                         ) As SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                             (SELECT 
                                     sell_out_SCHEME_header_uid, line_number, sku_uid, is_deleted, 
                                     qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                                     total_credit_note_amount, serial_nos,uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                                 FROM 
                                     sell_out_scheme_line
                             ) As SubQuery");
                }

                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ISellOutSchemeLine>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY line_number OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<ISellOutSchemeLine> sellOutSchemeLines = await ExecuteQueryAsync<ISellOutSchemeLine>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<ISellOutSchemeLine> pagedResponse = new PagedResponse<ISellOutSchemeLine>
                {
                    PagedData = sellOutSchemeLines,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISellOutSchemeLine> GetSellOutSchemeLineByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };

                var sql = @"SELECT 
                sell_out_SCHEME_header_uid, line_number, sku_uid, is_deleted, 
                qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                total_credit_note_amount, serial_nos,uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
            FROM 
                sell_out_scheme_line
            WHERE uid = @UID";

                return await ExecuteSingleAsync<ISellOutSchemeLine>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ISellOutSchemeLine>> GetSellOutSchemeLinesByUIDs(List<string> UIDs)
        {
            try
            {
                if (UIDs == null || !UIDs.Any())
                {
                    return new List<ISellOutSchemeLine>();
                }
                string sql = $@"
            SELECT 
                sell_out_SCHEME_header_uid, line_number, sku_uid, is_deleted, 
                qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                total_credit_note_amount, serial_nos, uid, 
                created_by, created_time, modified_by, modified_time, server_add_time, 
                server_modified_time
            FROM 
                sell_out_scheme_line
            WHERE uid IN @UIDs";

                var parameters = new Dictionary<string, object>
                {
                    { "UIDs", UIDs }
                };

                return (await ExecuteQueryAsync<ISellOutSchemeLine>(sql, parameters)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine)
        {
            try
            {
                var sql = @"
                           INSERT INTO sell_out_scheme_line (
                        sell_out_scheme_header_uid, line_number, sku_uid, is_deleted, 
                        qty, qty_scanned, reason, unit_price, unit_credit_note_amount, 
                        total_credit_note_amount, serial_nos,uid, 
                                                        created_by, created_time, modified_by, modified_time, server_add_time, 
                                                        server_modified_time
                    ) VALUES (
                        @SellOutSchemeHeaderUid, @LineNumber, @SkuUid, @IsDeleted, 
                        @Qty, @QtyScanned, @Reason, @UnitPrice, @UnitCreditNoteAmount, 
                        @TotalCreditNoteAmount, @SerialNos,@Uid, 
                                @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                @ServerModifiedTime
                    );";

                return await ExecuteNonQueryAsync(sql, sellOutSchemeLine);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> UpdateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine)
        {
            var sql = @"
        UPDATE sell_out_scheme_line
             SET
                 sell_out_SCHEME_header_uid = @SellOutSchemeHeaderUid,
                 line_number = @LineNumber,
                 sku_uid = @SkuUid,
                 is_deleted = @IsDeleted,
                 qty = @Qty,
                 qty_scanned = @QtyScanned,
                 reason = @Reason,
                 unit_price = @UnitPrice,
                 unit_credit_note_amount = @UnitCreditNoteAmount,
                 total_credit_note_amount = @TotalCreditNoteAmount,
                 serial_nos = @SerialNos, modified_by=@ModifiedBy, 
                 modified_time=@ModifiedTime,
                 server_modified_time=@ServerModifiedTime
             WHERE
                 uid = @Uid;";

            return await ExecuteNonQueryAsync(sql, sellOutSchemeLine);
        }
        public async Task<int> DeleteSellOutSchemeLine(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        {"UID", UID}
    };
            var sql = @"DELETE FROM sell_out_scheme_line WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> CreateSellOutLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            try
            {
                string sql = """
                                INSERT INTO sell_out_scheme_line (
                                    uid, created_by, created_time, modified_by, modified_time, 
                                    server_add_time, server_modified_time, ss, sell_out_scheme_header_uid, 
                                    line_number, sku_uid, is_deleted, qty, qty_scanned, reason, 
                                    unit_price, unit_credit_note_amount, total_credit_note_amount, serial_nos
                                ) VALUES (
                                    @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                                    @ServerAddTime, @ServerModifiedTime, @SS, @SellOutSchemeHeaderUID, 
                                    @LineNumber, @SKUUID, @IsDeleted, @Qty, @QtyScanned, @Reason, 
                                    @UnitPrice, @UnitCreditNoteAmount, @TotalCreditNoteAmount, @SerialNos
                                );
                                """;
                return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, sellOutSchemeLines);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateSellOutSchemeLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            try
            {
                var sql = @"
                            UPDATE sell_out_scheme_line
                                 SET
                                     sell_out_SCHEME_header_uid = @SellOutSchemeHeaderUid,
                                     line_number = @LineNumber,
                                     sku_uid = @SkuUid,
                                     is_deleted = @IsDeleted,
                                     qty = @Qty,
                                     qty_scanned = @QtyScanned,
                                     reason = @Reason,
                                     unit_price = @UnitPrice,
                                     unit_credit_note_amount = @UnitCreditNoteAmount,
                                     total_credit_note_amount = @TotalCreditNoteAmount,
                                     serial_nos = @SerialNos, modified_by=@ModifiedBy, 
                                     modified_time=@ModifiedTime,
                                     server_modified_time=@ServerModifiedTime
                                 WHERE
                                     uid = @Uid;";
                return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, sellOutSchemeLines);
            }
            catch(Exception ex)
            {
                throw;
            }
            

           
        }

        public async Task<List<IPreviousOrders>> GetPreviousOrdersByChannelPartnerUID(string UID)
        {
            try
            {
                if (string.IsNullOrEmpty(UID))
                {
                    return new List<IPreviousOrders>();
                }
                string sql = $@"select * from(SELECT 
                                            oh.uid AS OrderUID,
                                            oh.org_uid AS OrgUID,
                                            ol.sku_uid AS SKUUID,
                                            ol.sku_code AS SKUCode,
                                            s.name AS SKUName,
                                            ol.unit_price AS UnitPrice,
                                            oh.modified_time AS ModifiedTime,
	                                        ol.unit_price as LastUnitPrice,
	                                        ROW_NUMBER() over (Partition by ol.sku_uid  order by oh.modified_time desc) as rowNum
    
                                        FROM 
                                            purchase_order_header oh
                                        INNER JOIN 
                                            purchase_order_line ol 
                                        ON 
                                            ol.purchase_order_header_uid = oh.uid
                                        INNER JOIN 
                                            sku s 
                                        ON 
                                            ol.sku_uid = s.uid) SQ
                                        WHERE 
                                            OrgUID = @UID and rowNum=1
                                        ORDER BY 
                                            ModifiedTime DESC, 
                                            OrderUID,SKUUID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

             return (await ExecuteQueryAsync<IPreviousOrders>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching previous orders: {ex.Message}", ex);
            }
        }

    }
}
