using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class ManageScheme : BaseModel, IManageScheme
    {
        public string EmpName { get; set; }
        public string Code { get; set; }
        public string ChannelPartnerName { get; set; }
        public string ChannelPartnerUID { get; set; }
        public string Branch { get; set; }
        public string ChannelPartner { get; set; }
        public string SchemeType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Status { get; set; }
        public bool HasHistory { get; set; }
        public string EndDateRemarks { get; set; }
        public string EndDateUpdatedByEmpUID { get; set; }
        public DateTime EndDateUpdatedOn { get; set; }
    }
}
