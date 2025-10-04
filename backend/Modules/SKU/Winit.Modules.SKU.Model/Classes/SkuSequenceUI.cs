using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SkuSequenceUI : SkuSequence
    {
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
