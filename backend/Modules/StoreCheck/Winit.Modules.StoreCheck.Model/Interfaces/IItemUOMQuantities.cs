using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IItemUOMQuantities
    {
        Decimal? pcQty { get; set; }
        Decimal? CaseQty { get; set; }
        Decimal? TotalQty { get; set; }
    }
}
