using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IInvoiceInfo
    {
        public DateTime? InvoiceDate { get; set; }
        public string SourceType { get; set; }
        public string InvoiceNo { get; set; }
        public decimal EnteredAmount { get; set; }
    }
}
