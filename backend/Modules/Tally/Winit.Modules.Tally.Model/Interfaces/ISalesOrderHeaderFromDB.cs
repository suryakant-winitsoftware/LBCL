using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ISalesOrderHeaderFromDB : IBaseModel
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string PARTYNAME { get; set; }
        public string VOUCHERTYPENAME { get; set; }
        public string VOUCHERNUMBER { get; set; }
        public string REFERENCE { get; set; }
        public string PARTYLEDGERNAME { get; set; }
        public string BASICBASEPARTYNAME { get; set; }
        public string FBTPAYMENTTYPE { get; set; }
        public string PERSISTEDVIEW { get; set; }
        public string BASICBUYERNAME { get; set; }
        public string OrderDate { get; set; }
        public string CGST { get; set; }
        public string SGST { get; set; }
        public string Amount { get; set; }
        public string ShipToAddressLine1 { get; set; }
        public string ShipToAddressLine2 { get; set; }
        public string ShipToAddressLine3 { get; set; }
        public string ShipToAddressLandmark { get; set; }
        public string ShipToAddressPincode { get; set; }
        public string ShipToAddressEmail { get; set; }
        public string ShipToAddressMobile { get; set; }
        public string ShipToAddressTown { get; set; }
        public string ShipToAddressDistrict { get; set; }
        public string ShipToAddressState { get; set; }
        public string GSTIN { get; set; }
        public string GSTINType { get; set; }

        public List<ISalesOrderLineFromDB> lstSalesOrderLineFromDB { get; set; }
        public List<ISalesOrderTaxDetailsTally> lstSalesOrderTaxDetails { get; set; }
    }
}
