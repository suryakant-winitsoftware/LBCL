using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Mobile.Model.Classes
{
    public class MobileAppAction : BaseModel, IMobileAppAction
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
