using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.Models.Common
{
    public class SelectionItemFilter : SelectionItem
    {
        public SelectionMode Mode { get; set; }
        public SortDirection Direction { get; set; }
        public FilterActionType ActionType { get; set; }
        public Type DataType { get; set; } = typeof(string);
        public FilterGroupType FilterGroup { get; set; } = FilterGroupType.Field;
        public string? ImgPath { get; set; }
    }
}
