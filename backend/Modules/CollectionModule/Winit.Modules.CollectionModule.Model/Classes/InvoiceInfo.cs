using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class InvoiceInfo : IInvoiceInfo
    {
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string SourceType { get; set; }
        public decimal EnteredAmount { get; set; }
    }
}
