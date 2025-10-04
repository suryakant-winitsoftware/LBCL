using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.RSSMailQueue.Model.Interfaces;

namespace Winit.Modules.RSSMailQueue.Model.Classes
{
    public class RSSMailQueue:BaseModel,IRSSMailQueue
    {
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public int MailStatus { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromMail { get; set; }
        public string CCMail { get; set; }
        public string ToMail { get; set; }
        public bool HasAttachment { get; set; }
        public string AttachmentFormatCode { get; set; }
        public string FormatCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
