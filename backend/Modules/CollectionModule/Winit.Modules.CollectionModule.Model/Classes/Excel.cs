using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class Excel : IExcel
    {
        public string CustomerCode { get; set; }
        public string Currency { get; set; }
        public string PaymentMode { get; set; }
        public string BankCode { get; set; }
        public string BankBranchName { get; set; }
        public string ChequeNumber { get; set; }
        public string TrxType { get; set; }
        public string TrxCode { get; set; }
        public DateTime CollectedDate { get; set; }
        public string PaidAmount { get; set; }
    }
}
