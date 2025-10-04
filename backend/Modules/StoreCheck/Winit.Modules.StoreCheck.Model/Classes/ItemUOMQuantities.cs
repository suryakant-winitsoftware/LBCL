using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Interfaces;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class ItemUOMQuantities : IItemUOMQuantities
    {

        public Decimal? pcQty { get; set; }
        public Decimal? CaseQty { get; set; }
        public Decimal? TotalQty { get; set; }
        public ItemUOMQuantities()
        {
            pcQty = null;
            CaseQty = null;
            TotalQty = null;
        }
    }
}
