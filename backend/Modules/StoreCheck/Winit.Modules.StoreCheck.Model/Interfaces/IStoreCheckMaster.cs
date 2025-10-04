using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Classes;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IStoreCheckMaster
    {
         StoreCheckHistoryView? StoreCheckHistory { get; set; }
         List<StoreCheckItemHistory>? StoreCheckHistoryList { get; set; }
         List<StoreCheckItemExpiryDREHistory>? StoreCheckItemExpiryDREHistoriesList { get; set; }
         List<StoreCheckItemUomQty>? StoreCheckItemUomQtyList { get; set; }
    }
}
