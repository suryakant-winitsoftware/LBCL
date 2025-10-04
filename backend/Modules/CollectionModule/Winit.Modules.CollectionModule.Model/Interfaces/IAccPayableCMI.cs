using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccPayableCMI : IBaseModel
    {
        public string OU { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string TaxInvoiceNumber { get; set; }
        public decimal TotalBalanceAmount { get; set; }
        public DateTime TaxInvoiceDate { get; set; }
        public int CountOfInvoices { get; set; }
    }
}
