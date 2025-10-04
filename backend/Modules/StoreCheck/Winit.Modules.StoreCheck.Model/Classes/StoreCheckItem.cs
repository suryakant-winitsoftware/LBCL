using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckItem
    {
        public string Uid { get; set; }
        public string SkuUid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Multiplier { get; set; }

        
    }
}
