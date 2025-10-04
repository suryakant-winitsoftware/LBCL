using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class SalesOrderHeaderFromTally : BaseModel, ISalesOrderHeaderFromTally
    {
        public string DistributorCode { get; set; }
        public string DMSUID { get; set; }
        public string VOUCHERNUMBER { get; set; }
        public string DATE { get; set; }
        public string GUID { get; set; }
        public string STATENAME { get; set; }
        public string COUNTRYOFRESIDENCE { get; set; }
        public string PARTYNAME { get; set; }
        public string VOUCHERTYPENAME { get; set; }
        public string PARTYLEDGERNAME { get; set; }
        public string BASICBASEPARTYNAME { get; set; }
        public string PERSISTEDVIEW { get; set; }
        public string PLACEOFSUPPLY { get; set; }
        public string BASICBUYERNAME { get; set; }
        public string BASICDATETIMEOFINVOICE { get; set; }
        public string CONSIGNEEPINNUMBER { get; set; }
        public string CONSIGNEESTATENAME { get; set; }
        public string VOUCHERKEY { get; set; }
        public string AMOUNT { get; set; }
        public string CGST { get; set; }
        public string SGST { get; set; }
        public string GST { get; set; }
        public string DistributorOrgUID { get; set; }
        public List<ISalesOrderLineFromTally>? SalesOrderLines { get; set; }
    }
}