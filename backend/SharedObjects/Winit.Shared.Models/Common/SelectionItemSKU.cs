using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class SelectionItemSKU : SelectionItem
    {
        public bool IsMCL { get; set; }
        public bool IsPromo { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
    }
}
