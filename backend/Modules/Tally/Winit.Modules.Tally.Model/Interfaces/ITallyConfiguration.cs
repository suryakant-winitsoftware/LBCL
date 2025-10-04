using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallyConfiguration : IBaseModel
    {
        public int Id { get; set; }
        public string OrgUID { get; set; }
        public string ErrorLogType { get; set; }
        public string BaseDir { get; set; }
        public string LogFolder { get; set; }
        public string LogPath { get; set; }
        public string RequestLogFolder { get; set; }
        public string ResponseLogFolder { get; set; }
        public int IsRequestLogEnabled { get; set; }
        public int IsResponseLogEnabled { get; set; }
        public int PULL_LEDGER_FROM_TALLY { get; set; }
        public int PULL_ORDERS_FROM_TALLY { get; set; }
        public int PULL_INVENTORY_FROM_TALLY { get; set; }
        public int PULL_PAYMENT_RECEIPTS_FROM_TALLY { get; set; }
        public int PUSH_SALES_TO_TALLY { get; set; }
        public int PUSH_RETAILER_TO_TALLY { get; set; }
        public int PULL_SALES_FROM_TALLY { get; set; }
        public string WebSerivceApiEndpointSales { get; set; }
        public string WebSerivceApiEndpointRetailer { get; set; }
        public string TallyURL { get; set; }
        public string DMSURL { get; set; }
        public string COUNTRYNAME { get; set; }
        public string LEDSTATENAME { get; set; }
        public string GSTREGISTRATIONTYPE { get; set; }
        public string VOUCHERTYPE_Sales { get; set; }
        public string LEDGERNAME_ACCOUNTING { get; set; }
        public string LEDGER_PARENT { get; set; }
        public string STOCK_GODOWN_NAME { get; set; }
        public string STOCK_BATCH_NAME { get; set; }
        public string ORDER_DISCOUNT_NAME { get; set; }
        public string ROUND_TYPE { get; set; }
        public string ROUND_TYPE_NAME { get; set; }
        public string ROUND_METHOD_TYPE { get; set; }
        public string DistCode { get; set; }
        public string DistName { get; set; }
        public int PUSH_PURCHASE_TO_TALLY { get; set; }
        public string VOUCHERTYPE_PURCHASE { get; set; }
        public string PURCHASE_PARTYNAME { get; set; }
        public string WebSerivceApiEndpointPurchase { get; set; }
    }
}
