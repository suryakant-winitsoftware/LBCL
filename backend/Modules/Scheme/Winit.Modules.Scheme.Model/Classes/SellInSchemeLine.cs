using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellInSchemeLine : BaseModel, ISellInSchemeLine
    {
        public string SellInSchemeHeaderUID { get; set; }
        public int LineNumber { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string DiscountType { get; set; }
        public string SKUName { get; set; }
        public int IsDeleted { get; set; }
        public decimal CommittedQty { get; set; }
        public decimal Temp_CommittedQty { get; set; }
        public decimal DPPrice { get; set; }
        public decimal MinimumSellingPrice { get; set; }
        public decimal LadderingPrice { get; set; }
        public int RequestPrice { get; set; }
        public decimal Temp_RequestPrice { get; set; }
        public string InvoiceDiscountType { get; set; }
        public string CreditNoteDiscountType { get; set; }
        public decimal Discount { get; set; }
        public decimal InvoiceDiscount { get; set; }
        public decimal Temp_InvoiceDiscount { get; set; }
        public decimal CreditNoteDiscount { get; set; }
        public decimal Temp_CreditNoteDiscount { get; set; }
        public decimal FinalDealerPrice { get; set; }
        public decimal ProvisionAmount2 { get; set; }
        public decimal ProvisionAmount3 { get; set; }
        public decimal StandingProvisionAmount { get; set; }
        public decimal Margin { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool RequestPriceSelected { get; set; }
        public bool CommittedQtySelected { get; set; }
        public bool InvoiceDiscountSelected { get; set; }
        public bool CreditNoteDiscountSelected { get; set; }
        public bool P2Selected { get; set; }
        public bool P3Selected { get; set; }
    }
}
