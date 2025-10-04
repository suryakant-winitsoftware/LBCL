using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderViewModelDCO
    {
        public bool IsNewOrder { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public List<SalesOrderLine>? SalesOrderLines { get; set; }
        public List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? WHStockLedgerList { get; set; }
        public Winit.Modules.JourneyPlan.Model.Classes.StoreHistory? StoreHistory { get; set; }
        public Winit.Modules.CollectionModule.Model.Classes.AccPayable? AccPayable { get; set; }
        public ActionType ActionType { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}
