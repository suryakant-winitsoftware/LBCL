using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IPayment :IBaseModel
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
