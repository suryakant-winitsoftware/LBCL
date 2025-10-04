using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface IRetailersFromTally : IBaseModel
    {
        public string LedgerName { get; set; }
        public string ParentName { get; set; }
        public string GUID { get; set; }
        public string OpeningBalance { get; set; }
        public string PrimaryGroup { get; set; }
        public string RemoteAltGUID { get; set; }
        public string Address { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string CountryName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Pincode { get; set; }
        public string StateName { get; set; }
        public string IncomeTaxNumber { get; set; }
        public string CountryOfResidence { get; set; }
        public string DistributorCode { get; set; }
        public string GSTIN { get; set; }
        public string Status { get; set; }
        public string DistributorOrgUID { get; set; }
    }
}
