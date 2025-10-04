using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Classes;

namespace Winit.Modules.Email.Model.Interfaces
{
    public interface IMailRequest
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string FromEmail { get; set; }
        public int Priority { get; set; }
        public string MessageType { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public int HasAttachment { get; set; }
        public string AttachmentFormat { get; set; }
        public string MailFormat { get; set; }
        public string RequestStatus { get; set; }
        public DateTime? SentTime { get; set; }
        public string ErrorDetails { get; set; }
        public int RetryCount { get; set; }
        public string BatchId { get; set; }
        public List<EmailModelReceiver> Receivers { get; set; }
    }
}
