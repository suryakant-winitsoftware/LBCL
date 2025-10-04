using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckItemHistory : BaseModel, IStoreCheckItemHistory
    {
        public string? StoreCheckHistoryUID { get; set; }
        public string? GroupByCode { get; set; }
        public string? GroupByValue { get; set; }
        public string? SKUUID { get; set; }
        public string? SKUCode { get; set; }
        public string? UOM { get; set; }
        public decimal SuggestedQty { get; set; }
        public decimal StoreQty { get; set; }
        public decimal BackstoreQty { get; set; }
        public decimal ToFillQty { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDreSelected { get; set; }
    }
}
