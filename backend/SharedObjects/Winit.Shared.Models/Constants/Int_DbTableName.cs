using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants
{
    public class Int_DbTableName
    {
        public const string SyncLog = "int_sync_log";
        public const string SyncLogDetail = "int_sync_log_detail";
        public const string IntegrationQueue = "int_integration_queue";
        public const string CustomerMasterPush = "int_Customer_Master_Push";
        public const string PurchaseOrderHeader = "int_Purchase_Order_Header";
        public const string PurchaseOrderLine = "int_Purchase_Order_Line";
        public const string ItemMaster = "int_item_master";
        public const string PriceLaddering = "int_price_laddering";
        public const string PriceMaster = "int_price_master";
        public const string TaxMaster = "int_Tax_Master";
        public const string CustomerMasterPull = "int_Customer_Master_Pull";
        public const string InvoiceSerialNo = "int_invoice_serialNo";
        public const string InvoiceLine = "int_invoice_line";
        public const string InvoiceHeader = "int_invoice_header"; 
        public const string PurchaseOrderStatus = "int_PurchaseOrder_Status";
        public const string PurchaseOrderCancellation = "int_PurchaseOrder_Cancellation";
        public const string TemporaryCreditLimit = "int_temporary_credit_limit";
        public const string CreditNoteProvision = "int_credit_note_provision";
        public const string CreditNoteProvisionPush = "int_provision_creditnote_push";
        public static string MonthTableSuffix => DateTime.Now.ToString("yyMM");
        public const string QueueTableSuffix = "Queue";

    }
}
