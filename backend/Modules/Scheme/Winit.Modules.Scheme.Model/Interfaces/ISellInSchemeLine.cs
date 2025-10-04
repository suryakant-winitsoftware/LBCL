using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISellInSchemeLine : IBaseModel
    {
        string SellInSchemeHeaderUID { get; set; }
        int LineNumber { get; set; }
        string SKUUID { get; set; }
        string DiscountType { get; set; }
        string SKUName { get; set; }
        string SKUCode{ get; set; }
        int IsDeleted { get; set; }
        decimal CommittedQty { get; set; }
        decimal Temp_CommittedQty { get; set; }
        decimal DPPrice { get; set; }
        decimal MinimumSellingPrice { get; set; }
        decimal LadderingPrice { get; set; }
        int RequestPrice { get; set; }
        decimal Temp_RequestPrice { get; set; }
        string InvoiceDiscountType { get; set; }
        string CreditNoteDiscountType { get; set; }
        decimal Discount { get; set; }
        decimal InvoiceDiscount { get; set; }
        decimal Temp_InvoiceDiscount { get; set; }
        decimal CreditNoteDiscount { get; set; }
        decimal Temp_CreditNoteDiscount { get; set; }
        decimal FinalDealerPrice { get; set; }
        decimal ProvisionAmount2 { get; set; }
        decimal ProvisionAmount3 { get; set; }
        decimal StandingProvisionAmount { get; set; }
        decimal Margin { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        bool RequestPriceSelected { get; set; }
        bool CommittedQtySelected { get; set; }
        bool InvoiceDiscountSelected { get; set; }
        bool CreditNoteDiscountSelected { get; set; }
        bool P2Selected { get; set; }
        bool P3Selected { get; set; }
    }
}
