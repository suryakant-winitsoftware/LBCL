using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Mobile.Model.Classes
{
    public class MobileErrorLog : BaseModel, IMobileErrorLog
    {
        public string EmpUID { get; set; }
        public string DeviceId { get; set; }
        public string VersionId { get; set; }
        public string SourceType { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }
}
