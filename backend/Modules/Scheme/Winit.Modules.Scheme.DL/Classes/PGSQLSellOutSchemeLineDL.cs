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
    public class PGSQLSellOutSchemeLineDL:PostgresDBManager, ISellOutSchemeLineDL
    {
        public PGSQLSellOutSchemeLineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                        @TotalCreditNoteAmount, @SerialNos::json,@Uid, 
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
                 serial_nos = @SerialNos::json, modified_by=@ModifiedBy, 
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

        public Task<int> CreateSellOutLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateSellOutSchemeLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISellOutSchemeLine>> GetSellOutSchemeLinesByUIDs(List<string> UIDs)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPreviousOrders>> GetPreviousOrdersByChannelPartnerUID(string UID)
        {
            throw new NotImplementedException();
        }
    }
}
