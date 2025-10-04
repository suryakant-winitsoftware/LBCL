using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IOrg : Base.Model.IBaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string OrgTypeUID { get; set; }
        public string ParentUID { get; set; }
        public string CountryUID { get; set; }
        public string CompanyUID { get; set; }
        public string TaxGroupUID { get; set; }
        public string Status { get; set; }
        public string SeqCode { get; set; }
        public bool HasEarlyAccess { get; set; }
        public bool IsSelected { get; set; }
        public bool ShowInUI { get; set; }
        public bool? ShowInTemplate { get; set; }


    }
}
