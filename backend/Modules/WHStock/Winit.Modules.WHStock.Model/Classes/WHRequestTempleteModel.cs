using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

using Winit.Modules.WHStock.Model.Interfaces;

namespace Winit.Modules.WHStock.Model.Classes
{
    public class WHRequestTempleteModel 
    {
       public WHStockRequest WHStockRequest { get; set; }
        public List<WHStockRequestLine> WHStockRequestLines { get; set; }
        public List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? WHStockLedgerList { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}
