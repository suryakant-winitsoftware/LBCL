using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class Action
    {
        public long Id { get; set; }
        public int RuleId { get; set; }
        public string ActionType { get; set; }
        public string EmailTemplate { get; set; }
        public string NotificationTemplate { get; set; }
        public string SmsTemplate { get; set; }
    }
}
