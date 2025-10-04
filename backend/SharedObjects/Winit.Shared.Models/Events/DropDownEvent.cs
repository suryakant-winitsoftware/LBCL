using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Events
{
    public class DropDownEvent
    {
        public string UID { get; set; }
        public Winit.Shared.Models.Enums.SelectionMode SelectionMode { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> SelectionItems { get; set; }
    }
}
