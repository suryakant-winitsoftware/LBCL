using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ReturnOrder.Model.Interfaces
{
    public interface IReturnOrderMaster
    {
        public Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder? ReturnOrder { get; set; }

        public List<IReturnOrderLine>? ReturnOrderLineList { get; set; }
        public List<IReturnOrderTax>? ReturnOrderTaxList { get; set; }

    }
}
