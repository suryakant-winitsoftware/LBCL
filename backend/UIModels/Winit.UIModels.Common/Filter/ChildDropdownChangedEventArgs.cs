using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.UIModels.Common.Filter
{
    public class ChildDropdownChangedEventArgs
    {
        public string ParentColumnName { get; set; }
        public List<ISelectionItem> SelectionItems { get; set; }
    }
}
