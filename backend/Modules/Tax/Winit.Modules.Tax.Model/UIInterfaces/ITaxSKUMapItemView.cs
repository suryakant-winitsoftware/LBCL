using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.UIInterfaces
{
    public interface ITaxSKUMapItemView:IBaseModel
    {
        public bool IsChecked { get; set; }
        public string CompanyUID { get; set; }
        public string SKUName { get; set; }
        public string SKUCode { get; set; }
        public string SKUUID { get; set; }
        public string TaxUID { get; set; }
        public ActionType ActionType { get; set; }
    }
}
