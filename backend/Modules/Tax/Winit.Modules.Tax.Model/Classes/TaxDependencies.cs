using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxDependencies : BaseModel, ITaxDependencies
    {
        public string TaxUID { get; set; }
        public string DependsOnTaxUID { get; set; }
    }
}
