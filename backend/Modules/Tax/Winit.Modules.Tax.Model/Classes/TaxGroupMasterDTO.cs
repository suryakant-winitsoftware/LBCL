using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxGroupMasterDTO
    {
        public TaxGroup TaxGroup { get; set; }
        public List<TaxGroupTaxes> TaxGroupTaxes { get; set; }
    }
}
