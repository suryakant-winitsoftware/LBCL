using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Classes;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISmsModel
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string Sender { get; set; }
        public int Priority { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public string RequestStatus { get; set; }
        public DateTime RequestTime { get; set; } 
        public DateTime? SentTime { get; set; }
        public string ErrorDetails { get; set; }
        public int RetryCount { get; set; } 
        public string GatewayProvider { get; set; }
        public string ResponseCode { get; set; }
        public DateTime? ResponseTime { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseMessage { get; set; }
        public string BatchId { get; set; }
        public List<SmsModelReceiver> Receivers { get; set; }
    }
}
