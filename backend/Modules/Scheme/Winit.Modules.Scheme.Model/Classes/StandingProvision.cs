using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class StandingProvision : IStandingProvision
    {
        public string SKUUID { get; set; }
        public decimal? Amount { get; set; }
    }
}
