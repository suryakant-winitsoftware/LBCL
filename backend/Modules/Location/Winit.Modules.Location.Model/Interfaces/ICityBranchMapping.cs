using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ICityBranchMapping : IBaseModel
    {
       string CityLocationUID { get; set; }
       string BranchLocationUID { get; set; }
    }
}
