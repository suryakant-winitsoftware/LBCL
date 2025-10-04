using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models
{
    public class LogEntry
    {
        public string UID { get; set; }
        public string LinkedItemUID { get; set; }
        public string NextUID { get; set; }
        public string LinkedItemType { get; set; }
        public string RequestBody { get; set; }
        public string UserCode { get; set; }
        public string CustomerCode { get; set; }
        public string RequestCreatedTime { get; set; }//app
        public string RequestPostedToApiTime { get; set; }//app
        public string NotificationReceivedTime { get; set; } //app
        public string RequestSent2LogApiTime { get; set; } //app
        public string IsFailed { get; set; } //app and backend
        public string Comments { get; set; } //app and backend
        public string AppComments { get; set; } //app and backend
        public string ModifiedTime { get; set; } //app and backend
    }
}
