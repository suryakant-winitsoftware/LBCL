using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Classes
{
    public class CategoryWiseTopChhannelPartnersRequest
    {
        public int CurrentYear { get; set; }
        public int LastYear { get; set; }
        public int Count { get; set; }
        public List<string> Types { get; set; } = [];
        public List<string> Groups { get; set; } = [];
    }
}
