using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallySKU : BaseModelV2, ITallySKU
    {
        public string DistributorCode { get; set; }
        public string SkuCode { get; set; }
        public string SkuName { get; set; }
    }
}
