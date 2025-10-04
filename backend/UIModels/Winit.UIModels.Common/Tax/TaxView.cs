using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common.Tax
{
    public class TaxView
    {
        public string TaxType { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public decimal Rate { get; set; }
    }
}
