using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.Model.Classes
{
    public class AppRequest : IAppRequest
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string OrgUID { get; set; }
        public string LinkedItemType { get; set; }
        public int YearMonth { get; set; }
        public string LinkedItemUID { get; set; }
        public string EmpUID { get; set; }
        public string JobPositionUID { get; set; }
        public DateTime? RequestCreatedTime { get; set; }
        public DateTime? RequestPostedToAPITime { get; set; }
        public DateTime? RequestReceivedByAPITime { get; set; }
        public string? NextUID { get; set; }
        public string RequestBody { get; set; }
        public string RequestUIDs { get; set; }
        public bool IsNotificationReceived { get; set; }
    }
}
