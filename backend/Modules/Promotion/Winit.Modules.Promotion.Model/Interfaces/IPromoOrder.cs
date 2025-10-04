using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromoOrder : IBaseModel
    {
        public string PromotionUID { get; set; }
        public string SelectionModel { get; set; }
        public string QualificationLevel { get; set; }
        public int MinDealCount { get; set; }
        public int MaxDealCount { get; set; }
    }

 }
