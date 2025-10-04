using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IViewLoadRequestItemViewUI
    {
               
        public IWHStockRequestItemViewUI WHStockRequest { get; set; }
        public List<IWHStockRequestLineItemViewUI> WHStockRequestLines { get; set; }
    }
}
