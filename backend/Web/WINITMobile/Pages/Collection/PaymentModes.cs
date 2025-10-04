using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Pages.Collection
{
    public class PaymentModes
    {
        public const string ENABLE_PAYMENT_MODE_CASH = "ENABLE_PAYMENT_MODE_CASH";
        public const string ENABLE_PAYMENT_MODE_CHEQUE = "ENABLE_PAYMENT_MODE_CHEQUE";
        public const string ENABLE_PAYMENT_MODE_POS = "ENABLE_PAYMENT_MODE_POS";
        public const string ENABLE_PAYMENT_MODE_ONLINE = "ENABLE_PAYMENT_MODE_ONLINE";
        
        public const string CASH = "Cash";
        public const string CHEQUE = "Cheque";
        public const string POS = "POS";
        public const string ONLINE = "Online";
    }
    public class ModeType
    {
        public string Name { get; set; } = "";
        public int Order => MapOrder(Name);
        public int MapOrder(string Name)
        {
            switch (Name)
            {
                case PaymentModes.CASH:
                    return (int)Mode.Cash;
                case PaymentModes.CHEQUE:
                    return (int)Mode.Cheque;
                case PaymentModes.POS:
                    return (int)Mode.POS;
                case PaymentModes.ONLINE:
                    return (int)Mode.Online;
                default:
                    return 0;
            }
        }
    }
    public enum Mode
    {
        Cash,Cheque,POS,Online
    }
    
}
