using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccPayableView : IAccPayable
    {
        public string TaxInvoiceNumber { get; set; }
        public DateTime TaxInvoiceDate { get; set; }
    }
}
