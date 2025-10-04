using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallySKU : IBaseModelV2
    {
        public string DistributorCode { get; set; }
        public string SkuCode { get; set; }
        public string SkuName { get; set; }
    }
}
