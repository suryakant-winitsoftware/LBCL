using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockAudit.Model.Interfaces;

namespace Winit.Modules.StockAudit.Model.Classes
{
    public  class StockAuditDisplay: IStockAuditDisplay
    {
        public string UID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string BaseUOM { get; set; }
        public string OuterUOM { get; set; }
        public Decimal BaseUOMConverision { get; set; }
        public Decimal OuterUOMConversion { get; set; }
        public Decimal TotalQuantity { get; set; }
        public string FirstUOMQty { get; set; }
        public string SecondUOMQty { get; set; }
        public Shared.Models.Common.ISelectionItem FirstSelectedUOM { get; set; }
        public Shared.Models.Common.ISelectionItem SecondSelectedUOM { get; set; }
        public List<Shared.Models.Common.ISelectionItem> FirstUOMForSelection { get; set; }
        public List<Shared.Models.Common.ISelectionItem> SecondUOMForSelection { get; set; }

    }
}
