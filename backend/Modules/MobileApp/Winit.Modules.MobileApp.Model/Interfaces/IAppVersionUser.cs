using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Mobile.Model.Interfaces
{
    public interface IAppVersionUser : IBaseModel
    {
        public string EmpUID { get; set; }
        public string Name { get; set; }
        public string? DeviceType { get; set; }
        public string DeviceId { get; set; }
        public string? AppVersion { get; set; }
        public int AppVersionNumber { get; set; }
        public string? ApiVersion { get; set; }
        public DateTime? DeploymentDateTime { get; set; }
        public string? NextAppVersion { get; set; }
        public int NextAppVersionNumber { get; set; }
        public DateTime? PublishDate { get; set; }
        public bool IsTest { get; set; }
        public string? IMEINo { get; set; }
        public string? OrgUID { get; set; }
        public string? GcmKey { get; set; }
        public string? IMEINo2 { get; set; }
    }
}
