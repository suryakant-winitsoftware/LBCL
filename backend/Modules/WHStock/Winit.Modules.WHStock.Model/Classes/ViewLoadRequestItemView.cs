using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

using Winit.Modules.WHStock.Model.Interfaces;

namespace Winit.Modules.WHStock.Model.Classes
{
    public class ViewLoadRequestItemView: IViewLoadRequestItemView
    {
        public IWHStockRequestItemView WHStockRequest { get; set; }
        public List<IWHStockRequestLineItemView> WHStockRequestLines { get; set; }
    }
}
