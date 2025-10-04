using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxSlab :  ITaxSlab
    {
        public string TAXUID { get; set; }
        public decimal RangeStart { get; set; }
        public decimal RangeEnd { get; set; }
        public decimal TaxRate { get; set; }
        public string Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUpTo { get; set; }
    }
    

}
