using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Interfaces;

namespace Winit.Modules.Email.Model.Classes
{
    public class EmailRequestModel : IEmailRequestModel
    {
        public string UID { get; set; }
        public string MailRequestUID { get; set; }
        public int RetryCount { get; set; }
        public string Subject { get; set; }
        public string FromMail { get; set; }
        public string MailFormat { get; set; }
        public int Priority { get; set; } = 1;
        public string MessageType { get; set; } = "Transactional";
        public string Content { get; set; }
        public List<string> ToMail { get; set; }
        public List<string> CcMail { get; set; }
        public List<string> BccMail { get; set; }
    }
}
