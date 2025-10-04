using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.UIState.Classes
{
    public class PageState
    {
        public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();
        public string SelectedTabUID { get; set; }
        public string ExtraData { get; set; }
    }
}
