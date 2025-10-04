using Microsoft.Extensions.Configuration;
using System.Text;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Classes
{
    public class OraclePurchaseOrderDetailsDL : OracleServerDBManager, IPurchaseOrderDetailsDL
    {
        public OraclePurchaseOrderDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<string> InsertPurchaseOrderIntoOraceleStaging(List<Iint_PurchaseOrderHeader> purchaseOrderHeaders, List<Iint_PurchaseOrderLine> purchaseOrderLines)
        {
            try
            {
               int headerCount= await PurchaseHeaderInsertion(purchaseOrderHeaders);
                if (headerCount < 0)
                    throw new Exception("Purchase Order Header Insertion Failed");
               int lineCount= await PurchaseLineInsertion(purchaseOrderLines);
                if (lineCount < 0)
                    throw new Exception("Purchase Order Line Insertion Failed");

                return "success";
            }
            catch { throw; }
        }
        private async Task<int> PurchaseHeaderInsertion(List<Iint_PurchaseOrderHeader> purchaseOrderHeaders)
        {
            try
            {
                //var sql = new StringBuilder($@"insert into cux.cux_cmi_dms_so_headers (purchase_order_uid,dms_order_number,dms_order_date,order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person,inserted_on)
                //values (:PurchaseOrderUid,:DmsOrderNumber,:DmsOrderDate,:OrderType,:SalesDivision,:SalesOffice,:DealerCode,:BillTo,:ShipTo,:OperatingUnit,:WarehouseCode,:SalesPerson,TO_DATE(:InsertedOn, 'DD-MON-YYYY HH24:MI:SS'))");
                 var sql = new StringBuilder($@"insert into cux.cux_cmi_dms_so_headers (purchase_order_uid,dms_order_number,dms_order_date,order_type,sales_division,sales_office,dealer_code,bill_to,ship_to,operating_unit,warehouse_code,sales_person)
                values (:PurchaseOrderUid,:DmsOrderNumber,:DmsOrderDate,:OrderType,:SalesDivision,:SalesOffice,:DealerCode,:BillTo,:ShipTo,:OperatingUnit,:WarehouseCode,:SalesPerson)");
                return await ExecuteNonQueryAsync(sql.ToString(), purchaseOrderHeaders);
            }
            catch { throw; }
        }

        private async Task<int> PurchaseLineInsertion(List<Iint_PurchaseOrderLine> purchaseOrderLines)
        {
            try
            {
                var sql = new StringBuilder($@"insert into cux.cux_cmi_dms_so_lines (purchase_order_line_uid,purchase_order_uid,item_code,ordered_qty,mrp,dp,laddering_percentage,laddering_discount,
                sell_in_discount_unit_value,sell_in_discount_unit_percentage,sell_in_discount_total_value,net_unit_price,total_amount,tax_percetage,sell_in_cn_p1_unit_percentage,
                sell_in_cn_p1_unit_value,sell_in_cn_p1_value,cash_discount_percentage,cash_discount_value,sell_in_p2_amount,sell_in_p3_amount,p3_standing_amount,
                sell_in_scheme_code,qps_scheme_code,p2_qps_unit_value,p2_qps_total_value,p3_qps_unit_value,p3_qps_total_value) 
                values (:PurchaseOrderLineUid,:PurchaseOrderUid,:ItemCode,:OrderedQty,:Mrp,:Dp,:LadderingPercentage,:LadderingDiscount,:SellInDiscountUnitValue,
                :SellInDiscountUnitPercentage,:SellInDiscountTotalValue,:NetUnitPrice,:TotalAmount,:TaxPercentage,:SellInCnP1UnitPercentage,:SellInCnP1UnitValue,
                :SellInCnP1Value,:CashDiscountPercentage,:CashDiscountValue,:SellInP2Amount,:SellInP3Amount,:P3StandingAmount,
                :SellInSchemeCode,:QpsSchemeCode,:P2QpsUnitValue,:P2QpsTotalValue,:P3QpsUnitValue,:P3QpsTotalValue )");
               
                return await ExecuteNonQueryAsync(sql.ToString(), purchaseOrderLines);
            }
            catch { throw; }
        }
         


    }
}
