
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes
{
    public class ReturnOrderMaster :  IReturnOrderMaster
    {
        public Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder? ReturnOrder { get; set; }

        public List<IReturnOrderLine>? ReturnOrderLineList { get; set; }
        public List<IReturnOrderTax>? ReturnOrderTaxList { get; set; }

    }

    public class ReturnOrderMasterDTO 
    {
        public Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder? ReturnOrder { get; set; }

        public List<ReturnOrderLine>? ReturnOrderLineList { get; set; }
        public List<ReturnOrderTax>? ReturnOrderTaxList { get; set; }
        public Winit.Modules.JourneyPlan.Model.Classes.StoreHistory? StoreHistory { get; set; }
        public List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? WHStockLedgerList { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }

    }
}
