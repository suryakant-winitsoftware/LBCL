using System;
using Winit.Modules.CollectionModule.Model.Classes;


namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface ICollectionPrint
    {
        public List<ICollectionPrintDetails> collectionPrintDetails { get; set; }
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AliasName { get; set; }
        public string LegalName { get; set; }
        public string Type { get; set; }
        public string StoreRating { get; set; }
        public string CreatedByJobPositionUID { get; set; }
        public string CountryUID { get; set; }
        public string RegionUID { get; set; }
        public string CityUID { get; set; }
        public string OutletName { get; set; }


        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Landmark { get; set; }
        public string Area { get; set; }
        public string SubArea { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string PhoneExtension { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Email { get; set; }


        public string Sales_Org { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string Applicable_Type { get; set; }
        public string Applicable_Code { get; set; }
        public string Payment_Mode { get; set; }
        public int Advance_Paid_Days { get; set; }
        public string Discount_Type { get; set; }
        public decimal Discount_Value { get; set; }
        public bool IsActive { get; set; }
        public DateTime Valid_From { get; set; }
        public DateTime Valid_To { get; set; }
        public bool Applicable_OnPartial_Payments { get; set; }
        public bool Applicable_OnOverDue_Customers { get; set; }
        public string ReceiptNumber { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyUID { get; set; }
        public string OrgUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public string Status { get; set; }

        public string SessionUserCode { get; set; }
        public string StoreUID { get; set; }
        public string UID { get; set; }
        public string AccCollectionUID { get; set; }
        public string BankUID { get; set; }
        public string Branch { get; set; }
        public string ChequeNo { get; set; }
        public string Comments { get; set; }
        public string ApproveComments { get; set; }
        public string DefaultCurrencyUID { get; set; }
        public decimal DefaultCurrencyExchangeRate { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public DateTime? ChequeDate { get; set; }
        public DateTime? RealizationDate { get; set; }

    }
}
