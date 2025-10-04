using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes;

public class LocationTemplateMaster 
{
    public LocationTemplate LocationTemplate { get; set; }
    public  List<LocationTemplateLine> LocationMappingLineList { get; set; }
}





