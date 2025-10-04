using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.Models.Common
{
    public class SelectionItem: ISelectionItem
    {
        public string UID { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Label { get; set; }
        public object? ExtData { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSelected_InDropDownLevel { get; set; }
    }
   
}
