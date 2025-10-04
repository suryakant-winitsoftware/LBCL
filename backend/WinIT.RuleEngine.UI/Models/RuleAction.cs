using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class RuleAction
    {
        public string Id { get; set; }
        public string RuleId { get; set; }
        [Required(ErrorMessage = "Choose ActionType")]
        public string ActionType { get; set; }
        [Required(ErrorMessage = "Enter Template")]
        public string Template { get; set; }

        //public string EmailTemplate { get; set; }
        //public string NotificationTemplate { get; set; }
        //public string SmsTemplate { get; set; }
    }
}
