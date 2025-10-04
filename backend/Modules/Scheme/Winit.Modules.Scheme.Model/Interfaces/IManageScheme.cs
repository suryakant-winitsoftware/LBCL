using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IManageScheme:IBaseModel
    {
         string EmpName { get; set; }
         string Code { get; set; }
         string ChannelPartnerName { get; set; }
         string ChannelPartnerUID { get; set; }
         string ChannelPartner { get; set; }
         string Branch { get; set; }
         string SchemeType { get; set; }
         DateTime? CreatedOn { get; set; }
         DateTime? ValidFrom { get; set; }
         DateTime? ValidUpto { get; set; }
         DateTime? LastUpdated { get; set; }
         string Status { get; set; }
        bool HasHistory {  get; set; }
        public string EndDateRemarks { get; set; }
        public string EndDateUpdatedByEmpUID { get; set; }
        public DateTime EndDateUpdatedOn { get; set; }
    }
}
