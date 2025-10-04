using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IOrgType : Base.Model.IBaseModel
    {
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public bool IsCompanyOrg { get; set; }
        public bool IsFranchiseeOrg { get; set; }
        public bool IsWH { get; set; }
        public string WarehouseType { get; set; }
        public bool ShowInUI { get; set; }
        public bool? ShowInTemplate { get; set; }
    }
}
