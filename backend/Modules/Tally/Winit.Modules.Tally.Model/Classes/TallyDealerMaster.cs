using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallyDealerMaster : BaseModel , ITallyDealerMaster
    {
        public string LedgerName { get; set; }
        public string ParentName { get; set; }
        public string PrimaryGroup { get; set; }
        public string OpeningBalance { get; set; }
        public string RemoteAltGuid { get; set; }
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
        public string Status { get; set; }
        public string GSTIN { get; set; }
    }
}
