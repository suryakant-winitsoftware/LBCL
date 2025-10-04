using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class Location : BaseModel, ILocation
{
    public string CompanyUID { get; set; }
    public string LocationTypeUID { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string ParentUID { get; set; }
    public int ItemLevel { get; set; }
    public bool HasChild { get; set; }
    public string LocationTypeName { get; set; } = string.Empty;
    public string LocationTypeCode { get; set; } = string.Empty;
}

