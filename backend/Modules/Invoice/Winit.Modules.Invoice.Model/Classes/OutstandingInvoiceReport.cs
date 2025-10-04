using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes
{
    public class OutstandingInvoiceReport : BaseModel ,IOutstandingInvoiceReport
    {
        public string Cgroup { get; set; }          // Customer Group
        public string Ccode { get; set; }           // Customer Code
        public string RegistryID { get; set; }      // Registry ID
        public string Cname { get; set; }           // Customer Name
        public string SONo { get; set; }            // Sales Order Number
        public string CommercialInvoiceNo { get; set; }   // Commercial / Tax Invoice No
        public DateTime VatInvoiceDate { get; set; }       // VAT / Tax Invoice Date
        public string ARINVNO { get; set; }         // AR Invoice No
        public DateTime ARDate { get; set; }        // AR Date
        public decimal InvAmount { get; set; }      // Invoice Amount
        public decimal UnpaidAmount { get; set; }   // Unpaid Amount
        public string InvoiceType { get; set; }     // Invoice Type
        public string TOP { get; set; }             // Terms of Payment
        public DateTime InvoiceDueDate { get; set; }  // Invoice Due Date
        public string EmpCode { get; set; }         // Employee Code
        public string EmpName { get; set; }         // Employee Name
        public string SalesDealerCode { get; set; } // Sales Dealer Code
        public string SalesDealerName { get; set; } // Sales Dealer Name
        public string ServiceDealerCode { get; set; } // Service Dealer Code
        public string ServiceDealerName { get; set; } // Service Dealer Name
        public string SOCode { get; set; }          // Sales Order Code
        public string SO { get; set; }              // Sales Order
        public string DivisionCode { get; set; }    // Division Code
        public string Division { get; set; }        // Division
        public string WarehouseCode { get; set; }   // Warehouse Code
        public string Warehouse { get; set; }       // Warehouse
        public string GLCode { get; set; }          // General Ledger Code
        public string GLDesc { get; set; }          // General Ledger Description
        public string CustomerPONo { get; set; }    // Customer PO No
        public int InvoiceAge { get; set; }         // Invoice Age
        public string InvoiceAgeBucket { get; set; } // Invoice Age Bucket
        public int CreditAge { get; set; }          // Credit Age
        public string CreditAgeBucket { get; set; } // Credit Age Bucket
        public string BillToSiteLocation { get; set; } // Bill To Site Location
        public string ShipToAddress { get; set; }   // Ship To Address
        public string Currency { get; set; }        // Currency
        public string OU { get; set; }              // Operating Unit
    }
}
