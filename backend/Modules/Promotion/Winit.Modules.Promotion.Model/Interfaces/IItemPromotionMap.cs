using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IItemPromotionMap : IBaseModel
    {
        public string SKUType { get; set; }
        public string SKUTypeUID { get; set; }
        public string PromotionUID { get; set; }
    }

 }
