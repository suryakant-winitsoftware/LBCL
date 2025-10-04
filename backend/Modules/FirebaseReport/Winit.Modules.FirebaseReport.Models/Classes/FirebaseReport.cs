using Winit.Modules.Base.Model;
using Winit.Modules.FirebaseReport.Models.Interfaces;

namespace Winit.Modules.FirebaseReport.Models.Classes
{
    public class FirebaseReport : BaseModel, IFirebaseReport
    {
        public string LinkedItemUID { get; set; }
        public string LinkedItemType { get; set; }
        public string RequestBody { get; set; }
        public string UserCode { get; set; }
        public string NextUID { get; set; }
        public string CustomerCode { get; set; }
        public DateTime RequestCreatedTime { get; set; }
        public DateTime RequestPosted2ApiTime { get; set; }
        public DateTime RequestReceivedByApiTime { get; set; }
        public DateTime RequestSent2ServiceTime { get; set; }
        public DateTime RequestReceivedByServiceTime { get; set; }
        public DateTime RequestPostedToDBTime { get; set; }
        public DateTime NotificationSentTime { get; set; }
        public DateTime NotificationReceivedTime { get; set; }
        public DateTime RequestSent2LogApiTime { get; set; }
        public Boolean IsFailed { get; set; }
        public string Comments { get; set; }
        public int Status { get; set; }
        public string AppComments { get; set; }
    }
}
