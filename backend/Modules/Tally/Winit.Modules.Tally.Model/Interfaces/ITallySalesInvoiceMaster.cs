using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallySalesInvoiceMaster : IBaseModel
    {
        public string Dmsuid { get; set; }
        public string VoucherNumber { get; set; }
        public string Date { get; set; }
        public string Guid { get; set; }
        public string StateName { get; set; }
        public string CountryOfResidence { get; set; }
        public string PartyName { get; set; }
        public string VoucherTypeName { get; set; }
        public string PartyLedgerName { get; set; }
        public string BasicBasePartyName { get; set; }
        public string PlaceOfSupply { get; set; }
        public string BasicBuyerName { get; set; }
        public string BasicDatetimeOfInvoice { get; set; }
        public string ConsigneePinNumber { get; set; }
        public string ConsigneeStateName { get; set; }
        public string VoucherKey { get; set; }
        public string Amount { get; set; }
        public string PersistedView { get; set; }
        public string DistributorCode { get; set; }
        public string Cgst { get; set; }
        public string Sgst { get; set; }
        public string Gst { get; set; }
        public string Status { get; set; }
    }
}
