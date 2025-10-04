using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromoOfferItem : IBaseModel
    {
        public string PromoOfferUID { get; set; }
        public string ItemCriteriaType { get; set; }
        public string ItemCriteriaSelected { get; set; }
        public bool IsCompulsory { get; set; }
        public string ItemUOM { get; set; }
        public decimal? Quantity { get; set; }
    }

 }
