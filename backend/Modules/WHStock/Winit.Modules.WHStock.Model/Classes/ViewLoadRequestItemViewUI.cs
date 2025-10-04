using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.WHStock.Model.Interfaces;

namespace Winit.Modules.WHStock.Model.Classes
{
    public class ViewLoadRequestItemViewUI 
    {
               
        public WHStockRequestItemViewUI WHStockRequest { get; set; }
        public List<WHStockRequestLineItemViewUI> WHStockRequestLines { get; set; }


    }
}
