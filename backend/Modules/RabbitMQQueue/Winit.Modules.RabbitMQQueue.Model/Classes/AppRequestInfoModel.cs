using Winit.Modules.Base.Model;
using Winit.Modules.RabbitMQQueue.Model.Interfaces;

namespace Winit.Modules.RabbitMQQueue.Model.Classes
{
    public class AppRequestInfoModel : BaseModel, IAppRequestInfoModel
    {
        public string OrgUID { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public int YearMonth { get; set; }
        public string AppRequestUID { get; set; }
        public DateTime RequestSentToServiceTime { get; set; }
        public DateTime RequestReceivedByServiceTime { get; set; }
        public DateTime RequestPostedToDBTime { get; set; }
        public DateTime RequestSentToLogAPITime { get; set; }
        public bool IsFailed { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
    }
}
