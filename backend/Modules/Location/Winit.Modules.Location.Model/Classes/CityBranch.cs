using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class CityBranch : ICityBranch
{
    public string UID { get; set; }
    public string StateCodeName { get; set; }
    public string CityCodeName { get; set; }
    public  string BranchCodeName { get; set; }

}

