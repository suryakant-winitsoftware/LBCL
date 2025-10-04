using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockAudit.DL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.StockAudit.Model.Classes;
using System.Data;
namespace Winit.Modules.StockAudit.DL.Classes
{
    public class SQLiteStockAuditDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IWHStockAuditDL
    {
        public SQLiteStockAuditDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        //Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel);
        //   Task<PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
        //   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StockType);
        //   Task<(List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>, List<IWHStockRequestLineItemView>)> SelectLoadRequestDataByUID(string UID);
        //   Task<int> CUDWHStockAuditLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
        //       object connection = null, object transaction = null);

        public async Task<int> CUDWHStock(Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel wHRequestTempleteModel)
        {
            int count = 0;

            try
            {
                using (var connection = SqliteConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            if (wHRequestTempleteModel != null && wHRequestTempleteModel.WHStockAuditItemView != null)
                            {
                                count += await CUDWHStockAudit(wHRequestTempleteModel.WHStockAuditItemView, connection, transaction);

                            }
                            if (wHRequestTempleteModel != null && wHRequestTempleteModel.WHStockAuditDetailsItemView != null)
                            {
                                count += await CUDWHStockAuditLine(wHRequestTempleteModel.WHStockAuditDetailsItemView, connection, transaction);
                            }

                            transaction.Commit();

                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally { connection.Close(); }
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<int> CUDWHStockAudit(Winit.Modules.StockAudit.Model.Classes.WHStockAuditItemView wHStockAudit,
                   IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;

            try
            {

                switch (wHStockAudit.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:

                        count += await CreateWHStockAudit(wHStockAudit, connection, transaction);
                        break;
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        private async Task<int> CreateWHStockAudit(Winit.Modules.StockAudit.Model.Classes.WHStockAuditItemView wHStockAudit,
          IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"INSERT INTO wh_stock_audit (
    id,uid, company_uid, warehouse_uid, org_uid, audit_by, start_time , end_time , status 	, 
    job_position_uid , remarks , parent_uid , audit_number , parent_warehouse_uid , reference_uid , 
    user_journey_uid , adjustment_status , source , wh_stock_audit_template_uid , has_unload_stock , 
    route_uid , ss, created_time , modified_time , server_add_time , server_modified_time , 
    is_last_route , calculation_status , calculation_start_time , calculation_end_time 
) VALUES (
     @Id,@UID, @CompanyUID, @WareHouseUID, @OrgUID, @AuditBy, @StartTime, @EndTime, @Status, 
    @JobPositionUID, @Remarks, @ParentUID, @AuditNumber, @ParentWarehouseUID, @ReferenceUID, 
    @UserJourneyUID, @AdjustmentStatus, @Source, @WHStockAuditTemplateUID, @HasUnloadStock, 
    @RouteUID, @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
    @IsLastRoute, @CalculationStatus, @CalculationStartTime, @CalculationEndTime
);";
            var Parameters = new Dictionary<string, object>
                        {
                { "@Id", wHStockAudit.Id },
                { "@UID", wHStockAudit.UID },
                { "@CompanyUID", wHStockAudit.CompanyUID },
                { "@WareHouseUID", wHStockAudit.WareHouseUID },
                { "@OrgUID", wHStockAudit.OrgUID },
                { "@AuditBy", wHStockAudit.AuditBy },
                { "@StartTime", wHStockAudit.StartTime },
                { "@EndTime", wHStockAudit.EndTime },
                { "@Status", wHStockAudit.Status },
                { "@JobPositionUID", wHStockAudit.JobPositionUID },
                { "@Remarks", wHStockAudit.Remarks },
                { "@ParentUID", wHStockAudit.ParentUID },
                { "@AuditNumber", wHStockAudit.AuditNumber },
                { "@ParentWarehouseUID", wHStockAudit.ParentWarehouseUID },
                { "@ReferenceUID", wHStockAudit.ReferenceUID },
                { "@UserJourneyUID", wHStockAudit.UserJourneyUID },
                { "@AdjustmentStatus", wHStockAudit.AdjustmentStatus },
                { "@Source", wHStockAudit.Source },
                { "@WHStockAuditTemplateUID", wHStockAudit.WHStockAuditTemplateUID },
                { "@HasUnloadStock", wHStockAudit.HasUnloadStock },
                { "@RouteUID", wHStockAudit.RouteUID },
                { "@SS", wHStockAudit.SS },
                { "@CreatedTime", wHStockAudit.CreatedTime },
                { "@ModifiedTime", wHStockAudit.ModifiedTime },
                { "@ServerAddTime", wHStockAudit.ServerAddTime },
                { "@ServerModifiedTime", wHStockAudit.ServerModifiedTime },
                { "@IsLastRoute", wHStockAudit.IsLastRoute },
                //{ "@LastAuditTime", wHStockAudit.LastAuditTime },
                { "@CalculationStatus", wHStockAudit.CalculationStatus },
                { "@CalculationStartTime", wHStockAudit.CalculationStartTime },
                { "@CalculationEndTime", wHStockAudit.CalculationEndTime },          
                        };
            return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        }
        public async Task<int> CUDWHStockAuditLine(List<Winit.Modules.StockAudit.Model.Classes.WHStockAuditDetailsItemView> wHStockRequestLines,
           IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            if (wHStockRequestLines == null || wHStockRequestLines.Count == 0)
            {
                return count;
            }
            List<string> uidList = wHStockRequestLines.Select(po => po.UID).ToList();
            List<string> deletedUidList = wHStockRequestLines.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {


                foreach (WHStockAuditDetailsItemView wHStockAuditLine in wHStockRequestLines)
                {
                    switch (wHStockAuditLine.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:


                            await CreateWHStockAuditLine(wHStockAuditLine, connection, transaction);
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
        private async Task<int> CreateWHStockAuditLine(Winit.Modules.StockAudit.Model.Classes.WHStockAuditDetailsItemView wHStockAuditLine,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            { 
                var Query = @"INSERT INTO wh_stock_audit_line (
                            id,uid,wh_stock_audit_uid ,line_number ,sku_uid ,sku_code ,batch_number ,expiry_date ,serial_no ,base_price ,expected_qty ,
                            expected_value ,uom1 ,uom2,uom,conversion1 ,conversion2 ,available_qty1 ,available_qty2 ,available_qty ,available_value ,
                            stock_type ,discrepancy_qty ,discrepancy_value ,open_balance_bu ,stock_receipt_qty_bu ,delivered_qty_bu ,credits_qty_bu ,
                            credit_cage_qty_bu ,credit_customer_qty_bu ,adjustment_qty_bu ,closing_balance_qty_bu ,total_stock_count_bu ,variance_qty_bu ,
                            variance_value,final_closing_qty,ss ,created_time ,modified_time ,server_add_time ,server_modified_time ,total_stock_count_value ,
                            last_reconciliation_time 
                            ) VALUES (
                            @Id,@UID,@WHStockAuditUID,@LineNumber,@SKUUID,@SKUCode,@BatchNumber,@ExpiryDate,@SerialNo,@BasePrice,@ExpectedQty,
                            @ExpectedValue,@UOM1,@UOM2,@UOM,@Conversion1,@Conversion2,@AvailableQty1,@AvailableQty2,@AvailableQty,@AvailableValue
                            ,@StockType,@DiscrepencyQty,@DiscrepencyValue,@OpenBalanceBU,@StockReceiptQtyBU,@DeliveredQtyBU,@CreditsQtyBU,
                            @CreditCageQtyBU,@CreditCustomerQtyBU,@AdjustmentQtyBU,@ClosingBalanceQtyBU,@TotalStockCountBU,@VarianceQtyBU,
                            @VarianceValue,@FinalClosingQty,@SS,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@TotalStockCountValue,
                            @LastReconciliationTime
                            );";

                var Parameters = new Dictionary<string, object>
                {
                    { "@Id", wHStockAuditLine.Id },
                    { "@UID", wHStockAuditLine.UID }, 
                    { "@WHStockAuditUID", wHStockAuditLine.WHStockAuditUID },
                    { "@LineNumber", wHStockAuditLine.LineNumber },
                    { "@SKUUID", wHStockAuditLine.SKUUID },
                    { "@SKUCode", wHStockAuditLine.SKUCode },
                    { "@BatchNumber", wHStockAuditLine.BatchNumber },
                    { "@ExpiryDate", wHStockAuditLine.ExpiryDate },
                    { "@SerialNo", wHStockAuditLine.SerialNo },
                    { "@BasePrice", wHStockAuditLine.BasePrice },
                    { "@ExpectedQty", wHStockAuditLine.ExpectedQty },
                    { "@ExpectedValue", wHStockAuditLine.ExpectedValue },
                    { "@UOM1", wHStockAuditLine.UOM1 },
                    { "@UOM2", wHStockAuditLine.UOM2 },
                    { "@UOM", wHStockAuditLine.UOM },
                    { "@Conversion1", wHStockAuditLine.Conversion1 },
                    { "@Conversion2", wHStockAuditLine.Conversion2 },
                    { "@AvailableQty1", wHStockAuditLine.AvailableQty1 },
                    { "@AvailableQty2", wHStockAuditLine.AvailableQty2 },
                    { "@AvailableQty", wHStockAuditLine.AvailableQty },
                    { "@AvailableValue", wHStockAuditLine.AvailableValue },
                    { "@StockType", wHStockAuditLine.StockType },
                    { "@DiscrepencyQty", wHStockAuditLine.DiscrepencyQty },
                    { "@DiscrepencyValue", wHStockAuditLine.DiscrepencyValue },
                    { "@OpenBalanceBU", wHStockAuditLine.OpenBalanceBU },
                    { "@StockReceiptQtyBU", wHStockAuditLine.StockReceiptQtyBU },
                    { "@DeliveredQtyBU", wHStockAuditLine.DeliveredQtyBU },
                    { "@CreditsQtyBU", wHStockAuditLine.CreditsQtyBU },
                    { "@CreditCageQtyBU", wHStockAuditLine.CreditCageQtyBU },
                    { "@CreditCustomerQtyBU", wHStockAuditLine.CreditCustomerQtyBU },
                    { "@AdjustmentQtyBU", wHStockAuditLine.AdjustmentQtyBU },
                    { "@ClosingBalanceQtyBU", wHStockAuditLine.ClosingBalanceQtyBU },
                    { "@TotalStockCountBU", wHStockAuditLine.TotalStockCountBU },
                    { "@VarianceQtyBU", wHStockAuditLine.VarianceQtyBU },
                    { "@VarianceValue", wHStockAuditLine.VarianceValue },
                    { "@FinalClosingQty", wHStockAuditLine.FinalClosingQty },
                    { "@SS", wHStockAuditLine.SS },
                    { "@CreatedTime", wHStockAuditLine.CreatedTime },
                    { "@ModifiedTime", wHStockAuditLine.ModifiedTime },
                    { "@ServerAddTime", wHStockAuditLine.ServerAddTime },
                    { "@ServerModifiedTime", wHStockAuditLine.ServerModifiedTime },
                    { "@TotalStockCountValue", wHStockAuditLine.TotalStockCountValue },
                    { "@LastReconciliationTime", wHStockAuditLine.LastReconciliationTime }
                };
                return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
            }
            catch (Exception Ex)
            {
                throw;
            }


        }
    }
}
