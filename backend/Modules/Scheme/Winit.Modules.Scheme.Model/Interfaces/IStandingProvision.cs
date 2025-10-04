using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IStandingProvision
    {
        string SKUUID { get; set; }
        decimal? Amount{ get; set; }
    }
}
