using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ICityBranch 
    {
         string UID { get; set; }
         string StateCodeName { get; set; }
         string CityCodeName { get; set; }
         string BranchCodeName { get; set; }
    }
}
