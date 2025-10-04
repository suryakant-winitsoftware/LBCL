using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Mobile.Model.Interfaces
{
    public interface IMobileAppAction : IBaseModel
    {
        public string EmpUID { get; set; }
        public string Action { get; set; }
        public int Status { get; set; }
        public DateTime ActionDate { get; set; }
        public string Result { get; set; }
        public string OrgUID { get; set; }
        public string LoginId { get; set; }
    }
}
