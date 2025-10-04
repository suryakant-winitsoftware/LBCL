using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IStoreCheckItemHistory:IBaseModel
    {
         string StoreCheckHistoryUID { get; set; }
         string GroupByCode { get; set; }
         string GroupByValue { get; set; }
         string SKUUID { get; set; }
         string SKUCode { get; set; }
         string UOM { get; set; }
         decimal SuggestedQty { get; set; }
         decimal StoreQty { get; set; }
         decimal BackstoreQty { get; set; }
         decimal ToFillQty { get; set; }
         bool IsAvailable { get; set; }
         bool IsDreSelected { get; set; }
    }
}

