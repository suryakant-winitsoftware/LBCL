using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxSlab 
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
