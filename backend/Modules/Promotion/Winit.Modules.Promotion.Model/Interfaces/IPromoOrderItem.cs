using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromoOrderItem : IBaseModel
    {
        public string PromoOrderUID { get; set; }
        public string ParentUID { get; set; }
        public string ItemCriteriaType { get; set; }
        public string ItemCriteriaSelected { get; set; }
        public bool IsCompulsory { get; set; }
        public string ItemUOM { get; set; }
        public decimal PromoSplit { get; set; }
        public decimal? MinQty { get; set; }
        public decimal? MaxQty { get; set; }
    }

 }
