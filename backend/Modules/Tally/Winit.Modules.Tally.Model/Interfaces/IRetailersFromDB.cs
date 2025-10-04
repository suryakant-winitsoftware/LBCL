using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface IRetailersFromDB : IBaseModel
    {
        public string ledgerUID { get; set; }
        public string ledgerCode { get; set; }
        public string ledgerName { get; set; }
        public string parentName { get; set; }
        public string ledgerOrgUID { get; set; }
        public string ledgerOrgName { get; set; }
        public string ADDRESS { get; set; }
        public string ADDRESS1 { get; set; }
        public string ADDRESS2 { get; set; }
        public string COUNTRYNAME { get; set; }
        public string EMAIL { get; set; }
        public string PHONE { get; set; }
        public string PINCODE { get; set; }
        public string STATENAME { get; set; }
        public string INCOMETAXNUMBER { get; set; }
        public string PANNUMBER { get; set; }
        public string COUNTRYOFRESIDENCE { get; set; }
    }
}
