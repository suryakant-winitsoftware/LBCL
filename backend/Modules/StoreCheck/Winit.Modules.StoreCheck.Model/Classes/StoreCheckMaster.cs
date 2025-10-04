using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Interfaces;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckMaster : IStoreCheckMaster
    {
        public StoreCheckHistoryView? StoreCheckHistory { get; set; }
        public List<StoreCheckItemHistory>? StoreCheckHistoryList { get; set; }
        public List<StoreCheckItemExpiryDREHistory>? StoreCheckItemExpiryDREHistoriesList { get; set; }
        public List<StoreCheckItemUomQty>? StoreCheckItemUomQtyList { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}
