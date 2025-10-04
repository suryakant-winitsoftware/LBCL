using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Constants
{
    public class ActionTypeConstants
    {
        public const string Email = "Email";
        public const string Notification = "Notification";
        public const string Discount = "Discount";
        public const string Shipping = "Shipping ";
    }
    public enum RuleEngineStatus
    {
        Approved,
        Pending,
        InProgress,
        Rejected
    }
    public class requestStatus
    {
        public string status { get; set; }
        public string requesterid { get; set; }
    }
}
