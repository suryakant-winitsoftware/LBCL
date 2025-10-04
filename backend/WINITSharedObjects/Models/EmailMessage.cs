using System;
using System.Collections.Generic;

namespace WINITSharedObjects.Models
{
    public class EmailMessage
    {
        public int smtpport { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public string from { get; set; }
        public List<string> to { get; set; }
        public string smtpserver { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string displayName { get; set; }
        public bool IsBodyHtml { get; set; } = true;
        public bool EnableSsl { get; set; }
        public List<string> attachments { get; set; } = null;
    }
}
