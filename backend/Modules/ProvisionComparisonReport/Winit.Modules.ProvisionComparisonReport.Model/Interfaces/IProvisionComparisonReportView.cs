namespace Winit.Modules.ProvisionComparisonReport.Model.Interfaces
{
    public interface IProvisionComparisonReportView
    {
        public string ArNo { get; set; }
        public string GstInvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string ItemCode { get; set; }
        public string SalesOffice { get; set; }
        public string Branch { get; set; }
        public string ChannelPartner { get; set; }
        public string BroadClassification { get; set; }
        public Decimal Qty { get; set; }
        public string ProvisionType { get; set; }
        public Decimal DmsProvisionAmount { get; set; }
        public Decimal OracleProvisionAmount { get; set; }
        public Decimal Diff { get; set; }
    }
}
