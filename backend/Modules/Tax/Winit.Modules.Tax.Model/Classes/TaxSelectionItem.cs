using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxSelectionItem:TaxGroup,ITaxSelectionItem
    {
        public bool IsSelected { get; set; }
        public string TaxUID { get; set; }
        public string TaxName { get; set; }
    }
}
