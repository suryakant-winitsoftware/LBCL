using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.UIInterfaces;

namespace Winit.Modules.Tax.Model.UIClasses
{
    public class TaxView: ITaxView
    {
        public string TaxType { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public decimal Rate { get; set; }
    }
}
