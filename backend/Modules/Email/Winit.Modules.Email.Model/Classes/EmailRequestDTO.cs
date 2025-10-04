using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Email.Model.Classes
{
    public class EmailRequestDTO
    {
        public string UID { get; set; }
        public string MailRequestUID { get; set; }
        public string Content { get; set; }
        public int RetryCount { get; set; }
        public string Subject { get; set; }
        public string MailFormat { get; set; }
        public string FromMail { get; set; }
        public string ToMail { get; set; }
        public string CcMail { get; set; }
        public string BccMail { get; set; }
    }
}
