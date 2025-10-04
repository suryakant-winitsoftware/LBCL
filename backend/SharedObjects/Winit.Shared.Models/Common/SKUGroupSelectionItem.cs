using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class SKUGroupSelectionItem:SelectionItem
    {
        public bool AvailableForFilter { get; set; }
        public string ParentCode { get; set; }
    }
}
