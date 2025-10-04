using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IStoreCheckItemExpiryDREHistory : IBaseModel
    {
        public string StoreCheckItemHistoryUID { get; set; }
        public string StockType { get; set; }
        public string StockSubType { get; set; }
        public string Notes { get; set; }
        public string BatchNo { get; set; }
        public string ExpiryDate { get; set; }
        public string Reason { get; set; }
        public decimal? Qty { get; set; }
        public string Uom { get; set; }
        public bool IsRowModified { get; set; }
        public ActionType ActionType { get; set; }
    }
}
