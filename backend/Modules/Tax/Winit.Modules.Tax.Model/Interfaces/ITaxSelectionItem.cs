using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxSelectionItem:ITaxGroup
    {
        public bool IsSelected { get; set; }
        public string TaxUID { get; set; }
        public string TaxName { get; set;}
    }
}
