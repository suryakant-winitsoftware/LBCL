using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IWHStockRequestLineItemViewUI : IWHStockRequestLineItemView
    {
        public string SKUUID { get; set; }
        public decimal UOM1CNF { get; set; }
        public bool IsSelected { get; set; }
    }
}
