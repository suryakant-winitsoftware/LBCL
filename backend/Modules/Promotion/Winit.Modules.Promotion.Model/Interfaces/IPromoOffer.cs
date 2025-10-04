using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromoOffer : IBaseModel
    {
        public string PromoOrderUID { get; set; }
        public string Type { get; set; }
        public string QualificationLevel { get; set; }
        public string ApplicationLevel { get; set; }
        public string SelectionModel { get; set; }
        public bool HasOfferItemSelection { get; set; }
    }

 }
