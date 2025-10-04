using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Mobile.Model.Interfaces
{
    public interface IMobileErrorLog : IBaseModel
    {
       string EmpUID { get; set; }
       string DeviceId { get; set; }
       string VersionId { get; set; }
       string SourceType { get; set; }
       string ErrorMessage { get; set; }
       string StackTrace { get; set; }
    }
}
