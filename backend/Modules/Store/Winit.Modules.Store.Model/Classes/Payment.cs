using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class Payment: BaseModel,IPayment
    {
        public string PriceType { get; set; }
        public string PaymentMode { get; set; }
        public decimal AverageMonthlyIncome { get; set; }
        public string InvoiceDeliveryMethod { get; set; }
        public string BankUID { get; set; }
        public string AccountNumber { get; set; }
        public int NoOfCashCounters { get; set; }
    }
}
