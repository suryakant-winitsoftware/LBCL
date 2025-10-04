using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class CityBranchMapping : BaseModel, ICityBranchMapping
{
     public string CityLocationUID { get; set; }
    public  string BranchLocationUID { get; set; }
}

