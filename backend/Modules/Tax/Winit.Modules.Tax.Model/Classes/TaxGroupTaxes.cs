using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxGroupTaxes : BaseModel, ITaxGroupTaxes
    {
        public string TaxGroupUID { get; set; }
        public string TaxUID { get; set; }
        public ActionType ActionType { get; set; }
    }
}
