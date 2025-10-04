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
    public class StoreCheckItemUomQty : BaseModel, IStoreCheckItemUomQty
    {
        public string StoreCheckItemHistoryUID { get; set; }
        public string UOM { get; set; }
        public string BaseUom { get; set; }
        public decimal? UomMultiplier { get; set; }
        public decimal? StoreQty { get; set; }
        public decimal? StoreQtyBu { get; set; }
        public decimal? BackStoreQty { get; set; }
        public decimal? BackStoreQtyBu { get; set; }
        public ActionType ActionType { get; set; }
        public bool IsBaseUOMTaken { get; set; }
        public bool IsOuterUOMTaken { get; set; }
        public bool IsRowModified { get; set; }
    }
}
