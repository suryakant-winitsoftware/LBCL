using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccPayableView : AccPayable, IAccPayableView
    {
        public string TaxInvoiceNumber { get; set; }
        public DateTime TaxInvoiceDate { get; set; }
    }
}
