using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class EarlyPaymentDiscountAppliedDetails : EarlyPaymentDiscountConfiguration, IEarlyPaymentDiscountAppliedDetails
    {
        public string EarlyPaymentDiscountNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceDueAmount { get; set; }
    }
}
