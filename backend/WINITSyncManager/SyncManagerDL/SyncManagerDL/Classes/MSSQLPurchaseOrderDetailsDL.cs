using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;


namespace SyncManagerDL.Classes
{
    public class MSSQLPurchaseOrderDetailsDL : SqlServerDBManager, IPurchaseOrderDetailsDL
    {
        public MSSQLPurchaseOrderDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<Iint_PurchaseOrderHeader>> GetPurchaseOrderHeaderDetails(IEntityDetails entityDetails)
        {
            try
            {
                List<Iint_PurchaseOrderHeader> purchaseOrderHeader = await GetPurchaseOrderHeadersToPush();
                await InsertIntoHeaderMonthTable(purchaseOrderHeader, entityDetails);
                await InsertIntoHeaderQueueTable(entityDetails);
                var parameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var sql = new StringBuilder($@" select id,sync_log_id,uid, purchase_order_uid,dms_order_number,dms_order_date,
                order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person from {Int_DbTableName.PurchaseOrderHeader + Int_DbTableName.QueueTableSuffix}  where sync_log_id=@SyncLogDetailId");
                List<Iint_PurchaseOrderHeader> purchaseOrdersToPush = await ExecuteQueryAsync<Iint_PurchaseOrderHeader>(sql.ToString(), parameters);
                return purchaseOrdersToPush;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Iint_PurchaseOrderLine>> GetPurchaseOrderLineDetails(IEntityDetails entityDetails)
        {
            try
            {
                List<Iint_PurchaseOrderLine> purchaseOrderLines = await GetPurchaseOrderLinesToPush();
                await InsertIntoLineMonthTable(purchaseOrderLines, entityDetails);
                await InsertIntoLineQueueTable(entityDetails);
                var parameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var sql = new StringBuilder($@" select id,headerid,sync_log_id,uid,purchase_order_line_uid,purchase_order_uid,item_code,ordered_qty,mrp,dp,laddering_percentage,laddering_discount,
                sell_in_discount_unit_value,sell_in_discount_unit_percentage,sell_in_discount_total_value,net_unit_price,total_amount,tax_percentage,
                sell_in_cn_p1_unit_percentage,sell_in_cn_p1_unit_value,sell_in_cn_p1_value,cash_discount_percentage,cash_discount_value,sell_in_p2_amount,
                sell_in_p3_amount,p3_standing_amount,sell_in_scheme_code,qps_scheme_code,p2_qps_unit_value,p2_qps_total_value,p3_qps_unit_value,p3_qps_total_value
                from {Int_DbTableName.PurchaseOrderLine + Int_DbTableName.QueueTableSuffix}  where sync_log_id=@SyncLogDetailId");
                List<Iint_PurchaseOrderLine> purchaseOrdersToPush = await ExecuteQueryAsync<Iint_PurchaseOrderLine>(sql.ToString(), parameters);
                return purchaseOrdersToPush;
            }
            catch
            {
                throw;
            }
        }
        private async Task<List<Iint_PurchaseOrderLine>> GetPurchaseOrderLinesToPush()
        {
            try
            {
                //string? db = await GetSettingValueByKey("DB");
                //if (db == null)
                //    throw new Exception("There is no value in setting table for DB key");

                string? db = await GetSettingValueByKey("DB") ?? throw new Exception("There is no value in setting table for DB key");
                var parameters = new Dictionary<string, object?>() { };
                var sql = new StringBuilder($@"  select DISTINCT pl.id,ph.id as headerid, pl.uid as purchase_order_line_uid,purchase_order_header_uid as purchase_order_uid,
                pl.sku_code as item_code,pl.final_qty as ordered_qty,pl.mrp,pl.dp_price as dp,pl.laddering_percentage,
                pl.laddering_discount,pl.sell_in_discount_unit_value,pl.sell_in_discount_unit_percentage,
                pl.sell_in_discount_total_value,pl.effective_unit_price net_unit_price,pl.effective_unit_price * pl.final_qty total_amount, 
              case when  Isnull(pl.effective_unit_price,0)<>0 then  CAST(ROUND((pl.effective_unit_tax / pl.effective_unit_price) * 100,2) AS DECIMAL(18,2)) else 0 end tax_percentage,
                pl.sell_in_cn_p1_unit_percentage,pl.sell_in_cn_p1_unit_value,pl.sell_in_cn_p1_value,
                pl.cash_discount_percentage,pl.cash_discount_value,pl.sell_in_p2_amount,pl.sell_in_p3_amount,pl.p3_standing_amount,
                pl.sell_in_scheme_code,pl.qps_scheme_code, case when  Isnull(pl.effective_unit_price,0)<>0 then   CAST(ROUND((pl.p2_qps_total_value / pl.final_qty) ,2) AS DECIMAL(18,2))  
                 else 0 end p2_qps_unit_value,pl.p2_qps_total_value,case when  Isnull(pl.effective_unit_price,0)<>0 then   CAST(ROUND((pl.p3_qps_total_value / pl.final_qty) ,2) AS DECIMAL(18,2))  
                 else 0 end p3_qps_unit_value,pl.p3_qps_total_value
                from {db}.purchase_order_line PL 
                inner join {db}.purchase_order_header PH on PH.uid=PL.purchase_order_header_uid 
                inner join {db}.int_pushed_data_status I on PH.uid=I.linked_item_uid and I.linked_item_type='PurchaseOrder'
                where I.status='pending' ");
                List<Iint_PurchaseOrderLine> purchaseOrders = await ExecuteQueryAsync<Iint_PurchaseOrderLine>(sql.ToString(), parameters);
                return purchaseOrders;
            }
            catch
            {
                throw;
            }
        }
        private async Task<List<Iint_PurchaseOrderHeader>> GetPurchaseOrderHeadersToPush()
        {
            try
            {
                //string? db = await GetSettingValueByKey("DB");
                //if (db == null)
                //    throw new Exception("There is no value in setting table for DB key");
                string? db = await GetSettingValueByKey("DB") ?? throw new Exception("There is no value in setting table for DB key");
                var parameters = new Dictionary<string, object?>() { };
                var sql = new StringBuilder($@" select DISTINCT ph.id ,ph.uid as purchase_order_uid,order_number as dms_order_number,FORMAT(order_date, 'dd-MMM-yyyy HH:mm:ss') as dms_order_date,
                'PurchaseOrder' order_type,od.code as sales_division,so.code sales_office, o.code as dealer_code,
                ba.custom_field2 as bill_to,sa.custom_field2 as ship_to,ou.code operating_unit,
                WH.code AS warehouse_code,e.code as sales_person
                from  {db}.purchase_order_header ph
                inner join  {db}.int_pushed_data_status I on PH.uid=I.linked_item_uid and I.linked_item_type='PurchaseOrder'
                inner join  {db}.emp e on  e.uid=ph.reporting_emp_uid
                inner join  {db}.org o on o.uid = ph.org_uid
                inner join  {db}.org od on od.uid=ph.division_uid and od.org_type_uid='Supplier'
                inner join  {db}.org ou on ou.uid=ph.org_unit_uid  
                inner join  {db}.org WH on WH.uid=ph.source_warehouse_uid  
                inner join  {db}.address sa on sa.uid=ph.shipping_address_uid AND ISNULL(sa.custom_field2, '') != '' 
                inner join  {db}.address ba on ba.uid=ph.billing_address_uid AND ISNULL(ba.custom_field2, '') != ''
                inner join  {db}.sales_office so on so.uid=sa.sales_office_uid
                where i.status='pending' ");
                List<Iint_PurchaseOrderHeader> purchaseOrders = await ExecuteQueryAsync<Iint_PurchaseOrderHeader>(sql.ToString(), parameters);
                return purchaseOrders;
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> InsertIntoLineMonthTable(List<Iint_PurchaseOrderLine> purchaseLines, IEntityDetails entityDetails)
        {
            try
            {
                purchaseLines.ForEach(item => { item.SyncLogDetailId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {Int_DbTableName.PurchaseOrderLine + Int_DbTableName.MonthTableSuffix} (id,headerid,sync_log_id,uid,purchase_order_line_uid,purchase_order_uid,item_code,ordered_qty,mrp,dp,laddering_percentage,laddering_discount,
                sell_in_discount_unit_value,sell_in_discount_unit_percentage,sell_in_discount_total_value,net_unit_price,total_amount,tax_percentage,
                sell_in_cn_p1_unit_percentage,sell_in_cn_p1_unit_value,sell_in_cn_p1_value,cash_discount_percentage,cash_discount_value,sell_in_p2_amount,sell_in_p3_amount,p3_standing_amount,
                sell_in_scheme_code,qps_scheme_code,p2_qps_unit_value,p2_qps_total_value,p3_qps_unit_value,p3_qps_total_value) 
                VALUES (@Id,@HeaderId,@SyncLogDetailId, @UID,@PurchaseOrderLineUid,@PurchaseOrderUid,@ItemCode,@OrderedQty,@Mrp,@Dp,@LadderingPercentage,@LadderingDiscount,@SellInDiscountUnitValue,
                @SellInDiscountUnitPercentage,@SellInDiscountTotalValue,@NetUnitPrice,@TotalAmount,@TaxPercentage,@SellInCnP1UnitPercentage,@SellInCnP1UnitValue,
                @SellInCnP1Value,@CashDiscountPercentage,@CashDiscountValue,@SellInP2Amount,@SellInP3Amount,@P3StandingAmount,
                @SellInSchemeCode,@QpsSchemeCode,@P2QpsUnitValue,@P2QpsTotalValue,@P3QpsUnitValue,@P3QpsTotalValue)");
                return await ExecuteNonQueryAsync(monthSql.ToString(), purchaseLines);

            }
            catch { throw; }
        }
        private async Task<int> InsertIntoHeaderMonthTable(List<Iint_PurchaseOrderHeader> purchaseHeaders, IEntityDetails entityDetails)
        {
            try
            {
                purchaseHeaders.ForEach(item => { item.SyncLogDetailId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {Int_DbTableName.PurchaseOrderHeader + Int_DbTableName.MonthTableSuffix} (id,sync_log_id,uid, purchase_order_uid,dms_order_number,dms_order_date,
                order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person) 
                VALUES (@Id,@SyncLogDetailId, @UID,@PurchaseOrderUid,@DmsOrderNumber,@DmsOrderDate,@OrderType,@SalesDivision,@SalesOffice,@DealerCode,@BillTo,@ShipTo,@OperatingUnit,@WarehouseCode,@SalesPerson)");
                return await ExecuteNonQueryAsync(monthSql.ToString(), purchaseHeaders);

            }
            catch { throw; }
        }
        private async Task<int> InsertIntoLineQueueTable(IEntityDetails entityDetails)
        {
            try
            {
                var queueParameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var truncateQuery = new StringBuilder($@" truncate table  {Int_DbTableName.PurchaseOrderLine + Int_DbTableName.QueueTableSuffix};");
                await ExecuteNonQueryAsync(truncateQuery.ToString(), null);
                var queueSql = new StringBuilder($@" Insert into {Int_DbTableName.PurchaseOrderLine + Int_DbTableName.QueueTableSuffix} (id,headerid,sync_log_id,uid,purchase_order_line_uid,purchase_order_uid,item_code,ordered_qty,mrp,dp,laddering_percentage,laddering_discount,
                sell_in_discount_unit_value,sell_in_discount_unit_percentage,sell_in_discount_total_value,net_unit_price,total_amount,tax_percentage,
                sell_in_cn_p1_unit_percentage,sell_in_cn_p1_unit_value,sell_in_cn_p1_value,cash_discount_percentage,cash_discount_value,sell_in_p2_amount,sell_in_p3_amount,p3_standing_amount,
                sell_in_scheme_code,qps_scheme_code,p2_qps_unit_value,p2_qps_total_value,p3_qps_unit_value,p3_qps_total_value)
                
                select id,headerid,sync_log_id,uid,purchase_order_line_uid,purchase_order_uid,item_code,ordered_qty,mrp,dp,laddering_percentage,laddering_discount,
                sell_in_discount_unit_value,sell_in_discount_unit_percentage,sell_in_discount_total_value,net_unit_price,total_amount,tax_percentage,
                sell_in_cn_p1_unit_percentage,sell_in_cn_p1_unit_value,sell_in_cn_p1_value,cash_discount_percentage,cash_discount_value,sell_in_p2_amount,sell_in_p3_amount,p3_standing_amount,
                sell_in_scheme_code,qps_scheme_code,p2_qps_unit_value,p2_qps_total_value,p3_qps_unit_value,p3_qps_total_value from {Int_DbTableName.PurchaseOrderLine + Int_DbTableName.MonthTableSuffix} where sync_log_id =@SyncLogDetailId");

                return await ExecuteNonQueryAsync(queueSql.ToString(), queueParameters);
            }
            catch { throw; }
        }
        private async Task<int> InsertIntoHeaderQueueTable(IEntityDetails entityDetails)
        {
            try
            {
                var queueParameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var truncateQuery = new StringBuilder($@" truncate table  {Int_DbTableName.PurchaseOrderHeader + Int_DbTableName.QueueTableSuffix};");
                await ExecuteNonQueryAsync(truncateQuery.ToString(), null);
                var queueSql = new StringBuilder($@" Insert into {Int_DbTableName.PurchaseOrderHeader + Int_DbTableName.QueueTableSuffix} (id,sync_log_id,uid, purchase_order_uid,dms_order_number,dms_order_date,
                order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person)
                select id,sync_log_id,uid, purchase_order_uid,dms_order_number,dms_order_date,
                order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person from {Int_DbTableName.PurchaseOrderHeader + Int_DbTableName.MonthTableSuffix} where sync_log_id =@SyncLogDetailId");

                return await ExecuteNonQueryAsync(queueSql.ToString(), queueParameters);
            }
            catch { throw; }
        }

    }
}
